# 通信プロトコル仕様

## 概要
WebアプリとMainアプリ間の通信プロトコル定義。

## 通信方式

| 方式 | 用途 | ポート |
|------|------|--------|
| REST API | CRUD操作、状態取得 | 8080 |
| WebSocket | リアルタイム通信 | 8081 |

## REST API

### 共通仕様

#### リクエストヘッダー
```
Content-Type: application/json
X-Player-Id: {player_id}  (認証後)
```

#### レスポンス形式
```json
{
    "success": true,
    "data": { ... },
    "error": null
}
```

#### エラーレスポンス
```json
{
    "success": false,
    "data": null,
    "error": {
        "code": "ERROR_CODE",
        "message": "エラーメッセージ"
    }
}
```

### エンドポイント一覧

#### プレイヤー

##### POST /api/players - プレイヤー登録
Request:
```json
{
    "name": "たろう"
}
```
Response:
```json
{
    "success": true,
    "data": {
        "id": "p001",
        "name": "たろう",
        "coins": 100,
        "selectedBottleId": "B001"
    }
}
```

##### GET /api/players/{id} - プレイヤー情報取得
Response:
```json
{
    "success": true,
    "data": {
        "id": "p001",
        "name": "たろう",
        "coins": 350,
        "selectedBottleId": "B003",
        "bottles": ["B001", "B002", "B003"]
    }
}
```

##### PUT /api/players/{id}/bottle - 選択ボトル変更
Request:
```json
{
    "bottleId": "B003"
}
```

#### ボトル

##### GET /api/bottles - ボトルマスタ一覧
Response:
```json
{
    "success": true,
    "data": {
        "bottles": [
            {
                "id": "B001",
                "name": "ペットボトル",
                "rarity": 1,
                "difficulty": "かんたん",
                "baseCoin": 20
            }
        ]
    }
}
```

##### GET /api/players/{id}/bottles - 所持ボトル一覧
Response:
```json
{
    "success": true,
    "data": {
        "bottles": [
            {
                "id": "B001",
                "name": "ペットボトル",
                "rarity": 1,
                "acquiredAt": "2024-01-01T00:00:00Z"
            }
        ]
    }
}
```

#### ガチャ

##### GET /api/gacha - ガチャ機一覧
Response:
```json
{
    "success": true,
    "data": {
        "machines": [
            {
                "id": 1,
                "name": "ガチャ1号機",
                "total": 50,
                "remaining": 45
            }
        ]
    }
}
```

##### GET /api/gacha/{id}/contents - ガチャ内容
Response:
```json
{
    "success": true,
    "data": {
        "machineId": 1,
        "items": [
            {
                "bottleId": "B001",
                "name": "ペットボトル",
                "rarity": 1,
                "available": true
            }
        ]
    }
}
```

##### POST /api/gacha/{id}/pull - ガチャ実行
Request:
```json
{
    "playerId": "p001"
}
```
Response:
```json
{
    "success": true,
    "data": {
        "bottle": {
            "id": "B003",
            "name": "牛乳パック",
            "rarity": 2,
            "isNew": true
        },
        "coinsSpent": 100,
        "coinsEarned": 0,
        "remainingCoins": 250
    }
}
```

## WebSocket

### 接続URL
```
ws://{server_ip}:8081
```

### メッセージ形式
```json
{
    "type": "event_type",
    "data": { ... },
    "timestamp": 1234567890
}
```

### クライアント → サーバー

#### auth - 認証
```json
{
    "type": "auth",
    "data": {
        "playerId": "p001"
    }
}
```

#### throw - ボトル投げ
```json
{
    "type": "throw",
    "data": {
        "bottleId": "B001",
        "intensity": 0.75
    }
}
```

#### comment - コメント投稿
```json
{
    "type": "comment",
    "data": {
        "text": "やったー！"
    }
}
```

#### skip_comment - コメントスキップ
```json
{
    "type": "skip_comment",
    "data": {}
}
```

### サーバー → クライアント

#### auth_success - 認証成功
```json
{
    "type": "auth_success",
    "data": {
        "playerId": "p001",
        "playerName": "たろう",
        "coins": 350
    }
}
```

#### throw_started - 投げ開始
```json
{
    "type": "throw_started",
    "data": {
        "playerId": "p001",
        "playerName": "たろう",
        "bottleName": "ペットボトル"
    }
}
```

#### throw_result - 投げ結果
```json
{
    "type": "throw_result",
    "data": {
        "success": true,
        "coinsEarned": 50,
        "totalCoins": 400
    }
}
```

#### error - エラー
```json
{
    "type": "error",
    "data": {
        "code": "INSUFFICIENT_COINS",
        "message": "コインが足りません"
    }
}
```

### サーバー → 全クライアント（ブロードキャスト）

#### player_joined - プレイヤー参加
```json
{
    "type": "player_joined",
    "data": {
        "playerId": "p002",
        "playerName": "はなこ"
    }
}
```

#### player_left - プレイヤー離脱
```json
{
    "type": "player_left",
    "data": {
        "playerId": "p002"
    }
}
```

#### bottle_flipped - ボトルフリップ発生
```json
{
    "type": "bottle_flipped",
    "data": {
        "playerName": "たろう",
        "success": true,
        "comment": "やったー！"
    }
}
```

## エラーコード

| コード | 説明 |
|--------|------|
| `INVALID_REQUEST` | リクエスト不正 |
| `PLAYER_NOT_FOUND` | プレイヤー未登録 |
| `INSUFFICIENT_COINS` | コイン不足 |
| `GACHA_EMPTY` | ガチャ空 |
| `BOTTLE_NOT_OWNED` | ボトル未所持 |
| `SERVER_ERROR` | サーバーエラー |
| `CONNECTION_ERROR` | 接続エラー |
