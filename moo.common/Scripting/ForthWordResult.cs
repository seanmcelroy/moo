using System.Collections.Generic;
using moo.common.Models;
using moo.common.Scripting.ForthPrimatives;

namespace moo.common.Scripting
{
    public struct ForthWordResult
    {
        public static ForthWordResult SUCCESS = new()
        {
            isSuccessful = true
        };

        private bool isSuccessful;
        private readonly object? result;
        private readonly string reason;
        private Dbref? lastListItem;

        public bool IsSuccessful => isSuccessful;
        public object? Result => result;
        public string Reason => reason;
        public Dbref? LastListItem => lastListItem;

        public Dictionary<string, ForthVariable>? dirtyVariables;

        public ForthWordResult(string reason, Dbref? lastListItem = null)
        {
            this.isSuccessful = true;
            this.result = null;
            this.reason = reason;
            this.lastListItem = lastListItem;
            this.dirtyVariables = null;
        }

        public ForthWordResult(ForthErrorResult errorCode, string reason)
        {
            this.isSuccessful = false;
            this.result = errorCode;
            this.reason = reason ?? System.Enum.GetName(typeof(ForthErrorResult), errorCode) ?? errorCode.ToString();
            this.lastListItem = null;
            this.dirtyVariables = null;
        }
    }
}