using System.Collections.Generic;
using System.Text;

namespace moo.common.Models
{
    internal struct LockExpressionValue
    {
        internal bool negated;
        internal List<LockExpressionPart>? inners;
        internal string? terminal;

        public static LockExpressionValue Parse(string text) => ParseInternal(text).result;

        private static (LockExpressionValue result, int iAtExit) ParseInternal(string text)
        {
            var currentNegate = false;
            var currentPreceeding = LockExpressionBoolean.None;
            var sb = new StringBuilder();
            List<LockExpressionPart> inners = new();

            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '!':
                        currentNegate = true;
                        continue;
                    case '(':
                        var (result, iAtExit) = ParseInternal(text[(i + 1)..]);
                        inners.Add(new LockExpressionPart
                        {
                            precedingBoolean = currentPreceeding,
                            value = result
                        });
                        i += iAtExit + 2;
                        continue;
                    case '|':
                    case '&':
                        inners.Add(new LockExpressionPart
                        {
                            precedingBoolean = currentPreceeding,
                            value = new LockExpressionValue
                            {
                                negated = currentNegate,
                                terminal = sb.ToString()
                            }
                        });
                        sb.Clear();
                        currentNegate = false;
                        currentPreceeding = text[i] == '|' ? LockExpressionBoolean.Or : LockExpressionBoolean.And;
                        continue;
                    case ')':
                        inners.Add(new LockExpressionPart
                        {
                            precedingBoolean = currentPreceeding,
                            value = new LockExpressionValue
                            {
                                negated = currentNegate,
                                terminal = sb.ToString()
                            }
                        });
                        sb.Clear();
                        currentNegate = false;
                        currentPreceeding = LockExpressionBoolean.Or;
                        continue;
                    default:
                        sb.Append(text[i]);
                        continue;
                }
            }

            if (sb.Length > 0)
                inners.Add(new LockExpressionPart
                {
                    precedingBoolean = currentPreceeding,
                    value = new LockExpressionValue
                    {
                        negated = currentNegate,
                        terminal = sb.ToString()
                    }
                });

            return (new LockExpressionValue
            {
                negated = currentNegate,
                inners = inners.Count > 0 ? inners : null,
                terminal = inners.Count == 0 ? sb.ToString() : null
            }, text.Length - 1);
        }
    }
}