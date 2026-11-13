using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultMessageView : MonoBehaviour
    {
        [SerializeField] private ResultMessageAnimation _animation;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _commentText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMP_Text _buttonLabel;

        private bool _inputEnabled;

        #region Events

        internal event Action OnActionClicked; // 등장 완료 후 결과 버튼을 눌렀을 때 발생

        #endregion

        #region Properties

        internal bool IsConfigured => _animation != null && _animation.IsConfigured && _titleText != null && _commentText != null && _actionButton != null && _buttonLabel != null;

        #endregion

        #region View API

        // 결과 제목, 코멘트, 버튼 문구와 메시지의 글자색을 적용합니다.
        internal void SetContent(string title, string comment, string buttonLabel, Color textColor)
        {
            _titleText.text = title;
            _titleText.color = textColor;
            _commentText.text = comment ?? string.Empty;
            _commentText.color = textColor;
            _buttonLabel.text = buttonLabel ?? string.Empty;
        }

        // 제목, 코멘트, 버튼의 등장 연출이 끝나면 버튼 입력을 엽니다.
        internal IEnumerator Play()
        {
            yield return _animation.Play();
            _inputEnabled = true;
            _actionButton.interactable = true;
            _animation.SetFollowupInput(true);
        }

        // 버튼 입력을 잠그고 메시지 연출을 초기화합니다.
        internal void ResetPresentation()
        {
            _inputEnabled = false;
            _actionButton.interactable = false;
            _animation.ResetPresentation();
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_actionButton != null) _actionButton.onClick.AddListener(HandleAction);
        }

        private void OnDisable()
        {
            if (_actionButton != null) _actionButton.onClick.RemoveListener(HandleAction);
        }

        #endregion

        #region Input

        private void HandleAction()
        {
            if (_inputEnabled) OnActionClicked?.Invoke();
        }

        #endregion
    }
}



