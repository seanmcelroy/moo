using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Models;

namespace moo.common
{
    public interface IRunnable
    {
        Tuple<bool, string?> CanProcess(Dbref player, CommandResult command);

        Task<VerbResult> Process(
            Dbref player,
            PlayerConnection? connection,
            CommandResult command,
            CancellationToken cancellationToken);
    }
}