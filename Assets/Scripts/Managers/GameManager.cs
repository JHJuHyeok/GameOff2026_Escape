using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainSystem
{
    /// <summary>
    /// 게임 상태(Title / Playing / Paused / GameOver / Clear) 관리.
    /// - 씬 이름으로 Title / Playing 상태를 자동 설정 (어느 씬에서 바로 시작해도 상태가 맞음)
    /// - 일시정지는 Time.timeScale로 처리
    ///
    /// 구독 예: GameManager.Instance.OnStateChanged += (prev, next) => { ... };
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region 씬 이름 목록
        /// 씬 문자열 상수. 오타 방지용.
        /// 씬 추가 시 문자열 추가
        private const string titleScene = "MainMenu";
        private const string gameScene = "Game";
        #endregion

        public GameState State { get; private set; } = GameState.Boot;
        public GameState PreviousState { get; private set; } = GameState.Boot;

        public bool IsPlaying => State == GameState.Playing;
        public bool IsPaused => State == GameState.Paused;

        /// <summary>(이전 상태, 새 상태)</summary>
        public event Action<GameState, GameState> OnStateChanged;

        protected override void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplySceneState(SceneManager.GetActiveScene().name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            // InputManager의 Pause 액션으로 일시정지 토글 (timeScale 0에서도 Update는 동작)
            if (InputManager.HasInstance && InputManager.Instance.PausePressed)
                TogglePause();
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplySceneState(scene.name);

        private void ApplySceneState(string sceneName)
        {
            if (sceneName == gameScene) SetState(GameState.Playing);
            else if (sceneName == titleScene) SetState(GameState.Title);
        }

        // ---------------- 상태 변경 ----------------

        public void SetState(GameState newState)
        {
            if (newState == State) return;

            PreviousState = State;
            State = newState;

            if (newState == GameState.Paused) Time.timeScale = 0f;
            else if (PreviousState == GameState.Paused || newState == GameState.Playing) Time.timeScale = 1f;

            OnStateChanged?.Invoke(PreviousState, State);
        }

        // ---------------- 흐름 제어 ----------------

        public void StartGame() => SceneLoader.Instance.LoadScene(gameScene);
        public void RestartGame() => SceneLoader.Instance.ReloadScene();
        public void ReturnToTitle() => SceneLoader.Instance.LoadScene(titleScene);

        public void Pause() { if (State == GameState.Playing) SetState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }

        public void TogglePause()
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        public void GameOver() { if (State == GameState.Playing) SetState(GameState.GameOver); }
        public void Clear() { if (State == GameState.Playing) SetState(GameState.Clear); }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // WebGL에서는 동작하지 않음
#endif
        }
    }
}
