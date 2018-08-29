using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class ProgramBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();

        foreach (var key in new[] { "@prog", "@program" })
        {
            if (string.Compare(key, verb, true) == 0)
                return new Tuple<bool, string>(true, verb);
        }

        return new Tuple<bool, string>(false, null);
    }

    public Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        connection.EnterEditMode(command.getDirectObject(), async t =>
        {
            var script = Server.RegisterScript(command.getDirectObject(), connection.GetPlayer().id, t);

            // Move this to my inventory
            await script.MoveToAsync(connection.GetPlayer(), cancellationToken);

            Console.WriteLine($"Created new program {script.UnparseObject()}");
        });

        return Task.FromResult(new VerbResult(true, "Editor exited"));
    }
}