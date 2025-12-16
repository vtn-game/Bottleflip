using System;
using UnityEngine;
using BottleFlip.Network;

namespace BottleFlip.Web.Core
{
    /// <summary>
    /// WebアプリのWebSocket通信マネージャー
    /// </summary>
    public class WebNetworkManager : WebSocketClient
    {
        public static WebNetworkManager Instance { get; private set; }

        // イベント
        public event Action OnRegistered;
        public event Action OnAuthenticated;
        public event Action<ThrowResultData> OnThrowResult;

        private bool isRegistered = false;
        private bool isAuthenticated = false;

        public bool IsRegistered => isRegistered;
        public bool IsAuthenticated => isAuthenticated;

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
            // クライアント種別を登録
            var registerMsg = new RegisterMessage("web");
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

                case "auth_success":
                    HandleAuthSuccess(json);
                    break;

                case "throw_result":
                    HandleThrowResult(json);
                    break;

                case "pong":
                    // Pong応答（特に処理なし）
                    break;

                default:
                    Debug.Log($"[WebNetwork] Unknown message: {messageType}");
                    break;
            }
        }

        private void HandleRegisterSuccess(string json)
        {
            isRegistered = true;
            Debug.Log("[WebNetwork] Registered as web client");
            OnRegistered?.Invoke();

            // 既存プレイヤーなら自動認証
            if (GameManager.Instance.IsRegistered)
            {
                Authenticate();
            }
        }

        private void HandleAuthSuccess(string json)
        {
            isAuthenticated = true;
            Debug.Log("[WebNetwork] Authenticated");
            OnAuthenticated?.Invoke();
        }

        private void HandleThrowResult(string json)
        {
            var msg = MessageParser.Parse<ThrowResultMessage>(json);
            Debug.Log($"[WebNetwork] Throw result: {(msg.data.success ? "SUCCESS" : "FAIL")}");

            if (msg.data.success)
            {
                GameManager.Instance.AddCoins(msg.data.coinsEarned);
            }

            OnThrowResult?.Invoke(msg.data);
        }

        /// <summary>
        /// プレイヤー認証
        /// </summary>
        public void Authenticate()
        {
            var player = GameManager.Instance.LocalPlayer;
            if (player == null)
            {
                Debug.LogWarning("[WebNetwork] No player data for authentication");
                return;
            }

            var authMsg = new AuthMessage(player.playerId, player.playerName);
            _ = Send(authMsg);
        }

        /// <summary>
        /// ボトル投げ送信
        /// </summary>
        public void SendThrow(string bottleId, float intensity)
        {
            var msg = new ThrowMessage(bottleId, intensity);
            _ = Send(msg);
            Debug.Log($"[WebNetwork] Sent throw: {bottleId}, intensity={intensity:F2}");
        }

        /// <summary>
        /// コメント送信
        /// </summary>
        public void SendComment(string text)
        {
            var msg = new CommentMessage(text);
            _ = Send(msg);
            Debug.Log($"[WebNetwork] Sent comment: {text}");
        }

        /// <summary>
        /// コメントスキップ送信
        /// </summary>
        public void SendSkipComment()
        {
            var msg = new SkipCommentMessage();
            _ = Send(msg);
            Debug.Log("[WebNetwork] Sent skip comment");
        }
    }
}
