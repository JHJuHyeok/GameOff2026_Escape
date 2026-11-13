using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Toast
{
    [DisallowMultipleComponent]
    public sealed class ToastAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _rootGroup;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.2f;

        private Tween _fade;

        #region Properties

        internal bool IsConfigured => _rootGroup != null; //연출을 위한 루트 그룹이 연결되어 있는지 나타냅니다

        #endregion

        #region Animation API

        //전체 페이드 대상의 알파값을 설정, 즉시 적용 여부를 지정할 수 있습니다
        internal void SetVisible(bool visible, bool immediate = false)
        {
            Stop();
            float alpha = visible ? 1f : 0f;
            if (immediate || !isActiveAndEnabled || _fadeDuration <= 0f)
            {
                _rootGroup.alpha = alpha;
                return;
            }

            _fade = _rootGroup.DOFade(alpha, _fadeDuration).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Normal, true)
                .SetAutoKill(true).SetRecyclable(false);
            _fade.Play();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            Stop();
        }

        #endregion

        #region Tween

        private void Stop()
        {
            _fade?.Kill();
            _fade = null;
        }

        #endregion
    }
}



