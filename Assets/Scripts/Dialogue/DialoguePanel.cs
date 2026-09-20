using System;
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
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _choiceButtons = Array.Empty<Button>();
        [SerializeField] private Text[] _choiceLabels = Array.Empty<Text>();

        public void Configure(
            GameObject panelRoot,
            Text speakerLabel,
            Text bodyLabel,
            Button continueButton,
            Button closeButton,
            Button[] choiceButtons,
            Text[] choiceLabels)
        {
            _panelRoot = panelRoot;
            _speakerLabel = speakerLabel;
            _bodyLabel = bodyLabel;
            _continueButton = continueButton;
            _closeButton = closeButton;
            _choiceButtons = choiceButtons;
            _choiceLabels = choiceLabels;
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

        public void Show(DialogueNode node, System.Collections.Generic.IReadOnlyList<DialogueChoice> choices)
        {
            _panelRoot.SetActive(true);
            _speakerLabel.text = node?.Speaker == null ? string.Empty : node.Speaker.DisplayName;
            _bodyLabel.text = node?.Text ?? string.Empty;
            bool hasChoices = choices != null && choices.Count > 0;
            _continueButton.gameObject.SetActive(!hasChoices);

            for (int index = 0; index < _choiceButtons.Length; index++)
            {
                bool isVisible = choices != null && index < choices.Count;
                _choiceButtons[index].gameObject.SetActive(isVisible);
                if (isVisible && index < _choiceLabels.Length)
                {
                    _choiceLabels[index].text = choices[index].Text;
                }
            }

            GameObject initialFocus = hasChoices ? _choiceButtons[0].gameObject : _continueButton.gameObject;
            EventSystem.current?.SetSelectedGameObject(initialFocus);
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }
    }
}
