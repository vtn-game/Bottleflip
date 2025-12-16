# コインシステム機能

## 機能ID
`MAIN-FUNC-002`

## 概要
ゲーム内通貨（コイン）の管理システム。

## コイン獲得

### ボトルフリップ成功報酬

| レア度 | 基本報酬 | 難易度ボーナス |
|--------|----------|----------------|
| ★1 | 20 | +0〜10 |
| ★2 | 30 | +0〜15 |
| ★3 | 50 | +0〜25 |
| ★4 | 80 | +0〜40 |
| ★5 | 150 | +0〜75 |

### 難易度ボーナス計算

```csharp
public int CalculateBonus(BottleData bottle, float flipQuality)
{
    // flipQuality: 0.0（ギリギリ成功）〜 1.0（完璧）
    int maxBonus = bottle.BaseCoin / 2;
    return Mathf.RoundToInt(maxBonus * flipQuality);
}

public float CalculateFlipQuality(Bottle bottle)
{
    // 直立度（完全に垂直だと1.0）
    float uprightness = 1.0f - (Vector3.Angle(bottle.transform.up, Vector3.up) / 15f);

    // 安定度（静止していると1.0）
    float stability = 1.0f - Mathf.Clamp01(bottle.Rigidbody.velocity.magnitude);

    return (uprightness + stability) / 2f;
}
```

### 初回ボーナス
- 新規プレイヤー登録時: 100コイン

### ガチャ被り時
- 既所持ボトルを引いた場合、コインに変換
- 変換レート: ガチャシステム仕様参照

## コイン消費

### ガチャ
- 1回: 100コイン

## データ管理

### プレイヤーコインデータ

```csharp
public class PlayerCoinData
{
    public string PlayerId { get; set; }
    public int CurrentCoins { get; set; }
    public int TotalEarned { get; set; }   // 累計獲得
    public int TotalSpent { get; set; }    // 累計消費
}
```

### トランザクションログ

```csharp
public class CoinTransaction
{
    public string TransactionId { get; set; }
    public string PlayerId { get; set; }
    public int Amount { get; set; }        // +獲得 / -消費
    public string Reason { get; set; }     // "flip_success", "gacha", "bonus"など
    public DateTime Timestamp { get; set; }
}
```

## API

### コイン操作

```csharp
public interface ICoinService
{
    // コイン獲得
    Task<int> AddCoins(string playerId, int amount, string reason);

    // コイン消費
    Task<bool> SpendCoins(string playerId, int amount, string reason);

    // 残高確認
    Task<int> GetBalance(string playerId);

    // 履歴取得
    Task<List<CoinTransaction>> GetHistory(string playerId, int limit = 50);
}
```

### 使用例

```csharp
// ボトルフリップ成功時
var earned = await coinService.AddCoins(playerId, 50, "flip_success");

// ガチャ購入時
if (await coinService.SpendCoins(playerId, 100, "gacha"))
{
    // ガチャ実行
}
else
{
    // コイン不足エラー
}
```

## バリデーション

### 消費時チェック
1. 残高確認
2. 同時操作の排他制御
3. トランザクション記録

### 不正対策
- サーバー側でのみコイン操作を実行
- クライアントからのコイン値直接変更は不可
- 操作ログの保存
