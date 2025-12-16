using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BottleFlip.Network
{
    /// <summary>
    /// WebSocket接続状態
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    /// <summary>
    /// WebSocketクライアント基底クラス
    /// NativeWebSocketのラッパーとして機能
    /// </summary>
    public abstract class WebSocketClient : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] protected string serverUrl = "ws://localhost:8081";
        [SerializeField] protected float reconnectInterval = 3f;
        [SerializeField] protected int maxReconnectAttempts = 5;
        [SerializeField] protected float pingInterval = 30f;

        protected NativeWebSocket.WebSocket websocket;
        protected ConnectionState state = ConnectionState.Disconnected;
        protected int reconnectAttempts = 0;
        protected float lastPingTime;

        public ConnectionState State => state;
        public bool IsConnected => state == ConnectionState.Connected;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<string> OnMessageReceived;

        protected virtual async void Start()
        {
            await Connect();
        }

        protected virtual void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
            // Ping送信
            if (IsConnected && Time.time - lastPingTime > pingInterval)
            {
                SendPing();
                lastPingTime = Time.time;
            }
        }

        protected virtual void OnDestroy()
        {
            Disconnect();
        }

        /// <summary>
        /// サーバーに接続
        /// </summary>
        public async Task Connect()
        {
            if (state == ConnectionState.Connecting || state == ConnectionState.Connected)
            {
                return;
            }

            state = ConnectionState.Connecting;
            Debug.Log($"[WebSocket] Connecting to {serverUrl}...");

            try
            {
                websocket = new NativeWebSocket.WebSocket(serverUrl);

                websocket.OnOpen += HandleOpen;
                websocket.OnClose += HandleClose;
                websocket.OnError += HandleError;
                websocket.OnMessage += HandleMessage;

                await websocket.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebSocket] Connection failed: {e.Message}");
                state = ConnectionState.Disconnected;
                OnError?.Invoke(e.Message);
                TryReconnect();
            }
        }

        /// <summary>
        /// 切断
        /// </summary>
        public async void Disconnect()
        {
            if (websocket != null && websocket.State == NativeWebSocket.WebSocketState.Open)
            {
                await websocket.Close();
            }
            state = ConnectionState.Disconnected;
        }

        /// <summary>
        /// メッセージ送信
        /// </summary>
        public async Task Send(object message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocket] Not connected. Cannot send message.");
                return;
            }

            var json = JsonUtility.ToJson(message);
            await websocket.SendText(json);
        }

        /// <summary>
        /// JSONメッセージ送信（Dictionary用）
        /// </summary>
        public async Task SendJson(string json)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[WebSocket] Not connected. Cannot send message.");
                return;
            }

            await websocket.SendText(json);
        }

        protected virtual void HandleOpen()
        {
            Debug.Log("[WebSocket] Connected!");
            state = ConnectionState.Connected;
            reconnectAttempts = 0;
            lastPingTime = Time.time;
            OnConnected?.Invoke();
            OnConnectionEstablished();
        }

        protected virtual void HandleClose(NativeWebSocket.WebSocketCloseCode closeCode)
        {
            Debug.Log($"[WebSocket] Disconnected: {closeCode}");
            state = ConnectionState.Disconnected;
            OnDisconnected?.Invoke();
            TryReconnect();
        }

        protected virtual void HandleError(string error)
        {
            Debug.LogError($"[WebSocket] Error: {error}");
            OnError?.Invoke(error);
        }

        protected virtual void HandleMessage(byte[] data)
        {
            var message = System.Text.Encoding.UTF8.GetString(data);
            OnMessageReceived?.Invoke(message);
            ProcessMessage(message);
        }

        protected async void TryReconnect()
        {
            if (reconnectAttempts >= maxReconnectAttempts)
            {
                Debug.LogError("[WebSocket] Max reconnect attempts reached.");
                return;
            }

            state = ConnectionState.Reconnecting;
            reconnectAttempts++;

            var delay = reconnectInterval * Mathf.Pow(2, reconnectAttempts - 1);
            Debug.Log($"[WebSocket] Reconnecting in {delay}s... (attempt {reconnectAttempts}/{maxReconnectAttempts})");

            await Task.Delay((int)(delay * 1000));
            await Connect();
        }

        protected void SendPing()
        {
            var ping = new NetworkMessage { type = "ping" };
            _ = Send(ping);
        }

        /// <summary>
        /// 接続確立時の処理（サブクラスでオーバーライド）
        /// </summary>
        protected abstract void OnConnectionEstablished();

        /// <summary>
        /// メッセージ処理（サブクラスでオーバーライド）
        /// </summary>
        protected abstract void ProcessMessage(string json);
    }

    /// <summary>
    /// 基本的なネットワークメッセージ
    /// </summary>
    [Serializable]
    public class NetworkMessage
    {
        public string type;
        public long timestamp;

        public NetworkMessage()
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
