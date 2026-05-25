using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    [TestClass]
    public class ForthWordTests
    {
        [TestMethod]
        public async Task TrailingVarIsRejectedNotCrashing()
        {
            // Pre-fix: programData[x+1] threw ArgumentOutOfRangeException.
            const string program = ": main var ;"; // var with no name
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task TrailingVarBangIsRejectedNotCrashing()
        {
            // Sibling of TrailingVarIsRejectedNotCrashing — same bounds check
            // had to be added to var! per #24.
            const string program = ": main 1 var! ;"; // var! with a value but no name
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task VarBangWithEmptyStackIsRejected()
        {
            // var! must pop a value off the stack; if the stack is empty it
            // should return STACK_UNDERFLOW rather than throwing on Pop.
            const string program = ": main var! foo ;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }
    }
}