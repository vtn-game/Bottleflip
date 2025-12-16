using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BottleFlip.Web.Core;
using BottleFlip.Web.Features;
using BottleFlip.Network;

namespace BottleFlip.Web.Screens
{
    /// <summary>
    /// メイン画面（ボトル投げ画面）のコントローラー
    /// </summary>
    public class MainScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text coinText;
        [SerializeField] private Text bottleNameText;
        [SerializeField] private Text hintText;
        [SerializeField] private Image bottleImage;
        [SerializeField] private Button changeBottleButton;
        [SerializeField] private Button gachaButton;

        [Header("Shake Detection")]
        [SerializeField] private ShakeDetector shakeDetector;
        [SerializeField] private Image shakeIndicator;

        [Header("Debug")]
        [SerializeField] private Button debugThrowButton;

        private bool canThrow = true;

        private void Start()
        {
            SetupUI();
            SetupEvents();
        }

        private void OnDestroy()
        {
            CleanupEvents();
        }

        private void SetupUI()
        {
            // プレイヤー情報表示
            if (GameManager.Instance.LocalPlayer != null)
            {
                playerNameText.text = GameManager.Instance.LocalPlayer.playerName;
            }

            UpdateCoinDisplay(GameManager.Instance.Coins);
            UpdateBottleDisplay();

            hintText.text = "スマホを振って投げよう！";

            // ボタン設定
            changeBottleButton.onClick.AddListener(OnChangeBottleClicked);
            gachaButton.onClick.AddListener(OnGachaClicked);

            // デバッグボタン
            if (debugThrowButton != null)
            {
                debugThrowButton.onClick.AddListener(OnDebugThrowClicked);
#if !UNITY_EDITOR
                debugThrowButton.gameObject.SetActive(false);
#endif
            }
        }

        private void SetupEvents()
        {
            // コイン変更イベント
            GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;

            // 振り検知イベント
            if (shakeDetector != null)
            {
                shakeDetector.OnShakeDetected += OnShakeDetected;
                shakeDetector.OnShakeStarted += OnShakeStarted;
                shakeDetector.OnShakeEnded += OnShakeEnded;
            }

            // ネットワークイベント
            WebNetworkManager.Instance.OnThrowResult += OnThrowResult;
        }

        private void CleanupEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
            }

            if (shakeDetector != null)
            {
                shakeDetector.OnShakeDetected -= OnShakeDetected;
                shakeDetector.OnShakeStarted -= OnShakeStarted;
                shakeDetector.OnShakeEnded -= OnShakeEnded;
            }

            if (WebNetworkManager.Instance != null)
            {
                WebNetworkManager.Instance.OnThrowResult -= OnThrowResult;
            }
        }

        private void UpdateCoinDisplay(int coins)
        {
            coinText.text = $"{coins} コイン";
        }

        private void UpdateBottleDisplay()
        {
            // TODO: ボトルデータベースから情報取得
            bottleNameText.text = "ペットボトル";
        }

        // ========== イベントハンドラー ==========

        private void OnShakeStarted()
        {
            if (shakeIndicator != null)
            {
                shakeIndicator.color = Color.yellow;
            }
        }

        private void OnShakeEnded()
        {
            if (shakeIndicator != null)
            {
                shakeIndicator.color = Color.white;
            }
        }

        private void OnShakeDetected(float intensity)
        {
            if (!canThrow) return;

            ThrowBottle(intensity);
        }

        private void ThrowBottle(float intensity)
        {
            canThrow = false;

            var bottleId = GameManager.Instance.SelectedBottleId;
            WebNetworkManager.Instance.SendThrow(bottleId, intensity);

            hintText.text = "投げた！結果を待っています...";

            Debug.Log($"[MainScreen] Threw bottle: {bottleId}, intensity: {intensity:F2}");
        }

        private void OnThrowResult(ThrowResultData result)
        {
            // コメント画面に遷移
            // 結果情報を渡す
            PlayerPrefs.SetInt("LastThrowSuccess", result.success ? 1 : 0);
            PlayerPrefs.SetInt("LastThrowCoins", result.coinsEarned);

            SceneManager.LoadScene("Comment");
        }

        private void OnChangeBottleClicked()
        {
            SceneManager.LoadScene("BottleSelect");
        }

        private void OnGachaClicked()
        {
            SceneManager.LoadScene("Gacha");
        }

        private void OnDebugThrowClicked()
        {
            if (shakeDetector != null)
            {
                shakeDetector.TriggerManualShake(0.5f);
            }
        }

        /// <summary>
        /// コメント画面から戻ってきた時の再有効化
        /// </summary>
        public void EnableThrow()
        {
            canThrow = true;
            hintText.text = "スマホを振って投げよう！";
        }
    }
}
