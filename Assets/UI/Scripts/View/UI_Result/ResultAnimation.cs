using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultAnimation : MonoBehaviour
    {
        [SerializeField] private Image _bandBackground;
        [SerializeField] private string _progressProperty = "_Progress";
        [SerializeField, Min(0.01f)] private float _fillDuration = 0.45f;

        private Material _originalMaterial;
        private Material _runtimeMaterial;
        private Tween _fill;
        private float _progress;
        private Color _bandColor;

        #region Properties

        internal bool IsConfigured => _bandBackground != null; // 전체 화면 배경 Image가 연결됐는지 여부

        #endregion

        #region Animation API

        // 전체 화면 배경에 결과별 색을 적용하고 채움 진행도를 0으로 되돌립니다.
        internal void Prepare(Color color)
        {
            if (_runtimeMaterial == null && !string.IsNullOrEmpty(_progressProperty))
            {
                Material material = _bandBackground.material;
                if (material != null && material.HasProperty(_progressProperty))
                {
                    _originalMaterial = material;
                    _runtimeMaterial = new Material(material);
                    _bandBackground.material = _runtimeMaterial;
                }
            }

            _bandColor = color;
            _bandBackground.color = color;
            SetProgress(0f);
            _bandBackground.enabled = true;
        }

        // 배경 채움, 선택적인 중앙 효과, 메시지 연출을 순서대로 재생합니다.
        internal IEnumerator Play(IEnumerator centerEffect, IEnumerator message)
        {
            // 에디터 메뉴로 시작할 때도 트윈은 다음 게임 프레임에서 생성합니다.
            yield return null;

            _fill = DOTween.To(() => _progress, SetProgress, 1f, Mathf.Max(0.01f, _fillDuration))
                .SetEase(Ease.Linear).SetUpdate(UpdateType.Normal, true).SetAutoKill(false).SetRecyclable(false).Play();
            yield return _fill.WaitForCompletion();
            StopFill();

            if (centerEffect != null) yield return centerEffect;
            yield return message;
        }

        // 채움 연출을 멈추고 배경을 즉시 숨깁니다.
        internal void ResetPresentation()
        {
            StopFill();
            SetProgress(0f);
            _bandBackground.enabled = false;
        }

        #endregion

        #region Unity Messages

        private void OnDestroy()
        {
            StopFill();
            if (_runtimeMaterial == null) return;
            if (_bandBackground != null) _bandBackground.material = _originalMaterial;

            Destroy(_runtimeMaterial);
        }

        #endregion

        #region Band

        private void SetProgress(float progress)
        {
            _progress = progress;
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.SetFloat(_progressProperty, progress);
                return;
            }

            // 셰이더를 연결하기 전에도 전체 순서를 확인할 수 있게 띠를 페이드합니다.
            Color color = _bandColor;
            color.a *= progress;
            _bandBackground.color = color;
        }

        private void StopFill()
        {
            _fill?.Kill();
            _fill = null;
        }

        #endregion
    }
}



