using System;
using System.Collections.Generic;
using UnityEngine;

namespace BottleFlip.Web.Core
{
    /// <summary>
    /// 画面の種類
    /// </summary>
    public enum ScreenType
    {
        Main,
        Gacha,
        BottleSelect,
        Comment
    }

    /// <summary>
    /// 1シーン構成での画面管理
    /// オーバーレイ画面の表示・非表示を管理
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [Header("Screen References")]
        [SerializeField] private GameObject mainScreen;
        [SerializeField] private GameObject gachaScreen;
        [SerializeField] private GameObject bottleSelectScreen;
        [SerializeField] private GameObject commentScreen;

        [Header("Overlay Settings")]
        [SerializeField] private GameObject overlayBackground;

        // 現在開いているオーバーレイ画面のスタック
        private Stack<ScreenType> _overlayStack = new Stack<ScreenType>();

        // イベント
        public event Action<ScreenType> OnScreenOpened;
        public event Action<ScreenType> OnScreenClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 初期状態: メイン画面のみ表示
            InitializeScreens();
        }

        private void InitializeScreens()
        {
            // メイン画面は常に表示
            if (mainScreen != null)
                mainScreen.SetActive(true);

            // オーバーレイ画面は非表示
            if (gachaScreen != null)
                gachaScreen.SetActive(false);
            if (bottleSelectScreen != null)
                bottleSelectScreen.SetActive(false);
            if (commentScreen != null)
                commentScreen.SetActive(false);

            // 背景も非表示
            if (overlayBackground != null)
                overlayBackground.SetActive(false);
        }

        /// <summary>
        /// オーバーレイ画面を開く
        /// </summary>
        public void OpenScreen(ScreenType screenType)
        {
            var screen = GetScreen(screenType);
            if (screen == null)
            {
                Debug.LogWarning($"[ScreenManager] Screen not found: {screenType}");
                return;
            }

            // 背景表示
            if (overlayBackground != null)
                overlayBackground.SetActive(true);

            // 画面表示
            screen.SetActive(true);
            _overlayStack.Push(screenType);

            // アニメーション開始（OverlayScreenがあれば）
            var overlayScreen = screen.GetComponent<OverlayScreen>();
            if (overlayScreen != null)
            {
                overlayScreen.OnOpen();
            }

            OnScreenOpened?.Invoke(screenType);
            Debug.Log($"[ScreenManager] Opened: {screenType}");
        }

        /// <summary>
        /// 現在のオーバーレイ画面を閉じる
        /// </summary>
        public void CloseCurrentScreen()
        {
            if (_overlayStack.Count == 0)
            {
                Debug.LogWarning("[ScreenManager] No overlay screen to close");
                return;
            }

            var screenType = _overlayStack.Pop();
            var screen = GetScreen(screenType);

            if (screen != null)
            {
                var overlayScreen = screen.GetComponent<OverlayScreen>();
                if (overlayScreen != null)
                {
                    overlayScreen.OnClose(() =>
                    {
                        screen.SetActive(false);
                        UpdateBackgroundVisibility();
                    });
                }
                else
                {
                    screen.SetActive(false);
                    UpdateBackgroundVisibility();
                }
            }

            OnScreenClosed?.Invoke(screenType);
            Debug.Log($"[ScreenManager] Closed: {screenType}");
        }

        /// <summary>
        /// 指定した画面を閉じる
        /// </summary>
        public void CloseScreen(ScreenType screenType)
        {
            var screen = GetScreen(screenType);
            if (screen == null || !screen.activeSelf) return;

            // スタックから削除
            var tempStack = new Stack<ScreenType>();
            while (_overlayStack.Count > 0)
            {
                var current = _overlayStack.Pop();
                if (current != screenType)
                {
                    tempStack.Push(current);
                }
            }
            while (tempStack.Count > 0)
            {
                _overlayStack.Push(tempStack.Pop());
            }

            // 画面を閉じる
            var overlayScreen = screen.GetComponent<OverlayScreen>();
            if (overlayScreen != null)
            {
                overlayScreen.OnClose(() =>
                {
                    screen.SetActive(false);
                    UpdateBackgroundVisibility();
                });
            }
            else
            {
                screen.SetActive(false);
                UpdateBackgroundVisibility();
            }

            OnScreenClosed?.Invoke(screenType);
            Debug.Log($"[ScreenManager] Closed: {screenType}");
        }

        /// <summary>
        /// 全てのオーバーレイ画面を閉じる
        /// </summary>
        public void CloseAllOverlays()
        {
            while (_overlayStack.Count > 0)
            {
                var screenType = _overlayStack.Pop();
                var screen = GetScreen(screenType);
                if (screen != null)
                {
                    screen.SetActive(false);
                }
                OnScreenClosed?.Invoke(screenType);
            }

            if (overlayBackground != null)
                overlayBackground.SetActive(false);

            Debug.Log("[ScreenManager] Closed all overlays");
        }

        /// <summary>
        /// 画面が開いているか確認
        /// </summary>
        public bool IsScreenOpen(ScreenType screenType)
        {
            var screen = GetScreen(screenType);
            return screen != null && screen.activeSelf;
        }

        /// <summary>
        /// オーバーレイが開いているか
        /// </summary>
        public bool HasOpenOverlay => _overlayStack.Count > 0;

        private GameObject GetScreen(ScreenType screenType)
        {
            return screenType switch
            {
                ScreenType.Main => mainScreen,
                ScreenType.Gacha => gachaScreen,
                ScreenType.BottleSelect => bottleSelectScreen,
                ScreenType.Comment => commentScreen,
                _ => null
            };
        }

        private void UpdateBackgroundVisibility()
        {
            if (overlayBackground != null)
            {
                overlayBackground.SetActive(_overlayStack.Count > 0);
            }
        }
    }
}
