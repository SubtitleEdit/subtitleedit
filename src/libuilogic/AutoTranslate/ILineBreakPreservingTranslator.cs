namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// An engine that keeps every line break of the request at the matching place in the
    /// reply, also inside a sentence. The merge/split helper then joins the rows of a
    /// sentence spread over several rows with a line break instead of a space, and reads the
    /// row boundaries straight back from the reply, so the words land in the row whose
    /// timing they belong to. An engine without this marker gets the sentence on one line
    /// and its reply is cut up by the length/duration heuristics (#14803).
    /// </summary>
    public interface ILineBreakPreservingTranslator
    {
    }
}
