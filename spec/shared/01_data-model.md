# データモデル仕様

## 概要
WebアプリとMainアプリで共有するデータモデル定義。

## プレイヤー

### Player

```csharp
public class Player
{
    public string Id { get; set; }           // UUID形式
    public string Name { get; set; }         // プレイヤー名（最大12文字）
    public int Coins { get; set; }           // 所持コイン
    public string SelectedBottleId { get; set; }  // 選択中ボトルID
    public DateTime CreatedAt { get; set; }  // 登録日時
    public DateTime LastLoginAt { get; set; } // 最終ログイン
}
```

### PlayerSession

```csharp
public class PlayerSession
{
    public string SessionId { get; set; }    // セッションID
    public string PlayerId { get; set; }     // プレイヤーID
    public bool IsConnected { get; set; }    // 接続状態
    public DateTime ConnectedAt { get; set; } // 接続開始日時
}
```

## ボトル

### BottleMaster（マスタデータ）

```csharp
public class BottleMaster
{
    public string Id { get; set; }           // ボトルID (例: "B001")
    public string Name { get; set; }         // ボトル名
    public int Rarity { get; set; }          // レア度 (1-5)
    public string Difficulty { get; set; }   // 難易度表示名
    public float DifficultyValue { get; set; } // 難易度係数 (0.5-2.0)
    public int BaseCoin { get; set; }        // 基本獲得コイン
    public float Mass { get; set; }          // 質量
    public float CenterOfMass { get; set; }  // 重心オフセット
    public string PrefabPath { get; set; }   // 3Dモデルパス
    public string SpritePath { get; set; }   // 2D画像パス
}
```

### PlayerBottle（所持ボトル）

```csharp
public class PlayerBottle
{
    public string PlayerId { get; set; }
    public string BottleId { get; set; }
    public DateTime AcquiredAt { get; set; } // 入手日時
}
```

### ボトルマスタ初期データ

| ID | 名前 | レア度 | 難易度 | コイン |
|----|------|--------|--------|--------|
| B001 | ペットボトル | ★1 | かんたん | 20 |
| B002 | 缶ジュース | ★1 | かんたん | 20 |
| B003 | 牛乳パック | ★2 | かんたん | 30 |
| B004 | 水筒 | ★2 | ふつう | 30 |
| B005 | ワインボトル | ★3 | ふつう | 50 |
| B006 | シャンパンボトル | ★3 | むずかしい | 50 |
| B007 | 魔法瓶 | ★4 | むずかしい | 80 |
| B008 | ガラス瓶 | ★4 | げきむず | 80 |
| B009 | 伝説のボトル | ★5 | げきむず | 150 |
| B010 | 金のボトル | ★5 | げきむず | 150 |

## ガチャ

### GachaMachine

```csharp
public class GachaMachine
{
    public int MachineId { get; set; }       // 機械ID (1-3)
    public string Name { get; set; }         // ガチャ機名
    public int TotalCount { get; set; }      // 初期総数
    public List<GachaItem> Items { get; set; } // 中身
}
```

### GachaItem

```csharp
public class GachaItem
{
    public string BottleId { get; set; }     // ボトルID
    public bool IsAvailable { get; set; }    // 排出可能か
    public DateTime? DrawnAt { get; set; }   // 排出日時（済みの場合）
    public string DrawnByPlayerId { get; set; } // 排出したプレイヤー
}
```

## 投げアクション

### ThrowData

```csharp
public class ThrowData
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string BottleId { get; set; }
    public float Intensity { get; set; }     // 振り強度 (0.0-1.0)
    public long Timestamp { get; set; }      // Unixタイムスタンプ
}
```

### FlipResult

```csharp
public class FlipResult
{
    public string PlayerId { get; set; }
    public bool IsSuccess { get; set; }
    public int CoinsEarned { get; set; }
    public float FlipQuality { get; set; }   // 0.0-1.0
}
```

## コメント

### Comment

```csharp
public class Comment
{
    public string Id { get; set; }           // コメントID
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string BottleId { get; set; }
    public string Text { get; set; }         // コメント内容（最大30文字）
    public bool WasSuccessful { get; set; }  // 成功時のコメントか
    public DateTime CreatedAt { get; set; }
}
```

## コイン

### CoinTransaction

```csharp
public class CoinTransaction
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public int Amount { get; set; }          // 正:獲得, 負:消費
    public string Reason { get; set; }       // 理由コード
    public DateTime Timestamp { get; set; }
}
```

### 理由コード

| コード | 説明 |
|--------|------|
| `initial_bonus` | 初回ボーナス |
| `flip_success` | フリップ成功報酬 |
| `gacha_pull` | ガチャ消費 |
| `gacha_dupe` | ガチャ被り変換 |
| `admin_grant` | 管理者付与 |
