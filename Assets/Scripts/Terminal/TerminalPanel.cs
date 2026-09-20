using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Terminal
{
    [AddComponentMenu("Null Pointer/Terminal/Terminal Panel")]
    public sealed class TerminalPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _bodyLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _entryButtons = Array.Empty<Button>();
        [SerializeField] private Text[] _entryLabels = Array.Empty<Text>();

        public void Configure(
            GameObject panelRoot,
            Text titleLabel,
            Text bodyLabel,
            Text statusLabel,
            Button closeButton,
            Button[] entryButtons,
            Text[] entryLabels)
        {
            _panelRoot = panelRoot;
            _titleLabel = titleLabel;
            _bodyLabel = bodyLabel;
            _statusLabel = statusLabel;
            _closeButton = closeButton;
            _entryButtons = entryButtons;
            _entryLabels = entryLabels;
        }

        public void Bind(TerminalController controller)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(controller.Close);
            for (int index = 0; index < _entryButtons.Length; index++)
            {
                int capturedIndex = index;
                _entryButtons[index].onClick.RemoveAllListeners();
                _entryButtons[index].onClick.AddListener(() => controller.SelectEntry(capturedIndex));
            }
        }

        public void Show(TerminalData terminal)
        {
            _panelRoot.SetActive(true);
            _titleLabel.text = terminal == null ? string.Empty : terminal.MenuTitle;
            _bodyLabel.text = "Bir kayıt seçin.";
            _statusLabel.text = string.Empty;

            for (int index = 0; index < _entryButtons.Length; index++)
            {
                bool isVisible = terminal != null && index < terminal.Entries.Count;
                _entryButtons[index].gameObject.SetActive(isVisible);
                if (isVisible && index < _entryLabels.Length)
                {
                    TerminalEntry entry = terminal.Entries[index];
                    _entryLabels[index].text = $"[{entry.Category}] {entry.Title}";
                }
            }

            GameObject initialFocus = terminal != null && terminal.Entries.Count > 0
                ? _entryButtons[0].gameObject
                : _closeButton.gameObject;
            EventSystem.current?.SetSelectedGameObject(initialFocus);
        }

        public void ShowEntry(TerminalEntry entry, bool evidenceWasCollected)
        {
            _bodyLabel.text = entry?.Body ?? string.Empty;
            _statusLabel.text = evidenceWasCollected ? "Kanıt kaydedildi" : string.Empty;
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }
    }
}
