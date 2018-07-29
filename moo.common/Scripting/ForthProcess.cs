using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public class ForthProcess
{
    private readonly List<ForthWord> words;
    private readonly Stack<ForthDatum> stack = new Stack<ForthDatum>();
    // Variables that came from a caller program
    private readonly Dictionary<string, object> outerScopeVariables = new Dictionary<string, object>();
    // Variables local to my process context, which I could pass on to programs I call and all my words can see (LVAR)
    private readonly ConcurrentDictionary<string, object> programLocalVariables = new ConcurrentDictionary<string, object>();
    private readonly Dbref scriptId;
    private readonly Player me;
    private readonly String scopeId;
    private readonly String outerScopeId;
    private bool hasRan;

    public ForthProcess(
        Dbref scriptId,
        List<ForthWord> words,
        Player me,
        string outerScopeId = null,
        Dictionary<string, object> outerScopeVariables = null)
    {
        this.scriptId = scriptId;
        this.words = words;
        this.me = me;
        this.scopeId = Guid.NewGuid().ToString();

        if (outerScopeId != null)
        {
            this.outerScopeId = outerScopeId;

            if (outerScopeVariables != null)
            {
                foreach (var kvp in outerScopeVariables)
                    this.outerScopeVariables.Add(kvp.Key, kvp.Value);
            }
        }
    }

    public ConcurrentDictionary<string, object> GetProgramLocalVariables() {
        return this.programLocalVariables;
    }

    public void SetProgramLocalVariable(string name, object value)
    {
        if (!programLocalVariables.TryAdd(name, value))
        {
            programLocalVariables[name] = value;
        }
    }

    public async Task<ForthProgramResult> RunAsync(Dbref trigger, string command, object[] args, CancellationToken cancellationToken)
    {
        if (hasRan)
        {
            return new ForthProgramResult(ForthProgramErrorResult.INTERNAL_ERROR, $"Execution scope {scopeId} tried to run twice.");
        }
        hasRan = true;

        // Execute the last word.
        return await words.Last().RunAsync(this, stack, me, trigger, command, cancellationToken);
    }
}