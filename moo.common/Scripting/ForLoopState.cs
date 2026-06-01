namespace moo.common.Scripting
{
    // Iteration state for FOR (start end -- ). Counts from start up to and
    // including end by 1. Fuzzball's basic FOR is ascending; descending and
    // explicit-step variants are out of scope for PR2.
    internal sealed class ForLoopState : LoopState
    {
        private readonly int end;
        private int current;
        private bool started;
        private bool exhausted;

        public ForLoopState(int start, int end)
        {
            this.current = start;
            this.end = end;
        }

        public override int CurrentIndex => current;
        public override bool IsExhausted => exhausted;

        public override bool Advance()
        {
            if (exhausted) return false;
            if (!started)
            {
                started = true;
                if (current > end) { exhausted = true; return false; }
                return true;
            }
            current++;
            if (current > end) { exhausted = true; return false; }
            return true;
        }
    }
}
