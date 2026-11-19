using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Checklist
{
    [DisallowMultipleComponent]
    public sealed class ChecklistView : MonoBehaviour
    {
        public static ChecklistView Instance { get; private set; }

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _shortcutText;
        [SerializeField] private CanvasGroup _bodyGroup;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private ChecklistRuleView _ruleItemPrefab;
        [SerializeField] private ChecklistAnimation _animation;
        [SerializeField] private bool _startOpen;

        private readonly List<ChecklistRuleView> _itemViews = new();
        private bool _isAnimating;

        #region Events

        // 펼침 연출이 끝나고 본문 입력이 활성화된 뒤 발생합니다.
        public event Action OnOpened;

        // 접힘 연출이 끝나고 헤더만 남은 뒤 발생합니다.
        public event Action OnClosed;

        #endregion

        #region Properties

        // 연출 중에는 현재 요청된 목표 상태를 나타냅니다.
        public bool IsOpen { get; private set; }
        public bool IsAnimating => _isAnimating;

        #endregion

        #region Public API

        public bool Open()
        {
            return SetOpen(true);
        }

        public bool Close()
        {
            return SetOpen(false);
        }

        public bool Toggle()
        {
            return SetOpen(!IsOpen);
        }

        public void SetTitle(string title)
        {
            if (_titleText != null) _titleText.text = title ?? string.Empty;
        }

        public void SetShortcut(string shortcut)
        {
            if (_shortcutText != null) _shortcutText.text = shortcut ?? string.Empty;
        }

        // 규칙마다 개별 행을 생성하고 표시 순서대로 번호를 붙입니다.
        // 기존 내용을 교체하고 스크롤을 맨 위로 초기화합니다.
        // null을 전달하면 내용을 비웁니다.
        public bool SetItems(IEnumerable<string> contents)
        {
            if (_scrollRect == null || _scrollRect.content == null || _ruleItemPrefab == null)
            {
                Debug.LogWarning("Scroll Rect의 Content와 Rule Item Prefab을 연결해주세요.",this);
                return false;
            }

            _scrollRect.StopMovement();
            ClearItemViews();

            if (contents != null)
            {
                int number = 1;

                foreach (string content in contents)
                {
                    if (string.IsNullOrWhiteSpace(content)) continue;
                    ChecklistRuleView itemView = Instantiate(_ruleItemPrefab,_scrollRect.content,false);
                    itemView.SetContent(number, content);
                    itemView.gameObject.SetActive(true);

                    _itemViews.Add(itemView);
                    number++;
                }
            }
            RebuildLayout(true);
            return true;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            Instance = this;
            IsOpen = _startOpen;
        }

        private void OnEnable()
        {
            _isAnimating = false;
            if (_animation != null) _animation.SetExpandedImmediately(_bodyGroup, IsOpen);
            ApplyInputState();
            RebuildLayout(false);
        }

        private void OnDisable()
        {
            bool notifyClosed = IsOpen || _isAnimating;
            IsOpen = false;
            _isAnimating = false;

            if (_animation != null) _animation.SetExpandedImmediately(_bodyGroup, false);
            ApplyInputState();

            if (notifyClosed) OnClosed?.Invoke();
        }

        #endregion

        #region Presentation

        private bool SetOpen(bool open)
        {
            if (!isActiveAndEnabled || IsOpen == open) return false;

            if (_bodyGroup == null || _animation == null || !_animation.IsConfigured)
            {
                Debug.LogWarning("Body Group과 Checklist Animation의 참조를 연결해주세요.",this); return false;
            }

            IsOpen = open;
            _isAnimating = true;

            ApplyInputState();
            bool started = _animation.TryPlay(_bodyGroup,open,CompleteTransition,CompleteTransition);

            // 연출 컴포넌트가 비활성화되어 있다면 즉시 적용합니다.
            if (!started) CompleteTransition();

            return true;
        }

        private void CompleteTransition()
        {
            if (!_isAnimating || !isActiveAndEnabled) return;

            _isAnimating = false;
            _animation.SetExpandedImmediately(_bodyGroup, IsOpen);

            ApplyInputState();
            RebuildLayout(false);

            if (IsOpen) OnOpened?.Invoke();
            else OnClosed?.Invoke();
        }

        private void ApplyInputState()
        {
            bool canInteract = isActiveAndEnabled && IsOpen && !_isAnimating;
            if (_bodyGroup != null)
            {
                _bodyGroup.interactable = canInteract;
                _bodyGroup.blocksRaycasts = canInteract;
            }

            if (_scrollRect != null)
            {
                if (!canInteract) _scrollRect.StopMovement();

                // 접기 시작 시 진행 중인 스크롤 드래그도 종료합니다.
                _scrollRect.enabled = canInteract;
            }
        }

        private void ClearItemViews()
        {
            foreach (ChecklistRuleView itemView in _itemViews)
            {
                if (itemView == null) continue;

                // Destroy 처리 전에도 레이아웃에서 제외합니다.
                itemView.gameObject.SetActive(false);
                Destroy(itemView.gameObject);
            }
            _itemViews.Clear();
        }

        private void RebuildLayout(bool resetScroll)
        {
            if (_scrollRect == null || _scrollRect.content == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

            if (_scrollRect.isActiveAndEnabled)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_scrollRect.transform);
            }

            if (resetScroll)
            {
                _scrollRect.StopMovement();
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Checklist/Test Content And Open")]
        private void ShowTestContent()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            string[] samples =
            {
                "짧은 체크리스트 내용입니다.",
                "긴 내용은 영역의 너비에 맞춰 자동으로 줄바꿈되고, 항목의 높이도 내용에 맞춰 늘어납니다.",
                "내용이 표시 영역을 넘어가면 세로 스크롤로 확인할 수 있습니다."
            };

            var contents = new List<string>();
            for (int i = 0; i < 4; i++) contents.AddRange(samples);

            SetTitle("CHECK LIST");
            SetShortcut("I");

            if (SetItems(contents)) Open();
        }

        [ContextMenu("Checklist/Open")]
        private void OpenForTest()
        {
            if (Application.isPlaying) Open();
        }

        [ContextMenu("Checklist/Close")]
        private void CloseForTest()
        {
            if (Application.isPlaying) Close();
        }

        [ContextMenu("Checklist/Toggle")]
        private void ToggleForTest()
        {
            if (Application.isPlaying) Toggle();
        }

        [ContextMenu("Checklist/Clear Content")]
        private void ClearForTest()
        {
            if (Application.isPlaying) SetItems(null);
        }

        #endregion
#endif
    }
}