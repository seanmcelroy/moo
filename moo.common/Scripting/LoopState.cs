namespace moo.common.Scripting
{
    // Base type for per-loop iteration state carried by ControlFlowMarker.
    // Concrete subclasses (ForeachLoopState, ForLoopState) land in PR2 alongside
    // the FOREACH and FOR primitives. The i/j/k primitives in PR3 read
    // CurrentIndex by walking the control-flow stack.
    public abstract class LoopState
    {
        public abstract int CurrentIndex { get; }
        public abstract bool IsExhausted { get; }
        public abstract bool Advance();
    }
}
