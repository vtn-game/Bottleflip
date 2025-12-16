# Bottle Flip - AI開発ガイド

## プロジェクト概要

カジュアルみんわい系コミュニケーションゲーム「ボトルフリップ」の開発プロジェクト。

### コンセプト
- 「ゲームはシンプル」で「みんなで一つの世界を共有」して「コミュニケーションする」
- みんなでボトルを投げて自分の「痕跡」を残す

### システム構成
- **server**: Node.js WebSocketサーバー（AWS上で稼働）
- **unityWeb**: スマホ向けWebアプリ（Unity WebGL、同サーバー上で配信）
- **unityMain**: 母艦アプリ（Unity Standalone、配信・表示用）
- **unityShared**: Unity共通ライブラリ

### 通信フロー
```
スマホ(Web) ←→ Server(Node.js) ←→ 母艦アプリ
              WebSocket           WebSocket
```

## ディレクトリ構造

```
/
├── CLAUDE.md              # このファイル（AI用ルール）
├── README.md              # プロジェクト概要・セットアップ手順
├── server/                # Node.js WebSocketサーバー
│   ├── src/
│   │   ├── main.ts
│   │   ├── config/
│   │   └── server/
│   ├── package.json
│   └── tsconfig.json
├── unityShared/           # Unity共通ライブラリ
│   └── Runtime/
│       ├── Network/       # WebSocket通信基盤
│       └── Data/          # 共通データモデル
├── unityWeb/              # Webアプリコード（Unity WebGL）
│   └── Assets/Scripts/
│       ├── Core/          # GameManager, NetworkManager
│       ├── Features/      # ShakeDetector等
│       └── Screens/       # 各画面コントローラー
├── unityMain/             # 母艦アプリコード（Unity Standalone）
│   └── Assets/Scripts/
│       ├── Core/          # GameManager, NetworkManager
│       ├── Features/      # BottleFlip, CommentDisplay等
│       └── Screens/       # 各画面コントローラー
└── spec/                  # 仕様書
    ├── unityWeb/
    ├── unityMain/
    └── shared/
```

## 開発ルール

### 全般
- コメント: 日本語OK、複雑なロジックには必ずコメント

### server（Node.jsサーバー）
- **出力先**: `/server/` ディレクトリ
- **言語**: TypeScript
- **ランタイム**: Node.js 22.x LTS
- **通信**: ws (WebSocket)、Express (HTTP)
- **ポート**: HTTP=8080, WebSocket=8081

#### 注意点
- 母艦とWebアプリの中継役として機能
- ゲームロジックは母艦側で処理、サーバーはメッセージルーティングのみ
- 状態管理は最小限に（接続管理程度）

### Unity全般
- 言語: C# (Unity 2022.3 LTS以上推奨)
- コーディング規約: Microsoft C# Coding Conventions準拠

### unityWeb（Webアプリ）
- **出力先**: `/unityWeb/` ディレクトリ
- **ビルドターゲット**: WebGL
- **UI**: Unity UI (uGUI) または UI Toolkit
- **通信**: UnityWebRequest、NativeWebSocket
- **センサー**: JavaScript経由でDeviceMotionAPI連携

#### 注意点
- WebGLはマルチスレッド非対応
- ファイルI/O制限あり（LocalStorage/IndexedDB使用）
- iOS Safariでは加速度センサーにパーミッション必要

### unityMain（母艦アプリ）
- **出力先**: `/unityMain/` ディレクトリ
- **ビルドターゲット**: Windows Standalone（優先）、Mac、Linux
- **物理演算**: Unity Physics (Rigidbody)
- **役割**: この世界で起きていることが真実（ゲームロジックの中心）

#### 注意点
- ボトルフリップの物理演算と成否判定を担当
- QRコード生成ライブラリ必要
- 配信・ストリーミング用途を想定

### unityShared（共通ライブラリ）
- **出力先**: `/unityShared/` ディレクトリ
- **Assembly Definition**: `BottleFlip.Shared`
- **依存ライブラリ**: NativeWebSocket

#### 含まれるもの
- `WebSocketClient`: WebSocket通信の基底クラス
- `Messages`: 通信メッセージ定義
- `PlayerData`, `BottleData`: 共通データモデル

### 共通
- **データ形式**: JSON (Newtonsoft.Json推奨)
- **ID生成**: GUID/UUID形式
- **日時**: UTC基準、ISO 8601形式

## 仕様書の読み方

### 画面仕様 (screens/)
- 画面IDで一意に識別（例: `WEB-SCR-001`）
- UI要素、動作仕様、遷移先を定義

### 機能仕様 (features/)
- 機能IDで一意に識別（例: `WEB-FUNC-001`）
- 技術的な実装詳細、API、処理フローを定義

### 共通仕様 (shared/)
- データモデル: 両アプリで共有するデータ構造
- 通信プロトコル: REST API、WebSocketの定義

## コード生成時の指針

### ファイル配置
```csharp
// unityWeb の場合
// /unityWeb/Assets/Scripts/Screens/MainScreen.cs
// /unityWeb/Assets/Scripts/Features/ShakeDetector.cs

// unityMain の場合
// /unityMain/Assets/Scripts/Screens/MainView.cs
// /unityMain/Assets/Scripts/Features/BottleFlipController.cs
```

### 命名規則
- クラス名: PascalCase（例: `PlayerManager`）
- メソッド名: PascalCase（例: `GetPlayerData`）
- 変数名: camelCase（例: `playerName`）
- 定数: UPPER_SNAKE_CASE（例: `MAX_PLAYERS`）
- プライベート変数: _camelCase（例: `_isConnected`）

### アーキテクチャパターン
- MVP または MVC パターン推奨
- シングルトンは最小限に（GameManager、NetworkManager程度）
- イベント駆動で疎結合に

## テスト

### 優先度
1. 通信処理（WebSocket、REST API）
2. 物理演算（ボトルフリップ判定）
3. コイン計算（獲得・消費ロジック）

### テスト方法
- Unity Test Framework使用
- モック/スタブで外部依存を分離

## よくある実装パターン

### WebSocket通信
```csharp
public class NetworkManager : MonoBehaviour
{
    private WebSocket _ws;

    public async Task ConnectAsync(string url)
    {
        _ws = new WebSocket(url);
        _ws.OnMessage += OnMessage;
        await _ws.Connect();
    }

    private void OnMessage(byte[] data)
    {
        var message = JsonConvert.DeserializeObject<Message>(
            Encoding.UTF8.GetString(data)
        );
        // イベント発火
    }
}
```

### 振り検知
```csharp
public class ShakeDetector : MonoBehaviour
{
    private const float THRESHOLD = 2.0f;

    void Update()
    {
        var accel = Input.acceleration;
        if (accel.magnitude > THRESHOLD)
        {
            OnShakeDetected?.Invoke(accel.magnitude);
        }
    }

    public event Action<float> OnShakeDetected;
}
```

## 禁止事項

- ハードコードされたIPアドレス（設定ファイル化すること）
- 同期的なネットワーク処理（async/await使用）
- Update()内での重い処理（コルーチンまたはJob System使用）
- 未使用のusingディレクティブ放置

## 参考リンク

- Unity Documentation: https://docs.unity3d.com/
- NativeWebSocket (推奨): https://github.com/endel/NativeWebSocket
- WebSocket-Sharp: https://github.com/sta/websocket-sharp
- QRCoder: https://github.com/codebude/QRCoder
- VTN-Connect (参考実装): https://github.com/vtn-team/VTN-Connect
