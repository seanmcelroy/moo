using System.Threading.Tasks;
using moo.common.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    /// <summary>
    /// End-to-end tests for the FOREACH and FOR control-flow primitives
    /// introduced in PR2 (REPORT-5.2.md).
    /// </summary>
    [TestClass]
    public class LoopForeachAndForTests
    {
        [TestMethod]
        public async Task ForeachListIteratesAllElements()
        {
            // Sums the values of a 3-element list. FOREACH pushes key, value
            // each iteration; `swap pop` drops the key, leaving the value
            // to be added to the accumulator below.
            const string program = @": main
  { 10 20 30 } array_make
  0 swap
  foreach
    swap pop
    +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(60, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachListPushesIntegerKeysInOrder()
        {
            // Verifies FOREACH pushes the iteration index as the key for
            // list arrays. Sum of keys for a 3-element list is 0+1+2 = 3.
            const string program = @": main
  { 10 20 30 } array_make
  0 swap
  foreach
    pop
    +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(3, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachEmptyArraySkipsBody()
        {
            // Empty-array branch: FOREACH pushes the loop marker + skip;
            // REPEAT pops both and falls through. The body must not run,
            // so the sentinel 999 inside the body never reaches the stack.
            const string program = @": main
  0 array_make
  foreach
    pop pop
    999
  repeat
  42
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(42, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachDictIteratesAllPairs()
        {
            // Sums the values of a dictionary array. FOREACH on a dict
            // pushes (key, value) where the key is the original dict key.
            const string program = @": main
  ""a"" 10 ""b"" 20 ""c"" 30 3 array_make_dict
  0 swap
  foreach
    swap pop
    +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(60, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachTypeMismatchOnNonArray()
        {
            // FOREACH of an integer must fail with TYPE_MISMATCH.
            const string program = @": main
  42
  foreach
    pop pop
  repeat
;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task ForeachStackUnderflowOnEmptyStack()
        {
            // FOREACH with nothing on the stack must fail with STACK_UNDERFLOW.
            const string program = @": main
  foreach
    pop pop
  repeat
;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task ForeachBreakExitsLoopEarly()
        {
            // Iterates [1, 2, 3]; accumulates until value = 2, then BREAK.
            // Expected accumulator: 1 + 2 = 3 (third iteration is skipped).
            // Exercises BREAK inside IF/THEN — the idiomatic MUF pattern.
            const string program = @": main
  { 1 2 3 } array_make
  0 swap
  foreach
    swap pop
    +
    dup 3 = if
      break
    then
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(3, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachBreakInSkippedIfDoesNotFire()
        {
            // BREAK inside an `if` whose condition is false must NOT fire.
            // Pre-fix, BREAK's skipped-branch check missed InIfAndSkip and
            // would fire anyway, exiting the loop after the first iteration.
            // With the fix, the loop runs to completion: 1 + 2 + 3 = 6.
            const string program = @": main
  { 1 2 3 } array_make
  0 swap
  foreach
    swap pop
    +
    0 if
      break
    then
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(6, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachExitReturnsFromWord()
        {
            // EXIT inside FOREACH returns from the current word with success.
            // The accumulator gets the first element (1), then EXIT fires
            // before any further iteration.
            const string program = @": main
  { 1 2 3 } array_make
  0 swap
  foreach
    swap pop
    +
    exit
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(1, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForeachNestedLoops()
        {
            // Outer iterates [a, b] (2 elements), inner iterates [10, 20, 30]
            // and sums. Total iterations of inner body = 2 * 3 = 6, summing
            // (10+20+30) twice = 120.
            const string program = @": main
  0
  { 1 2 } array_make
  foreach
    swap pop
    pop
    { 10 20 30 } array_make
    foreach
      swap pop
      +
    repeat
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(120, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForCountsInclusively()
        {
            // 1 5 FOR runs the body 5 times (inclusive of endpoints).
            // Counter access via `i` lands in PR3; we verify via body
            // side-effect counting.
            const string program = @": main
  0
  1 5 for
    1 +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(5, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForSingleIterationWhenStartEqualsEnd()
        {
            // 3 3 FOR runs the body exactly once.
            const string program = @": main
  0
  3 3 for
    1 +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(1, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForEmptyRangeRunsZeroTimes()
        {
            // 5 1 FOR (start > end) must not execute the body at all.
            const string program = @": main
  0
  5 1 for
    1 +
  repeat
  100 +
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(100, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ForStackUnderflowOnSingleValue()
        {
            // FOR requires two values on the stack; only one is provided.
            const string program = @": main
  5 for
    1
  repeat
;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task ForTypeMismatchOnNonInteger()
        {
            // FOR requires integer start and end.
            const string program = @": main
  ""a"" 5 for
    1
  repeat
;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task ForNestedInsideForeach()
        {
            // Outer FOREACH iterates 2 elements; inner FOR runs 3 iterations
            // each. Accumulator increments per inner-body run: 2 * 3 = 6.
            const string program = @": main
  0
  { 1 2 } array_make
  foreach
    swap pop
    pop
    1 3 for
      1 +
    repeat
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(6, stack.Pop().Value);
        }
    }
}
