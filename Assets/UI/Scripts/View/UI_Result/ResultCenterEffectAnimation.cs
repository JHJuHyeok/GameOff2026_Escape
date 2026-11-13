using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultCenterEffectAnimation : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;
        [SerializeField] private Sprite _standingSprite;
        [SerializeField] private Sprite _inPainSprite;
        [SerializeField] private Sprite _collapsedSprite;
        [SerializeField, Min(0f)] private float _holdDuration = 0.45f;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.25f;

        private Tween _fade;

        #region Properties

        internal bool IsConfigured => _frameImage != null; // 실루엣을 표시할 Image가 연결됐는지 여부

        #endregion

        #region Animation API

        // 실패 실루엣 3장을 순서대로 표시합니다. 마지막 이미지가 사라지면 재생을 마칩니다.
        internal IEnumerator Play()
        {
            Hide();

            yield return PlayPose(_standingSprite);
            yield return PlayPose(_inPainSprite);
            yield return PlayPose(_collapsedSprite);

            Hide();
        }

        // 현재 페이드를 끊고 이미지를 숨깁니다. 다음 재생을 위해 알파를 복구합니다.
        internal void Hide()
        {
            _fade?.Kill();
            _fade = null;

            if (_frameImage == null) return;

            _frameImage.enabled = false;
            _frameImage.sprite = null;
            SetAlpha(1f);
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            Hide();
        }

        #endregion

        #region Pose Animation

        private IEnumerator PlayPose(Sprite sprite)
        {
            if (sprite == null || !isActiveAndEnabled) yield break;

            _frameImage.sprite = sprite;
            SetAlpha(1f);
            _frameImage.enabled = true;

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, _holdDuration));

            if (!isActiveAndEnabled) yield break;

            _fade = _frameImage.DOFade(0f, Mathf.Max(0.01f, _fadeDuration))
                .SetEase(Ease.Linear).SetUpdate(UpdateType.Normal, true)
                .SetAutoKill(false).SetRecyclable(false).Play();

            yield return _fade.WaitForCompletion();
            Hide();
        }

        private void SetAlpha(float alpha)
        {
            Color color = _frameImage.color;
            color.a = alpha;
            _frameImage.color = color;
        }

        #endregion
    }
}
