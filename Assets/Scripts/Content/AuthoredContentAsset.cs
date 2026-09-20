using UnityEngine;

namespace NullPointer.Content
{
    public abstract class AuthoredContentAsset : ScriptableObject, IStableContent
    {
        [SerializeField] private string _stableId = string.Empty;
        [SerializeField, TextArea] private string _editorDescription = string.Empty;

        public string StableId => _stableId;

        public string EditorDescription => _editorDescription;

        public string DiagnosticName => string.IsNullOrEmpty(name) ? GetType().Name : name;
    }
}
