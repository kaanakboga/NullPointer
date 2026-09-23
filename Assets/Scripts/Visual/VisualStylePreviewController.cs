using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [AddComponentMenu("Null Pointer/Visual/Visual Style Preview Controller")]
    public sealed class VisualStylePreviewController : MonoBehaviour
    {
        [SerializeField] private UiTransition _panelTransition;
        [SerializeField] private MemoryDistortionController _memoryFx;
        [SerializeField] private MemoryDistortionProfile _memoryProfile;
        [SerializeField] private Button _primaryButton;
        [SerializeField] private Button _memoryButton;

        public void Configure(
            UiTransition panelTransition,
            MemoryDistortionController memoryFx,
            MemoryDistortionProfile memoryProfile,
            Button primaryButton,
            Button memoryButton)
        {
            _panelTransition = panelTransition;
            _memoryFx = memoryFx;
            _memoryProfile = memoryProfile;
            _primaryButton = primaryButton;
            _memoryButton = memoryButton;
        }

        private void Start()
        {
            _primaryButton?.onClick.AddListener(ReplayPanel);
            _memoryButton?.onClick.AddListener(PlayMemoryFx);
            _panelTransition?.PlayEntrance();
            if (_primaryButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(_primaryButton.gameObject);
            }
        }

        private void ReplayPanel()
        {
            _panelTransition?.PlayReveal();
        }

        private void PlayMemoryFx()
        {
            _memoryFx?.Play(_memoryProfile);
        }

        private void OnDestroy()
        {
            _primaryButton?.onClick.RemoveListener(ReplayPanel);
            _memoryButton?.onClick.RemoveListener(PlayMemoryFx);
        }
    }
}
