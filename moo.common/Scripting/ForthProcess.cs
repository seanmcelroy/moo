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
    private readonly IEnumerable<ForthWord> words;
    private readonly Stack<ForthDatum> stack = new Stack<ForthDatum>();
    // Variables that came from a caller program
    private readonly Dictionary<string, ForthVariable> outerScopeVariables = new Dictionary<string, ForthVariable>();
    // Variables local to my process context, which I could pass on to programs I call and all my words can see ($DEF, LVAR)
    private readonly ConcurrentDictionary<string, ForthVariable> programLocalVariables = new ConcurrentDictionary<string, ForthVariable>();

    private readonly Server server;
    private readonly Dbref scriptId;
    private readonly PlayerConnection connection;
    private readonly String scopeId;
    private readonly String outerScopeId;
    private bool hasRan;

    public Server Server => server;

    public ForthProcess(
        Server server,
        Dbref scriptId,
        IEnumerable<ForthWord> words,
        PlayerConnection connection,
        string outerScopeId = null,
        Dictionary<string, ForthVariable> outerScopeVariables = null)
    {
        this.server = server;
        this.scriptId = scriptId;
        this.words = words;
        this.connection = connection;
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

    public ConcurrentDictionary<string, ForthVariable> GetProgramLocalVariables()
    {
        return this.programLocalVariables;
    }

    public void SetProgramLocalVariable(string name, ForthVariable value)
    {
        if (!programLocalVariables.TryAdd(name, value))
        {
            programLocalVariables[name] = value;
        }
    }

    public bool HasWord(string wordName) => this.words.Any(w => string.Compare(w.name, wordName, true) == 0);

    public async Task<ForthProgramResult> RunWordAsync(string wordName, Dbref trigger, string command, Dbref? lastListItem, CancellationToken cancellationToken)
    {
        return await this.words.Single(w => string.Compare(w.name, wordName, true) == 0).RunAsync(this, stack, connection, trigger, command, lastListItem, cancellationToken);
    }

    public async Task NotifyAsync(Dbref target, string message)
    {
        await server.NotifyAsync(target, message);
    }

    public async Task NotifyRoomAsync(Dbref target, string message, List<Dbref> exclude = null)
    {
        await server.NotifyRoomAsync(target, message, exclude);
    }

    public async Task<ForthProgramResult> RunAsync(Dbref trigger, string command, object[] args, CancellationToken cancellationToken)
    {
        if (hasRan)
        {
            return new ForthProgramResult(ForthProgramErrorResult.INTERNAL_ERROR, $"Execution scope {scopeId} tried to run twice.");
        }
        hasRan = true;

        // Execute the last word.
        if (args != null && args.Length > 0 && args[0] != null)
        {
            if (args[0].GetType() == typeof(string))
                stack.Push(new ForthDatum((string)args[0]));
        }
        return await words.Last().RunAsync(this, stack, connection, trigger, command, null, cancellationToken);
    }
}