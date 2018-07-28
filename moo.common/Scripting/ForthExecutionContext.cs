using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public class ForthExecutionContext
{
    private readonly List<List<ForthDatum>> programLines;
    private readonly Stack<ForthDatum> stack = new Stack<ForthDatum>();
    private readonly Player me;

    public ForthExecutionContext(List<List<ForthDatum>> programLines, Player me)
    {
        this.programLines = programLines;
        this.me = me;
    }

    public async Task<ForthProgramResult> RunAsync(object[] args, CancellationToken cancellationToken)
    {
        // For each line
        int lineCount = 0;
        foreach (var line in programLines)
        {
            lineCount++;

            if (cancellationToken.IsCancellationRequested)
                return new ForthProgramResult(ForthProgramErrorResult.INTERRUPTED);

            // For each element in line
            foreach (var datum in line)
            {
                // Debug, print stack
                if (stack.Count == 0)
                    await me.sendOutput($"DEBUG ({lineCount}): ()");
                else
                    await me.sendOutput($"DEBUG ({lineCount}): (" +
                    stack.Reverse().Select(s =>
                    {
                        return s.Value.ToString();
                    }).Aggregate((c, n) => c + " " + n) + ") " + datum.Value);

                // Literals
                if (datum.Type == ForthDatum.DatumType.Integer ||
                    datum.Type == ForthDatum.DatumType.String)
                {
                    stack.Push(datum);
                    continue;
                }

                // Primatives
                if (datum.Type == ForthDatum.DatumType.Primitive)
                {

                    switch (((string)datum.Value).ToLowerInvariant())
                    {
                        case "pop":
                            {
                                // POP ( x -- ) 
                                // Pops the top of the stack into oblivion.
                                if (stack.Count == 0)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "POP requires at least one item on the stack, but the stack is empty.");

                                stack.Pop();
                                break;
                            }
                        case "popn":
                            {
                                // POPN ( ?n..?1 i -- ) 
                                // Pops the top i stack items.
                                if (stack.Count == 0)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "POPN requires at least one parameter on the stack");

                                var si = stack.Pop();
                                if (si.Type != DatumType.Integer)
                                    return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "POPN requires the top parameter on the stack to be an integer");

                                int i = (int)si.Value;
                                if (stack.Count < i)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"POPN would have removed {i} items on the stack, but only {stack.Count} were present.");

                                for (int n = 0; n < i; n++)
                                    stack.Pop();

                                break;
                            }
                        case "dup":
                            {
                                // DUP ( x -- x x ) 
                                // Duplicates the item at the top of the stack.
                                if (stack.Count == 0)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DUP requires a parameter on the stack");

                                var x = stack.Peek();
                                stack.Push(x);

                                break;
                            }
                        case "dupn":
                            {
                                // DUPN ( ?n..?1 i -- ?n..?1 ?n..?1 ) 
                                // Duplicates the top i stack items.
                                if (stack.Count == 0)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DUPN requires at least one parameter on the stack");

                                var si = stack.Pop();
                                if (si.Type != DatumType.Integer)
                                    return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DUPN requires the top parameter on the stack to be an integer");

                                int i = (int)si.Value;
                                if (stack.Count < i)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"DUPN would have duplicated {i} items on the stack, but only {stack.Count} were present.");

                                foreach (var source in stack.Reverse().Take(i))
                                    stack.Push(source);

                                break;
                            }
                        case "ldup":
                            {
                                // LDUP ( {?} -- {?} {?} ) 
                                // Duplicates a stackrange on top of the stack.
                                foreach (var source in stack.ToArray())
                                {
                                    stack.Push(source);
                                }
                                break;
                            }
                        case "swap":
                            {
                                // SWAP ( x y -- y x ) 
                                // Takes objects x and y on the stack and reverses their order.
                                if (stack.Count < 2)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "SWAP requires at least two parameters on the stack");

                                var y = stack.Pop();
                                var x = stack.Pop();
                                stack.Push(y);
                                stack.Push(x);

                                break;
                            }
                        case "over":
                            {
                                // OVER ( x y -- x y x ) 
                                // Duplicates the second-to-top thing on the stack. This is the same as 2 pick.
                                if (stack.Count < 2)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "OVER requires at least two parameters on the stack");

                                var x = stack.Reverse().Skip(1).Take(1).Single();
                                stack.Push(x);

                                break;
                            }
                        case "rot":
                            {
                                // ROT ( x y z -- y z x ) 
                                // Rotates the top three things on the stack. This is equivalent to 3 rotate.
                                if (stack.Count < 3)
                                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "ROT requires at least three parameters on the stack");

                                var z = stack.Pop();
                                var y = stack.Pop();
                                var x = stack.Pop();
                                stack.Push(y);
                                stack.Push(z);
                                stack.Push(x);

                                break;
                            }
                        case "rotate":
                            {
                                var result = Rotate.Execute(stack);
                                if (default(ForthProgramResult).Equals(result))
                                    continue;
                                if (!result.isSuccessful)
                                    return result;
                                break;
                            }
                        case "pick":
                            {
                                var result = Pick.Execute(stack);
                                if (default(ForthProgramResult).Equals(result))
                                    continue;
                                if (!result.isSuccessful)
                                    return result;
                                break;
                            }
                        case "put":
                            {
                                var result = Put.Execute(stack);
                                if (default(ForthProgramResult).Equals(result))
                                    continue;
                                if (!result.isSuccessful)
                                    return result;
                                break;
                            }
                    }
                }
            }
        }

        // Debug, print stack
        if (stack.Count == 0)
            await me.sendOutput($"DEBUG ({lineCount}): ()");
        else
            await me.sendOutput($"DEBUG ({lineCount}): (" +
            stack.Reverse().Select(s =>
            {
                return s.Value.ToString();
            }).Aggregate((c, n) => c + " " + n) + ")");

        return new ForthProgramResult(null, "Program complete");
    }
}