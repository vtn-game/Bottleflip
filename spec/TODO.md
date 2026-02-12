# 仕様TODO

未定義・要確認の項目をここにまとめる

---

## 未実装クラス・データモデル（仕様書に定義済み、コード未実装）

> 参照: `spec/shared/01_data-model.md`, `spec/rule/classes.md`

### unityShared/Runtime/Data/

- [ ] `PlayerSession` クラス（セッション管理用データモデル）
  - 仕様: `spec/shared/01_data-model.md` L22-31
  - SessionId, PlayerId, IsConnected, ConnectedAt
- [ ] `PlayerBottle` クラス（所持ボトル管理）
  - 仕様: `spec/shared/01_data-model.md` L56-63
  - PlayerId, BottleId, AcquiredAt
- [ ] `GachaMachine` クラス（ガチャ機データ）
  - 仕様: `spec/shared/01_data-model.md` L84-91
  - MachineId, Name, TotalCount, Items
- [ ] `GachaItem` クラス（ガチャ中身）
  - 仕様: `spec/shared/01_data-model.md` L96-103
  - BottleId, IsAvailable, DrawnAt, DrawnByPlayerId
- [ ] `FlipResult` クラス（フリップ結果データ）
  - 仕様: `spec/shared/01_data-model.md` L124-131
  - PlayerId, IsSuccess, CoinsEarned, FlipQuality
- [ ] `Comment` データモデル（完全版）
  - 仕様: `spec/shared/01_data-model.md` L138-147
  - 現状: `CommentData`（Messages.cs）に playerId, playerName, text のみ
  - 不足: Id, BottleId, WasSuccessful, CreatedAt
- [ ] `CoinTransaction` クラス（コイン取引ログ）
  - 仕様: `spec/shared/01_data-model.md` L155-163
  - Id, PlayerId, Amount, Reason, Timestamp

---

## 未実装画面（仕様書に定義済み、コード未実装）

### unityWeb

- [ ] **名前入力画面コントローラー** (`WEB-SCR-001`)
  - 仕様: `spec/unityWeb/screens/01_name-input.md`
  - 現状: `Bootstrap.cs` で名前入力シーンへの遷移ロジックはあるが、画面コントローラーが存在しない
  - 必要: NameInputScreenController.cs（InputField, バリデーション, サーバー登録API呼び出し）
- [ ] **ガチャ演出・結果画面** (`WEB-SCR-005`) の分離
  - 仕様: `spec/unityWeb/screens/05_gacha-result.md` - レア度別演出、NEW!/被り表示
  - 現状: `GachaScreenController.cs` 内にインライン実装（仮データ）
  - 必要: 専用のGachaResultScreenControllerまたは演出システムの充実

### unityMain

- [ ] **管理パネル画面** (`MAIN-SCR-002`)
  - 仕様: `spec/unityMain/screens/02_admin-panel.md`
  - 必要: AdminPanelController.cs
  - 機能: サーバー設定、ガチャ管理、プレイヤー管理、デバッグタブ
  - ショートカット: F1トグル表示

---

## 未実装機能（仕様書に定義済み、コード未実装）

### REST APIクライアント（Web側）

- [ ] **REST API通信クラスが未実装**
  - 仕様: `spec/shared/02_communication.md`, `spec/unityWeb/features/03_connection.md`
  - 現状: WebSocket通信のみ実装済み。REST APIは全く未実装
  - 必要なエンドポイント:
    - `POST /api/players` - プレイヤー登録
    - `GET /api/players/{id}` - プレイヤー情報取得
    - `GET /api/players/{id}/bottles` - 所持ボトル一覧
    - `PUT /api/players/{id}/bottle` - 選択ボトル変更
    - `GET /api/bottles` - ボトルマスタ一覧
    - `GET /api/gacha` - ガチャ機一覧
    - `GET /api/gacha/{id}/contents` - ガチャ内容
    - `POST /api/gacha/{id}/pull` - ガチャ実行

### コインシステム（Main側）

- [ ] **CoinSystemController が未実装**
  - 仕様: `spec/unityMain/features/02_coin-system.md`
  - 現状: `BottleFlipController.CompleteFlip()` で固定 50 コイン
  - 必要:
    - `ICoinService` インターフェース（AddCoins, SpendCoins, GetBalance, GetHistory）
    - ボトル種別に応じた報酬計算（BottleMaster.baseCoin + 難易度ボーナス）
    - `FlipQuality` 計算（直立度 + 安定度）
    - トランザクションログ

### ガチャシステム（サーバー連携）

- [ ] **ガチャがクライアントローカル実装のまま**
  - 仕様: `spec/unityWeb/features/02_gacha-system.md` - サーバーAPIと連携するボックスガチャ
  - 現状: `GachaScreenController.GenerateGachaResult()` でクライアント側ランダム生成（仮実装）
  - 必要: REST API (`POST /api/gacha/{id}/pull`) との連携

### ボトル選択画面（サーバー連携）

- [ ] **所持ボトルデータがハードコード**
  - 仕様: `spec/unityWeb/screens/03_bottle-select.md` - サーバーから所持ボトル取得
  - 現状: `BottleSelectScreenController.LoadOwnedBottles()` に仮データ3件がハードコード
  - 必要: REST API (`GET /api/players/{id}/bottles`) との連携

### QRコード生成（Main側）

- [ ] **QRコード生成が未実装**
  - 仕様: `spec/unityMain/features/04_server.md` L151-165
  - 現状: `MainViewController.GenerateQRCode()` はプレースホルダー（ログ出力のみ）
  - 必要: QRCoderライブラリの導入と実装

### データ永続化（Main側）

- [ ] **サーバー側データ永続化が未実装**
  - 仕様: `spec/unityMain/features/04_server.md` L92-119
  - 必要: players.json, bottles.json, gacha_state.json, transactions.json
  - 保存タイミング: データ変更時（デバウンス1秒）、アプリ終了時

---

## 未実装WebSocketメッセージ

- [ ] `throw_started` (サーバー→クライアント) - 投げ開始通知
  - 仕様: `spec/shared/02_communication.md` L265-275
- [ ] `player_joined` / `player_left` の Web 側ハンドリング
  - 仕様: `spec/shared/02_communication.md` L300-321
  - 現状: `WebNetworkManager.ProcessMessage()` で未処理
- [ ] `bottle_flipped` ブロードキャスト
  - 仕様: `spec/shared/02_communication.md` L323-333
- [ ] `error` メッセージの Web 側ハンドリング
  - 仕様: `spec/shared/02_communication.md` L289-298
  - エラーコード: INVALID_REQUEST, PLAYER_NOT_FOUND, INSUFFICIENT_COINS 等

---

## コンパイルエラー・バグ

- [ ] **`BottleSelectScreenController.cs:240`** - `GameManager.Instance?.SetSelectedBottle()` メソッドが存在しない
  - `GameManager.cs` では `SelectBottle()` として定義されている
  - 修正: `SetSelectedBottle` → `SelectBottle` にリネーム
- [ ] **`CommentScreenController.cs:186-189`** - スキップ時にサーバー通知が送信されない
  - `OnSkipClicked()` が `WebNetworkManager.Instance?.SendSkipComment()` を呼んでいない
  - 仕様: `spec/shared/02_communication.md` L243-249 - `skip_comment` メッセージ送信が必要

---

## 高優先度（仕様面の未定義項目）

### ボトル関連
- [ ] 各ボトルの物理特性（重さ、バランス、サイズ）の数値定義
- [ ] ボトルごとのコイン獲得倍率の設定
- [ ] ボトルの3Dモデル/2D画像アセットの準備
- [ ] レアリティごとの成功エフェクト定義

### ガチャ関連
- [ ] 10連天井保証のアルゴリズム詳細（再抽選方式 or 差し替え方式）
- [ ] 重複ボトル獲得時の処理（かけら変換システム）
- [ ] ガチャ演出のアセット準備

### コイン関連
- [ ] 初回ボーナス/チュートリアル報酬の有無と量
- [ ] デイリーログインボーナスの実装有無
- [ ] 連続成功ボーナスの実装有無と倍率

---

## 中優先度

### 通信関連
- [ ] サーバー側の詳細仕様（別プロジェクト）
- [ ] 認証フローの詳細（ゲスト認証 or アカウント）
- [ ] 再接続ロジックの詳細（リトライ回数、間隔）

### UI/UX関連
- [ ] 各画面のワイヤーフレーム/デザインカンプ
- [ ] アニメーション仕様の詳細
- [ ] 効果音・BGMの定義

### データ永続化
- [ ] サーバー同期の実装（現状ローカルのみ）
- [ ] データ破損時のリカバリ方針

---

## 低優先度（将来実装）

### ピックアップガチャ
- [ ] 期間限定ガチャの仕様
- [ ] ピックアップ対象の確率UP率
- [ ] イベントスケジュール管理

### ソーシャル機能
- [ ] ランキング機能
- [ ] フレンド機能
- [ ] シェア機能

### 収益化
- [ ] 課金アイテムの有無
- [ ] 広告実装の有無

---

## 確認済み（解決済み）

_解決した項目はここに移動_
