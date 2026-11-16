using System;
using UnityEngine;

namespace SH.Project.UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class InventoryUIView : MonoBehaviour
    {
        [SerializeField] private PlayerInventoryUIView _playerView;
        [SerializeField] private ContainerInventoryUIView _containerView;

        // ================================테스트 용 제거 대상
        [Header("아이템 UI 미리보기")]
        [SerializeField] private ItemData _previewItemData;
        //====================================================

        private GridInventory _previewPlayerInventory;
        private GridInventory _previewContainerInventory;

        #region Events

        public event Action OnPlayerOpened; // 플레이어 창 등장 완료
        public event Action OnPlayerClosed; // 플레이어 창 닫힘 완료
        public event Action OnContainerOpened; // 컨테이너 창 등장 완료
        public event Action OnContainerClosed; // 컨테이너 창 닫힘 완료

        #endregion

        #region Properties

        public bool IsPlayerOpen => _playerView != null && _playerView.IsOpen;
        public bool IsContainerOpen => _containerView != null && _containerView.IsOpen;

        #endregion

        #region Public API

        /// <summary>플레이어 인벤토리만 표시하고 컨테이너 창은 닫습니다.</summary>
        public bool ShowPlayer(GridInventory playerInventory)
        {
            if (!isActiveAndEnabled || _playerView == null) return false;
            if (!_playerView.Show(playerInventory)) return false;
            if (_containerView != null) _containerView.Close();

            return true;
        }

        /// <summary>플레이어와 컨테이너 인벤토리를 함께 표시합니다.</summary>
        public bool ShowContainer(GridInventory playerInventory, GridInventory containerInventory, string title, Sprite background = null)
        {
            if (!isActiveAndEnabled || _playerView == null || _containerView == null || playerInventory == null || containerInventory == null)
            {
                return false;
            }

            if (!_playerView.Show(playerInventory)) return false;
            return _containerView.Show(containerInventory, title, background);
        }

        /// <summary>컨테이너 창만 닫습니다. 플레이어 창은 유지합니다.</summary>
        public void CloseContainer()
        {
            if (_containerView != null) _containerView.Close();
        }

        /// <summary>두 창의 퇴장 연출을 함께 시작합니다.</summary>
        public void CloseAll()
        {
            if (_containerView != null) _containerView.Close();
            if (_playerView != null) _playerView.Close();
        }

        /// <summary>플레이어 창의 무게 표시를 갱신합니다.</summary>
        public void SetPlayerWeight(string displayValue)
        {
            if (_playerView != null) _playerView.SetWeight(displayValue);
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_playerView != null)
            {
                _playerView.OnOpened += HandlePlayerOpened;
                _playerView.OnClosed += HandlePlayerClosed;
            }

            if (_containerView != null)
            {
                _containerView.OnOpened += HandleContainerOpened;
                _containerView.OnClosed += HandleContainerClosed;
            }
        }

        private void OnDisable()
        {
            if (_playerView != null) _playerView.HideImmediately();
            if (_containerView != null) _containerView.HideImmediately();
            if (_playerView != null)
            {
                _playerView.OnOpened -= HandlePlayerOpened;
                _playerView.OnClosed -= HandlePlayerClosed;
            }

            if (_containerView != null)
            {
                _containerView.OnOpened -= HandleContainerOpened;
                _containerView.OnClosed -= HandleContainerClosed;
            }
        }

        #endregion

        #region Child Events

        private void HandlePlayerOpened()
        {
            OnPlayerOpened?.Invoke();
        }

        private void HandlePlayerClosed()
        {
            // 플레이어 창 없이 컨테이너만 남지 않도록 정리합니다.
            if (_containerView != null) _containerView.HideImmediately();

            OnPlayerClosed?.Invoke();
        }

        private void HandleContainerOpened()
        {
            OnContainerOpened?.Invoke();
        }

        private void HandleContainerClosed()
        {
            OnContainerClosed?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Inventory UI/Show Player")]
        private void ShowPlayerForTest()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;

            _previewPlayerInventory ??= new GridInventory(10, 10, "preview_player");

            if (!ShowPlayer(_previewPlayerInventory))
            {
                Debug.LogWarning("플레이어 View의 참조와 연출 설정을 확인해주세요.", this); return;
            }

            SetPlayerWeight("0 / 20");
        }

        [ContextMenu("Inventory UI/Show Player And Box 3x3")]
        private void ShowBoxForTest()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            _previewPlayerInventory ??= new GridInventory(10, 10, "preview_player");
            _previewContainerInventory ??= new GridInventory(6, 6, "preview_box");

            if (!ShowContainer(_previewPlayerInventory, _previewContainerInventory, "상자"))
            {
                Debug.LogWarning("두 View의 참조, 레이아웃, 연출 설정을 확인해주세요.", this); return;
            }

            SetPlayerWeight("0 / 20");
        }

        [ContextMenu("Inventory UI/Close Container")]
        private void CloseContainerForTest()
        {
            if (Application.isPlaying) CloseContainer();
        }

        [ContextMenu("Inventory UI/Close All")]
        private void CloseAllForTest()
        {
            if (Application.isPlaying) CloseAll();
        }

        [ContextMenu("Inventory UI/Show Player With Test Item")]
        private void ShowPlayerWithItemForTest()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (_previewItemData == null)
            {
                Debug.LogWarning("Preview Item Data에 테스트용 ItemData를 연결해주세요.", this); return;
            }

            // 실제 플레이어 데이터와 분리된 UI 확인용 인벤토리입니다.
            var inventory = new GridInventory(10, 10, "preview_player_with_item");
            var item = new ItemInstance(_previewItemData);

            // 기존 API로 테스트 데이터를 구성합니다.
            if (!inventory.TryPlace(item, Vector2Int.zero))
            {
                Debug.LogWarning("테스트 아이템이 10×10 격자에 들어가지 않습니다.", this); return;
            }

            if (!ShowPlayer(inventory))
            {
                Debug.LogWarning("플레이어 View의 참조와 연출 설정을 확인해주세요.", this); return;
            }

            _previewPlayerInventory = inventory;
            SetPlayerWeight($"{_previewItemData.Weight:0.#} / 20");
        }

        #endregion
#endif
    }
}