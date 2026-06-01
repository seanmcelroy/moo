using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using moo.common.Scripting.ForthPrimatives;

namespace moo.common.Scripting
{
    public static class ForthUtility
    {
        public static bool TryPopStackRange(
            this Stack<ForthDatum> stack,
            [NotNullWhen(true)] out List<ForthDatum>? stackrange,
            [NotNullWhen(false)] out ForthErrorResult? err)
        {
            ArgumentNullException.ThrowIfNull(stack);

            if (stack.Count < 1)
            {
                stackrange = null;
                err = ForthErrorResult.STACK_UNDERFLOW;
                return false;
            }

            var n1 = stack.Pop();
            if (n1.Type != ForthDatum.DatumType.Integer)
            {
                stackrange = null;
                err = ForthErrorResult.TYPE_MISMATCH;
                return false;
            }

            var rangeLength = n1.UnwrapInt();

            if (stack.Count < rangeLength)
            {
                stackrange = null;
                err = ForthErrorResult.STACK_UNDERFLOW;
                return false;
            }

            stackrange = new List<ForthDatum>(rangeLength);
            for (int i = 0; i < rangeLength; i++)
                stackrange.Add(stack.Pop());
            err = null;

            return true;
        }

        public static bool TryPopArrayIndex(
            this Stack<ForthDatum> stack,
            [NotNullWhen(true)] out object? index,
            [NotNullWhen(false)] out ForthErrorResult? err)
        {
            ArgumentNullException.ThrowIfNull(stack);

            if (stack.Count < 1)
            {
                index = null;
                err = ForthErrorResult.STACK_UNDERFLOW;
                return false;
            }

            var sIndex = stack.Pop();
            if ((sIndex.Type != ForthDatum.DatumType.Integer
                && sIndex.Type != ForthDatum.DatumType.String)
                || sIndex.Value == null)
            {
                index = null;
                err = ForthErrorResult.TYPE_MISMATCH;
                return false;
            }

            index = sIndex.Value;
            err = null;
            return true;
        }
    }
}