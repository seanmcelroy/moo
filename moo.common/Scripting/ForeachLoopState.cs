using System.Collections.Generic;

namespace moo.common.Scripting
{
    // Iteration state for FOREACH. Holds an enumerator over (key, value)
    // pairs and tracks the current zero-based iteration index so PR3's
    // i/j/k primitives can read it.
    internal sealed class ForeachLoopState : LoopState
    {
        private readonly IEnumerator<KeyValuePair<object, ForthDatum>> enumerator;
        private int index = -1;
        private bool exhausted;

        public ForeachLoopState(IEnumerator<KeyValuePair<object, ForthDatum>> enumerator)
        {
            this.enumerator = enumerator;
        }

        public override int CurrentIndex => index;
        public override bool IsExhausted => exhausted;

        public KeyValuePair<object, ForthDatum> Current => enumerator.Current;

        public override bool Advance()
        {
            if (exhausted) return false;
            if (enumerator.MoveNext())
            {
                index++;
                return true;
            }
            exhausted = true;
            return false;
        }
    }
}
