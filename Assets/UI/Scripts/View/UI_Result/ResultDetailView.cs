using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Result
{
    public readonly struct ResultDetailData
    {
        #region Properties

        public TimeSpan SurvivalTime { get; } // 생존 시간. 표시할 때 초 미만은 버림
        public long TotalInventoryValue { get; } // 인벤토리에 남은 아이템의 총가치

        #endregion

        #region Constructor

        public ResultDetailData(TimeSpan survivalTime, long totalInventoryValue)
        {
            SurvivalTime = survivalTime;
            TotalInventoryValue = totalInventoryValue;
        }

        #endregion
    }

    [DisallowMultipleComponent]
    public sealed class ResultDetailView : MonoBehaviour
    {
        [SerializeField] private ResultDetailAnimation _animation;
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private Button _mainMenuButton;

        private Coroutine _presentation;
        private ResultDetailData _data;
        private bool _isReady;

        #region Events

        public event Action OnShown; // 숫자와 버튼 표시가 끝나 메인 버튼 입력을 받을 수 있을 때 발생
        public event Action OnHidden; // Hide()로 패널과 입력이 숨겨졌을 때 발생
        public event Action OnMainMenuRequested; // 입력이 열린 상태에서 메인으로 버튼을 눌렀을 때 발생

        #endregion

        #region Properties

        public bool IsOpen { get; private set; } // 상세 화면 표시 중인지 여부. 등장 중에도 true
        public ResultDetailData Data => _data; // 마지막으로 적용한 상세 데이터

        internal bool IsConfigured => _animation != null && _animation.IsConfigured && _panelGroup != null && _mainMenuButton != null;

        #endregion

        #region Public API

        // 상세 화면을 열고 숫자 연출을 시작합니다. 이미 열려 있으면 false를 반환합니다.
        public bool Show(ResultDetailData data)
        {
            if (!enabled || !IsConfigured || IsOpen || (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)) return false;

            _data = data;
            gameObject.SetActive(true);
            IsOpen = true;
            SetInput(false);
            _animation.ResetPresentation();
            _presentation = StartCoroutine(Present());
            return true;
        }

        // 상세 데이터를 갱신합니다. 열려 있으면 새 값을 즉시 표시합니다.
        public void SetData(ResultDetailData data)
        {
            _data = data;
            if (!IsOpen) return;

            StopPresentation();
            _animation.ShowFinal(FormatTime(), FormatInventoryValue());
            CompletePresentation();
        }

        // 상세 연출을 중단하고 패널을 즉시 숨깁니다.
        public void Hide()
        {
            if (!IsOpen) return;

            gameObject.SetActive(false);
            OnHidden?.Invoke();
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(HandleMainMenu);
        }

        private void OnDisable()
        {
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(HandleMainMenu);

            IsOpen = false;
            StopPresentation();
            if (IsConfigured)
            {
                SetInput(false);
                _animation.ResetPresentation();
            }
        }

        #endregion

        #region Presentation

        private IEnumerator Present()
        {
            yield return null;
            yield return _animation.Play(FormatTime(), FormatInventoryValue());
            _presentation = null;
            CompletePresentation();
        }

        private void CompletePresentation()
        {
            if (_isReady)
                return;

            SetInput(true);
            OnShown?.Invoke();
        }

        private void SetInput(bool enabled)
        {
            _isReady = enabled;
            _panelGroup.interactable = enabled;
            _panelGroup.blocksRaycasts = enabled;
            _mainMenuButton.interactable = enabled;
        }

        private void StopPresentation()
        {
            if (_presentation == null) return;

            StopCoroutine(_presentation);
            _presentation = null;
        }

        private string FormatTime()
        {
            long seconds = Math.Max(0, _data.SurvivalTime.Ticks / TimeSpan.TicksPerSecond);
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", seconds / 60, seconds % 60);
        }

        private string FormatInventoryValue()
        {
            return _data.TotalInventoryValue.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void HandleMainMenu()
        {
            if (_isReady) OnMainMenuRequested?.Invoke();
        }

        #endregion
    }
}