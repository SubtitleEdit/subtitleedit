// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Highlighting
{
    /// <summary>
    /// Manages a list of syntax highlighting definitions.
    /// </summary>
    /// <remarks>
    /// All memers on this class (including instance members) are thread-safe.
    /// </remarks>
    public class HighlightingManager : IHighlightingDefinitionReferenceResolver
    {
        private sealed class DelayLoadedHighlightingDefinition : IHighlightingDefinition
        {
            private readonly object _lockObj = new object();
            private Func<IHighlightingDefinition> _lazyLoadingFunction;
            private IHighlightingDefinition _definition;
            private Exception _storedException;

            public DelayLoadedHighlightingDefinition(string name, Func<IHighlightingDefinition> lazyLoadingFunction)
            {
                Name = name;
                _lazyLoadingFunction = lazyLoadingFunction;
            }

            public string Name => field ?? GetDefinition().Name;

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                                                             Justification = "The exception will be rethrown")]
            private IHighlightingDefinition GetDefinition()
            {
                Func<IHighlightingDefinition> func;
                lock (_lockObj)
                {
                    if (_definition != null)
                        return _definition;
                    func = _lazyLoadingFunction;
                }
                Exception exception = null;
                IHighlightingDefinition def = null;
                try
                {
                    using (var busyLock = BusyManager.Enter(this))
                    {
                        if (!busyLock.Success)
                            throw new InvalidOperationException("Tried to create delay-loaded highlighting definition recursively. Make sure the are no cyclic references between the highlighting definitions.");
                        def = func();
                    }
                    if (def == null)
                        throw new InvalidOperationException("Function for delay-loading highlighting definition returned null");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                lock (_lockObj)
                {
                    _lazyLoadingFunction = null;
                    if (_definition == null && _storedException == null)
                    {
                        _definition = def;
                        _storedException = exception;
                    }
                    if (_storedException != null)
                        throw new HighlightingDefinitionInvalidException("Error delay-loading highlighting definition", _storedException);
                    return _definition;
                }
            }

            public HighlightingRuleSet MainRuleSet => GetDefinition().MainRuleSet;

            public HighlightingRuleSet GetNamedRuleSet(string name)
            {
                return GetDefinition().GetNamedRuleSet(name);
            }

            public HighlightingColor GetNamedColor(string name)
            {
                return GetDefinition().GetNamedColor(name);
            }

            public IEnumerable<HighlightingColor> NamedHighlightingColors => GetDefinition().NamedHighlightingColors;

            public override string ToString()
            {
                return Name;
            }

            public IDictionary<string, string> Properties => GetDefinition().Properties;
        }

        private readonly object _lockObj = new object();
        private readonly Dictionary<string, IHighlightingDefinition> _highlightingsByName = new Dictionary<string, IHighlightingDefinition>();
        private readonly Dictionary<string, IHighlightingDefinition> _highlightingsByExtension = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly List<IHighlightingDefinition> _allHighlightings = new List<IHighlightingDefinition>();

        /// <summary>
        /// Gets a highlighting definition by name.
        /// Returns null if the definition is not found.
        /// </summary>
        public IHighlightingDefinition GetDefinition(string name)
        {
            lock (_lockObj)
            {
                return _highlightingsByName.GetValueOrDefault(name);
            }
        }

        /// <summary>
        /// Gets a copy of all highlightings.
        /// </summary>
        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
        {
            get
            {
                lock (_lockObj)
                {
                    return new ReadOnlyCollection<IHighlightingDefinition>(_allHighlightings);
                }
            }
        }

        /// <summary>
        /// Gets a highlighting definition by extension.
        /// Returns null if the definition is not found.
        /// </summary>
        public IHighlightingDefinition GetDefinitionByExtension(string extension)
        {
            lock (_lockObj)
            {
                return _highlightingsByExtension.GetValueOrDefault(extension);
            }
        }

        /// <summary>
        /// Registers a highlighting definition.
        /// </summary>
        /// <param name="name">The name to register the definition with.</param>
        /// <param name="extensions">The file extensions to register the definition for.</param>
        /// <param name="highlighting">The highlighting definition.</param>
        public void RegisterHighlighting(string name, string[] extensions, IHighlightingDefinition highlighting)
        {
            if (highlighting == null)
                throw new ArgumentNullException(nameof(highlighting));

            lock (_lockObj)
            {
                _allHighlightings.Add(highlighting);
                if (name != null)
                {
                    _highlightingsByName[name] = highlighting;
                }
                if (extensions != null)
                {
                    foreach (var ext in extensions)
                    {
                        _highlightingsByExtension[ext] = highlighting;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a highlighting definition.
        /// </summary>
        /// <param name="name">The name to register the definition with.</param>
        /// <param name="extensions">The file extensions to register the definition for.</param>
        /// <param name="lazyLoadedHighlighting">A function that loads the highlighting definition.</param>
        public void RegisterHighlighting(string name, string[] extensions, Func<IHighlightingDefinition> lazyLoadedHighlighting)
        {
            if (lazyLoadedHighlighting == null)
                throw new ArgumentNullException(nameof(lazyLoadedHighlighting));
            RegisterHighlighting(name, extensions, new DelayLoadedHighlightingDefinition(name, lazyLoadedHighlighting));
        }

        /// <summary>
        /// Gets the default HighlightingManager instance.
        /// The default HighlightingManager comes with built-in highlightings.
        /// </summary>
        public static HighlightingManager Instance => DefaultHighlightingManager.Instance;

        internal sealed class DefaultHighlightingManager : HighlightingManager
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new static DefaultHighlightingManager Instance { get; } = new DefaultHighlightingManager();

            public DefaultHighlightingManager()
            {
                Resources.RegisterBuiltInHighlightings(this);
            }

            // Registering a built-in highlighting
            internal void RegisterHighlighting(string name, string[] extensions, string resourceName)
            {
                try
                {

                    RegisterHighlighting(name, extensions, LoadHighlighting(resourceName));
                }
                catch (HighlightingDefinitionInvalidException ex)
                {
                    throw new InvalidOperationException("The built-in highlighting '" + name + "' is invalid.", ex);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                                                             Justification = "LoadHighlighting is used only in release builds")]
            private Func<IHighlightingDefinition> LoadHighlighting(string resourceName)
            {
                IHighlightingDefinition Func()
                {
                    XshdSyntaxDefinition xshd;
                    using (var s = Resources.OpenStream(resourceName))
                    using (var reader = XmlReader.Create(s))
                    {
                        // in release builds, skip validating the built-in highlightings
                        xshd = HighlightingLoader.LoadXshd(reader, true);
                    }
                    return HighlightingLoader.Load(xshd, this);
                }

                return Func;
            }
        }
    }
}
