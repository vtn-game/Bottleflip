# Bottle Flip

カジュアルみんわい系コミュニケーションゲーム（クライアント側）

## 概要

スマホを振ってボトルを投げ、成功したらコメントを残す。
みんなで一つの世界を共有するコミュニケーションゲーム。

**このリポジトリはUnityクライアント（Web/母艦）を管理します。サーバーは別プロジェクトです。**

## システム構成

```
┌─────────────────┐     WebSocket      ┌─────────────────┐
│   スマホ(Web)   │ ←────────────────→ │    Server       │
│   unityWeb      │                    │   (Node.js)     │
│  [このリポジトリ]│                    │  [別プロジェクト] │
└─────────────────┘                    └────────┬────────┘
                                                │
                                                │ WebSocket
                                                │
                                       ┌────────┴────────┐
                                       │   母艦アプリ    │
                                       │   unityMain     │
                                       │  [このリポジトリ]│
                                       └─────────────────┘
```

## ディレクトリ構造

```
/
├── unityShared/         # Unity共通ライブラリ
│   └── Runtime/
│       ├── Network/     # WebSocket通信
│       └── Data/        # データモデル
│
├── unityWeb/            # スマホWebアプリ (Unity WebGL)
│   └── Assets/
│       └── Scripts/
│           ├── Core/
│           ├── Features/
│           └── Screens/
│
├── unityMain/           # 母艦アプリ (Unity Standalone)
│   └── Assets/
│       └── Scripts/
│           ├── Core/
│           ├── Features/
│           └── Screens/
│
└── spec/                # 仕様書
    ├── unityWeb/
    ├── unityMain/
    └── shared/          # 通信プロトコル仕様含む
```

## セットアップ

### 1. Unity共通ライブラリ

1. NativeWebSocketをインストール
   - Package Manager > Add package from git URL
   - `https://github.com/endel/NativeWebSocket.git#upm`

2. `unityShared/Runtime/Network/NativeWebSocket.cs` を削除（スタブファイル）

3. unityShared フォルダを各Unityプロジェクトの Packages にシンボリックリンク

### 2. unityWeb (WebGLアプリ)

1. Unity 2022.3 LTS以上で開く
2. Build Settings > WebGL を選択
3. Player Settings で圧縮形式を設定
4. Build

### 3. unityMain (母艦アプリ)

1. Unity 2022.3 LTS以上で開く
2. Build Settings > Windows/Mac/Linux を選択
3. Build

## 起動順序

1. サーバー起動（別プロジェクト）
2. 母艦アプリ起動（サーバーに自動接続）
3. スマホでWebアプリにアクセス

## 通信プロトコル

詳細は `spec/shared/02_communication.md` を参照。

### WebSocket ポート
- HTTP: 8080
- WebSocket: 8081

### 主要メッセージ

| メッセージ | 方向 | 説明 |
|-----------|------|------|
| register | Client→Server | クライアント登録 |
| auth | Web→Server | プレイヤー認証 |
| throw | Web→Server→Main | ボトル投げ |
| throw_result | Main→Server→Web | 結果通知 |
| comment | Web→Server→Main | コメント投稿 |

## 開発メモ

### WebGL注意点
- 加速度センサーはDeviceMotionEvent経由
- iOS 13+では明示的なパーミッション必要
- async/awaitはサポートされるがマルチスレッドは非対応

### 母艦アプリ
- この世界で起きていることが真実
- 物理演算結果に基づいて成否判定
- 配信・ストリーミング用途を想定

## ライセンス

MIT
