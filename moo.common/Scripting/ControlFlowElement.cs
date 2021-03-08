namespace moo.common.Scripting
{
    public enum ControlFlowElement
    {
        InIfAndContinue,
        InIfAndSkip,
        InElseAndContinue,
        InElseAndSkip,
        SkippedBranch,
        BeginMarker,
        ForMarker,
        ForEachMarker,
        SkipToAfterNextUntilOrRepeat
    }
}