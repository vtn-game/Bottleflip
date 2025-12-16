// =============================================================================
// NativeWebSocket Placeholder
// =============================================================================
// 実際のプロジェクトでは、以下のいずれかを使用してください：
//
// 1. NativeWebSocket (推奨)
//    GitHub: https://github.com/endel/NativeWebSocket
//    Install: Package Manager > Add package from git URL
//    URL: https://github.com/endel/NativeWebSocket.git#upm
//
// 2. websocket-sharp
//    GitHub: https://github.com/sta/websocket-sharp
//
// このファイルはコンパイルエラーを防ぐためのスタブです。
// 実際のライブラリをインポートしたら、このファイルを削除してください。
// =============================================================================

using System;
using System.Threading.Tasks;

namespace NativeWebSocket
{
    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public enum WebSocketCloseCode
    {
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    public class WebSocket
    {
        public WebSocketState State { get; private set; } = WebSocketState.Closed;

        public event Action OnOpen;
        public event Action<string> OnError;
        public event Action<WebSocketCloseCode> OnClose;
        public event Action<byte[]> OnMessage;

        private string url;

        public WebSocket(string url)
        {
            this.url = url;
        }

        public Task Connect()
        {
            // Stub implementation
            UnityEngine.Debug.LogWarning("[NativeWebSocket] This is a stub. Please install NativeWebSocket package.");
            State = WebSocketState.Open;
            OnOpen?.Invoke();
            return Task.CompletedTask;
        }

        public Task Close()
        {
            State = WebSocketState.Closed;
            OnClose?.Invoke(WebSocketCloseCode.Normal);
            return Task.CompletedTask;
        }

        public Task Send(byte[] data)
        {
            return Task.CompletedTask;
        }

        public Task SendText(string text)
        {
            return Task.CompletedTask;
        }

        public void DispatchMessageQueue()
        {
            // Stub - 実際のライブラリではここでメッセージをディスパッチ
        }
    }
}
