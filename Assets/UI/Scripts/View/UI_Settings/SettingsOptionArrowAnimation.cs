using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class SettingsOptionArrowAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _label;
        [SerializeField, Min(0f)] private float _strength = 0.12f;
        [SerializeField, Min(0.01f)] private float _duration = 0.18f;
        [SerializeField, Min(1)] private int _vibrato = 4;
        [SerializeField, Range(0f, 1f)] private float _elasticity = 0.45f;

        private Button _button;
        private Vector3 _defaultScale;
        private Tween _punchTween;

        #region Unity Messages

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_label != null) _defaultScale = _label.localScale;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(PlayPunch);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlayPunch);
            ResetPunch();
        }

        #endregion

        #region Animation

        private void PlayPunch()
        {
            if (!isActiveAndEnabled || _label == null || !_button.IsInteractable()) return;

            ResetPunch();

            float strength = Mathf.Max(0f, _strength);
            Vector3 punch = new Vector3(_defaultScale.x * strength, _defaultScale.y * strength, 0f);
            Tween tween = _label.DOPunchScale(punch, Mathf.Max(0.01f, _duration),Mathf.Max(1, _vibrato), Mathf.Clamp01(_elasticity));
            _punchTween = tween;

            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_punchTween != tween) return;
                _punchTween = null;
                RestoreLabelScale();
            });
            tween.Play();
        }

        private void ResetPunch()
        {
            Tween tween = _punchTween;
            _punchTween = null;
            tween?.Kill();
            RestoreLabelScale();
        }

        private void RestoreLabelScale()
        {
            if (_label != null) _label.localScale = _defaultScale;
        }

        #endregion
    }
}
