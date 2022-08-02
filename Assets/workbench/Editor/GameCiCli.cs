using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildMethods.Settings;
using UnityEngine;

namespace BuildMethods
{
    public class GameCiCli
    {
        public static async Task<bool> CheckGameCiInstalled()
        {
            var commmandString = $"yarn --cwd ./../unity-builder/ run --version";
            var result = await EditorSystem.RunProcessAsync(commmandString);
            return result.Item1 == 0;
        }
        
        public static async Task<string> ListGameCi()
        {
            var commmandString = $"yarn --cwd ./../unity-builder/ run cli -m aws-list-all";
            var result = await EditorSystem.RunProcessAsync(commmandString);
            return result.Item2;
        }
        
        public static async Task<string> ListGameCiJobs()
        {
            var commmandString = $"yarn --cwd ./../unity-builder/ run cli -m aws-list-jobs";
            var result = await EditorSystem.RunProcessAsync(commmandString);
            return result.Item2
                .Split("\n")
                .Where(x=>x.StartsWith("Task Stack"))
                .Aggregate((x, y)=>x+"\n"+y);
        }

        public static async Task<string> RunCommand(string command, bool logResult = false)
        {
            var commmandString = $"yarn --cwd ./../unity-builder/ run cli -m {command}";
            var result = await EditorSystem.RunProcessAsync(commmandString);
            if(logResult) Debug.Log(result.Item2);
            return result.Item2;
        }

        public static async Task<string> RunHelp()
        {
            var commmandString = $"yarn --cwd ./../unity-builder/ run cli --help";
            var result = await EditorSystem.RunProcessAsync(commmandString);
            return result.Item2;
        }

        public static string GetRelativePathToProject()
        {
            return "--projectPath " + Path.GetRelativePath("./../unity-builder/", Path.Combine(Application.dataPath, ".."));
        }

        public static string GetInputOverrides()
        {
            return GameCISettingsData.GetOrCreateSettings().OverrideEnabled
                ? "--populateOverride true --readInputFromOverrideList " + GameCISettingsData.GetOrCreateSettings().Overrides.Select(x => x.Key).Aggregate((x, y) => x + "," + y)
                : "";
        }
    }
}