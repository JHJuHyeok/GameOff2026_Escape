using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    public sealed class SettingsNavigationView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _mainButton;

        #region Events

        internal event Action OnBackClicked; //Back 버튼 클릭 이벤트
        internal event Action OnMainClicked; //main 버튼 클릭 이벤트

        #endregion

        #region Properties

        //참조 확인 용 프로퍼티
        internal bool IsConfigured => _backButton != null && _mainButton != null;

        #endregion

        #region View API

        //메인 버튼을 화면에 활성화 하거나 비활성화 함
        internal void SetMainVisible(bool visible)
        {
            if (_mainButton != null) _mainButton.gameObject.SetActive(visible);
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_backButton != null) _backButton.onClick.AddListener(HandleBackClicked);
            if (_mainButton != null) _mainButton.onClick.AddListener(HandleMainClicked);
        }

        private void OnDisable()
        {
            if (_backButton != null) _backButton.onClick.RemoveListener(HandleBackClicked);
            if (_mainButton != null) _mainButton.onClick.RemoveListener(HandleMainClicked);
        }

        #endregion

        #region Input Callbacks

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }

        private void HandleMainClicked()
        {
            OnMainClicked?.Invoke();
        }

        #endregion
    }
}
