using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    public sealed class SettingsVolumeSliderView : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        #region Events

        internal event Action<float> OnValueChanged; //0~1 사이의 음량 값이 전달용 이벤트

        #endregion

        #region Properties

        //참조 확인 용 프로퍼티
        internal bool IsConfigured => _slider != null;

        #endregion

        #region View API

        //이벤트 없이 슬라이더에 음량을 표시
        internal void SetVolume(float volume)
        {
            if (_slider != null) _slider.SetValueWithoutNotify(Mathf.Clamp01(volume));
        }

        #endregion

        #region Unity Messages

        private void Reset()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            if (_slider != null) _slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            if (_slider != null) _slider.onValueChanged.RemoveListener(HandleValueChanged);
        }

        #endregion

        #region Input Callback

        private void HandleValueChanged(float volume)
        {
            OnValueChanged?.Invoke(volume);
        }

        #endregion
    }
}
