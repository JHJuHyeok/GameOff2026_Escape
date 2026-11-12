using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    public sealed class SettingsMuteToggleAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _checkmark;
        [SerializeField, Range(0.01f, 1f)] private float _startScale = 0.75f;
        [SerializeField, Min(0.01f)] private float _duration = 0.16f;

        private Toggle _toggle;
        private Vector3 _defaultScale;
        private Tween _scaleTween;

        #region Unity Messages

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();

            if (_checkmark != null) _defaultScale = _checkmark.localScale;
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(HandleValueChanged);
            ResetCheckmark();
        }

        #endregion

        #region Animation

        private void HandleValueChanged(bool isOn)
        {
            ResetCheckmark();

            if (!isOn || _checkmark == null) return;

            _checkmark.localScale = _defaultScale * _startScale;
            Tween tween = _checkmark.DOScale(_defaultScale, Mathf.Max(0.01f, _duration));
            _scaleTween = tween;

            tween.SetEase(Ease.OutBack);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_scaleTween == tween) _scaleTween = null;
            });
            tween.Play();
        }

        private void ResetCheckmark()
        {
            Tween tween = _scaleTween;
            _scaleTween = null;
            tween?.Kill();

            if (_checkmark != null) _checkmark.localScale = _defaultScale;
        }

        #endregion
    }
}
