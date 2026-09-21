using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Progression
{
    [AddComponentMenu("Null Pointer/Progression/Objective Presenter")]
    public sealed class ObjectivePresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _label;
        private ObjectiveService _service;

        public void Configure(GameObject root, Text label)
        {
            _root = root;
            _label = label;
        }

        public void Initialize(ObjectiveService service)
        {
            if (_service != null)
            {
                _service.ObjectiveChanged -= OnObjectiveChanged;
            }

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _service.ObjectiveChanged += OnObjectiveChanged;
            Render();
        }

        private void OnObjectiveChanged(ObjectiveChanged change)
        {
            Render();
        }

        private void Render()
        {
            ObjectiveData objective = _service.GetActive().FirstOrDefault();
            _root.SetActive(objective != null);
            _label.text = objective == null ? string.Empty : $"AMAÇ // {objective.Title}";
        }

        private void OnDestroy()
        {
            if (_service != null)
            {
                _service.ObjectiveChanged -= OnObjectiveChanged;
            }
        }
    }
}
