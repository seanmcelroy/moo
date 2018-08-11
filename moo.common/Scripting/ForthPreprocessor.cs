using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class ForthPreprocessor
{
    private static Random random = new Random(Environment.TickCount);

    public static async Task<ForthPreprocessingResult> Preprocess(PlayerConnection connection, string program)
    {
        var sb = new StringBuilder();
        var defines = new Dictionary<string, string>() {
            { "PR_MODE", "0" },
            { "FG_MODE", "1" },
            { "BG_MODE", "2" },
            { "PREEMPT", "pr_mode setmode" },
            { "FOREGROUND", "fg_mode setmode" },
            { "BACKGROUND", "bg_mode setmode" },
            { "EVENT_WAIT", "0 array_make event_waitfor" },
            { "NOTIFY_EXCEPT", "1 swap notify_exclude" },
            { "INSTRING", "tolower swap tolower swap instr" },
            { "RINSTRING", "tolower swap tolower swap rinstr" },
            { "}LIST", "} array_make" },
            { "}DICT", "} 2 / array_make_dict" },
            { "}TELL", "} array_make me @ 1 array_make array_notify" },
            { "}JOIN", "} array_make \"\" array_join" },
            { "}CAT", "} array_make array_interpret" },
            { "SORTTYPE_CASEINSENS", "1" },
            { "SORTTYPE_DESCENDING", "2" },
            { "SORTTYPE_SHUFFLE", "4" },
            { "SORTTYPE_NOCASE_ASCEND", "1" },
            { "SORTTYPE_CASE_DESCEND", "2" },
            { "SORTTYPE_NOCASE_DESCEND", "3" },
            { "ARRAY_INTERSECT", "2 array_nintersect" }
        };
        var controlFlow = new Stack<ControlFlowMarker>();
        var verbosity = 0;

        var x = -1;
        foreach (var line in program.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
        {
            x++;

            if (line.TrimStart().StartsWith("$"))
            {
                // $ifdef
                {
                    var ifdefMatch = Regex.Match(line, @"^\s*\$if(?<negate>n)?def\s+(?<defName>[^\s\r\n\=]{1,20})(?:\s*$|\s*\=\s*(?<defValue>[^\r\n]+)$)");
                    if (ifdefMatch.Success)
                    {
                        // I could be an 'if' inside a skipped branch.
                        if (controlFlow.Count > 0)
                        {
                            var controlCurrent = controlFlow.Peek();
                            if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                             || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                             || controlCurrent.Element == ControlFlowElement.SkippedBranch
                             || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                            {
                                if (verbosity >= 2 && verbosity <= 3)
                                    await connection.sendOutput($"SKIPPED LINE: {line}");
                                controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkippedBranch, x));
                                continue;
                            }
                        }

                        var isTrue = ifdefMatch.Groups["defValue"].Success
                        ? (defines.ContainsKey(ifdefMatch.Groups["defName"].Value) && defines[ifdefMatch.Groups["defName"].Value].Equals(ifdefMatch.Groups["defValue"].Value))
                        : (defines.ContainsKey(ifdefMatch.Groups["defName"].Value));

                        var negate = ifdefMatch.Groups["negate"].Success;

                        if (isTrue ^ negate)
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndContinue, x));
                        else
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndSkip, x));
                        continue;
                    }
                }

                // $else
                if (string.Compare("$else", line.Trim(), true) == 0)
                {
                    // I could be an 'else' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            if (verbosity >= 2 && verbosity <= 3)
                                await connection.sendOutput($"SKIPPED LINE: {line}");
                            continue;
                        }
                    }

                    if (controlFlow.Count == 0)
                        return new ForthPreprocessingResult("$else encountered without preceding $ifdef/$ifndef");

                    var currentControl = controlFlow.Pop();
                    if (currentControl.Element == ControlFlowElement.InIfAndContinue)
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndSkip, x));
                    else
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndContinue, x));

                    continue;
                }

                // $endif
                if (string.Compare("$endif", line.Trim(), true) == 0)
                {
                    // I could be an 'else' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            if (verbosity >= 2 && verbosity <= 3)
                                await connection.sendOutput($"SKIPPED LINE: {line}");
                            // A skipped if will push a SkippedBranch, so we should pop it.
                            controlFlow.Pop();
                            continue;
                        }
                    }

                    if (controlFlow.Count == 0)
                        return new ForthPreprocessingResult("$endif encountered without preceding $ifdef/$ifndef");
                    controlFlow.Pop();
                    continue;
                }

                if (controlFlow.Count > 0)
                {
                    var controlCurrent = controlFlow.Peek();
                    if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                     || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                     || controlCurrent.Element == ControlFlowElement.SkippedBranch
                     || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                    {
                        // Debug, print stack
                        if (verbosity >= 2 && verbosity <= 3)
                            await connection.sendOutput($"SKIPPED LINE: {line}");
                        continue;
                    }
                }

                // $def
                {
                    var defMatch = Regex.Match(line, @"^\s*\$def\s+(?<defName>[^\s\r\n]{1,20})(?:\s*$|\s+(?<defValue>[^\r\n]+)$)");
                    if (defMatch.Success)
                    {
                        if (!defMatch.Groups["defValue"].Success)
                        {
                            defines.Add(defMatch.Groups["defName"].Value, null);
                        }
                        else
                            defines.Add(defMatch.Groups["defName"].Value, defMatch.Groups["defValue"].Value);

                        continue;
                    }
                }

                // $define
                {
                    var defineMatch = Regex.Match(line, @"^\s*\$define\s+(?<defName>[^\s\r\n]{1,20})(?:\s*\$enddef$|\s+(?<defValue>[^\r\n]+)\s+\$enddef$)");
                    if (defineMatch.Success)
                    {
                        if (!defineMatch.Groups["defValue"].Success)
                        {
                            defines.Add(defineMatch.Groups["defName"].Value, null);
                        }
                        else
                            defines.Add(defineMatch.Groups["defName"].Value, defineMatch.Groups["defValue"].Value);

                        continue;
                    }
                }

                // $echo
                {
                    var echoMatch = Regex.Match(line, @"^(?:\$echo\s+\""(?<value>[^\""]*)\"")");
                    if (echoMatch.Success)
                    {
                        await connection.sendOutput(echoMatch.Groups["value"].Value);
                        continue;
                    }
                }

                // $undef
                {
                    var undefMatch = Regex.Match(line, @"^\s*\$undef\s+(?<defName>[^\s\r\n]{1,20})\s*$");
                    if (undefMatch.Success)
                    {
                        if (defines.ContainsKey(undefMatch.Groups["defName"].Value))
                            defines.Remove(undefMatch.Groups["defName"].Value);
                    }
                }

                await connection.sendOutput($"UNHANDLED PREPROCESSOR LINE: {line}");
            }
            else
            {
                // We want to ensure any replaces do NOT happen in quoted strings
                var line2 = line;

                var holdingPen = new Dictionary<string, string>();
                foreach (System.Text.RegularExpressions.Match match in Regex.Matches(line, @"\""[^\r\n]*?(?<!\\)\""(?=[\s\r\n])"))
                {
                    var key = RandomString(match.Length);
                    holdingPen.Add(key, match.Value);
                    line2 = line2.Remove(match.Index, match.Length).Insert(match.Index, key);
                }

                // Strip comments
                if (line2.IndexOf('(') > -1)
                    line2 = Regex.Replace(line2, @"\([^\)]*\)", "", RegexOptions.Compiled);

                foreach (var define in defines.Where(d => d.Value != null))
                    line2 = Regex.Replace(line2, @"(?<=[\s\r\n])" + Regex.Escape(define.Key) + @"(?=[\s\r\n])", define.Value, RegexOptions.IgnoreCase);

                foreach (var hold in holdingPen)
                    line2 = line2.Replace(hold.Key, hold.Value);

                if (verbosity > 0 && verbosity <= 3 && line.CompareTo(line2) != 0)
                    await connection.sendOutput($"XFORM \"{line}\" into \"{line2}\"");
                else if (verbosity == 4)
                    await connection.sendOutput($"PRE: {line2}");

                sb.AppendLine(line2);
            }
        }

        return new ForthPreprocessingResult(sb.ToString(), null);
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*_+=-";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}