using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.MainMenu
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class MainMenuButtonAnimation : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private RectTransform _label;
        [SerializeField] private MainMenuLightAnimation _lightAnimation;
        [SerializeField] private Color _normalColor = new (0.533333f, 0.533333f, 0.560784f, 1f);
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Vector2 _hoverOffset = new (-40f, 0f);
        [SerializeField, Min(0.01f)] private float _duration = 0.15f;

        private Button _button;
        private TMP_Text _labelText;
        private Vector2 _restingPosition;
        private Sequence _sequence;
        private bool _initialized;
        private bool _isHovered;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_label == null) return;

            _labelText = _label.GetComponent<TMP_Text>();
            _restingPosition = _label.anchoredPosition;
            _initialized = _labelText != null;
        }

        private void OnEnable()
        {
            ResetImmediately();
        }

        private void OnDisable()
        {
            ResetImmediately();
        }

        private void Update()
        {
            if (_isHovered && !_button.IsInteractable())
            {
                ResetImmediately();
            }
        }

        /// <summary>포인터가 들어오면 글자와 배경의 호버 연출을 시작합니다.</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !_initialized) return;
            if (!_button.IsInteractable()) return;

            SetHovered(true);
        }

        /// <summary>포인터가 나가면 글자와 배경을 기본 상태로 되돌립니다.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            SetHovered(false);
        }

        /// <summary>진행 중인 연출을 정리하고 글자를 기본 색과 위치로 복원합니다.</summary>
        public void ResetImmediately()
        {
            _isHovered = false;
            StopTween();

            if (_lightAnimation != null)
            {
                _lightAnimation.SetHovered(this, false);
            }

            if (!_initialized) return;

            _label.anchoredPosition = _restingPosition;
            _labelText.color = _normalColor;
        }

        private void SetHovered(bool hovered)
        {
            if (_isHovered == hovered) return;

            _isHovered = hovered;

            if (_lightAnimation != null)
            {
                _lightAnimation.SetHovered(this, hovered);
            }

            AnimateVisual(hovered);
        }

        private void AnimateVisual(bool hovered)
        {
            StopTween();

            if (!_initialized) return;

            Vector2 targetPosition = _restingPosition;
            Color targetColor = _normalColor;

            if (hovered)
            {
                targetPosition += _hoverOffset;
                targetColor = _hoverColor;
            }

            float duration = Mathf.Max(0.01f, _duration);

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.Join(_label.DOAnchorPos(targetPosition, duration));
            sequence.Join(_labelText.DOColor(targetColor, duration));
            sequence.SetEase(Ease.OutQuad);
            sequence.SetUpdate(true);
            sequence.OnKill(() =>
            {
                if (_sequence == sequence) _sequence = null;
            });

            sequence.Play();
        }

        private void StopTween()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

#if UNITY_EDITOR
        [ContextMenu("Button/Test Hover")]
        private void TestHover()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            AnimateVisual(true);
        }

        [ContextMenu("Button/Reset")]
        private void TestReset()
        {
            if (!Application.isPlaying) return;
            ResetImmediately();
        }
#endif
    }
}