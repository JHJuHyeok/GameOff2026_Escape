using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    public sealed class SettingsMuteToggleView : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;

        #region Events

        internal event Action<bool> OnValueChanged; //true이면 음소거, false이면 음소거 해제

        #endregion

        #region Properties

        //참조 확인 용 프로퍼티
        internal bool IsConfigured => _toggle != null;

        #endregion

        #region View API

        //음소거 상태를 변경 이벤트 없이 토글에 표시
        internal void SetMuted(bool muted)
        {
            if (_toggle != null) _toggle.SetIsOnWithoutNotify(muted);
        }

        #endregion

        #region Unity Messages

        private void Reset()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            if (_toggle != null) _toggle.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(HandleValueChanged);
        }

        #endregion

        #region Input Callback

        private void HandleValueChanged(bool muted)
        {
            OnValueChanged?.Invoke(muted);
        }

        #endregion
    }
}
