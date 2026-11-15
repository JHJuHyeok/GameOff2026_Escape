using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.PlayerInfoHUD
{
    [DisallowMultipleComponent]
    public sealed class PlayerInfoAttributeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _gaugeFill;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private PlayerInfoAttributeAnimation _detailAnimation;

        #region Events

        internal event Action<PlayerInfoAttributeView> OnHoverEntered; //포인터가 상태 행의 요약 영역에 들어왔을 때 발생
        internal event Action<PlayerInfoAttributeView> OnHoverExited; //포인터가 상태 행의 요약 영역에서 벗어났을 때 발생

        #endregion

        #region Properties

        internal bool IsHovered { get; private set; } //포인터가 상태 행의 요약 영역에 들어왔는지 여부

        #endregion

        #region View API

        ///상태의 현재 값과 표시 문자열을 설정
        internal void SetValue(float normalizedValue, string displayValue)
        {
            if (_gaugeFill != null) _gaugeFill.fillAmount = Mathf.Clamp01(normalizedValue);
            if (_valueText != null) _valueText.text = displayValue ?? string.Empty;
        }

        //각 상태의 설명을 설정
        internal void SetDescription(string description)
        {
            if (_description != null) _description.text = description ?? string.Empty;
        }

        //각 상태의 색상을 설정
        internal void SetColors(Color nameColor, Color gaugeColor, Color valueColor)
        {
            if (_nameText != null) _nameText.color = nameColor;
            if (_gaugeFill != null) _gaugeFill.color = gaugeColor;
            if (_valueText != null) _valueText.color = valueColor;
        }

        //상태 아이콘과 표시 이름을 설정합
        internal void SetIdentity(Sprite icon, string displayName)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
            }
            if (_nameText != null) _nameText.text = displayName ?? string.Empty;
        }

        //상세 영역의 표시 상태를 설정함
        internal void SetDetailsVisible(bool visible, bool animate)
        {
            if (_detailAnimation != null) _detailAnimation.SetVisible(visible, animate);
        }

        #endregion

        #region Pointer Callbacks

        //포인트가 상태 행의 요약 영역에 들어오면 HUD 알림
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsHovered) return;
            IsHovered = true;
            OnHoverEntered?.Invoke(this);
        }

        //포인트가 상태 행의 요약 영역에서 벗어나면 HUD 알림
        public void OnPointerExit(PointerEventData eventData)
        {
            ClearHover();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            ClearHover();
        }

        #endregion

        #region Hover

        private void ClearHover()
        {
            if (!IsHovered) return;
            IsHovered = false;
            OnHoverExited?.Invoke(this);
        }

        #endregion
    }
}
