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
                var json = JsonUtility.ToJson(_mCustomSettingsData);
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
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Main Settings", 
                    EditorStyles.whiteLargeLabel);
                _mCustomSettingsData.CloudRunnerBranch = EditorGUILayout.TextField("Cloud Runner Branch", _mCustomSettingsData.CloudRunnerBranch);
                EditorGUILayout.TextField("Cloud Runner Repository", "https://github.com/game-ci/unity-builder");
                EditorGUILayout.HelpBox("You can implement a custom Cloud Runner Provider by forking the repo and specifying a custom URL", MessageType.Info);
                EditorGUILayout.Toggle("Debug", true);
                
                EditorGUILayout.Space(space);
                
                // Not needed ... any git source is accepted
                GUILayout.Label("Git Platform", 
                    EditorStyles.whiteLargeLabel);
                EditorGUILayout.TextField("Git Remote URL", "...");
                EditorGUILayout.TextField("Active SHA", "...");
                EditorGUILayout.TextField("Active Branch", "...");
                // EditorGUILayout.Toggle("GitHub", m_CustomSettings.AWS);
                // EditorGUILayout.Toggle("GitLab", !m_CustomSettings.AWS);
                // EditorGUILayout.Toggle("Other", false);
                // gitea?
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Cloud Runner Platform", 
                    EditorStyles.whiteLargeLabel);
                _mCustomSettingsData.AWS = EditorGUILayout.Toggle("Aws", _mCustomSettingsData.AWS);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("Kubernetes", !_mCustomSettingsData.AWS);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("GitHub Actions Workflow", !_mCustomSettingsData.AWS);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("GitLab Manual Pipeline Run", !_mCustomSettingsData.AWS);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("Argo Workflow", !_mCustomSettingsData.AWS);
                _mCustomSettingsData.K8s = !EditorGUILayout.Toggle("Tekton", !_mCustomSettingsData.AWS);
                EditorGUILayout.Toggle("Local Docker", false);
                EditorGUILayout.Toggle("Local", false);
                
                EditorGUILayout.Space(space);
                
                DrawSecretsMenu();
                
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Cache Settings", 
                    EditorStyles.whiteLargeLabel);
                EditorGUILayout.HelpBox("The Cloud Runner cache archives folders of your choosing (usually Unity Library folder and Git LFS) and persists the contents into new tasks", MessageType.Info);
                EditorGUILayout.Toggle("Cache Override Enabled", false);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Compression");
                EditorGUILayout.Toggle("None (fastest)", false);
                EditorGUILayout.Toggle("LZ4 (fast + smaller)", true);
                GUILayout.EndHorizontal();
                EditorGUILayout.TextField("Cache Directories", ".git/LFS,Library");
                EditorGUILayout.TextField("Include Files Pattern", "*");
                EditorGUILayout.TextField("Exclude Files Pattern", "");
                EditorGUILayout.TextField("Retain for time span (in days)", "30");
                EditorGUILayout.TextField("Max Cache Size (in GB)", "30");
                EditorGUILayout.TextField("Cache Override Command Push", "...");
                EditorGUILayout.TextField("Cache Override Command Pull", "...");
                EditorGUILayout.TextField("Cache Override Command Exists", "...");

                EditorGUILayout.Space(space);
                
                GUILayout.Label("Retained Workspace Settings", 
                    EditorStyles.whiteLargeLabel);
                EditorGUILayout.HelpBox("This setting prevents a Cloud Workspace from being cleaned up after use, following builds can use this to improve startup times at the cost of some extra storage size and slower cold/ephemeral scaling.", MessageType.Info);
                EditorGUILayout.Toggle("Retained Workspaces Enabled", false);
                EditorGUILayout.TextField("Target Workspaces Count", "...");
                EditorGUILayout.TextField("Max Workspaces (when all workspaces in-use)", "...");
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
                
                GUILayout.Label("Custom Pre/Post Hooks", 
                    EditorStyles.whiteLargeLabel);
                
                EditorGUILayout.Space(space);
                
                GUILayout.Label("Custom Pre/Post Jobs", 
                    EditorStyles.whiteLargeLabel);
                GUILayout.Label("Pre-built jobs", 
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.Toggle("Test", true);
                EditorGUILayout.Toggle("Push Cache", true);
                EditorGUILayout.Toggle("Push Artifacts", true);
                EditorGUILayout.Toggle("Deploy Steam", false);

                EditorGUILayout.Space(space);
                
                if (json != JsonUtility.ToJson(_mCustomSettingsData))
                {
                    EditorUtility.SetDirty(_mCustomSettingsData);
                    AssetDatabase.SaveAssetIfDirty(_mCustomSettingsData);
                }
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
