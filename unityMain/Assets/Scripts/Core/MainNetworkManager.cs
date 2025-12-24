using System;
using UnityEngine;
using BottleFlip.Network;

namespace BottleFlip.Main.Core
{
    /// <summary>
    /// 母艦アプリのWebSocket通信マネージャー
    /// </summary>
    public class MainNetworkManager : WebSocketClient
    {
        public static MainNetworkManager Instance { get; private set; }

        // イベント
        public event Action<GameStateData> OnGameStateReceived;
        public event Action<ThrowData> OnThrowReceived;
        public event Action<CommentData> OnCommentReceived;
        public event Action<string> OnCommentSkipped;
        public event Action<TableSlapData> OnTableSlapReceived;

        private bool isRegistered = false;
        public bool IsRegistered => isRegistered;

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

        protected override void OnConnectionEstablished()
        {
            // クライアント種別を登録（母艦）
            var registerMsg = new RegisterMessage("main");
            _ = Send(registerMsg);
        }

        protected override void ProcessMessage(string json)
        {
            var messageType = MessageParser.GetMessageType(json);

            switch (messageType)
            {
                case "register_success":
                    HandleRegisterSuccess(json);
                    break;

                case "player_joined":
                    HandlePlayerJoined(json);
                    break;

                case "player_left":
                    HandlePlayerLeft(json);
                    break;

                case "throw":
                    HandleThrow(json);
                    break;

                case "comment":
                    HandleComment(json);
                    break;

                case "skip_comment":
                    HandleSkipComment(json);
                    break;

                case "table_slap":
                    HandleTableSlap(json);
                    break;

                case "pong":
                    break;

                default:
                    Debug.Log($"[MainNetwork] Unknown message: {messageType}");
                    break;
            }
        }

        private void HandleRegisterSuccess(string json)
        {
            isRegistered = true;
            var msg = MessageParser.Parse<RegisterSuccessMessage>(json);

            Debug.Log($"[MainNetwork] Registered as main client");

            if (msg.data.state != null)
            {
                OnGameStateReceived?.Invoke(msg.data.state);

                // 既存プレイヤーを追加
                if (msg.data.state.players != null)
                {
                    foreach (var player in msg.data.state.players)
                    {
                        MainGameManager.Instance.AddPlayer(player.id, player.name);
                    }
                }
            }
        }

        private void HandlePlayerJoined(string json)
        {
            var msg = MessageParser.Parse<PlayerJoinedMessage>(json);
            MainGameManager.Instance.AddPlayer(msg.data.playerId, msg.data.playerName);
        }

        private void HandlePlayerLeft(string json)
        {
            var msg = MessageParser.Parse<PlayerLeftMessage>(json);
            MainGameManager.Instance.RemovePlayer(msg.data.playerId);
        }

        private void HandleThrow(string json)
        {
            var msg = MessageParser.Parse<ThrowMessage>(json);
            Debug.Log($"[MainNetwork] Throw received from {msg.data.playerName}");
            OnThrowReceived?.Invoke(msg.data);
        }

        private void HandleComment(string json)
        {
            var msg = MessageParser.Parse<CommentMessage>(json);
            Debug.Log($"[MainNetwork] Comment from {msg.data.playerName}: {msg.data.text}");
            OnCommentReceived?.Invoke(msg.data);
        }

        private void HandleSkipComment(string json)
        {
            var msg = MessageParser.Parse<SkipCommentMessage>(json);
            Debug.Log($"[MainNetwork] Comment skipped by {msg.data.playerId}");
            OnCommentSkipped?.Invoke(msg.data.playerId);
        }

        private void HandleTableSlap(string json)
        {
            var msg = MessageParser.Parse<TableSlapMessage>(json);
            Debug.Log($"[MainNetwork] Table slap from {msg.data.playerName}");
            OnTableSlapReceived?.Invoke(msg.data);
        }

        /// <summary>
        /// ボトルフリップ結果を送信
        /// </summary>
        public void SendThrowResult(string playerId, string playerName, bool success, int coinsEarned)
        {
            var msg = new ThrowResultMessage(playerId, playerName, success, coinsEarned);
            _ = Send(msg);
            Debug.Log($"[MainNetwork] Sent throw result: {success}");
        }
    }
}
