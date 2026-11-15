using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultDetailAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private CanvasGroup _buttonGroup;
        [SerializeField] private ResultDetailNumberAnimation _survivalTimeAnimation;
        [SerializeField] private ResultDetailNumberAnimation _inventoryValueAnimation;
        [SerializeField, Min(0.01f)] private float _panelFadeDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _buttonFadeDuration = 0.18f;

        private Tween _fade;

        #region Properties

        internal bool IsConfigured => _panelGroup != null && _buttonGroup != null && _panelGroup != _buttonGroup &&
            _survivalTimeAnimation != null && _survivalTimeAnimation.IsConfigured && _inventoryValueAnimation != null && _inventoryValueAnimation.IsConfigured &&
            _survivalTimeAnimation != _inventoryValueAnimation;

        #endregion

        #region Animation API

        // 패널, 생존 시간, 수집 가치, 버튼 순으로 연출합니다.
        internal IEnumerator Play(string survivalTime, string inventoryValue)
        {
            ResetPresentation();
            yield return FadeIn(_panelGroup, _panelFadeDuration);
            yield return _survivalTimeAnimation.Play(survivalTime);
            yield return _inventoryValueAnimation.Play(inventoryValue);
            yield return FadeIn(_buttonGroup, _buttonFadeDuration);
        }

        // 진행 중인 페이드를 끊고 두 값과 버튼을 즉시 표시합니다.
        internal void ShowFinal(string survivalTime, string inventoryValue)
        {
            StopFade();
            _survivalTimeAnimation.SetValue(survivalTime);
            _inventoryValueAnimation.SetValue(inventoryValue);
            _panelGroup.alpha = 1f;
            _buttonGroup.alpha = 1f;
        }

        // 페이드를 끊고 패널과 버튼을 숨기며 숫자 표시를 비웁니다.
        internal void ResetPresentation()
        {
            StopFade();
            _panelGroup.alpha = 0f;
            _buttonGroup.alpha = 0f;
            _survivalTimeAnimation.SetValue(string.Empty);
            _inventoryValueAnimation.SetValue(string.Empty);
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            StopFade();
        }

        #endregion

        #region Tween

        private IEnumerator FadeIn(CanvasGroup group, float duration)
        {
            _fade = group.DOFade(1f, Mathf.Max(0.01f, duration)).SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Normal, true).SetAutoKill(false).SetRecyclable(false).Play();

            yield return _fade.WaitForCompletion();
            StopFade();
        }

        private void StopFade()
        {
            _fade?.Kill();
            _fade = null;
        }

        #endregion
    }
}