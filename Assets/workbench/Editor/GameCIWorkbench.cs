using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildMethods;
using BuildMethods.Settings;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GameCIWorkbench : EditorWindow
{
    private static bool _installed;
    private static string _list;
    private int tab;
    private static GameCISettingsData _settingsData;
    private static string[] _listJobs;
    private bool _showInactiveBuilds;
    public static string HelpText { get; private set; }
    private static Vector2 _scroll;
    private static string _selectedBuild;

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
        HelpText = await GameCiCli.RunHelp();
    }

    [MenuItem("Window/GameCI Workbench")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GameCIWorkbench window = (GameCIWorkbench)EditorWindow.GetWindow(typeof(GameCIWorkbench));
        window.Show();
    }
    const float space = 35f;

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
        DrawStatus();
        DrawListResourcesCommand();
        DrawBuildInspection();
        DrawRunGameCiButtonAndOptions();
        DrawCustomHooksAndJobs();
        DrawGarbageCollection();
        EditorGUILayout.EndScrollView();
    }

    private static void DrawStatus()
    {
        EditorGUILayout.Space(space);
        GUILayout.EndHorizontal();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        DrawGameCiStatus();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        DrawGitStatus();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(space);
    }

    private static void DrawCustomHooksAndJobs()
    {
        DrawCustomHooks();

        DrawCustomJobs();
    }

    private static void DrawCustomJobs()
    {
        GUILayout.Label("Custom Jobs",
            EditorStyles.whiteLargeLabel);
        EditorGUILayout.HelpBox(
            "Custom Jobs enable you to schedule containers before, after or at a custom event supported by your task. Some commonly useful pre-built side jobs are also available.",
            MessageType.Info);
        GUILayout.Label("Pre-built jobs",
            EditorStyles.miniBoldLabel);
        EditorGUILayout.Toggle("(post) Push to AWS S3", true);
        EditorGUILayout.Toggle("(post) Deploy to Steam", false);

        EditorGUILayout.Space(space);
    }

    private static void DrawCustomHooks()
    {
        GUILayout.Label("Custom Hooks",
            EditorStyles.whiteLargeLabel);
        EditorGUILayout.HelpBox(
            "Custom Hooks enable you to schedule commands within a task to run before, after or at a custom event supported by your task.",
            MessageType.Info);

        EditorGUILayout.Space(space);
    }

    private static void DrawRunGameCiButtonAndOptions()
    {
        GUILayout.Label(
            "Run Builds",
            EditorStyles.whiteLargeLabel
        );
        GUILayout.Label("Tests",
            EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Toggle(true, "Run Playmode Tests");
        GUILayout.Toggle(true, "Run Editor Tests");
        EditorGUILayout.TextField("Test Category Filter", "*");
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Builds",
            EditorStyles.miniBoldLabel);
        GUILayout.Toggle(true, "Build StandaloneWindows64");
        GUILayout.Toggle(true, "Build StandaloneLinux64");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Toggle(true, "Run Custom Editor Method");
        EditorGUILayout.TextField("Custom Editor Method", "");
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Other Settings",
            EditorStyles.miniBoldLabel);
        EditorGUILayout.TextField("timeout (hours)", "48");
        GUILayout.Toggle(true, "Auto Watch On Run");
        if (GUILayout.Button("Run Game-CI"))
        {
            GameCiCli.RunCommand(
                $"cli {GameCiCli.GetRelativePathToProject()} {GameCiCli.GetInputOverrides()} --readInputOverrideCommand=\"{_settingsData.OverrideCommand}\"");
        }

        EditorGUILayout.HelpBox("This will start a new task using Game-CI (Cloud Runner).", MessageType.Info);

        EditorGUILayout.Space(space);
    }

    private static void DrawGitStatus()
    {
        GUILayout.Label("Git Status",
            EditorStyles.whiteLargeLabel);
        GUILayout.Label("unchecked changes",
            EditorStyles.miniBoldLabel);
        GUILayout.Label("checked changes",
            EditorStyles.miniBoldLabel);
        GUILayout.Label("un-pushed changes",
            EditorStyles.miniBoldLabel);
        EditorGUILayout.TextField("Latest Committed SHA", "...");
        EditorGUILayout.TextField("Latest Pushed SHA", "...");
        EditorGUILayout.TextField("Active Branch", "main");
        EditorGUILayout.TextField("Pushed Branch", "main");
        EditorGUILayout.TextField("Git Remote URL", "...");
    }

    private void DrawListResourcesCommand()
    {
        GUILayout.Label("List Cloud Resources", EditorStyles.whiteLargeLabel);
        GUILayout.BeginHorizontal();
        EditorGUILayout.Toggle("All", true);
        EditorGUILayout.Toggle("Tasks", false);
        EditorGUILayout.Toggle("Other", false);
        GUILayout.EndHorizontal();

        DrawButtonWithLog("aws-list-all", "List Resources (Console)");

        EditorGUILayout.Space(space);
    }

    private void DrawBuildInspection()
    {
        GUILayout.Label(
            "Inspect Builds",
            EditorStyles.whiteLargeLabel
        );
        for (int i = 0; i < _listJobs.Length; i++)
        {
            var build = _listJobs[i].Replace("Task Stack", "").Split(" - ").First();
            GUILayout.BeginHorizontal();
            GUILayout.Label(build,
                GUILayout.MaxWidth(250f));
            GUILayout.Label($"Runtime {_listJobs[i].Replace("Task Stack", "").Replace("Age ", "").Split(" - ").Last()}",
                GUILayout.MaxWidth(150f));
            GUILayout.Label($"Running");
            if (_selectedBuild != build && GUILayout.Button("Inspect"))
            {
                _selectedBuild = _listJobs[i].Replace("Task Stack", "").Split(" - ").First();
            }
            else if (_selectedBuild == build && GUILayout.Button("Deselect"))
            {
                _selectedBuild = null;
            }
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
        _showInactiveBuilds = GUILayout.Toggle(_showInactiveBuilds, "Show Inactive Builds (2)");
        if (_selectedBuild != null)
        {
            EditorGUILayout.Space(space);
            GUILayout.Label(
                "Selected Build",
                EditorStyles.whiteLargeLabel
            );
            GUILayout.Label(_selectedBuild);
        }
        EditorGUILayout.Space(space);
    }

    private static void DrawGameCiStatus()
    {
        GUILayout.Label(
            "Cloud Runner Resources Status",
            EditorStyles.whiteLargeLabel
        );
        EditorGUILayout.BeginHorizontal(EditorStyles.textField);
        GUILayout.Label(_list, EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGarbageCollection()
    {
        GUILayout.Label("Garbage Collection", EditorStyles.whiteLargeLabel);
        EditorGUILayout.HelpBox("Garbage Collection is used to delete Cloud Resources, you can choose to delete inactive, old or all resources or just preview what would be deleted.", MessageType.Info);
        
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
