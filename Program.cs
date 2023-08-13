using System.Diagnostics;
using System.Text;

namespace timewin;

class Program
{
    static int Main(string[] args)
    {
        var psi = new ProcessStartInfo();
        psi.UseShellExecute = false;
        psi.FileName = "cmd.exe";
        psi.Arguments = "/C " + EscapeCmdExeMetachars(ArgsToCommandLine(args));
        var proc = Process.Start(psi);
        proc.WaitForExit();
        Console.WriteLine($"Total execution time: {(proc.ExitTime - proc.StartTime).TotalSeconds:0.000} sec");
        return proc.ExitCode;
    }

    // From RT.Util.Core.CommandRunner
    static string ArgsToCommandLine(IEnumerable<string> args)
    {
        var sb = new StringBuilder();
        foreach (var arg in args)
        {
            if (arg == null)
                continue;
            if (sb.Length != 0)
                sb.Append(' ');
            // For details, see http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx
            if (arg.Length != 0 && arg.IndexOfAny(_cmdChars) < 0)
                sb.Append(arg);
            else
            {
                sb.Append('"');
                for (int c = 0; c < arg.Length; c++)
                {
                    int backslashes = 0;
                    while (c < arg.Length && arg[c] == '\\')
                    {
                        c++;
                        backslashes++;
                    }
                    if (c == arg.Length)
                    {
                        sb.Append('\\', backslashes * 2);
                        break;
                    }
                    else if (arg[c] == '"')
                    {
                        sb.Append('\\', backslashes * 2 + 1);
                        sb.Append('"');
                    }
                    else
                    {
                        sb.Append('\\', backslashes);
                        sb.Append(arg[c]);
                    }
                }
                sb.Append('"');
            }
        }
        return sb.ToString();
    }
    static readonly char[] _cmdChars = new[] { ' ', '"', '\n', '\t', '\v' };

    // From RT.Util.Core.CommandRunner
    static string EscapeCmdExeMetachars(string command)
    {
        var result = new StringBuilder();
        foreach (var ch in command)
        {
            switch (ch)
            {
                case '(':
                case ')':
                case '%':
                case '!':
                case '^':
                case '"':
                case '<':
                case '>':
                case '&':
                case '|':
                    result.Append('^');
                    break;
            }
            result.Append(ch);
        }
        return result.ToString();
    }
}