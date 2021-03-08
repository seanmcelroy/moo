namespace moo.common.Scripting.ForthPrimatives
{
    public enum ForthErrorResult : byte
    {
        INTERRUPTED,
        STACK_UNDERFLOW,
        TYPE_MISMATCH,
        INVALID_VALUE,
        VARIABLE_NOT_FOUND,
        UNKNOWN_TYPE,
        INTERNAL_ERROR,
        VARIABLE_ALREADY_DEFINED,
        DIVISION_BY_ZERO,
        SYNTAX_ERROR,
        VARIABLE_IS_CONSTANT,
        NO_SUCH_OBJECT,
        INSUFFICIENT_PERMISSION
    }
}