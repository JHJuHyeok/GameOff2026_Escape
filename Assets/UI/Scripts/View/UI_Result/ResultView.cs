using System;
using System.Collections;
using UnityEngine;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private RectTransform _resultBand;
        [SerializeField] private ResultAnimation _animation;
        [SerializeField] private ResultCenterEffectAnimation _centerEffect;
        [SerializeField] private ResultMessageView _messageView;
        [SerializeField] private ResultDetailView _detailView;
        [SerializeField] private Color _failureBandColor = new Color32(85, 33, 38, 255);
        [SerializeField] private Color _successBandColor = new Color32(225, 225, 215, 255);
        [SerializeField] private Color _failureTextColor = Color.white;
        [SerializeField] private Color _successTextColor = new Color32(24, 24, 24, 255);

        private Coroutine _presentation;
        private bool _configured;
        private bool _success;

        #region Events

        public event Action OnMessageShown; // 코멘트와 버튼의 등장 연출이 끝나 입력이 열렸을 때 발생
        public event Action OnMessageHidden; // 표시 중이던 결과 UI가 Hide()로 닫혔을 때 발생
        public event Action OnMainMenuRequested; // 실패 또는 상세 결과에서 메인으로 버튼을 눌렀을 때 발생
        public event Action OnDetailRequested; // 성공 화면에서 결과 보기 버튼을 눌렀을 때 발생

        #endregion

        #region Properties

        public bool IsOpen { get; private set; } // 결과 띠를 표시 중인지 여부. 등장 연출 중에도 true

        #endregion

        #region Public API

        // 배경 채움, 중앙 실패 효과, 메시지 순으로 표시합니다. 요청을 받았으면 true를 반환합니다.
        public bool ShowFailure(string title, string comment, string buttonLabel = "메인으로")
        {
            return Show(false, title, comment, buttonLabel);
        }

        // 배경 채움이 끝나면 성공 메시지를 표시합니다. 요청을 받았으면 true를 반환합니다.
        public bool ShowSuccess(string title, string comment, string buttonLabel = "결과 보기")
        {
            return Show(true, title, comment, buttonLabel);
        }

        // 성공 메시지 표시가 끝난 뒤 상세 데이터를 받아 상세 화면으로 전환합니다.
        // 상세 화면을 열었으면 true를 반환합니다.
        public bool ShowDetail(ResultDetailData data)
        {
            if (!isActiveAndEnabled || !IsOpen || !_success || _presentation != null ||
                !_resultBand.gameObject.activeSelf || _detailView == null || !_detailView.Show(data)) return false;

            _messageView.ResetPresentation();
            _resultBand.gameObject.SetActive(false);
            return true;
        }

        // 진행 중인 연출을 중단하고 결과 화면 전체를 즉시 숨깁니다.
        public void Hide()
        {
            if (!IsOpen)
                return;

            ResetPresentation();
            OnMessageHidden?.Invoke();
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _configured = _resultBand != null && _animation != null && _animation.IsConfigured &&
                _centerEffect != null && _centerEffect.IsConfigured && _messageView != null && _messageView.IsConfigured;

            if (!_configured)
            {
                Debug.LogError("ResultView의 Result Band, Animation, Center Effect, Message View 연결을 확인해 주세요.", this);
                return;
            }

            ResetPresentation();
            if (_detailView != null) _detailView.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_configured) return;

            _messageView.OnActionClicked += HandleAction;
            if (_detailView != null) _detailView.OnMainMenuRequested += RequestMainMenu;
        }

        private void OnDisable()
        {
            if (!_configured) return;

            _messageView.OnActionClicked -= HandleAction;
            if (_detailView != null) _detailView.OnMainMenuRequested -= RequestMainMenu;

            ResetPresentation();
        }

        #endregion

        #region Presentation

        private bool Show(bool success, string title, string comment, string buttonLabel)
        {
            if (!isActiveAndEnabled || !_configured || IsOpen || string.IsNullOrWhiteSpace(title)) return false;

            _success = success;
            _messageView.SetContent(title, comment, buttonLabel, success ? _successTextColor : _failureTextColor);
            _animation.Prepare(success ? _successBandColor : _failureBandColor);
            _resultBand.gameObject.SetActive(true);
            IsOpen = true;
            _presentation = StartCoroutine(Present(success));
            return true;
        }

        private IEnumerator Present(bool success)
        {
            IEnumerator centerEffect = success ? null : _centerEffect.Play();
            yield return _animation.Play(centerEffect, _messageView.Play());
            _presentation = null;
            OnMessageShown?.Invoke();
        }

        private void ResetPresentation()
        {
            if (_presentation != null)
            {
                StopCoroutine(_presentation);
                _presentation = null;
            }

            IsOpen = false;
            _messageView.ResetPresentation();
            _centerEffect.Hide();
            _animation.ResetPresentation();
            _resultBand.gameObject.SetActive(false);
            if (_detailView != null) _detailView.Hide();
        }

        private void HandleAction()
        {
            if (_success) OnDetailRequested?.Invoke();
            else RequestMainMenu();
        }

        private void RequestMainMenu()
        {
            OnMainMenuRequested?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("테스트/실패 표시")]
        private void DebugShowFailure()
        {
            if (Application.isPlaying) ShowFailure("존재가 사라졌습니다", "스트레스가 한계에 도달했습니다.");
        }

        [ContextMenu("테스트/성공 표시")]
        private void DebugShowSuccess()
        {
            if (Application.isPlaying) ShowSuccess("탈출에 성공했습니다", "마침내 건물 밖으로 빠져나왔습니다.");
        }

        [ContextMenu("테스트/상세 결과 표시")]
        private void DebugShowDetail()
        {
            if (Application.isPlaying) ShowDetail(new ResultDetailData(TimeSpan.FromSeconds(754), 125000));
        }

        [ContextMenu("테스트/결과 닫기")]
        private void DebugHide()
        {
            if (Application.isPlaying) Hide();
        }

        #endregion
#endif
    }
}