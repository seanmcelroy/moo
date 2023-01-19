using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            ILogger? logger,
            CancellationToken cancellationToken);
    }
}