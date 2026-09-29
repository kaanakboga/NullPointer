using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Deduction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Deduction/Evidence Connection View")]
    public sealed class EvidenceConnectionView : MonoBehaviour
    {
        [SerializeField] private RectTransform _lineRoot;
        [SerializeField] private Image[] _linePool = Array.Empty<Image>();
        [SerializeField] private Color _lineColor = new(0.38f, 0.78f, 0.8f, 0.86f);

        private Coroutine _pulse;
        private int _activeLineCount;
        private bool _reducedEffects;

        public int ActiveLineCount => _activeLineCount;

        public void Configure(RectTransform lineRoot, Image[] linePool)
        {
            _lineRoot = lineRoot;
            _linePool = linePool ?? Array.Empty<Image>();
            Clear();
        }

        public void SetReducedEffects(bool reducedEffects)
        {
            _reducedEffects = reducedEffects;
        }

        public void Render(RectTransform[] selectedCards)
        {
            Clear();
            if (selectedCards == null || selectedCards.Length < 2 || _lineRoot == null)
            {
                return;
            }

            int lineCount = Mathf.Min(_linePool.Length, selectedCards.Length - 1);
            for (int index = 0; index < lineCount; index++)
            {
                RectTransform from = selectedCards[index];
                RectTransform to = selectedCards[index + 1];
                if (from == null || to == null || _linePool[index] == null)
                {
                    continue;
                }

                Vector2 fromPoint = ToLocal(from);
                Vector2 toPoint = ToLocal(to);
                Vector2 delta = toPoint - fromPoint;
                Image line = _linePool[index];
                RectTransform rect = line.rectTransform;
                rect.anchoredPosition = (fromPoint + toPoint) * 0.5f;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, delta.magnitude);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _reducedEffects ? 3f : 4f);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                line.color = _lineColor;
                line.gameObject.SetActive(true);
                _activeLineCount++;
            }
        }

        public void Pulse(bool success)
        {
            if (!Application.isPlaying)
            {
                Color previewColor = success
                    ? new Color(0.604f, 0.784f, 0.773f, 1f)
                    : new Color(0.722f, 0.29f, 0.337f, 0.72f);
                for (int index = 0; index < _activeLineCount; index++)
                {
                    if (_linePool[index] != null)
                    {
                        _linePool[index].color = previewColor;
                    }
                }

                return;
            }

            if (_pulse != null)
            {
                StopCoroutine(_pulse);
            }

            _pulse = StartCoroutine(PulseLines(success));
        }

        public void Clear()
        {
            _activeLineCount = 0;
            foreach (Image line in _linePool)
            {
                if (line != null)
                {
                    line.gameObject.SetActive(false);
                }
            }
        }

        private Vector2 ToLocal(RectTransform source)
        {
            Vector3 world = source.TransformPoint(source.rect.center);
            return _lineRoot.InverseTransformPoint(world);
        }

        private IEnumerator PulseLines(bool success)
        {
            float duration = _reducedEffects ? 0.12f : success ? 0.7f : 0.24f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float envelope = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                for (int index = 0; index < _activeLineCount; index++)
                {
                    if (_linePool[index] == null)
                    {
                        continue;
                    }

                    Color color = success
                        ? Color.Lerp(_lineColor, new Color(0.604f, 0.784f, 0.773f, 1f), envelope)
                        : Color.Lerp(_lineColor, new Color(0.722f, 0.29f, 0.337f, 0.72f), envelope);
                    _linePool[index].color = color;
                }

                yield return null;
            }

            for (int index = 0; index < _activeLineCount; index++)
            {
                if (_linePool[index] != null)
                {
                    _linePool[index].color = _lineColor;
                }
            }

            _pulse = null;
        }

        private void OnDisable()
        {
            if (_pulse != null)
            {
                StopCoroutine(_pulse);
                _pulse = null;
            }
        }
    }
}
