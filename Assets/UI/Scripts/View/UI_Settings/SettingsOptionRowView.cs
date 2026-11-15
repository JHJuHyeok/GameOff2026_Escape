using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    public sealed class SettingsOptionRowView : MonoBehaviour
    {
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _valueLabel;

        #region Events

        internal event Action<int> OnStepRequested; //이전 버튼 클릭 시 -1, 다음 버튼 클릭 시 1을 전달

        #endregion

        #region Properties

        //참조 확인 용 프로퍼티
        internal bool IsConfigured => _previousButton != null && _nextButton != null && _valueLabel != null;

        #endregion

        #region View API

        //선택된 옵션의 텍스트를 표시
        internal void SetText(string text)
        {
            if (_valueLabel != null) _valueLabel.text = text;
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_previousButton != null) _previousButton.onClick.AddListener(HandlePreviousClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(HandleNextClicked);
        }

        private void OnDisable()
        {
            if (_previousButton != null) _previousButton.onClick.RemoveListener(HandlePreviousClicked);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(HandleNextClicked);
        }

        #endregion

        #region Input Callbacks

        private void HandlePreviousClicked()
        {
            OnStepRequested?.Invoke(-1);
        }

        private void HandleNextClicked()
        {
            OnStepRequested?.Invoke(1);
        }

        #endregion
    }
}
