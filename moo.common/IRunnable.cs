using System;
using System.Threading;
using System.Threading.Tasks;

public interface IRunnable
{
    Tuple<bool, string?> CanProcess(PlayerConnection player, CommandResult command);

    Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken);
}