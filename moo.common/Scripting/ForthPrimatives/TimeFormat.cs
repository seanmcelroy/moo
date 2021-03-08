using System;
using System.Globalization;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class TimeFormat
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            TIMEFMT (s i -- s) 

            Takes a format string and a SYSTIME integer and returns a string formatted with the time.
            The format string is ascii text with formatting commands:

            %% -- "%"

            %a -- abbreviated weekday name.

            %A -- full weekday name.

            %b -- abbreviated month name.

            %B -- full month name.

            %C -- "%A %B %e, %Y"

            %c -- "%x %X"

            %D -- "%m/%d/%y"

            %d -- month day, "01" - "31"

            %e -- month day, " 1" - "31"

            %h -- "%b"

            %H -- hour, "00" - "23"

            %I -- hour, "01" - "12"

            %j -- year day, "001" - "366"

            %k -- hour, " 0" - "23"

            %l -- hour, " 1" - "12"

            %M -- minute, "00" - "59"

            %m -- month, "01" - "12"

            %p -- "AM" or "PM"

            %R -- "%H:%M"

            %r -- "%I:%M:%S %p"

            %S -- seconds, "00" - "59"

            %T -- "%H:%M:%S"

            %U -- week number of the year. "00" - "52"

            %w -- week day number, "0" - "6"

            %W -- week# of year, starting on a monday, "00" - "52"

            %X -- "%H:%M:%S"

            %x -- "%m/%d/%y"

            %y -- year, "00" - "99"

            %Y -- year, "1900" - "2155"

            %Z -- Time zone. "GMT", "EDT", "PST", etc.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TIMEFMT requires two parameters");

            var si = parameters.Stack.Pop();
            if (si.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TIMEFMT requires the top parameter on the stack to be an integer");

            var sfmt = parameters.Stack.Pop();
            if (sfmt.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TIMEFMT requires the second-to-top parameter on the stack to be a format string");

            var offset = DateTimeOffset.FromUnixTimeSeconds(si.UnwrapInt());
            var dt = offset.DateTime;
            var formatString = (string?)sfmt.Value ?? string.Empty;

            // Normalize MUF format string
            formatString = formatString
                // Escaped part 1
                .Replace("%%", @"~\,./~")
                // Macros
                .Replace("%C", "%A %B %e, %Y")
                .Replace("%c", "%x %X")
                .Replace("%D", "%m/%d/%y")
                .Replace("%h", "%b")
                .Replace("%R", "%H:%M")
                .Replace("%r", "%I:%M:%S %p")
                .Replace("%T", "%H:%M:%S")
                .Replace("%X", "%H:%M:%S")
                .Replace("%x", "%m/%d/%y")
                // Formats
                .Replace("%a", "ddd")
                .Replace("%A", "dddd")
                .Replace("%b", "MMM")
                .Replace("%B", "MMMM")
                .Replace("%d", "dd")
                .Replace("%e", "d")
                .Replace("%H", "HH")
                .Replace("%I", "hh")
                .Replace("%j", dt.DayOfYear.ToString("000"))
                .Replace("%k", "H")
                .Replace("%l", "h")
                .Replace("%M", "mm")
                .Replace("%m", "MM")
                .Replace("%p", "tt")
                .Replace("%S", "ss")
                .Replace("%U", GetIso8601WeekOfYear(dt).ToString("00"))
                .Replace("%w", ((int)dt.DayOfWeek).ToString())
                .Replace("%W", GetIso8601WeekOfYear(dt).ToString("00")) // TODO: May be slightly incorrect
                .Replace("%y", "yy")
                .Replace("%Y", "yyyy")
                .Replace("%Z", "GMT");

            var formatted = dt.ToString(formatString).Replace(@"~\,./~", "%");

            parameters.Stack.Push(new ForthDatum(formatted));

            return ForthPrimativeResult.SUCCESS;
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}