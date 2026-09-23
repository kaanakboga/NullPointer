using System.Collections;
using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/UI Transition")]
    public sealed class UiTransition : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private bool _fade = true;
        [SerializeField] private bool _slide = true;
        [SerializeField] private bool _scale = true;
        [SerializeField] private Vector2 _entranceOffset = new(0f, -24f);
        [SerializeField] private Vector3 _entranceScale = new(0.985f, 0.985f, 1f);
        [SerializeField, Min(0.01f)] private float _duration = 0.24f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private AnimationCurve _easing = null;

        private Coroutine _routine;
        private Vector2 _restingPosition;
        private Vector3 _restingScale;
        private bool _hasRestingPose;
        private float _visibility = 1f;

        public bool IsAnimating => _routine != null;

        public void Configure(
            RectTransform target,
            CanvasGroup canvasGroup,
            bool fade,
            bool slide,
            bool scale,
            Vector2 entranceOffset,
            Vector3 entranceScale,
            float duration)
        {
            _target = target;
            _canvasGroup = canvasGroup;
            _fade = fade;
            _slide = slide;
            _scale = scale;
            _entranceOffset = entranceOffset;
            _entranceScale = entranceScale;
            _duration = Mathf.Max(0.01f, duration);
            CaptureRestingPose();
        }

        public void PlayEntrance()
        {
            gameObject.SetActive(true);
            CaptureRestingPose();
            StartTransition(true, false);
        }

        public void PlayReveal()
        {
            gameObject.SetActive(true);
            CaptureRestingPose();
            _visibility = 0f;
            ApplyPose(_visibility, true);
            StartTransition(true, false);
        }

        public void PlayExit()
        {
            CaptureRestingPose();
            StartTransition(false, true);
        }

        public void SnapVisible()
        {
            StopActiveRoutine();
            gameObject.SetActive(true);
            CaptureRestingPose();
            _visibility = 1f;
            ApplyPose(_visibility, true);
        }

        private void Awake()
        {
            CaptureRestingPose();
            _visibility = _canvasGroup != null ? Mathf.Clamp01(_canvasGroup.alpha) : 1f;
        }

        private void CaptureRestingPose()
        {
            if (_hasRestingPose)
            {
                return;
            }

            _target ??= transform as RectTransform;
            if (_target == null)
            {
                return;
            }

            _restingPosition = _target.anchoredPosition;
            _restingScale = _target.localScale;
            _hasRestingPose = true;
            _easing ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private void StartTransition(bool entering, bool deactivateAfter)
        {
            StopActiveRoutine();
            _routine = StartCoroutine(Animate(entering, deactivateAfter));
        }

        private IEnumerator Animate(bool entering, bool deactivateAfter)
        {
            float startVisibility = _visibility;
            float targetVisibility = entering ? 1f : 0f;
            float transitionDuration = _duration * Mathf.Abs(targetVisibility - startVisibility);
            if (transitionDuration <= 0.001f)
            {
                _visibility = targetVisibility;
                ApplyPose(_visibility, true);
                _routine = null;
                if (deactivateAfter)
                {
                    gameObject.SetActive(false);
                }

                yield break;
            }

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                float delta = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += Mathf.Max(0f, delta);
                float normalized = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = _easing.Evaluate(normalized);
                _visibility = Mathf.LerpUnclamped(startVisibility, targetVisibility, eased);
                ApplyPose(_visibility, true);
                yield return null;
            }

            _visibility = targetVisibility;
            ApplyPose(_visibility, true);
            _routine = null;
            if (deactivateAfter)
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyPose(float visibility, bool updateInteraction)
        {
            if (_target != null)
            {
                if (_slide)
                {
                    _target.anchoredPosition = _restingPosition + _entranceOffset * (1f - visibility);
                }

                if (_scale)
                {
                    _target.localScale = Vector3.LerpUnclamped(_entranceScale, _restingScale, visibility);
                }
            }

            if (_canvasGroup != null)
            {
                if (_fade)
                {
                    _canvasGroup.alpha = visibility;
                }

                if (updateInteraction)
                {
                    bool interactive = visibility >= 0.999f;
                    _canvasGroup.interactable = interactive;
                    _canvasGroup.blocksRaycasts = interactive;
                }
            }
        }

        private void StopActiveRoutine()
        {
            if (_routine == null)
            {
                return;
            }

            StopCoroutine(_routine);
            _routine = null;
        }

        private void OnDisable()
        {
            StopActiveRoutine();
        }
    }
}
