using System;
using System.Collections.Generic;
using UnityEngine;
using BottleFlip.Data;

namespace BottleFlip.Main.Core
{
    /// <summary>
    /// 母艦アプリのゲームマネージャー
    /// この世界で起きていることが真実
    /// </summary>
    public class MainGameManager : MonoBehaviour
    {
        public static MainGameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private MainNetworkManager networkManager;
        [SerializeField] private BottleDatabase bottleDatabase;

        // 接続中プレイヤー
        private Dictionary<string, PlayerInfo> connectedPlayers = new Dictionary<string, PlayerInfo>();

        // イベント
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;
        public event Action<int> OnPlayerCountChanged;

        public int PlayerCount => connectedPlayers.Count;
        public BottleDatabase BottleDatabase => bottleDatabase;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// プレイヤー参加処理
        /// </summary>
        public void AddPlayer(string playerId, string playerName)
        {
            if (connectedPlayers.ContainsKey(playerId))
            {
                Debug.LogWarning($"[MainGame] Player already exists: {playerName}");
                return;
            }

            var playerInfo = new PlayerInfo { id = playerId, name = playerName };
            connectedPlayers[playerId] = playerInfo;

            Debug.Log($"[MainGame] Player joined: {playerName} ({PlayerCount} players)");
            OnPlayerJoined?.Invoke(playerInfo);
            OnPlayerCountChanged?.Invoke(PlayerCount);
        }

        /// <summary>
        /// プレイヤー離脱処理
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            if (connectedPlayers.TryGetValue(playerId, out var playerInfo))
            {
                connectedPlayers.Remove(playerId);
                Debug.Log($"[MainGame] Player left: {playerInfo.name} ({PlayerCount} players)");
                OnPlayerLeft?.Invoke(playerInfo);
                OnPlayerCountChanged?.Invoke(PlayerCount);
            }
        }

        /// <summary>
        /// プレイヤー情報取得
        /// </summary>
        public PlayerInfo GetPlayer(string playerId)
        {
            connectedPlayers.TryGetValue(playerId, out var info);
            return info;
        }

        /// <summary>
        /// 全プレイヤー取得
        /// </summary>
        public IEnumerable<PlayerInfo> GetAllPlayers()
        {
            return connectedPlayers.Values;
        }
    }

    /// <summary>
    /// プレイヤー情報（母艦側）
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public string id;
        public string name;
    }
}
