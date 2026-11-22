using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuLightAnimation : MonoBehaviour
    {
        [SerializeField] private Image _lightImage;
        [SerializeField] private Color _normalColor = new (0.721569f, 0.721569f, 0.815686f, 1f);
        [SerializeField] private Color _hoverColor = Color.red;
        [SerializeField, Min(0.01f)] private float _hoverDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float _returnDuration = 0.35f;
        [SerializeField] private Ease _ease = Ease.OutSine;

        // 여러 버튼이 같은 Light를 사용하므로 호버 요청을 한곳에서 관리합니다.
        private readonly HashSet<Object> _hoverSources = new();
        private Tween _colorTween;

        private void OnEnable()
        {
            ResetImmediately();
        }

        private void OnDisable()
        {
            ResetImmediately();
        }

        /// <summary>
        /// 요청자의 호버 상태를 등록합니다.
        /// 하나 이상의 요청자가 호버 중이면 호버 색을 유지합니다.
        /// </summary>
        public void SetHovered(Object source, bool hovered)
        {
            if (source == null) return;
            if (hovered && !isActiveAndEnabled) return;

            bool wasHovered = _hoverSources.Count > 0;

            if (hovered)
            {
                _hoverSources.Add(source);
            }
            else
            {
                _hoverSources.Remove(source);
            }

            bool isHovered = _hoverSources.Count > 0;

            if (!isActiveAndEnabled || wasHovered == isHovered) return;

            Color targetColor = _normalColor;
            float duration = _returnDuration;

            if (isHovered)
            {
                targetColor = _hoverColor;
                duration = _hoverDuration;
            }

            AnimateColor(targetColor, duration);
        }

        /// <summary>모든 호버 요청과 연출을 정리하고 기본 색으로 즉시 복원합니다.</summary>
        public void ResetImmediately()
        {
            _hoverSources.Clear();
            StopTween();

            if (_lightImage != null)
            {
                _lightImage.color = _normalColor;
            }
        }

        private void AnimateColor(Color targetColor, float duration)
        {
            StopTween();

            if (_lightImage == null) return;
            Tween tween = _lightImage.DOColor(targetColor,Mathf.Max(0.01f, duration));
            _colorTween = tween;

            tween.SetEase(_ease);
            tween.SetUpdate(true);
            tween.OnKill(() =>
            {
                if (_colorTween == tween) _colorTween = null;
            });
        }

        private void StopTween()
        {
            Tween tween = _colorTween;
            _colorTween = null;
            tween?.Kill();
        }

#if UNITY_EDITOR
        [ContextMenu("Light/Test Hover")]
        private void TestHover()
        {
            if (!Application.isPlaying) return;
            SetHovered(this, true);
        }

        [ContextMenu("Light/Reset")]
        private void TestReset()
        {
            if (!Application.isPlaying) return;
            ResetImmediately();
        }
#endif
    }
}