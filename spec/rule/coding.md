# コーディングルール

## 基本原則
- ファイルおよびクラスはなるべく分割する事
- クラス間の関係性は疎結合である事
- コメントは日本語OK、複雑なロジックには必ずコメント

## 命名規則

| 対象 | ルール | 例 |
|-----|-------|-----|
| クラス名 | PascalCase | `PlayerManager` |
| メソッド名 | PascalCase | `GetPlayerData()` |
| 変数名 | camelCase | `playerName` |
| 定数 | UPPER_SNAKE_CASE | `MAX_PLAYERS` |
| プライベート変数 | _camelCase | `_isConnected` |
| イベント | On + 動詞過去形 | `OnCoinsChanged` |

## アーキテクチャパターン
- MVP または MVC パターン推奨
- シングルトンは最小限に（GameManager、NetworkManager程度）
- イベント駆動で疎結合に

## 非同期処理
- ネットワーク処理は必ず `async/await` を使用
- `Update()` 内での重い処理は避ける
- 必要に応じてコルーチンを使用

## データ形式
- JSON: `Newtonsoft.Json` または `JsonUtility`
- ID生成: GUID/UUID形式
- 日時: UTC基準、ISO 8601形式

## ライブラリ

### 必須
- NativeWebSocket: WebSocket通信

### 推奨
- UniTask: コルーチンの代替（必要に応じて）
- DOTween: アニメーション

## 禁止事項
- ハードコードされたIPアドレス（設定ファイル化すること）
- 同期的なネットワーク処理
- `Update()` 内での重い処理
- 未使用の `using` ディレクティブ放置

## WebGL固有の注意
- マルチスレッド非対応
- ファイルI/O制限あり（LocalStorage/IndexedDB使用）
- `System.Net.Sockets` 使用不可（WebSocket使用）

## エラーハンドリング
- try-catch は外部連携部分で使用
- エラーログは `Debug.LogError()` で出力
- ユーザーへのフィードバックを忘れずに
