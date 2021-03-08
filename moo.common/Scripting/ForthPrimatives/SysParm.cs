using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class SysParm
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SYSPARM ( s -- s ) 

            Takes a tuneable system parameter and returns its value as a string.

            For an integer it returns it as a string, 
                a time is returned as a string containing the number of seconds, 
                a dbref is returned in standard dbref format, and boolean is returned as 'yes' or 'no' 

            Checking an invalid parameter or a parameter requiring higher permissions than the program has will return an empty string.

            Parameters available:

            (str) dumpwarn_mesg - Message to warn of a coming DB dump

            (str) deltawarn_mesg - Message to warn of a coming delta dump

            (str) dumpdeltas_mesg - Message telling of a delta dump

            (str) dumping_mesg - Message telling of a DB dump

            (str) dumpdone_mesg - Message notifying a dump is done

            (str) penny - A single currency

            (str) pennies - Plural currency

            (str) cpenny - Capitolized currency

            (str) cpennies - Capitolized plural currency

            (str) muckname - The name of the MUCK

            (str) rwho_passwd - Password for RWHO servers (Wizbit only)

            (str) rwho_server - RWHO server to connect to (Wizbit only)

            (str) huh_mesg - Message for invalid commands

            (str) leave_mesg - Message given when QUIT is used

            (str) idle_boot_mesg - Message given to an idle booted user

            (str) register_mesg - Message for a failed 'create' at login

            (str) playermax_warnmesg - Message warning off too many connects

            (str) playermax_bootmesg - Error given when a player cannot connect

            (time) rwho_interval - Interval between RWHO updates

            (time) dump_interval - Interval between dumps

            (time) dump_warntime - Warning prior to a dump

            (time) monolithic_interval - Max time between full DB dumps

            (time) clean_interval - Interval between unused object purges

            (time) aging_time - When an object is considered old and unused

            (time) maxidle - Maximum idle time allowed

            (int) max_object_endowment - Max value of an object

            (int) object_cost - Cost to create an object

            (int) exit_cost - Cost to create an exit

            (int) link_cost - Cost to link an exit

            (int) room_cost - Cost to dig a room

            (int) lookup_cost - Cost to lookup a player name

            (int) max_pennies - Max number of pennies a player can own

            (int) penny_rate - Rate for finding pennies

            (int) start_pennies - Starting wealth for new players

            (int) kill_base_cost - Number of pennies for a 100 percent chance

            (int) kill_min_cost - Minimum cost for doing a kill

            (int) kill_bonus - Bonus for a successful kill

            (int) command_burst_size - Maximum number of commands per burst

            (int) commands_per_time - Commands per time slice after burst

            (int) command_time_msec - Time slice length in milliseconds

            (int) max_delta_objs - Max percent of changed objects for a delta

            (int) max_loaded_objs - Max percent of the DB in memory at once

            (int) max_force_level - Maximum number of forces within one command

            (int) max_process_limit - Total processes allowed

            (int) max_plyr_processes - Processes allowed for each player

            (int) max_instr_count - Max preempt mode instructions

            (int) instr_slice - Max uninterrupted instructions per time slice

            (int) mpi_max_commands - Max number of uninterruptable MPI commands

            (int) pause_min - Pause between input and output servicing

            (int) free_frames_pool - Number of program frames pre-allocated

            (int) listen_mlev - Minimum MUCKER level for _listen programs

            (int) playermax_limit - Manimum allowed connections

            (ref) player_start - The home for players without a home

            (bool) use_hostnames - Do reverse domain name lookup

            (bool) log_commands - The server logs commands (Wizbit only)

            (bool) log_failed_commands - The server logs failed commands (Wizbit only)

            (bool) log_programs - The server logs programs (Wizbit only)

            (bool) dbdump_warning - Warn about coming DB dumps

            (bool) deltadump_warning - Warn about coming delta dumps

            (bool) periodic_program_purge - Purge unused programs from memory

            (bool) support_rwho - Use RWHO server

            (bool) secure_who - WHO works only in command mode

            (bool) who_doing - Server support for @doing

            (bool) realms_control - Support for realm wizzes

            (bool) allow_listeners - Allow listeners

            (bool) allow_listeners_obj - Objects can be listeners

            (bool) allow_listeners_env - Listeners can be up the environment

            (bool) allow_zombies - Zombie objects allowed

            (bool) wiz_vehicles - Only wizzes can make vehicles

            (bool) force_mlev1_name_notify - M1 programs forced to show name on notify

            (bool) restrict_kill - Can only kill KILL_OK players

            (bool) registration - Only wizzes can create players

            (bool) teleport_to_player - Allow use of exits linked to players

            (bool) secure_teleport - Check teleport permissions for personal exits

            (bool) exit_darking - Players can set exits dark

            (bool) thing_darking - Players can set objects dark

            (bool) dark_sleepers - Sleepers are effectively dark

            (bool) who_hides_dark - Dark players are hidden (Wizbit only)

            (bool) compatible_priorities - Backwards compatibility for exit priorities

            (bool) do_mpi_parsing - Parse MPI strings in messages

            (bool) look_propqueues - Look triggers _lookq propqueue

            (bool) lock_envcheck - Locks will check the environment

            (bool) diskbase_propvals - Allow diskbasing of property values

            (bool) idleboot - Enable or disable idlebooting

            (bool) playermax - Enable or disable connection limit
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SYSPARM requires one parameter");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SYSPARM requires the top parameter on the stack to be a string");

            parameters.Stack.Push(new ForthDatum(""));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}