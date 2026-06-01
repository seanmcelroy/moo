using System.Threading.Tasks;
using moo.common.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    /// <summary>
    /// End-to-end tests for the CONTINUE flow-control primitive
    /// introduced in PR3 (REPORT-5.3.md).
    /// </summary>
    [TestClass]
    public class LoopContinueTests
    {
        [TestMethod]
        public async Task ContinueInForeachSkipsValueTwo()
        {
            // Iterates [1, 2, 3]; when value == 2, CONTINUE skips the
            // accumulator add. Expected accumulator: 1 + 3 = 4.
            const string program = @": main
  { 1 2 3 } array_make
  0 swap
  foreach
    swap pop
    dup 2 = if
      pop continue
    then
    +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(4, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueInForSkipsBodyRemainder()
        {
            // 1..5 FOR; CONTINUE fires when the accumulator reaches 3,
            // skipping the increment for the remaining iterations.
            // Iter 1: acc=0, !3 → +1 → 1
            // Iter 2: acc=1, !3 → +1 → 2
            // Iter 3: acc=2, !3 → +1 → 3
            // Iter 4: acc=3, =3 → continue (skip +1)
            // Iter 5: acc=3, =3 → continue
            // Final accumulator: 3. Demonstrates CONTINUE skips body
            // remainder while FOR still runs all 5 iterations.
            const string program = @": main
  0
  1 5 for
    dup 3 = if continue then
    1 +
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(3, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueInBeginRepeatReiterates()
        {
            // Counts up: 0,1,2,3 with CONTINUE skipping when counter < 2.
            // Loop body: increment, dup, if >= 3 push 1 (truthy for the
            // exit flag), else 0; CONTINUE before reaching the flag means
            // we re-enter from BEGIN.
            // Simpler shape: just verify that CONTINUE re-runs the body.
            const string program = @": main
  0
  begin
    1 +
    dup 3 < if continue then
    dup 5 >=
  until
;";
            // Iteration 1: 0+1=1, 1<3 true → continue → re-enter
            // Iteration 2: 1+1=2, 2<3 true → continue → re-enter
            // Iteration 3: 2+1=3, 3<3 false → dup 5>= → 3>=5 false → loop
            // Iteration 4: 3+1=4, 4<3 false → 4>=5 false → loop
            // Iteration 5: 4+1=5, 5<3 false → 5>=5 true → exit
            // Final stack: [5]
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(5, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueInBeginUntilReentersOpener()
        {
            // Demonstrates that CONTINUE inside a BEGIN..UNTIL loop
            // re-enters from immediately after BEGIN, NOT from UNTIL.
            // If CONTINUE jumped to UNTIL, UNTIL would pop the wrong
            // value (the partial accumulator) and the loop would exit
            // unpredictably. With opener-jump, the body re-runs and
            // builds the exit flag freshly each pass.
            const string program = @": main
  0
  begin
    1 +
    dup 2 = if continue then
    dup 4 >=
  until
;";
            // Iter 1: 0+1=1, 1=2 false → dup 4>= → 1>=4 false → loop
            // Iter 2: 1+1=2, 2=2 true → continue → re-enter from after BEGIN
            // Iter 3: 2+1=3, 3=2 false → 3>=4 false → loop
            // Iter 4: 3+1=4, 4=2 false → 4>=4 true → exit
            // Final stack: [4]
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(4, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueInsideIfCleansUpIfMarker()
        {
            // The idiomatic guard pattern: `if condition then continue`.
            // Verifies that CONTINUE inside an IF correctly cleans up
            // the InIfAndContinue marker (otherwise it would accumulate
            // across iterations and the body would eventually fail).
            const string program = @": main
  { 1 2 3 4 5 } array_make
  0 swap
  foreach
    swap pop
    dup 3 = if
      pop continue
    then
    +
  repeat
;";
            // Sum = 1 + 2 + 4 + 5 = 12 (3 skipped via CONTINUE).
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(12, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueInNestedLoopOnlyContinuesInner()
        {
            // Outer FOREACH over [a, b]; inner FOREACH over [10, 20, 30].
            // Inner has CONTINUE on value==20. Outer is unaffected.
            // Inner sum per outer iteration: 10 + 30 = 40.
            // Total: 2 * 40 = 80.
            const string program = @": main
  0
  { 1 2 } array_make
  foreach
    swap pop
    pop
    { 10 20 30 } array_make
    foreach
      swap pop
      dup 20 = if
        pop continue
      then
      +
    repeat
  repeat
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(80, stack.Pop().Value);
        }

        [TestMethod]
        public async Task ContinueOutsideLoopErrors()
        {
            // CONTINUE at the top level (no enclosing loop) must fail.
            const string program = @": main
  continue
;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }
    }
}
