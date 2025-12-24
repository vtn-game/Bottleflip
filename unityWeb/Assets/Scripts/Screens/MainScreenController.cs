using UnityEngine;
using UnityEngine.UI;
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

        [Header("Permission")]
        [SerializeField] private GameObject permissionPanel;
        [SerializeField] private Button permissionButton;

        [Header("Table Slap")]
        [SerializeField] private Button tableSlapButton;
        [SerializeField] private Text slapCountText;
        [SerializeField] private int maxSlapCount = 3;

        [Header("Debug")]
        [SerializeField] private Button debugThrowButton;
        [SerializeField] private Text debugAccelText;

        private bool canThrow = true;
        private int _remainingSlapCount = 0;
        private bool _isWaitingResult = false;

        private void Start()
        {
            SetupUI();
            SetupEvents();
        }

        private void OnDestroy()
        {
            CleanupEvents();
        }

        private void Update()
        {
            // デバッグ用: 加速度表示
            if (debugAccelText != null && shakeDetector != null)
            {
                var accel = shakeDetector.CurrentAcceleration;
                debugAccelText.text = $"Accel: ({accel.x:F2}, {accel.y:F2}, {accel.z:F2})";
            }
        }

        private void SetupUI()
        {
            // プレイヤー情報表示
            if (GameManager.Instance != null && GameManager.Instance.LocalPlayer != null)
            {
                if (playerNameText != null)
                    playerNameText.text = GameManager.Instance.LocalPlayer.playerName;
            }

            if (GameManager.Instance != null)
                UpdateCoinDisplay(GameManager.Instance.Coins);

            UpdateBottleDisplay();

            if (hintText != null)
                hintText.text = "スマホを振って投げよう！";

            // ボタン設定
            if (changeBottleButton != null)
                changeBottleButton.onClick.AddListener(OnChangeBottleClicked);
            if (gachaButton != null)
                gachaButton.onClick.AddListener(OnGachaClicked);

            // デバッグボタン
            if (debugThrowButton != null)
            {
                debugThrowButton.onClick.AddListener(OnDebugThrowClicked);
#if !UNITY_EDITOR
                debugThrowButton.gameObject.SetActive(false);
#endif
            }

            // デバッグテキスト
            if (debugAccelText != null)
            {
#if !UNITY_EDITOR
                debugAccelText.gameObject.SetActive(false);
#endif
            }

            // パーミッションパネル（初期非表示）
            if (permissionPanel != null)
                permissionPanel.SetActive(false);

            if (permissionButton != null)
                permissionButton.onClick.AddListener(OnPermissionButtonClicked);

            // 台パンボタン
            if (tableSlapButton != null)
            {
                tableSlapButton.onClick.AddListener(OnTableSlapClicked);
                tableSlapButton.gameObject.SetActive(false); // 初期は非表示
            }

            UpdateSlapCountDisplay();
        }

        private void SetupEvents()
        {
            // コイン変更イベント
            if (GameManager.Instance != null)
                GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;

            // 振り検知イベント（加速度ベクトル付き）
            if (shakeDetector != null)
            {
                shakeDetector.OnShakeDetectedWithVector += OnShakeDetectedWithVector;
                shakeDetector.OnShakeStarted += OnShakeStarted;
                shakeDetector.OnShakeEnded += OnShakeEnded;
                shakeDetector.OnPermissionRequired += OnPermissionRequired;
                shakeDetector.OnPermissionResult += OnPermissionResultReceived;
            }

            // ネットワークイベント
            if (WebNetworkManager.Instance != null)
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
                shakeDetector.OnShakeDetectedWithVector -= OnShakeDetectedWithVector;
                shakeDetector.OnShakeStarted -= OnShakeStarted;
                shakeDetector.OnShakeEnded -= OnShakeEnded;
                shakeDetector.OnPermissionRequired -= OnPermissionRequired;
                shakeDetector.OnPermissionResult -= OnPermissionResultReceived;
            }

            if (WebNetworkManager.Instance != null)
            {
                WebNetworkManager.Instance.OnThrowResult -= OnThrowResult;
            }
        }

        private void UpdateCoinDisplay(int coins)
        {
            if (coinText != null)
                coinText.text = $"{coins} コイン";
        }

        private void UpdateBottleDisplay()
        {
            // TODO: ボトルデータベースから情報取得
            if (bottleNameText != null)
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

        /// <summary>
        /// パーミッションが必要な場合（iOS 13+）
        /// </summary>
        private void OnPermissionRequired()
        {
            if (permissionPanel != null)
            {
                permissionPanel.SetActive(true);
            }

            if (hintText != null)
            {
                hintText.text = "センサーを使用するには許可が必要です";
            }

            Debug.Log("[MainScreen] Permission required - showing permission UI");
        }

        /// <summary>
        /// パーミッションボタンクリック
        /// </summary>
        private void OnPermissionButtonClicked()
        {
            if (shakeDetector != null)
            {
                shakeDetector.RequestPermission();
            }
        }

        /// <summary>
        /// パーミッション結果受信
        /// </summary>
        private void OnPermissionResultReceived(bool granted)
        {
            if (permissionPanel != null)
            {
                permissionPanel.SetActive(false);
            }

            if (granted)
            {
                if (hintText != null)
                {
                    hintText.text = "スマホを振って投げよう！";
                }
                Debug.Log("[MainScreen] Permission granted - ready to throw");
            }
            else
            {
                if (hintText != null)
                {
                    hintText.text = "センサーが使用できません";
                }
                Debug.LogWarning("[MainScreen] Permission denied");
            }
        }

        /// <summary>
        /// 振り検知（加速度ベクトル付き）
        /// </summary>
        private void OnShakeDetectedWithVector(ShakeResult result)
        {
            if (!canThrow) return;

            // オーバーレイ画面が開いている場合は投げない
            if (ScreenManager.Instance != null && ScreenManager.Instance.HasOpenOverlay)
            {
                return;
            }

            ThrowBottle(result.Acceleration);
        }

        /// <summary>
        /// ボトル投げ（加速度ベクトル版）
        /// </summary>
        private void ThrowBottle(Vector3 acceleration)
        {
            canThrow = false;
            _isWaitingResult = true;

            // 台パン回数をリセット
            _remainingSlapCount = maxSlapCount;
            UpdateSlapCountDisplay();

            // 台パンボタンを表示
            if (tableSlapButton != null)
            {
                tableSlapButton.gameObject.SetActive(true);
                tableSlapButton.interactable = true;
            }

            var bottleId = GameManager.Instance?.SelectedBottleId ?? "B001";
            WebNetworkManager.Instance?.SendThrow(bottleId, acceleration);

            if (hintText != null)
                hintText.text = "投げた！台パンでボトルを跳ねさせろ！";

            Debug.Log($"[MainScreen] Threw bottle: {bottleId}, accel: {acceleration}");
        }

        private void OnThrowResult(ThrowResultData result)
        {
            _isWaitingResult = false;

            // 台パンボタンを非表示
            if (tableSlapButton != null)
            {
                tableSlapButton.gameObject.SetActive(false);
            }

            // 結果情報を保存
            PlayerPrefs.SetInt("LastThrowSuccess", result.success ? 1 : 0);
            PlayerPrefs.SetInt("LastThrowCoins", result.coinsEarned);

            // コメント画面をオーバーレイで表示
            ScreenManager.Instance?.OpenScreen(ScreenType.Comment);
        }

        // ========== 台パン関連 ==========

        /// <summary>
        /// 台パンボタンクリック
        /// </summary>
        private void OnTableSlapClicked()
        {
            if (!_isWaitingResult || _remainingSlapCount <= 0)
            {
                return;
            }

            // 台パン送信
            WebNetworkManager.Instance?.SendTableSlap();

            // 回数減少
            _remainingSlapCount--;
            UpdateSlapCountDisplay();

            // 回数が0になったらボタン無効化
            if (_remainingSlapCount <= 0)
            {
                if (tableSlapButton != null)
                {
                    tableSlapButton.interactable = false;
                }
            }

            Debug.Log($"[MainScreen] Table slap! Remaining: {_remainingSlapCount}");
        }

        /// <summary>
        /// 台パン回数表示を更新
        /// </summary>
        private void UpdateSlapCountDisplay()
        {
            if (slapCountText != null)
            {
                slapCountText.text = $"台パン: {_remainingSlapCount}/{maxSlapCount}";
            }
        }

        private void OnChangeBottleClicked()
        {
            ScreenManager.Instance?.OpenScreen(ScreenType.BottleSelect);
        }

        private void OnGachaClicked()
        {
            ScreenManager.Instance?.OpenScreen(ScreenType.Gacha);
        }

        private void OnDebugThrowClicked()
        {
            if (shakeDetector != null)
            {
                // ランダムな加速度ベクトルでテスト
                var randomAccel = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(2f, 5f),
                    Random.Range(-0.5f, 0.5f)
                );
                shakeDetector.TriggerManualShake(randomAccel);
            }
        }

        /// <summary>
        /// コメント画面から戻ってきた時の再有効化
        /// </summary>
        public void EnableThrow()
        {
            canThrow = true;
            if (hintText != null)
                hintText.text = "スマホを振って投げよう！";
        }
    }
}
