using System;
using System.Linq;
using NullPointer.Journal;
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

        public void Show()
        {
            gameObject.SetActive(true);
            ShowSection(JournalSection.Cases);
            EventSystem.current?.SetSelectedGameObject(_casesButton.gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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
            _content.text = entries.Count == 0
                ? "Henüz kayıt yok."
                : string.Join("\n\n", entries.Select(entry => $"{entry.Title}\n{entry.Body}"));
        }

        private void ShowEvidence()
        {
            _heading.text = "KANITLAR";
            var evidence = _service.GetCollectedEvidence();
            _content.text = evidence.Count == 0
                ? "Henüz kanıt kaydedilmedi."
                : string.Join("\n\n", evidence.Select(item => $"{item.DisplayName}\n{item.Description}"));
        }
    }
}
