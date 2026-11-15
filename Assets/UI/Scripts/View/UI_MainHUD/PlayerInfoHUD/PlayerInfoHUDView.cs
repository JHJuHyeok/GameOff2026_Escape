using System;
using UnityEngine;

namespace SH.Project.UI.PlayerInfoHUD
{
    public enum PlayerInfoHUDState
    {
        Collapsed,
        Expanded
    }

    public enum PlayerAttribute
    {
        Hunger,
        Thirst,
        Stamina,
        Weight,
        Stress
    }

    [DisallowMultipleComponent]
    public sealed class PlayerInfoHUDView : MonoBehaviour
    {
        [SerializeField] private PlayerInfoAttributeView _hunger;
        [SerializeField] private PlayerInfoAttributeView _thirst;
        [SerializeField] private PlayerInfoAttributeView _stamina;
        [SerializeField] private PlayerInfoAttributeView _weight;
        [SerializeField] private PlayerInfoAttributeView _stress;

        private PlayerInfoAttributeView[] _rows;
        private PlayerInfoAttributeView _hoveredRow;

        #region Events

        public event Action OnOpened; //HUD 열림 시 발생
        public event Action OnClosed; //HUD 닫힘 시 발생

        #endregion

        #region Properties

        public PlayerInfoHUDState State { get; private set; } = PlayerInfoHUDState.Collapsed; //HUD 표시 상태

        #endregion

        #region Public API

        ///HUD 표시 상태를 열림으로 설정
        public bool Open()
        {
            return SetState(PlayerInfoHUDState.Expanded);
        }

        //모든 상태의 상세 정보를 숨깁니다
        public bool Close()
        {
            return SetState(PlayerInfoHUDState.Collapsed);
        }

        //HUD 표시 상태를 설정
        public bool SetState(PlayerInfoHUDState state)
        {
            if (State == state || (state != PlayerInfoHUDState.Expanded && state != PlayerInfoHUDState.Collapsed)) return false;

            State = state;

            if (isActiveAndEnabled) RefreshDetails(animate: true);
            if (State == PlayerInfoHUDState.Expanded) OnOpened?.Invoke();
            else OnClosed?.Invoke();

            return true;
        }

        //상태의 현재 값과 표시 문자열을 설정
        public void SetAttributeValue(PlayerAttribute attribute, float normalizedValue, string displayValue)
        {
            PlayerInfoAttributeView row = FindRow(attribute);
            if (row != null) row.SetValue(normalizedValue, displayValue);
        }

        //상태의 상세 설명 문구를 갱신
        public void SetAttributeDescription(PlayerAttribute attribute, string description)
        {
            PlayerInfoAttributeView row = FindRow(attribute);
            if (row != null) row.SetDescription(description);
        }

        //상태의 색상을 설정함
        public void SetAttributeColors(PlayerAttribute attribute, Color nameColor, Color gaugeColor, Color valueColor)
        {
            PlayerInfoAttributeView row = FindRow(attribute);
            if (row != null) row.SetColors(nameColor, gaugeColor, valueColor);
        }

        //상태 아이콘과 표시 이름을 설정합
        public void SetAttributeIdentity(PlayerAttribute attribute, Sprite icon, string displayName)
        {
            PlayerInfoAttributeView row = FindRow(attribute);
            if (row != null) row.SetIdentity(icon, displayName);
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _rows = new[] { _hunger, _thirst, _stamina, _weight, _stress };
        }

        private void OnEnable()
        {
            foreach (PlayerInfoAttributeView row in _rows)
            {
                if (row == null) continue;

                row.OnHoverEntered += HandleHoverEntered;
                row.OnHoverExited += HandleHoverExited;

                if (row.IsHovered) _hoveredRow = row;
            }

            RefreshDetails(animate: false);
        }

        private void OnDisable()
        {
            foreach (PlayerInfoAttributeView row in _rows)
            {
                if (row == null) continue;

                row.OnHoverEntered -= HandleHoverEntered;
                row.OnHoverExited -= HandleHoverExited;
                row.SetDetailsVisible(visible: false, animate: false);
            }

            _hoveredRow = null;
        }

        #endregion

        #region Rows

        private PlayerInfoAttributeView FindRow(PlayerAttribute attribute)
        {
            switch (attribute)
            {
                case PlayerAttribute.Hunger: return _hunger;
                case PlayerAttribute.Thirst: return _thirst;
                case PlayerAttribute.Stamina: return _stamina;
                case PlayerAttribute.Weight: return _weight;
                case PlayerAttribute.Stress: return _stress;
                default: return null;
            }
        }

        private void RefreshDetails(bool animate, bool hideOtherRowsImmediately = false)
        {
            if (_rows == null) return;

            foreach (PlayerInfoAttributeView row in _rows)
            {
                if (row == null) continue;
                bool visible = State == PlayerInfoHUDState.Expanded || row == _hoveredRow;
                row.SetDetailsVisible(visible, animate && (visible || !hideOtherRowsImmediately));
            }
        }

        private void HandleHoverEntered(PlayerInfoAttributeView row)
        {
            _hoveredRow = row;
            if (State == PlayerInfoHUDState.Collapsed && isActiveAndEnabled) RefreshDetails(animate: true, hideOtherRowsImmediately: true);
        }

        private void HandleHoverExited(PlayerInfoAttributeView row)
        {
            if (_hoveredRow != row) return;

            _hoveredRow = null;

            if (State == PlayerInfoHUDState.Collapsed && isActiveAndEnabled) RefreshDetails(animate: true);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Player Info HUD/Open")]
        private void OpenForTest()
        {
            if (Application.isPlaying) Open();
        }

        [ContextMenu("Player Info HUD/Close")]
        private void CloseForTest()
        {
            if (Application.isPlaying) Close();
        }

        #endregion
#endif
    }
}
