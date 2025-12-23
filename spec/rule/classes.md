# クラス設計ルール

## unityWeb クラス構成

### Core/
| クラス名 | 責務 |
|---------|------|
| GameManager | プレイヤー情報・コイン・ボトル選択の管理 |
| WebNetworkManager | サーバーとのWebSocket通信 |
| ScreenManager | 画面遷移・オーバーレイ管理 |
| Bootstrap | アプリ初期化 |

### Features/
| クラス名 | 責務 |
|---------|------|
| ShakeDetector | 加速度センサーによる振り検知 |

### Screens/
| クラス名 | 責務 |
|---------|------|
| MainScreenController | メイン画面のUI制御 |
| GachaScreenController | ガチャ画面のUI制御 |
| BottleSelectScreenController | ボトル選択画面のUI制御 |
| CommentScreenController | コメント画面のUI制御 |
| OverlayScreen | オーバーレイ画面の基底クラス |

### Platform/
| クラス名 | 責務 |
|---------|------|
| WebGLDeviceMotion | JavaScript連携による加速度取得 |

---

## unityMain クラス構成

### Core/
| クラス名 | 責務 |
|---------|------|
| GameManager | ゲーム状態管理 |
| MainNetworkManager | サーバーとのWebSocket通信 |

### Features/
| クラス名 | 責務 |
|---------|------|
| BottleFlipController | ボトルの物理演算と判定 |
| CommentDisplayController | コメント表示 |
| CoinSystemController | コイン管理（TODO） |

### Screens/
| クラス名 | 責務 |
|---------|------|
| MainViewController | メイン画面の制御 |
| AdminPanelController | 管理パネル（TODO） |

---

## unityShared クラス構成

### Network/
| クラス名 | 責務 |
|---------|------|
| WebSocketClient | WebSocket通信の基底クラス |
| Messages | 通信メッセージ定義 |

### Data/
| クラス名 | 責務 |
|---------|------|
| PlayerData | プレイヤー情報 |
| BottleData | ボトル情報 |
| ThrowData | 投げ情報 |
| ThrowResultData | 投げ結果 |

---

## 継承関係

```
MonoBehaviour
  ├── GameManager (Singleton)
  ├── ScreenManager (Singleton)
  ├── WebSocketClient (abstract)
  │     ├── WebNetworkManager
  │     └── MainNetworkManager
  ├── OverlayScreen
  │     ├── GachaScreenController
  │     ├── BottleSelectScreenController
  │     └── CommentScreenController
  └── その他のコントローラー
```
