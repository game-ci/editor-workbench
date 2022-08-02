using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildMethods.Settings
{
    public class GameCISettingsData : ScriptableObject
    {
        public const string k_MyCustomSettingsPath = "Assets/Editor/GameCISettings.asset";

        [SerializeField]
        public Boolean AWS;
        [SerializeField]
        public Boolean K8s;
        
        [SerializeField]
        public Boolean OverrideEnabled;
        
        [SerializeField]
        public string CloudRunnerBranch = "main";
        
        [SerializeField]
        public List<InputOverride> Overrides = new List<InputOverride>();
        
        [SerializeField]
        public string OverrideCommand = GoogleCloudPullSecretOverride;
        
        private static readonly string GoogleCloudPullSecretOverride = "gcloud secrets versions access 1 --secret=\"{0}\"";
        
        internal static GameCISettingsData GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameCISettingsData>(k_MyCustomSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GameCISettingsData>();
                settings.AWS = false;
                settings.Overrides = new List<InputOverride>();
                AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}