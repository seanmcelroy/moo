using System;
using System.Collections.Generic;

namespace Tests
{
    public static class CollectionUtility
    {
        public static Stack<T> ClonePreservingOrder<T>(this Stack<T> original)
        {
            var arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
    }
}