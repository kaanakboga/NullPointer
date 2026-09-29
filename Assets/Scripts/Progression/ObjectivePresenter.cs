using System;
using System.Linq;
using System.Collections;
using NullPointer.Visual;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Progression
{
    [AddComponentMenu("Null Pointer/Progression/Objective Presenter")]
    public sealed class ObjectivePresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _label;
        [SerializeField] private Text _stateLabel;
        [SerializeField] private CyberNoirPanelPresentation _presentation;
        private ObjectiveService _service;
        private Coroutine _settleRoutine;

        public void Configure(GameObject root, Text label)
        {
            _root = root;
            _label = label;
        }

        public void ConfigurePresentation(CyberNoirPanelPresentation presentation, Text stateLabel)
        {
            _presentation = presentation;
            _stateLabel = stateLabel;
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

        public void ShowPreview(ObjectiveData objective)
        {
            _root.SetActive(objective != null);
            _label.text = objective == null ? string.Empty : objective.Title;
            if (_stateLabel != null)
            {
                _stateLabel.text = objective == null ? string.Empty : "AKTİF SORUŞTURMA";
            }

            if (objective != null)
            {
                _presentation?.SnapVisible();
            }
        }

        private void OnObjectiveChanged(ObjectiveChanged change)
        {
            Render();
            if (_root.activeSelf)
            {
                _presentation?.Reveal();
                if (_settleRoutine != null)
                {
                    StopCoroutine(_settleRoutine);
                }

                _settleRoutine = StartCoroutine(SettleUpdate());
            }
        }

        private void Render()
        {
            ObjectiveData objective = _service.GetActive().FirstOrDefault();
            _root.SetActive(objective != null);
            _label.text = objective == null ? string.Empty : objective.Title;
            if (_stateLabel != null)
            {
                _stateLabel.text = objective == null ? string.Empty : "AKTİF SORUŞTURMA / GÜNCELLENDİ";
            }
        }

        private IEnumerator SettleUpdate()
        {
            yield return new WaitForSecondsRealtime(1.1f);
            if (_stateLabel != null)
            {
                _stateLabel.text = "AKTİF SORUŞTURMA";
            }

            _settleRoutine = null;
        }

        private void OnDestroy()
        {
            if (_settleRoutine != null)
            {
                StopCoroutine(_settleRoutine);
                _settleRoutine = null;
            }

            if (_service != null)
            {
                _service.ObjectiveChanged -= OnObjectiveChanged;
            }
        }
    }
}
