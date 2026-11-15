using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.MainHUD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class MainHUDSettingsButtonView : MonoBehaviour
    {
        private Button _button;

        #region Events

        /// <summary>인게임 HUD의 설정 버튼이 클릭됐을 때 발생합니다.</summary>
        internal event Action OnClicked;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClicked);
        }

        #endregion

        #region Button Callback

        private void HandleClicked()
        {
            OnClicked?.Invoke();
        }

        #endregion
    }
}
