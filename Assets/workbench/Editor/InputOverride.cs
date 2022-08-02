using UnityEngine;

namespace BuildMethods.Settings
{
    [System.Serializable]
    public class InputOverride
    {
        [SerializeField]
        public string Key;
        [SerializeField]
        public bool Active;
    }
}