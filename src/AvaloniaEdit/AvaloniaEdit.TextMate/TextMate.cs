using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public static class TextMate
    {
        /// <summary>
        /// Installs TextMate integration into the specified text editor, enabling advanced syntax highlighting and editing
        /// features.
        /// </summary>
        /// <remarks>Ensure that the editor and registry options are properly configured before calling this method.</remarks>
        /// <param name="editor">The text editor instance in which to install the TextMate integration. Must not be null.</param>
        /// <param name="registryOptions">The registry configuration options that determine how TextMate features are applied
        /// within the editor. Must not be null.</param>
        /// <param name="initCurrentDocument">Indicates whether the current document in the editor should be initialized with TextMate features upon installation.
        /// The default is <see langword="true"/>.</param>
        /// <param name="exceptionHandler">An optional delegate that handles exceptions occurring during the installation process and during subsequent
        /// TextMate-related operations. This includes, for example, failures while loading grammars or themes from <paramref name="registryOptions"/>,
        /// and exceptions thrown during ongoing syntax highlighting such as background tokenization, reacting to document/content changes, or editor
        /// option changes. If null, this API does not catch those exceptions, and they will propagate according to normal .NET exception handling
        /// (for example, to the caller, to the thread invoking the operation, or as unhandled exceptions).</param>
        /// <returns>An <see cref="Installation"/> object representing the result of the TextMate installation, which can be used to
        /// manage the integration lifecycle.</returns>
        public static Installation InstallTextMate(
            this TextEditor editor,
            IRegistryOptions registryOptions,
            bool initCurrentDocument = true,
            Action<Exception> exceptionHandler = null)
        {
            return new Installation(editor, registryOptions, initCurrentDocument, exceptionHandler);
        }

        public class Installation : IDisposable
        {
            private bool _isDisposed;
            private readonly object _lock = new object();
            private readonly Registry _textMateRegistry;
            private readonly TextEditor _editor;
            private Action<Exception> _exceptionHandler;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed",
                Justification = "Disposed in Dispose(bool) and DisposeEditorModel methods")]
            private TextEditorModel _editorModel;
            private IGrammar _grammar;
            private TMModel _tmModel;
            private TextMateColoringTransformer _transformer;
            private readonly bool _ownsTransformer;
            private ReadOnlyDictionary<string, string> _themeColorsDictionary;
            public IRegistryOptions RegistryOptions { get; }
            public TextEditorModel EditorModel => Volatile.Read(ref _editorModel);

            public event EventHandler<Installation> AppliedTheme;

            /// <summary>
            /// Initializes a new instance of the Installation class, configuring syntax highlighting for the specified
            /// text editor using the provided registry options.
            /// </summary>
            /// <remarks>This constructor sets up the necessary components for syntax highlighting and
            /// registers event handlers to respond to document changes in the editor.</remarks>
            /// <param name="editor">The text editor instance to be configured for syntax highlighting. Cannot be null.</param>
            /// <param name="registryOptions">The registry options that define syntax highlighting rules and themes. Cannot be null.</param>
            /// <param name="initCurrentDocument">Indicates whether to initialize syntax highlighting for the current document in the editor immediately
            /// upon installation. The default is <see langword="true"/>.</param>
            /// <param name="exceptionHandler">
            /// An optional delegate that handles exceptions occurring during the installation and initialization process,
            /// during syntax highlighting and line transformation operations (for example, within <see cref="TextMateColoringTransformer" />),
            /// and during internal operations such as applying themes, loading grammars, and reacting to editor document changes.
            /// </param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="editor"/> or <paramref name="registryOptions"/> is null.</exception>
            public Installation(
                TextEditor editor,
                IRegistryOptions registryOptions,
                bool initCurrentDocument = true,
                Action<Exception> exceptionHandler = null)
            {
                RegistryOptions = registryOptions ?? throw new ArgumentNullException(nameof(registryOptions));
                _editor = editor ?? throw new ArgumentNullException(nameof(editor));
                _exceptionHandler = exceptionHandler;

                _textMateRegistry = new Registry(registryOptions);
                _transformer = _editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

                if (_transformer is null)
                {
                    _transformer = new TextMateColoringTransformer(_editor.TextArea.TextView, _exceptionHandler);
                    _editor.TextArea.TextView.LineTransformers.Add(_transformer);
                    _ownsTransformer = true;
                }

                SetTheme(registryOptions.GetDefaultTheme());

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                if (initCurrentDocument)
                {
                    OnEditorOnDocumentChanged(editor, EventArgs.Empty);
                }
            }

            public void SetGrammar(string scopeName)
            {
                ThrowIfDisposed();

                lock (_lock)
                {
                    ThrowIfDisposed();

                    SetGrammarInternal(_textMateRegistry.LoadGrammar(scopeName));
                }

                // Redraw outside lock - _editor is readonly; Redraw() is a safe
                // invalidation call that schedules a render pass.
                _editor.TextArea.TextView.Redraw();
            }

            public void SetGrammarFile(string path)
            {
                ThrowIfDisposed();

                lock (_lock)
                {
                    ThrowIfDisposed();

                    if (_transformer == null || _editor?.TextArea?.TextView.LineTransformers == null)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(TextMate)} is not initialized. You must call {nameof(TextMate)}.{nameof(InstallTextMate)} before using this feature.");
                    }

                    SetGrammarInternal(_textMateRegistry.LoadGrammarFromPathSync(path, 0, null));
                }

                // Redraw outside lock - _editor is readonly; Redraw() is a safe
                // invalidation call that schedules a render pass.
                _editor.TextArea.TextView.Redraw();
            }

            /// <summary>
            /// Assigns the grammar and pushes it to the transformer.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Caller MUST hold <see cref="_lock"/> and MUST have verified <c>!_isDisposed</c>.
            /// Does NOT call <see cref="AvaloniaEdit.TextEditor.TextArea.TextView.Redraw()"/>.
            /// The caller is responsible for triggering a redraw after releasing the lock.
            /// </para>
            /// </remarks>
            /// <param name="grammar">The grammar to apply. Defines the tokenization rules for syntax highlighting.
            /// May be <see langword="null"/> to clear the current grammar.</param>
            private void SetGrammarInternal(IGrammar grammar)
            {
                _grammar = grammar;
                _transformer.SetGrammar(_grammar);
            }

            public bool TryGetThemeColor(string colorKey, out string colorString)
            {
                ThrowIfDisposed();

                // Volatile.Read provides a lock-free, thread-safe snapshot of the reference.
                // ReadOnlyDictionary is inherently safe for concurrent reads, so no lock is
                // needed for a single-reference read of an immutable collection.
                var dict = Volatile.Read(ref _themeColorsDictionary);

                if (dict == null)
                    throw new ObjectDisposedException(nameof(Installation));

                return dict.TryGetValue(colorKey, out colorString);
            }

            public void SetTheme(IRawTheme theme)
            {
                ThrowIfDisposed();

                EventHandler<Installation> appliedTheme;

                lock (_lock)
                {
                    ThrowIfDisposed();

                    if (_textMateRegistry == null || _transformer == null || _editor?.TextArea?.TextView.LineTransformers == null)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(TextMate)} is not initialized. You must call {nameof(TextMate)}.{nameof(InstallTextMate)} before using this feature.");
                    }

                    _textMateRegistry.SetTheme(theme);

                    var registryTheme = _textMateRegistry.GetTheme();
                    _transformer.SetTheme(registryTheme);

                    _tmModel?.InvalidateLine(0);

                    _editorModel?.InvalidateViewPortLines();

                    _themeColorsDictionary = registryTheme.GetGuiColorDictionary();

                    // Capture delegate under lock; invoke outside to prevent deadlocks
                    // if a subscriber calls back into this Installation.
                    appliedTheme = AppliedTheme;
                }

                appliedTheme?.Invoke(this, this);
            }

            /// <summary>
            /// Releases all resources used by this <see cref="Installation"/> instance.
            /// </summary>
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by this <see cref="Installation"/> instance
            /// and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">
            /// <c>true</c> to release both managed and unmanaged resources;
            /// <c>false</c> to release only unmanaged resources.
            /// </param>
            protected virtual void Dispose(bool disposing)
            {
                // Fast path: Volatile.Read avoids lock acquisition when already disposed.
                if (Volatile.Read(ref _isDisposed))
                    return;

                if (!disposing)
                    return;

                TextEditorModel editorModel;
                TMModel tmModel;
                TextMateColoringTransformer transformer;

                lock (_lock)
                {
                    // Authoritative check under lock.
                    if (Volatile.Read(ref _isDisposed))
                        return;

                    // Volatile.Write ensures that Volatile.Read callers outside the lock
                    // (ThrowIfDisposed, fast-path guards) see this write with proper
                    // memory ordering. Both sides use the Volatile API to satisfy
                    // analyzers that require matching synchronization primitives.
                    // Mark disposed first to prevent reentrancy from the DocumentChanged
                    // event handler recreating state during teardown.
                    Volatile.Write(ref _isDisposed, true);

                    // Capture mutable field references into locals so teardown can proceed
                    // outside the lock without racing against other methods.
                    editorModel = _editorModel;
                    _editorModel = null;

                    tmModel = _tmModel;
                    _tmModel = null;

                    transformer = _transformer;
                    _transformer = null;

                    _grammar = null;
                    _themeColorsDictionary = null;
                    _exceptionHandler = null;

                    // Sever delegate chains to prevent subscribers (e.g., ViewModels) from
                    // rooting this Installation and its entire object graph.
                    AppliedTheme = null;
                }

                // Perform teardown outside lock to avoid calling external code under lock,
                // which could lead to deadlocks if any callback attempts to re-acquire _lock.
                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeEditorModel(editorModel);

                DisposeTMModel(tmModel, transformer);

                // Only remove/dispose if we created it; otherwise we don't own its lifecycle.
                if (_ownsTransformer && transformer != null)
                {
                    _editor.TextArea.TextView.LineTransformers.Remove(transformer);
                    DisposeTransformer(transformer);
                }
                else if (transformer != null)
                {
                    // We don't own the transformer's lifecycle, but we do own the
                    // model/document references we pushed into it via SetModel.
                    // Clear them to prevent stale GC roots.
                    transformer.SetModel(null, null);
                }
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                // Fast path: Volatile.Read avoids lock acquisition when already disposed.
                // Don't use ThrowIfDisposed here - throwing exceptions in an event handler
                // is problematic and can tear down the application.
                if (Volatile.Read(ref _isDisposed))
                    return;

                lock (_lock)
                {
                    // Authoritative check under lock
                    if (Volatile.Read(ref _isDisposed))
                        return;

                    try
                    {
                        DisposeEditorModel(_editorModel);
                        DisposeTMModel(_tmModel, _transformer);

                        _editorModel = new TextEditorModel(_editor.TextArea.TextView, _editor.Document, _exceptionHandler);
                        _tmModel = new TMModel(_editorModel);
                        _tmModel.SetGrammar(_grammar);

                        _transformer.SetModel(_editor.Document, _tmModel);
                        _tmModel.AddModelTokensChangedListener(_transformer);
                    }
                    catch (Exception ex)
                    {
                        _exceptionHandler?.Invoke(ex);
                    }
                }
            }

            /// <summary>
            /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
            /// Uses <see cref="Volatile.Read(ref bool)"/> for a lock-free memory-barrier-safe
            /// read paired with <see cref="Volatile.Write(ref bool, bool)"/> in
            /// <see cref="Dispose(bool)"/>, suitable as a fast-path guard before acquiring
            /// <see cref="_lock"/>.
            /// </summary>
            void ThrowIfDisposed()
            {
                if (Volatile.Read(ref _isDisposed))
                    throw new ObjectDisposedException(nameof(Installation));
            }

            static void DisposeTransformer(TextMateColoringTransformer transformer)
            {
                if (transformer == null)
                    return;

                transformer.Dispose();
            }

            static void DisposeTMModel(TMModel tmModel, TextMateColoringTransformer transformer)
            {
                if (tmModel == null)
                    return;

                if (transformer != null)
                    tmModel.RemoveModelTokensChangedListener(transformer);

                tmModel.Dispose();
            }

            static void DisposeEditorModel(TextEditorModel editorModel)
            {
                if (editorModel == null)
                    return;

                editorModel.Dispose();
            }
        }
    }
}