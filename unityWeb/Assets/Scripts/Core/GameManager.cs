using System;
using UnityEngine;
using BottleFlip.Data;

namespace BottleFlip.Web.Core
{
    /// <summary>
    /// Webアプリのゲームマネージャー
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private WebNetworkManager networkManager;

        // プレイヤー情報
        public LocalPlayerData LocalPlayer { get; private set; }
        public int Coins { get; private set; }
        public string SelectedBottleId { get; private set; }

        // イベント
        public event Action<int> OnCoinsChanged;
        public event Action<string> OnBottleChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLocalData();
        }

        private void LoadLocalData()
        {
            LocalPlayer = LocalPlayerData.Load();
            if (LocalPlayer != null)
            {
                SelectedBottleId = LocalPlayer.selectedBottleId ?? "B001";
                Debug.Log($"[GameManager] Loaded player: {LocalPlayer.playerName}");
            }
        }

        /// <summary>
        /// 初回プレイヤー登録
        /// </summary>
        public void RegisterPlayer(string playerName)
        {
            LocalPlayer = new LocalPlayerData
            {
                playerId = Guid.NewGuid().ToString(),
                playerName = playerName,
                selectedBottleId = "B001"
            };
            LocalPlayer.Save();

            Coins = 100; // 初回ボーナス
            SelectedBottleId = "B001";

            Debug.Log($"[GameManager] Registered new player: {playerName}");
        }

        /// <summary>
        /// ボトル変更
        /// </summary>
        public void SelectBottle(string bottleId)
        {
            SelectedBottleId = bottleId;
            if (LocalPlayer != null)
            {
                LocalPlayer.selectedBottleId = bottleId;
                LocalPlayer.Save();
            }
            OnBottleChanged?.Invoke(bottleId);
        }

        /// <summary>
        /// コイン更新
        /// </summary>
        public void SetCoins(int amount)
        {
            Coins = amount;
            OnCoinsChanged?.Invoke(Coins);
        }

        /// <summary>
        /// コイン追加
        /// </summary>
        public void AddCoins(int amount)
        {
            Coins += amount;
            OnCoinsChanged?.Invoke(Coins);
        }

        /// <summary>
        /// プレイヤー登録済みか
        /// </summary>
        public bool IsRegistered => LocalPlayer != null;
    }
}
