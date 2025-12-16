# サーバー機能

## 機能ID
`MAIN-FUNC-004`

## 概要
母艦アプリに内蔵されるゲームサーバー。WebアプリとのHTTP/WebSocket通信を処理する。

## サーバー構成

### 技術スタック
- HTTP Server: Unity内蔵 または NativeWebSocket
- WebSocket: websocket-sharp または NativeWebSocket
- データ保存: JSON ファイル（ローカル）

### ポート構成

| ポート | 用途 |
|--------|------|
| 8080 | HTTP API |
| 8081 | WebSocket |

## 起動・停止

### 起動処理

```csharp
public class GameServer : MonoBehaviour
{
    private HttpServer httpServer;
    private WebSocketServer wsServer;

    public async Task StartServer()
    {
        // IPアドレス取得
        string localIP = GetLocalIPAddress();

        // HTTPサーバー起動
        httpServer = new HttpServer(localIP, 8080);
        httpServer.Start();

        // WebSocketサーバー起動
        wsServer = new WebSocketServer(localIP, 8081);
        wsServer.Start();

        // QRコード生成
        GenerateQRCode($"http://{localIP}:8080");

        Debug.Log($"Server started at {localIP}");
    }

    public void StopServer()
    {
        httpServer?.Stop();
        wsServer?.Stop();
    }
}
```

### 自動起動
- アプリ起動時に自動でサーバー開始
- 管理パネルで手動停止/再起動可能

## セッション管理

### プレイヤーセッション

```csharp
public class PlayerSession
{
    public string SessionId { get; set; }
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public WebSocket Connection { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}
```

### セッションライフサイクル
1. WebSocket接続 → セッション作成
2. 認証メッセージ → プレイヤー紐付け
3. アクティビティ → LastActivity更新
4. 切断 → セッション削除

### タイムアウト
- 非アクティブ: 5分でセッション無効化
- 再接続: 1分以内なら同一セッション維持

## データ永続化

### 保存データ

```
/data
├── players.json      # プレイヤーマスタ
├── bottles.json      # 所持ボトルデータ
├── gacha_state.json  # ガチャ状態
└── transactions.json # コイン履歴
```

### players.json 形式

```json
{
    "players": [
        {
            "id": "p001",
            "name": "たろう",
            "coins": 350,
            "selectedBottleId": "B003",
            "createdAt": "2024-01-01T00:00:00Z"
        }
    ]
}
```

### 保存タイミング
- データ変更時に即座保存（デバウンス: 1秒）
- アプリ終了時に強制保存

## ブロードキャスト

### 全体通知

```csharp
public void BroadcastToAll(string eventName, object data)
{
    var message = JsonConvert.SerializeObject(new {
        type = eventName,
        data = data
    });

    foreach (var session in activeSessions.Values)
    {
        session.Connection?.Send(message);
    }
}
```

### 通知イベント

| イベント | 説明 | タイミング |
|----------|------|------------|
| `player_joined` | プレイヤー参加 | 認証成功時 |
| `player_left` | プレイヤー離脱 | 切断時 |
| `bottle_thrown` | ボトル投げ | 投げ開始時 |
| `flip_result` | フリップ結果 | 判定完了時 |
| `gacha_updated` | ガチャ更新 | ガチャ実行時 |

## QRコード生成

### 生成処理

```csharp
public Texture2D GenerateQRCode(string url)
{
    // QRコード生成ライブラリ使用
    var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
    var qrCode = new QRCode(qrCodeData);

    return qrCode.GetGraphic(10);
}
```

### 表示
- メインビューの下部に常時表示
- URLテキストも併記

## エラーハンドリング

| エラー | 対応 |
|--------|------|
| ポート使用中 | 別ポートで再試行、通知表示 |
| ネットワークエラー | 再起動オプション表示 |
| データ破損 | バックアップから復元、初期化オプション |
