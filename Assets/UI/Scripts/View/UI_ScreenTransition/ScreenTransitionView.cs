using System;
using System.Collections;
using UnityEngine;

namespace SH.Project.UI.ScreenTransition
{
    public enum ScreenTransitionState
    {
        Idle,
        Start,
        Waiting,
        End
    }

    [DisallowMultipleComponent]
    public sealed class ScreenTransitionView : MonoBehaviour
    {
        [SerializeField] private ScreenTransitionAnimation _animation;

        #region Events

        public event Action OnTransitionStarted; //화면을 덮는 연출이 시작했을 때 발생
        public event Action OnTransitionWaiting; //화면을 완전히 덮고 열기 요청을 기다리기 시작했을 때 발생
        public event Action OnTransitionEnded; //화면 전환이 끝났을 때 발생
        public event Action OnTransitionCancelled; //진행중인 전환을 중단하고 화면을 즉시 열었을 때 발생

        #endregion

        #region Properties

        /// <summary>화면 전환의 현재 상태를 가져옵니다.</summary>
        public ScreenTransitionState State { get; private set; } = ScreenTransitionState.Idle;

        #endregion

        #region Public API

        ///화면을 덮는 연출을 시작
        public bool StartTransition()
        {
            return State == ScreenTransitionState.Idle && Close();
        }

        //화면이 덮힌 상태에서 화면을 여는 연출을 시작
        public bool EndTransition()
        {
            return State == ScreenTransitionState.Waiting && Open();
        }

        //현재 마스크 위치에서 화면을 덮는 연출을 시작
        public bool Close()
        {
            if (!isActiveAndEnabled || _animation == null || State == ScreenTransitionState.Start || State == ScreenTransitionState.Waiting) return false;
            if (!_animation.TryPlay(true, HandleCoverCompleted, HandleAnimationInterrupted)) return false;

            State = ScreenTransitionState.Start;
            OnTransitionStarted?.Invoke();
            return true;
        }

        //현재 마스크 위치에서 화면을 열고, 열리는 중이면 방향을 전환
        public bool Open()
        {
            if (!isActiveAndEnabled || _animation == null || State == ScreenTransitionState.Idle || State == ScreenTransitionState.End) return false;
            if (!_animation.TryPlay(false, HandleRevealCompleted, HandleAnimationInterrupted)) return false;

            State = ScreenTransitionState.End;
            return true;
        }

        //진행중인 전환을 중지하고 바로 덮기/열기 상태로 이동
        public bool SetCovered(bool covered)
        {
            if (!isActiveAndEnabled || _animation == null || !_animation.isActiveAndEnabled) return false;
            if (!_animation.SetCoveredImmediately(covered)) return false;

            State = covered ? ScreenTransitionState.Waiting : ScreenTransitionState.Idle;
            return true;
        }

        //진행중인 전환을 중지하고 바로 오픈
        public bool CancelTransition()
        {
            if (State == ScreenTransitionState.Idle) return false;

            State = ScreenTransitionState.Idle;

            if (_animation != null) _animation.SetCoveredImmediately(false);

            OnTransitionCancelled?.Invoke();
            return true;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            if (State == ScreenTransitionState.Idle && _animation != null) _animation.SetCoveredImmediately(false);
        }

        private void OnDisable()
        {
            CancelTransition();
        }

        #endregion

        #region Animation Callbacks

        private void HandleCoverCompleted()
        {
            State = ScreenTransitionState.Waiting;
            OnTransitionWaiting?.Invoke();
        }

        private void HandleRevealCompleted()
        {
            State = ScreenTransitionState.Idle;
            OnTransitionEnded?.Invoke();
        }

        private void HandleAnimationInterrupted()
        {
            CancelTransition();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Screen Transition/Start")]
        private void StartForTest()
        {
            if (Application.isPlaying && isActiveAndEnabled) StartCoroutine(PlayTransitionForTest(true));
        }

        [ContextMenu("Screen Transition/End")]
        private void EndForTest()
        {
            if (Application.isPlaying && isActiveAndEnabled) StartCoroutine(PlayTransitionForTest(false));
        }

        private IEnumerator PlayTransitionForTest(bool cover)
        {
            // 컨텍스트 메뉴가 에디터를 멈춘 시간이 다음 프레임의 unscaledDeltaTime에 포함됩니다.
            // 그 프레임의 DOTween 업데이트가 끝난 뒤 트윈을 만들어 메뉴 대기 시간을 제외합니다.
            yield return null;

            if (!Application.isPlaying || !isActiveAndEnabled) yield break;

            if (cover)
            {
                if (!Close()) Debug.LogWarning("화면을 덮는 연출을 시작할 수 없습니다. 현재 상태와 Inspector 참조를 확인하세요.", this);
            }
            else
            {
                if (!Open()) Debug.LogWarning("화면을 여는 연출을 시작할 수 없습니다. 현재 상태와 Inspector 참조를 확인하세요.", this);
            }
        }

        [ContextMenu("Screen Transition/Cover Immediately")]
        private void CoverImmediatelyForTest()
        {
            if (Application.isPlaying) SetCovered(true);
        }

        [ContextMenu("Screen Transition/Cancel")]
        private void CancelForTest()
        {
            if (Application.isPlaying) CancelTransition();
        }

        #endregion
#endif
    }
}
