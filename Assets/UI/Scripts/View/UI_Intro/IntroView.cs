using System;
using UnityEngine;

namespace SH.Project.UI.Intro
{
    public enum IntroState
    {
        Idle,
        Playing,
        Completed,
        Cancelled
    }

    [DisallowMultipleComponent]
    public sealed class IntroView : MonoBehaviour
    {
        [SerializeField] private Canvas _staticCanvas;
        [SerializeField] private Canvas _dynamicCanvas;
        [SerializeField] private IntroAnimation _introAnimation;

        #region Events

        public event Action OnIntroStarted;  //인트로 화면을 표시하고 로고 연출을 시작했을 때
        public event Action OnIntroCompleted; //인트로 화면을 숨기고 로고 연출이 완료되었을 때
        public event Action OnIntroCancelled; //인트로 화면을 숨기고 로고 연출이 취소되었을 때

        #endregion

        #region Properties

        /// <summary>
        /// 인트로의 현재 실행 상태를 가져옵니다
        /// 초기 상태는 Idle 입니다
        /// </summary>
        public IntroState State { get; private set; } = IntroState.Idle;

        #endregion

        #region Public API

        /// <summary>인트로 화면을 표시하고 로고 연출을 시작합니다.</summary>
        /// <returns>재생을 시작했으면 true, 이미 재생 중이거나 실행할 수 없으면 false입니다.</returns>
        public bool Play()
        {
            if (!isActiveAndEnabled || State == IntroState.Playing) return false;
            if (_staticCanvas == null || _dynamicCanvas == null || _introAnimation == null) return false;
            if (!_introAnimation.TryPlay(HandleAnimationCompleted, HandleAnimationInterrupted)) return false;

            State = IntroState.Playing;
            SetCanvasesVisible(true);
            OnIntroStarted?.Invoke();
            return true;
        }

        /// <summary>재생 중인 인트로를 즉시 중단하고 인트로 화면을 숨깁니다.</summary>
        /// <returns>진행 중인 인트로를 취소했으면 true, 재생 중이 아니었으면 false입니다.</returns>
        public bool Cancel()
        {
            if (State != IntroState.Playing) return false;
            if (_introAnimation != null) _introAnimation.Stop();

            HandleAnimationInterrupted();
            return true;
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            Cancel();
        }

        #endregion

        #region Animation Callbacks

        private void HandleAnimationCompleted()
        {
            State = IntroState.Completed;
            SetCanvasesVisible(false);
            OnIntroCompleted?.Invoke();
        }

        private void HandleAnimationInterrupted()
        {
            State = IntroState.Cancelled;
            SetCanvasesVisible(false);
            OnIntroCancelled?.Invoke();
        }

        #endregion

        #region Visibility

        private void SetCanvasesVisible(bool visible)
        {
            if (_staticCanvas != null) _staticCanvas.enabled = visible;
            if (_dynamicCanvas != null) _dynamicCanvas.enabled = visible;
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Intro/Play")]
        private void PlayFromContextMenu()
        {
            if (!Application.isPlaying) return;
            if (!Play()) Debug.LogWarning("Intro playback did not start. Check the Inspector references and current state.", this);
        }

        [ContextMenu("Intro/Cancel")]
        private void CancelFromContextMenu()
        {
            if (Application.isPlaying) Cancel();
        }

        #endregion
#endif
    }
}
