using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Dialogue
{
    [AddComponentMenu("Null Pointer/Dialogue/Dialogue Panel")]
    public sealed class DialoguePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _speakerLabel;
        [SerializeField] private Text _bodyLabel;
        [SerializeField] private Text _historyLabel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _choiceButtons = Array.Empty<Button>();
        [SerializeField] private Text[] _choiceLabels = Array.Empty<Text>();
        private Coroutine _revealRoutine;
        private string _fullText = string.Empty;
        private IReadOnlyList<DialogueChoice> _pendingChoices = Array.Empty<DialogueChoice>();

        public bool IsRevealing => _revealRoutine != null;

        public void Configure(
            GameObject panelRoot,
            Text speakerLabel,
            Text bodyLabel,
            Text historyLabel,
            Button continueButton,
            Button closeButton,
            Button[] choiceButtons,
            Text[] choiceLabels)
        {
            _panelRoot = panelRoot;
            _speakerLabel = speakerLabel;
            _bodyLabel = bodyLabel;
            _historyLabel = historyLabel;
            _continueButton = continueButton;
            _closeButton = closeButton;
            _choiceButtons = choiceButtons;
            _choiceLabels = choiceLabels;
        }

        public void Configure(
            GameObject panelRoot,
            Text speakerLabel,
            Text bodyLabel,
            Button continueButton,
            Button closeButton,
            Button[] choiceButtons,
            Text[] choiceLabels)
        {
            Configure(
                panelRoot,
                speakerLabel,
                bodyLabel,
                null,
                continueButton,
                closeButton,
                choiceButtons,
                choiceLabels);
        }

        public void Bind(DialogueController controller)
        {
            _continueButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            _continueButton.onClick.AddListener(controller.Advance);
            _closeButton.onClick.AddListener(controller.Close);

            for (int index = 0; index < _choiceButtons.Length; index++)
            {
                int capturedIndex = index;
                _choiceButtons[index].onClick.RemoveAllListeners();
                _choiceButtons[index].onClick.AddListener(() => controller.SelectChoice(capturedIndex));
            }
        }

        public void Show(
            DialogueNode node,
            IReadOnlyList<DialogueChoice> choices,
            IReadOnlyList<string> history,
            float secondsPerCharacter)
        {
            StopReveal();
            _panelRoot.SetActive(true);
            _speakerLabel.text = node?.Speaker == null ? string.Empty : node.Speaker.DisplayName;
            _fullText = node?.Text ?? string.Empty;
            _pendingChoices = choices ?? Array.Empty<DialogueChoice>();
            if (_historyLabel != null)
            {
                _historyLabel.text = history == null || history.Count <= 1
                    ? string.Empty
                    : string.Join("\n", history);
            }

            HideChoices();
            _continueButton.gameObject.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(_continueButton.gameObject);
            if (secondsPerCharacter <= 0f || string.IsNullOrEmpty(_fullText))
            {
                _bodyLabel.text = _fullText;
                CompleteReveal();
            }
            else
            {
                _bodyLabel.text = string.Empty;
                _revealRoutine = StartCoroutine(RevealText(secondsPerCharacter));
            }
        }

        public bool RevealImmediately()
        {
            if (!IsRevealing)
            {
                return false;
            }

            StopReveal();
            _bodyLabel.text = _fullText;
            CompleteReveal();
            return true;
        }

        private IEnumerator RevealText(float secondsPerCharacter)
        {
            for (int count = 1; count <= _fullText.Length; count++)
            {
                _bodyLabel.text = _fullText.Substring(0, count);
                yield return new WaitForSecondsRealtime(secondsPerCharacter);
            }

            _revealRoutine = null;
            CompleteReveal();
        }

        private void CompleteReveal()
        {
            bool hasChoices = _pendingChoices.Count > 0;
            _continueButton.gameObject.SetActive(!hasChoices);

            for (int index = 0; index < _choiceButtons.Length; index++)
            {
                bool isVisible = index < _pendingChoices.Count;
                _choiceButtons[index].gameObject.SetActive(isVisible);
                if (isVisible && index < _choiceLabels.Length)
                {
                    _choiceLabels[index].text = _pendingChoices[index].Text;
                }
            }

            GameObject initialFocus = hasChoices ? _choiceButtons[0].gameObject : _continueButton.gameObject;
            EventSystem.current?.SetSelectedGameObject(initialFocus);
        }

        public void Hide()
        {
            StopReveal();
            _panelRoot.SetActive(false);
        }

        private void HideChoices()
        {
            foreach (Button button in _choiceButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        private void StopReveal()
        {
            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
                _revealRoutine = null;
            }
        }
    }
}
