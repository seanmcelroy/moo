namespace moo.common.Scripting
{
    public struct ControlFlowMarker
    {
        public readonly ControlFlowElement Element;
        public readonly int Index;

        public ControlFlowMarker(ControlFlowElement element, int index)
        {
            this.Element = element;
            this.Index = index;
        }
    }
}