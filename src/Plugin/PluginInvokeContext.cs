using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Plugin
{
    public class PluginInvokeContext
    {
        /// <summary>
        /// The invoker, Main.cs in most of the case.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// The subtitle file currently being used  by Subtitle Edit
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The video file currently being used by Subtitle Edit.
        /// </summary>
        public string Video { get; set; }

        /// <summary>
        /// The UI line break style that should be used to format text.
        /// </summary>
        public string UiLineBreak { get; set; }

        /// <summary>
        /// The SRT text formated to be passed into plugin
        /// </summary>
        public string SrtText { get; set; }

        /// <summary>
        /// The current frame rate being used
        /// </summary>
        public double Frame { get; set; }

        /// <summary>
        /// The raw text read from file when first loaded.
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// The instance of <see cref="SubtitleEdit.Core.Subtitle"/> that will be passed to the plugin.
        /// </summary>
        public Subtitle Subtitle { get; set; }

    }
}