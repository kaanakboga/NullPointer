using System;
using System.Collections;
using NullPointer.Visual;
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
        [SerializeField] private Text _cursorLabel;
        [SerializeField] private CyberNoirPanelPresentation _presentation;
        private Coroutine _cursorRoutine;

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

        public void ConfigurePresentation(CyberNoirPanelPresentation presentation, Text cursorLabel)
        {
            _presentation = presentation;
            _cursorLabel = cursorLabel;
        }

        public void Show(TerminalData terminal, System.Collections.Generic.IReadOnlyList<TerminalEntry> entries)
        {
            _panelRoot.SetActive(true);
            _presentation?.Reveal();
            _titleLabel.text = terminal == null ? string.Empty : terminal.MenuTitle;
            _bodyLabel.text = "> BAĞLANTI DOĞRULANDI\n> VERİ DİZİNİ HAZIR\n\nBir kayıt seçin.";
            _statusLabel.text = "MNEMOSYNE / YEREL ARAŞTIRMA DÜĞÜMÜ / SALT OKUNUR";
            StartCursor();

            for (int index = 0; index < _entryButtons.Length; index++)
            {
                bool isVisible = terminal != null && entries != null && index < entries.Count;
                _entryButtons[index].gameObject.SetActive(isVisible);
                if (isVisible && index < _entryLabels.Length)
                {
                    TerminalEntry entry = entries[index];
                    _entryLabels[index].text = $"{index + 1:00}  /{entry.Category.ToString().ToUpperInvariant()}\n     {entry.Title}";
                }
            }

            GameObject initialFocus = entries != null && entries.Count > 0
                ? _entryButtons[0].gameObject
                : _closeButton.gameObject;
            EventSystem.current?.SetSelectedGameObject(initialFocus);
        }

        public void ShowEntry(TerminalEntry entry, bool evidenceWasCollected)
        {
            _bodyLabel.text = entry == null
                ? string.Empty
                : $"> DOSYA AÇILDI :: {entry.Title}\n> BÜTÜNLÜK DENETİMİ :: TAMAMLANDI\n\n{entry.Body}";
            _statusLabel.text = evidenceWasCollected
                ? "◆ KANIT DİZİNE KAYDEDİLDİ"
                : "OKUMA OTURUMU / DEĞİŞİKLİK YAPILMADI";
        }

        public void Hide()
        {
            StopCursor();
            if (_presentation != null)
            {
                _presentation.HideAnimated();
            }
            else
            {
                _panelRoot.SetActive(false);
            }
        }

        private void StartCursor()
        {
            StopCursor();
            if (_cursorLabel != null && Application.isPlaying)
            {
                _cursorRoutine = StartCoroutine(BlinkCursor());
            }
            else if (_cursorLabel != null)
            {
                _cursorLabel.enabled = true;
            }
        }

        private IEnumerator BlinkCursor()
        {
            while (true)
            {
                _cursorLabel.enabled = !_cursorLabel.enabled;
                yield return new WaitForSecondsRealtime(0.48f);
            }
        }

        private void StopCursor()
        {
            if (_cursorRoutine != null)
            {
                StopCoroutine(_cursorRoutine);
                _cursorRoutine = null;
            }

            if (_cursorLabel != null)
            {
                _cursorLabel.enabled = false;
            }
        }

        private void OnDisable()
        {
            StopCursor();
        }
    }
}
