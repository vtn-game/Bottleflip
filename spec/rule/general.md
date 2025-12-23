# プロジェクト全体の設計ルール

## 概要
- 個々の詳細な設計は、code以下にクラス名と同じmdを記載し仕様化する
- 個々の仕様が無いものは、このページの情報をもとに生成する事

## アーキテクチャ

### 通信フロー
```
スマホ(Web) ←→ Server(Node.js) ←→ 母艦アプリ
              WebSocket           WebSocket
```

### 各アプリの責務

| アプリ | 責務 |
|-------|------|
| unityWeb | 入力（振り検知）、ガチャ、UI |
| Server | メッセージ中継、認証、データ管理 |
| unityMain | 物理演算、判定、表示、配信 |

### 真実の源泉
- **母艦アプリが真実**: ゲームロジックの判定は母艦が行う
- Webアプリは入力とUI担当、判定結果は受け取るだけ

## 設計原則

### シングルトンの使用
最小限に抑える。以下のみ許可:
- GameManager
- NetworkManager (WebNetworkManager / MainNetworkManager)
- ScreenManager

### イベント駆動
- コンポーネント間はイベント経由で疎結合に
- `Action<T>` または `event` を使用
- 購読解除を忘れずに（OnDestroy）

### 非同期処理
- ネットワーク処理は必ず `async/await` を使用
- WebGLではマルチスレッド非対応のため注意
