using System.Threading.Tasks;
using moo.common.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    /// <summary>
    /// End-to-end tests for variable declaration, write, and read across the
    /// full parser + runtime + Bang/At pipeline. Each test targets at least
    /// one bug from the audit; comments describe the historical buggy behavior.
    /// </summary>
    [TestClass]
    public class VariableExecutionTests
    {
        [TestMethod]
        public async Task VarDeclarationCanBeWrittenAndRead()
        {
            // Targets the interaction of #4 (case mismatch on declaration),
            // #13 (constness check) and the UNINITIALIZED.IsConstant=true bug.
            // Pre-fix: `var foo` registered "foo" as UNINITIALIZED (IsConstant
            // = true), and the dirtyVariables writeback rejected the assignment
            // with VARIABLE_IS_CONSTANT.
            const string program = ": main var foo 5 foo ! foo @ ;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, top.Type);
            Assert.AreEqual(5, top.Value);
        }

        [TestMethod]
        public async Task VarBangDeclaresAndReadsInitialValue()
        {
            // `var!` should declare AND initialize. Pre-fix var! did not lowercase
            // the name; this test uses lowercase to isolate the basic semantics.
            const string program = ": main 5 var! foo foo @ ;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(5, stack.Pop().Value);
        }

        [TestMethod]
        public async Task VarBangNameIsCaseInsensitive()
        {
            // Targets §3.A. Pre-fix: var! registered "Foo" (original case),
            // but lookup lowercased to "foo" → VARIABLE_NOT_FOUND. Now both
            // sides lowercase consistently.
            const string program = ": main 5 var! Foo foo @ ;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(5, stack.Pop().Value);
        }

        [TestMethod]
        public async Task VarNameIsCaseInsensitive()
        {
            // Sibling check for var (which the user fixed in the prior pass).
            const string program = ": main var Foo 5 Foo ! foo @ ;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(5, stack.Pop().Value);
        }

        [TestMethod]
        public async Task AtResolvesExistingVariableRatherThanThrowing()
        {
            // Targets the §2 regression: when the `!` was dropped from
            // `!variables.ContainsKey(...)`, the resolver returned `default`
            // for *existing* vars (so reads silently failed) and threw
            // KeyNotFoundException for missing vars. Post-fix, an existing
            // variable resolves to its value.
            const string program = ": main 42 var! answer answer @ ;";
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(42, stack.Pop().Value);
        }
    }
}
