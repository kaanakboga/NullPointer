using System;
using System.Linq;
using NullPointer.Journal;
using NullPointer.Visual;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Journal Panel")]
    public sealed class JournalPanel : MonoBehaviour
    {
        [SerializeField] private Text _heading;
        [SerializeField] private Text _content;
        [SerializeField] private Button _casesButton;
        [SerializeField] private Button _peopleButton;
        [SerializeField] private Button _evidenceButton;
        [SerializeField] private Button _questionsButton;
        [SerializeField] private Button _timelineButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _sectionMetadata;
        [SerializeField] private CyberNoirPanelPresentation _presentation;
        [SerializeField] private RectTransform _contentPane;
        private JournalService _service;
        private Action _closed;

        public void Configure(
            Text heading,
            Text content,
            Button cases,
            Button people,
            Button evidence,
            Button questions,
            Button timeline,
            Button back)
        {
            _heading = heading;
            _content = content;
            _casesButton = cases;
            _peopleButton = people;
            _evidenceButton = evidence;
            _questionsButton = questions;
            _timelineButton = timeline;
            _backButton = back;
        }

        public void Bind(JournalService service, Action closed)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _closed = closed;
            _casesButton.onClick.AddListener(() => ShowSection(JournalSection.Cases));
            _peopleButton.onClick.AddListener(() => ShowSection(JournalSection.People));
            _evidenceButton.onClick.AddListener(ShowEvidence);
            _questionsButton.onClick.AddListener(() => ShowSection(JournalSection.Questions));
            _timelineButton.onClick.AddListener(() => ShowSection(JournalSection.Timeline));
            _backButton.onClick.AddListener(Close);
        }

        public void ConfigurePresentation(
            CyberNoirPanelPresentation presentation,
            Text sectionMetadata,
            RectTransform contentPane = null)
        {
            _presentation = presentation;
            _sectionMetadata = sectionMetadata;
            _contentPane = contentPane;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _presentation?.Reveal();
            ShowSection(JournalSection.Cases);
            EventSystem.current?.SetSelectedGameObject(_casesButton.gameObject);
        }

        public void Hide()
        {
            if (_presentation != null)
            {
                _presentation.HideAnimated();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Close()
        {
            Hide();
            _closed?.Invoke();
        }

        private void ShowSection(JournalSection section)
        {
            _heading.text = section switch
            {
                JournalSection.Cases => "DOSYALAR",
                JournalSection.People => "KİŞİLER",
                JournalSection.Questions => "SORULAR",
                JournalSection.Timeline => "ZAMAN ÇİZELGESİ",
                _ => section.ToString()
            };
            var entries = _service.GetEntries(section);
            ApplyContentLayout(section == JournalSection.Questions || section == JournalSection.Timeline, entries.Count);
            if (_sectionMetadata != null)
            {
                _sectionMetadata.text = $"SECTION / {section.ToString().ToUpperInvariant()}\nKAYIT SAYISI / {entries.Count:00}";
            }
            _content.text = entries.Count == 0
                ? "Henüz kayıt yok."
                : string.Join("\n\n", entries.Select((entry, index) => $"{index + 1:00}  ◇  {entry.Title}\n       {entry.Body}"));
        }

        private void ShowEvidence()
        {
            _heading.text = "KANITLAR";
            var evidence = _service.GetCollectedEvidence();
            ApplyContentLayout(evidence.Count > 2, evidence.Count);
            if (_sectionMetadata != null)
            {
                _sectionMetadata.text = $"SECTION / EVIDENCE\nKAYIT SAYISI / {evidence.Count:00}";
            }
            _content.text = evidence.Count == 0
                ? "Henüz kanıt kaydedilmedi."
                : string.Join("\n\n", evidence.Select((item, index) => $"{index + 1:00}  ◆  {item.DisplayName}\n       {item.Description}"));
        }

        private void ApplyContentLayout(bool useWideLayout, int itemCount)
        {
            if (_contentPane == null)
            {
                return;
            }

            bool wide = useWideLayout || itemCount > 2;
            _contentPane.anchoredPosition = wide ? new Vector2(240f, -44f) : new Vector2(80f, -44f);
            _contentPane.sizeDelta = wide ? new Vector2(-520f, -198f) : new Vector2(-840f, -198f);
        }
    }
}
