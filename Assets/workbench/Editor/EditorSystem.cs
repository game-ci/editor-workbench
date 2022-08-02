using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace BuildMethods
{
    public class EditorSystem
    {
    
        public static void ExecuteCommand(string Command)
        {
            ProcessStartInfo ProcessInfo;
            System.Diagnostics.Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + Command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = true;

            Process = System.Diagnostics.Process.Start(ProcessInfo);
        }
    
        public static void ExecuteMultiLineCommand(string Command)
        {
            var lines=Command.Split("\\n");
            for (int i = 0; i < lines.Length; i++)
            {
                ExecuteCommand(lines[i]);
            }
        }
        
        public static async Task<Tuple<int,string>> RunShProcessAsync(string args)
        {
            string fileName = "C:\\Program Files\\Git\\bin\\sh.exe";
            args = $"-c \"{args}\"";
            using (var process = new Process
                   {
                       StartInfo =
                       {
                           FileName = fileName, Arguments = args,
                           UseShellExecute = false, CreateNoWindow = true,
                           RedirectStandardOutput = true, RedirectStandardError = true
                       },
                       EnableRaisingEvents = true
                   })
            {
                return await RunProcessAsync(process);
            }
        }     
        
        public static async Task<Tuple<int,string>> RunProcessAsync(string args)
        {
            string fileName = "cmd.exe";
            args = "/c" + args;
            using (var process = new Process
                   {
                       StartInfo =
                       {
                           FileName = fileName, Arguments = args,
                           UseShellExecute = false, CreateNoWindow = true,
                           RedirectStandardOutput = true, RedirectStandardError = true
                       },
                       EnableRaisingEvents = true
                   })
            {
                return await RunProcessAsync(process);
            }
        }      
        private static Task<Tuple<int,string>> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<Tuple<int,string>>();
            string output = "";
            string errorOutput = "";

            process.Exited += (s, ea) => tcs.SetResult(
                new Tuple<int, string>(
                    process.ExitCode, 
                    string.IsNullOrEmpty(errorOutput)?output:output+System.Environment.NewLine+errorOutput
                )
            );
            process.OutputDataReceived += (s, ea) =>output+=string.IsNullOrEmpty(ea.Data)?"":ea.Data+System.Environment.NewLine;
            process.ErrorDataReceived += (s, ea) => errorOutput+=string.IsNullOrEmpty(ea.Data)?"":"ERROR:"+ea.Data+System.Environment.NewLine;

            bool started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}