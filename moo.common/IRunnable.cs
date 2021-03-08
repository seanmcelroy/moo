using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;

namespace moo.common
{
    public interface IRunnable
    {
        Tuple<bool, string?> CanProcess(PlayerConnection player, CommandResult command);

        Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken);
    }
}