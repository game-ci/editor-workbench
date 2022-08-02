using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildMethods.Settings
{
    public class GameCISettings : SettingsProvider
    {

        private static readonly string k_MyCustomSettingsPath = $"Assets/Editor/{nameof(GameCISettings)}.asset";
        private GameCISettingsData _mCustomSettingsData;
        private int tab;
        private Vector2 _scroll;
        private string _json;

        public GameCISettings(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            _mCustomSettingsData = GameCISettingsData.GetOrCreateSettings();
        }

        public override void OnGUI(string searchContext)
        {
            using (CreateSettingsWindowGUIScope())
            {
                float space = 20f;
                GUILayout.Label("Game CI Installation Found Successfully", 
                    EditorStyles.miniLabel);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Documentation"))
                {
                    
                }
                if (GUILayout.Button("Open Local Game CI Folder"))
                {
                    
                }
                GUILayout.EndHorizontal();
                
                if (_json != JsonUtility.ToJson(_mCustomSettingsData) && GUILayout.Button("Save Changes"))
                {
                    EditorUtility.SetDirty(_mCustomSettingsData);
                    AssetDatabase.SaveAssetIfDirty(_mCustomSettingsData);
                }
                _json = JsonUtility.ToJson(_mCustomSettingsData);
                EditorGUILayout.Space(space);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, false, true);
                
                GUILayout.Label("Main Settings", 
                    EditorStyles.whiteLargeLabel);
                _mCustomSettingsData.CloudRunnerBranch = EditorGUILayout.TextField("Cloud Runner Release Branch", _mCustomSettingsData.CloudRunnerBranch);
                EditorGUILayout.TextField("Game-CI Remote URL", "https://github.com/game-ci/unity-builder");
                EditorGUILayout.Toggle("Enable Debug", true);
                EditorGUILayout.Toggle("Hide Editor Help Messages", false);
                EditorGUILayout.Toggle("Skip Cache Usage", false);
                
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Cloud Runner Platform", 
                    EditorStyles.whiteLargeLabel);
                GUILayout.Label("Beta", 
                    EditorStyles.miniLabel);
                _mCustomSettingsData.AWS = EditorGUILayout.Toggle("Aws", _mCustomSettingsData.AWS);
                EditorGUILayout.Space(space/4f);
                GUILayout.Label("Preview", 
                    EditorStyles.miniLabel);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("Kubernetes", !_mCustomSettingsData.AWS);
                EditorGUILayout.Toggle("Local Docker", false);
                EditorGUILayout.Toggle("Local", false);
                // GUILayout.Label("Proposed", 
                //     EditorStyles.miniLabel);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("GCP", false);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("Azure", false);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("GitHub Actions Workflow", false);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("GitLab Manual Pipeline Run", false);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("Argo Workflow", false);
                // _mCustomSettingsData.K8s = EditorGUILayout.Toggle("Tekton", false);
                
                EditorGUILayout.Space(space);
                
                DrawSecretsMenu();
                
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Cache Settings", 
                    EditorStyles.whiteLargeLabel);
                EditorGUILayout.HelpBox("The Cloud Runner cache is used to store selected folders (e.g Unity Library folder, Git LFS) and persists the contents into new tasks", MessageType.Info);
                EditorGUILayout.Toggle("Cache Override Enabled", false);
                EditorGUILayout.TextField("Cache Directories", ".git/LFS,Library");
                EditorGUILayout.TextField("Include Files Pattern", "*");
                EditorGUILayout.TextField("Exclude Files Pattern", "");
                EditorGUILayout.TextField("Retain for time span (in days)", "30");
                EditorGUILayout.TextField("Max Cache Size per entry (in GB)", "30");
                EditorGUILayout.TextField("Max Cache Size Total (in GB)", "300");
                EditorGUILayout.TextField("Cache Override Command Push", "...");
                EditorGUILayout.TextField("Cache Override Command Pull", "...");
                EditorGUILayout.TextField("Cache Override Command Exists", "...");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Compression");
                EditorGUILayout.Toggle("None (fastest)", false);
                EditorGUILayout.Toggle("LZ4 (fast + smaller)", true);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(space);
                
                GUILayout.Label("Retained Workspace Settings", 
                    EditorStyles.whiteLargeLabel);
                EditorGUILayout.HelpBox("This setting prevents a Cloud Workspace from being cleaned up after use, following builds can use this to improve startup times at the cost of some extra storage size and slower cold/ephemeral scaling.", MessageType.Info);
                EditorGUILayout.Toggle("Retained Workspaces Enabled", false);
                EditorGUILayout.TextField("Max Retained Workspaces", "...");
                EditorGUILayout.TextField("Workspace Lock Command", "...");
                EditorGUILayout.TextField("Workspace Release Command", "...");
                EditorGUILayout.TextField("Workspace Check Command", "...");
                // storage used?
                // cached build folders, branches and commits?
                // pre-warmed settings?
                // global pre and post hooks
                // global pre and post jobs
                // cron active?
                // identity
                // images

                EditorGUILayout.Space(space);
        
                GUILayout.Label("CLI Help", EditorStyles.whiteLargeLabel);
                EditorGUILayout.HelpBox("The is the command line --help output.", MessageType.Info);
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                GUILayout.Label(GameCIWorkbench.HelpText, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSecretsMenu()
        {
            GUILayout.Label("Secrets", EditorStyles.whiteLargeLabel);
            _mCustomSettingsData.OverrideEnabled = EditorGUILayout.Toggle("Override Enabled", _mCustomSettingsData.OverrideEnabled);
            _mCustomSettingsData.OverrideCommand = EditorGUILayout.TextField("Override Command", _mCustomSettingsData.OverrideCommand);
            
            // list secrets, enabled or disabled, command to pull secrets, use or don't use secret override
            var secretsArray = _mCustomSettingsData.Overrides.ToArray();
            for (int i = 0; i < secretsArray.Length; i++)
            {
                GUILayout.BeginHorizontal();
                secretsArray[i].Key = GUILayout.TextField(secretsArray[i].Key, GUILayout.MaxWidth(250f));
                secretsArray[i].Active =
                    GUILayout.Toggle(secretsArray[i].Active, secretsArray[i].Active ? "Enabled" : "Disabled");
                if (GUILayout.Button("Delete"))
                {
                    _mCustomSettingsData.Overrides.Remove(secretsArray[i]);
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add new Input Override"))
            {
                _mCustomSettingsData.Overrides.Add(new InputOverride());
            }
        }

        private IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateGameCISettingsProvider()
        {
            if (!IsSettingsAvailable())
            {
                GameCISettingsData.GetOrCreateSettings();
            }
            
            var provider = new GameCISettings($"Project/{nameof(GameCISettings)}", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }
        private static bool IsSettingsAvailable()
        {
            return File.Exists(k_MyCustomSettingsPath);
        }
    }
}