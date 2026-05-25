using System.Threading.Tasks;
using moo.common.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    /// <summary>
    /// End-to-end tests for control-flow execution in <see cref="ForthWord"/>.
    /// Each test targets a specific historical bug; the comment on each test
    /// describes the buggy behavior the test is meant to catch.
    /// </summary>
    [TestClass]
    public class LoopExecutionTests
    {
        [TestMethod]
        public async Task RepeatPreservesOuterControlFlow()
        {
            // Bug: the REPEAT search loop used `continue` instead of `break` when
            // it located the matching BeginMarker, so it kept popping the rest of
            // the control-flow stack. Any enclosing IF marker would be destroyed,
            // and the THEN that followed would fail with STACK_UNDERFLOW.
            const string program = @": main
  1 if
    0
    begin
      dup 3 < while
      1 +
    repeat
  then
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, top.Type);
            Assert.AreEqual(3, top.Value);
        }

        [TestMethod]
        public async Task UntilLoopRunsMultipleIterations()
        {
            // Bug: UNTIL set `x = nextControl.Index` instead of `x - 1`, so the
            // dispatch loop's `x++` would skip the BEGIN datum on rewind and the
            // BeginMarker was never re-pushed. The first iteration after the
            // initial pass would then fail with STACK_UNDERFLOW because the
            // search for BeginMarker came up empty.
            const string program = @": main
  0
  begin
    1 +
    dup 3 >=
  until
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, top.Type);
            Assert.AreEqual(3, top.Value);
        }

        [TestMethod]
        public async Task UntilImmediateTrueRunsOnceAndExits()
        {
            // Sanity baseline: a BEGIN/UNTIL whose body produces a truthy condition
            // on the first pass should exit cleanly with no looping involved.
            const string program = @": main
  begin
    42
    1
  until
;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(42, top.Value);
        }
    }
}
