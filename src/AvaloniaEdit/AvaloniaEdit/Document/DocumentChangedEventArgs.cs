using System;

namespace AvaloniaEdit.Document
{
    public class DocumentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the old TextDocument.
        /// </summary>
        public TextDocument OldDocument { get; private set; }
        /// <summary>
        /// Gets the new TextDocument.
        /// </summary>
        public TextDocument NewDocument { get; private set; }

        /// <summary>
        /// Provides data for the <see cref="ITextEditorComponent.DocumentChanged"/> event.
        /// </summary>
        public DocumentChangedEventArgs(TextDocument oldDocument, TextDocument newDocument)
        {
            OldDocument = oldDocument;
            NewDocument = newDocument;
        }
    }
}
