using System.Collections.Generic;

namespace moo.common.Scripting
{
    public struct EditorResult
    {
        public static EditorResult NORMAL_CONTINUE = new()
        {
            isSuccessful = true,
            shouldExit = false
        };

        public static EditorResult NORMAL_EXIT = new()
        {
            isSuccessful = true,
            shouldExit = true
        };

        private bool isSuccessful;
        private bool shouldExit;
        private readonly object? result;
        private readonly string reason;

        public bool IsSuccessful => isSuccessful;
        public bool ShouldExit => shouldExit;
        public object? Result => result;
        public string Reason => reason;

        public EditorResult(bool shouldExit, string reason)
        {
            this.isSuccessful = true;
            this.shouldExit = shouldExit;
            this.result = null;
            this.reason = reason;
        }

        public EditorResult(EditorErrorResult errorCode, string reason)
        {
            this.isSuccessful = false;
            this.shouldExit = true;
            this.result = errorCode;
            this.reason = reason ?? System.Enum.GetName(typeof(EditorErrorResult), errorCode) ?? errorCode.ToString();
        }
    }
}