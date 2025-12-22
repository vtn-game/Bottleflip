using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BottleFlip.Web.Core;

namespace BottleFlip.Web.Screens
{
    /// <summary>
    /// ガチャ画面のコントローラー
    /// オーバーレイとしてメイン画面の上に表示
    /// </summary>
    public class GachaScreenController : OverlayScreen
    {
        [Header("Gacha UI")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text coinText;
        [SerializeField] private Text descriptionText;

        [Header("Gacha Buttons")]
        [SerializeField] private Button singleGachaButton;
        [SerializeField] private Button tenGachaButton;
        [SerializeField] private Button closeButton;

        [Header("Gacha Cost")]
        [SerializeField] private int singleGachaCost = 100;
        [SerializeField] private int tenGachaCost = 900;

        [Header("Result Display")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Image resultBottleImage;
        [SerializeField] private Text resultBottleNameText;
        [SerializeField] private Text resultRarityText;
        [SerializeField] private Button resultCloseButton;

        [Header("Animation")]
        [SerializeField] private GameObject gachaAnimationObject;
        [SerializeField] private float gachaAnimationDuration = 2.0f;

        private bool _isGachaInProgress = false;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        private void OnEnable()
        {
            UpdateCoinDisplay();
            HideResult();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
            }
        }

        private void SetupButtons()
        {
            if (singleGachaButton != null)
            {
                singleGachaButton.onClick.AddListener(OnSingleGachaClicked);
            }

            if (tenGachaButton != null)
            {
                tenGachaButton.onClick.AddListener(OnTenGachaClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (resultCloseButton != null)
            {
                resultCloseButton.onClick.AddListener(HideResult);
            }
        }

        private void UpdateCoinDisplay()
        {
            int coins = GameManager.Instance?.Coins ?? 0;
            UpdateCoinDisplay(coins);
        }

        private void UpdateCoinDisplay(int coins)
        {
            if (coinText != null)
            {
                coinText.text = $"{coins} コイン";
            }

            // ボタンの有効/無効を更新
            if (singleGachaButton != null)
            {
                singleGachaButton.interactable = coins >= singleGachaCost && !_isGachaInProgress;
            }

            if (tenGachaButton != null)
            {
                tenGachaButton.interactable = coins >= tenGachaCost && !_isGachaInProgress;
            }
        }

        private void OnSingleGachaClicked()
        {
            if (_isGachaInProgress) return;

            int coins = GameManager.Instance?.Coins ?? 0;
            if (coins < singleGachaCost)
            {
                Debug.Log("[Gacha] Not enough coins for single gacha");
                return;
            }

            StartCoroutine(PerformGacha(1, singleGachaCost));
        }

        private void OnTenGachaClicked()
        {
            if (_isGachaInProgress) return;

            int coins = GameManager.Instance?.Coins ?? 0;
            if (coins < tenGachaCost)
            {
                Debug.Log("[Gacha] Not enough coins for 10-gacha");
                return;
            }

            StartCoroutine(PerformGacha(10, tenGachaCost));
        }

        private IEnumerator PerformGacha(int count, int cost)
        {
            _isGachaInProgress = true;
            UpdateCoinDisplay();

            Debug.Log($"[Gacha] Starting gacha x{count}, cost: {cost}");

            // コイン消費
            GameManager.Instance?.AddCoins(-cost);

            // アニメーション表示
            if (gachaAnimationObject != null)
            {
                gachaAnimationObject.SetActive(true);
            }

            yield return new WaitForSeconds(gachaAnimationDuration);

            // アニメーション非表示
            if (gachaAnimationObject != null)
            {
                gachaAnimationObject.SetActive(false);
            }

            // ガチャ結果を生成（TODO: 実際のガチャロジック）
            var result = GenerateGachaResult();

            // 結果表示
            ShowResult(result);

            _isGachaInProgress = false;
            UpdateCoinDisplay();

            Debug.Log($"[Gacha] Completed - Got: {result.bottleName} ({result.rarity})");
        }

        private GachaResult GenerateGachaResult()
        {
            // TODO: 実際のガチャロジック・確率テーブルを実装
            // 仮のランダム結果
            string[] bottleNames = { "ペットボトル", "牛乳瓶", "ワインボトル", "日本酒瓶", "ビール瓶" };
            string[] rarities = { "★", "★★", "★★★", "★★★★", "★★★★★" };

            // レアリティ確率（仮）: ★70%, ★★20%, ★★★7%, ★★★★2.5%, ★★★★★0.5%
            float roll = Random.Range(0f, 100f);
            int rarityIndex;
            if (roll < 0.5f) rarityIndex = 4;
            else if (roll < 3f) rarityIndex = 3;
            else if (roll < 10f) rarityIndex = 2;
            else if (roll < 30f) rarityIndex = 1;
            else rarityIndex = 0;

            int bottleIndex = Random.Range(0, bottleNames.Length);

            return new GachaResult
            {
                bottleId = $"B{(bottleIndex + 1):D3}",
                bottleName = bottleNames[bottleIndex],
                rarity = rarities[rarityIndex],
                rarityLevel = rarityIndex + 1
            };
        }

        private void ShowResult(GachaResult result)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (resultBottleNameText != null)
            {
                resultBottleNameText.text = result.bottleName;
            }

            if (resultRarityText != null)
            {
                resultRarityText.text = result.rarity;

                // レアリティに応じた色
                resultRarityText.color = result.rarityLevel switch
                {
                    5 => new Color(1f, 0.84f, 0f),  // 金
                    4 => new Color(0.8f, 0.5f, 1f), // 紫
                    3 => new Color(0.3f, 0.7f, 1f), // 青
                    2 => new Color(0.3f, 0.9f, 0.3f), // 緑
                    _ => Color.white
                };
            }

            // TODO: ボトル画像の設定
            // if (resultBottleImage != null) { ... }

            // ボトルを獲得（TODO: インベントリシステム）
            Debug.Log($"[Gacha] Added bottle to inventory: {result.bottleId}");
        }

        private void HideResult()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }

        protected override void OnOpenComplete()
        {
            base.OnOpenComplete();
            Debug.Log("[Gacha] Screen opened");
        }

        protected override void OnCloseComplete()
        {
            base.OnCloseComplete();
            HideResult();
            Debug.Log("[Gacha] Screen closed");
        }

        /// <summary>
        /// ガチャ結果データ
        /// </summary>
        private struct GachaResult
        {
            public string bottleId;
            public string bottleName;
            public string rarity;
            public int rarityLevel;
        }
    }
}
