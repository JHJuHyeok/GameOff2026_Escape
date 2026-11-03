using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainSystem
{
    /// <summary>
    /// 씬 전용 UI 매니저 (Persistent = false). 각 씬의 Canvas 쪽에 배치.
    ///  1) 게임 상태에 따라 패널을 자동으로 켜고 끔 (State Panels 목록을 인스펙터에서 설정)
    ///  2) 팝업 스택 (열기 / 맨 위 팝업 닫기)
    ///
    /// UI는 게임 로직을 직접 호출하지 않고 GameManager의 이벤트만 구독한다.
    ///
    /// 인스펙터 설정 예:
    ///   HUD        → Playing
    ///   PausePanel → Paused
    ///   GameOver   → GameOver
    ///   ClearPanel → Clear
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Serializable]
        public class StatePanel
        {
            public string label;
            public GameObject panel;
            public GameState[] visibleStates;
        }

        [SerializeField] private List<StatePanel> statePanels = new List<StatePanel>();

        private readonly Stack<GameObject> _popupStack = new Stack<GameObject>();

        protected override bool Persistent => false;

        protected override void Init()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnStateChanged += HandleStateChanged;
            ApplyState(gm.State);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameManager gm = GameManager.HasInstance ? GameManager.Instance : null;
            if (gm != null) gm.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.Playing) CloseAllPopups();
            ApplyState(current);
        }

        private void ApplyState(GameState state)
        {
            foreach (StatePanel sp in statePanels)
            {
                if (sp.panel == null) continue;
                sp.panel.SetActive(Array.IndexOf(sp.visibleStates, state) >= 0);
            }
        }

        // ---------------- 이름으로 패널 켜고 끄기 ----------------

        public void SetPanelActive(string label, bool active)
        {
            foreach (StatePanel sp in statePanels)
            {
                if (sp.label == label && sp.panel != null)
                {
                    sp.panel.SetActive(active);
                    return;
                }
            }
            Debug.LogWarning($"[UIManager] '{label}' 패널을 찾을 수 없습니다.");
        }

        // ---------------- 팝업 스택 ----------------

        public void OpenPopup(GameObject popup)
        {
            if (popup == null) return;
            popup.SetActive(true);
            _popupStack.Push(popup);
        }

        /// <summary>맨 위 팝업을 닫는다. 닫을 팝업이 있었으면 true</summary>
        public bool CloseTopPopup()
        {
            while (_popupStack.Count > 0)
            {
                GameObject popup = _popupStack.Pop();
                if (popup == null) continue;
                popup.SetActive(false);
                return true;
            }
            return false;
        }

        public void CloseAllPopups()
        {
            while (CloseTopPopup()) { }
        }

        public bool HasOpenPopup => _popupStack.Count > 0;
    }
}
