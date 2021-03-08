using static moo.common.Scripting.ForthDatum;
using static moo.common.Scripting.ForthProcess;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class SetMode
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SETMODE (i -- ) 

            Sets the current multitasking mode to the given mode. The integer this uses will be the same as one of
            those defined by the standard $defines bg_mode, fg_mode, and pr_mode, being background, foreground, and
            preempt mode, respectively. Programs set BOUND will run PREEMPT, ignoring this mode.
            */

            if (parameters.Stack.Count == 0)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SETMODE requires one parameter");

            var si = parameters.Stack.Pop();
            if (si.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SETMODE requires the top parameter on the stack to be an integer");

            int i = si.UnwrapInt();
            if (i < 0 || i > 2)
                return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, $"SETMODE requires PR_MODE, FG_MODE, or BG_MODE as valid parameters");

            // TODO: Change mode.
            if (i == (int)MultitaskingMode.Background)
                parameters.Process.Background();
            else if (i == (int)MultitaskingMode.Foreground)
                parameters.Process.Foreground();
            else if (i == (int)MultitaskingMode.Preempt)
                parameters.Process.Preempt();

            return ForthPrimativeResult.SUCCESS;
        }
    }
}