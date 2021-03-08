using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Models;

namespace moo.common.Scripting
{
    public class Editor
    {
        private readonly PlayerConnection connection;
        private readonly Script? script;
        private bool inputMode;

        private readonly List<string> buffer = new();

        private readonly List<string> inputModeBuffer = new();

        private int position;

        public string? ProgramText => buffer == null || buffer.Count == 0 ? null : buffer.Aggregate((c, n) => $"{c}\n{n}");

        public Editor(PlayerConnection connection, Script? script)
        {
            this.connection = connection;
            this.script = script;
        }

        public async Task<EditorResult> HandleInputAsync(string line, CancellationToken cancellationToken)
        {
            if (line == null || line.Length == 0)
                return EditorResult.NORMAL_CONTINUE;

            if (inputMode && string.Compare(".", line) != 0)
            {
                inputModeBuffer.Add(line);
                return EditorResult.NORMAL_CONTINUE;
            }
            else
            {
                var split = line.Split(' ');
                var commandChar = line.Length == 1 || (split.Length > 1 && split.Last().Length == 1) ? split.Last()[0] : (char)0;

                switch (commandChar)
                {
                    case 'c':
                        {
                            // Compile
                            var compileResult = await Script.CompileAsync(script, ProgramText, connection, cancellationToken);
                            if (!compileResult.Item1)
                                return new EditorResult(EditorErrorResult.COMPILE_ERROR, compileResult.Item2);

                            return EditorResult.NORMAL_CONTINUE;
                        }
                    case 'd':
                        {
                            // Delete mode
                            if (split.Length != 3)
                                return new EditorResult(EditorErrorResult.SYNTAX_ERROR, "Not enough arguments for 'd'");

                            var start = split.Reverse().Skip(2).FirstOrDefault();
                            if (!int.TryParse(start, out int s))
                                return new EditorResult(EditorErrorResult.SYNTAX_ERROR, "Arguments for 'd' must be integers");

                            var end = split.Reverse().Skip(1).FirstOrDefault();
                            if (!int.TryParse(end, out int e))
                                return new EditorResult(EditorErrorResult.SYNTAX_ERROR, "Arguments for 'd' must be integers");

                            var length = e - s + 1;

                            s = System.Math.Max(s, 0);
                            s = System.Math.Min(s, inputModeBuffer.Count);
                            length = System.Math.Max(length, 0);
                            length = System.Math.Min(length, inputModeBuffer.Count - s);

                            if (length > 0)
                                inputModeBuffer.RemoveRange(s - 1, length);

                            return EditorResult.NORMAL_CONTINUE;
                        }
                    case 'i':
                        {
                            // Insert mode
                            if (split.Length == 2)
                            {
                                var number = split.Reverse().Skip(1).FirstOrDefault();
                                if (!int.TryParse(number, out int num))
                                    return new EditorResult(EditorErrorResult.SYNTAX_ERROR, "Argument for 'i' must be an integer");

                                position = num == 0 ? 0 : num - 1;
                            }
                            else if (split.Length == 1)
                                position = 0;
                            else
                                return new EditorResult(EditorErrorResult.SYNTAX_ERROR, "Not enough arguments for 'i'");

                            inputModeBuffer.Clear();
                            inputMode = true;
                            return EditorResult.NORMAL_CONTINUE;
                        }
                    case 'q':
                        return EditorResult.NORMAL_EXIT;
                    case '.':
                        {
                            // Terminate input mode
                            if (inputMode)
                            {
                                inputMode = false;
                                buffer.InsertRange(position, inputModeBuffer);
                                inputModeBuffer.Clear();
                                return EditorResult.NORMAL_CONTINUE;
                            }
                            break;
                        }
                    default:
                        buffer.Add(line);
                        return EditorResult.NORMAL_CONTINUE;
                }

            }

            return new EditorResult(EditorErrorResult.SYNTAX_ERROR, $"Cannot parse line: {line}");
        }

    }
}