namespace moo.common.Scripting
{
    public struct ControlFlowMarker
    {
        public readonly ControlFlowElement Element;
        public readonly int Index;
        public readonly LoopState? Loop;

        public ControlFlowMarker(ControlFlowElement element, int index)
            : this(element, index, null) { }

        public ControlFlowMarker(ControlFlowElement element, int index, LoopState? loop)
        {
            this.Element = element;
            this.Index = index;
            this.Loop = loop;
        }
    }
}
