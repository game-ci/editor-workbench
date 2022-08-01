using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildMethods.Settings;
using UnityEditor;
using UnityEngine;

namespace BuildMethods
{
    
    [InitializeOnLoad]
    public class GameCIWorkbench : EditorWindow
    {
        private static bool _installed;
        private static string _list;
        private int tab;
        private static GameCISettingsData _settingsData;
        private static string[] _listJobs;
        private bool _showInactiveBuilds;
        private static string _helpText;
        private static Vector2 _helpScroll, _buildsScroll, _statusScroll;

        static GameCIWorkbench()
        {
            InitGameCiWorkbench();
        }

        static async void InitGameCiWorkbench()
        {
            //assume the unity-builder project is next to the unity project
            _installed = await GameCiCli.CheckGameCiInstalled();
            _list = await GameCiCli.ListGameCi();
            _listJobs = (await GameCiCli.ListGameCiJobs()).Split("\n");
            _settingsData = GameCISettingsData.GetOrCreateSettings();
            _helpText = await GameCiCli.RunHelp();
        }
    
        [MenuItem("Window/GameCI Workbench")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            GameCIWorkbench window = (GameCIWorkbench)EditorWindow.GetWindow(typeof(GameCIWorkbench));
            window.Show();
        }

        void OnGUI()
        {
            if (_settingsData is null)
            {
                return;
            }
            GUILayout.Label(
                "Game CI Workbench"
                + (_installed?"[Installed]":"[Not Installed]") 
                + (_settingsData.AWS ? "[AWS]":""), 
                EditorStyles.whiteLargeLabel
            );
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Refresh", GUILayout.MaxWidth(250f)))
            {
                InitGameCiWorkbench();
            }
            EditorGUILayout.TextField("Refresh Interval (seconds)", "15");
            GUILayout.Toggle(false, "Auto Refresh");
            GUILayout.EndHorizontal();
            float space = 20f;
            EditorGUILayout.Space(space);
            GUILayout.Label(
                "Run Builds",
                EditorStyles.whiteLargeLabel
            );
            GUILayout.Toggle(true, "Auto Watch On Run");
            EditorGUILayout.TextField("timeout (hours)", "48");
            // preview global pre and post hooks
            // preview global pre and post jobs
            // additional pre and post hooks
            // additional pre and post jobs
            // git commit, branch, unpushed, uncomitted, unchecked
            // git remote
            if(GUILayout.Button("Run Cli Build")){
                GameCiCli.RunCommand($"cli {GameCiCli.GetRelativePathToProject()} {GameCiCli.GetInputOverrides()} --readInputOverrideCommand=\"{_settingsData.OverrideCommand}\"");
            }
            EditorGUILayout.Space(space);
            
            DrawBuildInspection();
            
            EditorGUILayout.Space(space);
            
            DrawStatus();
                
            EditorGUILayout.Space(space);
            
            GUILayout.Label("List Resources", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("All", true);
            EditorGUILayout.Toggle("Tasks", false);
            EditorGUILayout.Toggle("Other", false);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            DrawButtonWithLog("aws-list-all", "List Resources");
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(space);
                
            DrawGarbageCollection();
            
            EditorGUILayout.Space(space);
            
            GUILayout.Label("Help Info", EditorStyles.miniBoldLabel);
            _helpScroll = EditorGUILayout.BeginScrollView(_helpScroll, EditorStyles.textField);
            GUILayout.Label(_helpText, EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }

        private void DrawBuildInspection()
        {
            GUILayout.Label(
                "Inspect Builds",
                EditorStyles.whiteLargeLabel
            );
            _buildsScroll = EditorGUILayout.BeginScrollView(_buildsScroll);
            for (int i = 0; i < _listJobs.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_listJobs[i].Replace("Task Stack", "").Split(" - ").First(),
                    GUILayout.MaxWidth(250f));
                GUILayout.Label($"Runtime {_listJobs[i].Replace("Task Stack", "").Replace("Age ", "").Split(" - ").Last()}",
                    GUILayout.MaxWidth(150f));
                GUILayout.Label($"Running");
                GUILayout.Button("Inspect");
                GUILayout.Button("Watch");
                GUILayout.Button("Stop");
                GUILayout.EndHorizontal();
            }
            for (int i = 0; i < (_showInactiveBuilds ? 2 : 0); i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Build {i}");
                GUILayout.Label($"Runtime 5m");
                GUILayout.Label($"Ended 15m ago");
                GUILayout.Button("Inspect");
                GUILayout.Button("Restart");
                GUILayout.Button("Cleanup");
                // artifacts
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            _showInactiveBuilds = GUILayout.Toggle(_showInactiveBuilds, "Show Inactive Builds (2)");
        }

        private static void DrawStatus()
        {
            GUILayout.Label(
                "Cloud Runner Resources Status",
                EditorStyles.whiteLargeLabel
            );
            _statusScroll = EditorGUILayout.BeginScrollView(_statusScroll, EditorStyles.textField);
            GUILayout.Label(_list, EditorStyles.miniBoldLabel);
            EditorGUILayout.EndScrollView();
        }

        private void DrawGarbageCollection()
        {
            GUILayout.Label("Garbage Collection", EditorStyles.whiteLargeLabel);
            
            GUILayout.Label("Garbage Collection Settings", EditorStyles.miniBoldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "1d Older");
            GUILayout.Toggle(false, "All");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Button("Preview Results");
            GUILayout.Button("Run Garbage Collection");
            GUILayout.EndHorizontal();
        }
        

        public static async Task RunHelp()
        {
            var result = await GameCiCli.RunHelp();
            Debug.Log(result);
        }
        

        private void DrawButtonWithLog(string command, string buttonMessage = null)
        {
            if (buttonMessage is null)
            {
                buttonMessage = command;
            }
            if (GUILayout.Button(buttonMessage))
            {
                Debug.Log($"running command locally \"{command}\"");
                GameCiCli.RunCommand(command, true);
            }
        }
    }
}
