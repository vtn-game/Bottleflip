using System;
using System.Collections;
using UnityEngine;

namespace BottleFlip.Web.Screens
{
    /// <summary>
    /// オーバーレイ画面の基底クラス
    /// 上からかぶせるUIの共通処理を提供
    /// </summary>
    public class OverlayScreen : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] protected float animationDuration = 0.3f;
        [SerializeField] protected AnimationType openAnimation = AnimationType.SlideFromTop;
        [SerializeField] protected AnimationType closeAnimation = AnimationType.SlideToTop;

        [Header("References")]
        [SerializeField] protected RectTransform contentPanel;
        [SerializeField] protected CanvasGroup canvasGroup;

        protected bool _isAnimating = false;

        public enum AnimationType
        {
            None,
            Fade,
            SlideFromTop,
            SlideToTop,
            SlideFromBottom,
            SlideToBottom,
            Scale
        }

        protected virtual void Awake()
        {
            // CanvasGroupがなければ追加
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // contentPanelがなければ自身を使用
            if (contentPanel == null)
            {
                contentPanel = GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// 画面を開く時の処理
        /// </summary>
        public virtual void OnOpen()
        {
            if (_isAnimating) return;
            StartCoroutine(PlayOpenAnimation());
        }

        /// <summary>
        /// 画面を閉じる時の処理
        /// </summary>
        public virtual void OnClose(Action onComplete = null)
        {
            if (_isAnimating)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(PlayCloseAnimation(onComplete));
        }

        protected virtual IEnumerator PlayOpenAnimation()
        {
            _isAnimating = true;

            // 初期状態を設定
            SetupOpenInitialState();

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                float easedT = EaseOutCubic(t);

                ApplyOpenAnimation(easedT);
                yield return null;
            }

            // 最終状態
            ApplyOpenAnimation(1f);
            _isAnimating = false;

            OnOpenComplete();
        }

        protected virtual IEnumerator PlayCloseAnimation(Action onComplete)
        {
            _isAnimating = true;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                float easedT = EaseInCubic(t);

                ApplyCloseAnimation(easedT);
                yield return null;
            }

            ApplyCloseAnimation(1f);
            _isAnimating = false;

            OnCloseComplete();
            onComplete?.Invoke();
        }

        protected virtual void SetupOpenInitialState()
        {
            switch (openAnimation)
            {
                case AnimationType.Fade:
                    if (canvasGroup != null) canvasGroup.alpha = 0f;
                    break;
                case AnimationType.SlideFromTop:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = new Vector2(0, Screen.height);
                    break;
                case AnimationType.SlideFromBottom:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = new Vector2(0, -Screen.height);
                    break;
                case AnimationType.Scale:
                    if (contentPanel != null)
                        contentPanel.localScale = Vector3.zero;
                    if (canvasGroup != null) canvasGroup.alpha = 0f;
                    break;
            }
        }

        protected virtual void ApplyOpenAnimation(float t)
        {
            switch (openAnimation)
            {
                case AnimationType.Fade:
                    if (canvasGroup != null) canvasGroup.alpha = t;
                    break;
                case AnimationType.SlideFromTop:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = Vector2.Lerp(
                            new Vector2(0, Screen.height),
                            Vector2.zero,
                            t);
                    break;
                case AnimationType.SlideFromBottom:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = Vector2.Lerp(
                            new Vector2(0, -Screen.height),
                            Vector2.zero,
                            t);
                    break;
                case AnimationType.Scale:
                    if (contentPanel != null)
                        contentPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    if (canvasGroup != null) canvasGroup.alpha = t;
                    break;
            }
        }

        protected virtual void ApplyCloseAnimation(float t)
        {
            switch (closeAnimation)
            {
                case AnimationType.Fade:
                    if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                    break;
                case AnimationType.SlideToTop:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = Vector2.Lerp(
                            Vector2.zero,
                            new Vector2(0, Screen.height),
                            t);
                    break;
                case AnimationType.SlideToBottom:
                    if (contentPanel != null)
                        contentPanel.anchoredPosition = Vector2.Lerp(
                            Vector2.zero,
                            new Vector2(0, -Screen.height),
                            t);
                    break;
                case AnimationType.Scale:
                    if (contentPanel != null)
                        contentPanel.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                    if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                    break;
            }
        }

        /// <summary>
        /// 開くアニメーション完了時のコールバック
        /// </summary>
        protected virtual void OnOpenComplete()
        {
            Debug.Log($"[{GetType().Name}] Open complete");
        }

        /// <summary>
        /// 閉じるアニメーション完了時のコールバック
        /// </summary>
        protected virtual void OnCloseComplete()
        {
            Debug.Log($"[{GetType().Name}] Close complete");
        }

        /// <summary>
        /// 閉じるボタン用
        /// </summary>
        public void Close()
        {
            if (Core.ScreenManager.Instance != null)
            {
                Core.ScreenManager.Instance.CloseCurrentScreen();
            }
        }

        // イージング関数
        protected float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        protected float EaseInCubic(float t) => t * t * t;
    }
}
