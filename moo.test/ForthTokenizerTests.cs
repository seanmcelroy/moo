using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using moo.common.Models;
using moo.common.Scripting;

namespace moo.Test
{
    [TestClass]
    public class ForthTokenizerTests
    {
        [TestMethod]
        public async Task KeepVars()
        {
            const string programText = ": parse_args[ dict:opts str:args int:ignore -- arr:opts 0 | 1 ]\n    1 var! firstopt\n    begin\n        args @ striplead \" \" split args !\n        dup while\n        \n        dup \"{#|#by}\" smatch if pop show_usage 1 exit then\n        \n        firstopt @ if\n            0 firstopt !\n            ignore @ if\n                dup \"#help\"     swap stringpfx if pop continue then\n                dup \"#default\"  swap 3 stringminpfx if pop continue then\n                dup \"#reset\"    swap stringpfx if pop continue then\n$iflib $adultlock\n                dup \"#adult\"    swap stringpfx if pop continue then\n                dup \"#prude\"    swap stringpfx if pop continue then\n$endif\n                dup \"#never\"    swap stringpfx over strlen 2 > and if pop continue then\n                dup \"#always\"   swap stringpfx if pop continue then\n                dup \"#whereis\"  swap stringpfx if pop continue then\n                dup \"#set\"      swap stringpfx if pop continue then\n                dup \"#setdir\"   swap stringpfx if pop continue then\n            else\n                dup \"#help\"     swap stringpfx if pop show_usage_long 1 exit then\n                dup \"#default\"  swap 3 stringminpfx if pop args @ set_default 1 exit then\n                dup \"#reset\"    swap stringpfx if pop \"\" set_default 1 exit then\n$iflib $adultlock\n                compare-my-age if\n                    dup \"#adult\"    swap stringpfx if pop 1 set_adult 1 exit then\n                    dup \"#prude\"    swap stringpfx if pop 0 set_adult 1 exit then\n                then\n$endif\n                dup \"#never\"    swap stringpfx over strlen 2 > and if pop set_never 1 exit then\n                dup \"#always\"   swap stringpfx if pop set_always 1 exit then\n                dup \"#whereis\"  swap stringpfx if pop set_whereis 1 exit then\n                dup \"#set\"      swap stringpfx if pop args @ set_wa_prop 1 exit then\n                dup \"#setdir\"   swap stringpfx if pop args @ set_wadir_prop 1 exit then\n            then\n        then\n        \n        dup \"#names\"      swap stringpfx if pop \"names\"   opts @ \"optcol\" ->[] opts ! continue then\n        dup \"#comments\"   swap stringpfx if pop \"#notes\" then\n        dup \"#notes\"      swap stringpfx if pop \"notes\"   opts @ \"optcol\" ->[] opts ! continue then\n        dup \"#wfnames\"    swap stringpfx if pop \"wfnames\" opts @ \"optcol\" ->[] opts ! continue then\n        dup \"#directions\" swap stringpfx if pop \"dir\"     opts @ \"optcol\" ->[] opts ! continue then\n        \n        dup \"#bycount\"    swap stringpfx if pop \"cnt\" opts @ \"sortcol\" ->[] opts ! continue then\n        dup \"#byactive\"   swap stringpfx if pop \"act\" opts @ \"sortcol\" ->[] opts ! continue then\n        dup \"#bywfl\"      swap stringpfx if pop \"#bywatchfor\" then\n        dup \"#bywatchfor\" swap stringpfx if pop \"wfl\" opts @ \"sortcol\" ->[] opts ! 1 opts @ \"mincnt\" ->[] opts ! continue then\n        dup \"#byroom\"     swap stringpfx if pop \"locname\" opts @ \"sortcol\" ->[] 1 swap \"ascend\" ->[] opts ! continue then\n \n        dup \"#old\"        swap stringpfx if pop \"old\" opts @ \"style\" ->[] \"2ln\" swap \"format\" ->[] opts ! continue then\n        dup \"#new\"        swap stringpfx if pop \"new\" opts @ \"style\" ->[] \"col\" swap \"format\" ->[] opts ! continue then\n \n        dup \"#columnar\"   swap stringpfx if pop \"col\" opts @ \"format\" ->[] opts ! continue then\n        dup \"#twoline\"    swap stringpfx if pop \"2ln\" opts @ \"format\" ->[] opts ! continue then\n        dup \"#oneline\"    swap stringpfx if pop \"1ln\" opts @ \"format\" ->[] opts ! continue then\n \n        dup \"#quell\"      swap stringpfx if pop \"yes\" opts @ \"quell\"  ->[] opts ! continue then\n        dup \"#all\"        swap stringpfx if pop 1 opts @ \"showall\"  ->[] opts ! continue then\n        dup \"#reversed\"   swap stringpfx if pop opts @ \"ascend\" [] not opts @ \"ascend\" ->[] opts ! continue then\n \n        dup number? if atoi opts @ \"mincnt\" ->[] opts ! continue then\n        ignore @ not if\n            pop show_usage 1 exit\n        then\n    repeat pop\n    opts @ 0\n;";

            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            Assert.IsNotNull(preproc.ProcessedProgram);

            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.IsTrue(result.IsSuccessful);

            Assert.HasCount(1, result.Words);
            Assert.HasCount(3, result.Words[0].inputs);
        }

        [TestMethod]
        public async Task ParseQuotedStringEmpty()
        {
            var programText = $": w\n\"\";";
            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.HasCount(1, result.Words);
            var word = result.Words[0];
            Assert.HasCount(1, word.programData);
            Assert.AreEqual(ForthDatum.DatumType.String, word.programData[0].Type);
            Assert.AreEqual("", word.programData[0].Value);
        }

        [TestMethod]
        public async Task ParseQuotedStringSpace()
        {
            var programText = $": w\n\" \";";
            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.HasCount(1, result.Words);
            var word = result.Words[0];
            Assert.HasCount(1, word.programData);
            Assert.AreEqual(ForthDatum.DatumType.String, word.programData[0].Type);
            Assert.AreEqual(" ", word.programData[0].Value);
        }

        [TestMethod]
        public async Task ParseQuotedStringQuotedQuote()
        {
            // Source: : w \n "\""
            // Before #14/#15 the tokenizer kept the literal backslash and
            // emitted the two-character string `\"`. The escape-handling fix
            // correctly unescapes `\"` to a single `"` character, matching what
            // MUF source authors actually mean when they write `"\""`.
            var programText = $": w\n\"\\\"\";";
            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.HasCount(1, result.Words);
            var word = result.Words[0];
            Assert.HasCount(1, word.programData);
            Assert.AreEqual(ForthDatum.DatumType.String, word.programData[0].Type);
            Assert.AreEqual("\"", word.programData[0].Value);
        }

        [TestMethod]
        public async Task ParseQuotedStringQuotedQuoteMultiple()
        {
            // Source: : w \n "\""  "\" "
            // After the #14/#15 unescape pass: first string is `"` (one char),
            // second is `" ` (quote + space). See ParseQuotedStringQuotedQuote
            // for the rationale.
            var programText = $": w\n\"\\\"\" \"\\\" \";";
            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.HasCount(1, result.Words);
            var word = result.Words[0];
            Assert.HasCount(2, word.programData);
            Assert.AreEqual(ForthDatum.DatumType.String, word.programData[0].Type);
            Assert.AreEqual("\"", word.programData[0].Value);
            Assert.AreEqual(ForthDatum.DatumType.String, word.programData[1].Type);
            Assert.AreEqual("\" ", word.programData[1].Value);
        }

        [TestMethod]
        public async Task ParseQuotedStrings()
        {
            var programText = $": get-playerdbrefs  (playersstr -- dbref_range unrecstr)\n    0 \" \" \"\" 4 rotate\n    begin\n        dup while\n        \" \" split swap\n        dup not if pop continue then\n        dup \"(\" 1 strncmp not if\n            \" \" strcat swap strcat\n            \")\" split swap pop strip\n            continue\n        then\n        dup \"#\" 1 strncmp not if\n            dup 1 strcut swap pop\n            dup number? if\n                atoi dbref dup ok? if\n                    dup player? if\n                        swap pop 5 rotate 1 +\n                        -5 rotate -5 rotate\n                        continue\n                    else pop\n                    then\n                else pop\n                then\n            else pop\n            then\n        then\n        dup \"*\" 1 strncmp not if\n            1 strcut swap pop\n            4 pick \" \" 3 pick over tolower strcat strcat instr if\n                pop continue\n            then\n            4 rotate over tolower strcat \" \" strcat -4 rotate\n            dup me @ get-alias dup not if\n                pop \"\\\" *\" swap strcat\n                \"\\\" \" strcat rot\n                swap strcat swap continue\n            then\n            swap pop \" \" strcat\n            swap strcat single-space\n            continue\n        then\n        dup player-match? dup -1 = if\n            pop pop pop pop\n            strip exit\n        then\n        0 > if\n            swap pop 5 rotate\n            1 + -5 rotate -5 rotate\n        else\n            dup me @ get-alias dup if\n                tolower\n                5 pick \" \" 4 pick over strcat strcat instr if\n                    pop pop continue\n                then\n                5 rotate rot strcat \" \" strcat -4 rotate\n                \" \" strcat swap strcat single-space\n            else pop\n  \n                dup partial-match\n  \n                if\n                    swap pop 5 rotate 1 +\n                    -5 rotate -5 rotate\n                else\n                    \"\\\"\" swap strcat\n                    \"\\\" \" strcat rot\n                    swap strcat swap\n                then\n            then\n        then\n    repeat pop swap pop sort-stringwords\n;";
            var preproc = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(preproc.IsSuccessful);
            Assert.IsNotNull(preproc.ProcessedProgram);

            var result = await ForthTokenizer.Tokenzie(null, preproc.ProcessedProgram!, [], null);
            Assert.HasCount(1, result.Words);
            var word = result.Words[0];
            Assert.IsNotNull(word.programData);
        }

        [TestMethod]
        public async Task EscapedBackslashAtEndOfStringIsHandled()
        {
            var programText = ": w \"a\\\\\" ;"; // "a\\" — string "a\"
            var pre = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            var tok = await ForthTokenizer.Tokenzie(null, pre.ProcessedProgram!, [], null);
            Assert.IsTrue(tok.IsSuccessful, tok.Reason);
            Assert.HasCount(1, tok.Words);
            Assert.AreEqual("a\\", tok.Words[0].programData[0].Value);
        }

        [TestMethod]
        public async Task MultilineStringPreservesLineNumber()
        {
            var programText = ": w \"first\nsecond\" 1 ;\n";
            var pre = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            var tok = await ForthTokenizer.Tokenzie(null, pre.ProcessedProgram!, [], null);
            Assert.IsTrue(tok.IsSuccessful, tok.Reason);
            // The "1" datum should report line 2 (after the embedded newline).
            var oneDatum = tok.Words[0].programData[1];
            Assert.AreEqual(2, oneDatum.FileLineNumber);
        }

        [TestMethod]
        public async Task PrototypeWithOutputsParsesBothSides()
        {
            var programText = ": w[ a:int b:int -- c:int ] a @ b @ + c ! ;";
            var pre = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            var tok = await ForthTokenizer.Tokenzie(null, pre.ProcessedProgram!, [], null);
            Assert.IsTrue(tok.IsSuccessful, tok.Reason);
            Assert.HasCount(2, tok.Words[0].inputs);
            // and outputs.Count == 1 once the new field is added
        }

        [TestMethod]
        public async Task StrayQuoteOutsideWordDoesNotThrow()
        {
            var programText = "\"not in a word\"\n: w ;";
            var pre = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            var tok = await ForthTokenizer.Tokenzie(null, pre.ProcessedProgram!, [], null);
            Assert.IsTrue(tok.IsSuccessful, tok.Reason);
        }
    }
}
