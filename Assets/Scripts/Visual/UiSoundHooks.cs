using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/UI Sound Hooks")]
    public sealed class UiSoundHooks : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioClip _focus;
        [SerializeField] private AudioClip _confirm;
        [SerializeField] private AudioClip _back;
        [SerializeField] private AudioClip _invalid;

        public void Configure(AudioSource source, AudioClip focus, AudioClip confirm, AudioClip back, AudioClip invalid)
        {
            _source = source;
            _focus = focus;
            _confirm = confirm;
            _back = back;
            _invalid = invalid;
        }

        public void PlayFocus() => Play(_focus);
        public void PlayConfirm() => Play(_confirm);
        public void PlayBack() => Play(_back);
        public void PlayInvalid() => Play(_invalid);

        private void Play(AudioClip clip)
        {
            if (_source != null && clip != null)
            {
                _source.PlayOneShot(clip);
            }
        }
    }
}
