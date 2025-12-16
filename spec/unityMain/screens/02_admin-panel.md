# 管理パネル画面

## 画面ID
`MAIN-SCR-002`

## 概要
ゲーム管理者用の設定・管理パネル。F1キーなどでトグル表示。

## 表示条件
- 管理者キー押下時（F1）

## UI要素

| 要素ID | 種類 | 説明 |
|--------|------|------|
| panel_admin | Panel | 管理パネル本体 |
| tab_server | Tab | サーバー設定タブ |
| tab_gacha | Tab | ガチャ管理タブ |
| tab_players | Tab | プレイヤー管理タブ |
| tab_debug | Tab | デバッグタブ |

### サーバー設定タブ

| 要素ID | 種類 | 説明 |
|--------|------|------|
| txt_ip | Text | サーバーIPアドレス |
| txt_port | Text | ポート番号 |
| btn_start | Button | サーバー開始 |
| btn_stop | Button | サーバー停止 |
| txt_status | Text | サーバー状態 |

### ガチャ管理タブ

| 要素ID | 種類 | 説明 |
|--------|------|------|
| dropdown_machine | Dropdown | ガチャ機選択 |
| txt_remaining | Text | 残り数表示 |
| btn_reset | Button | ガチャリセット |
| btn_reset_all | Button | 全ガチャリセット |

### プレイヤー管理タブ

| 要素ID | 種類 | 説明 |
|--------|------|------|
| list_players | ListView | 接続プレイヤー一覧 |
| btn_kick | Button | プレイヤーキック |
| btn_give_coins | Button | コイン付与 |
| input_coins | InputField | 付与コイン数 |

### デバッグタブ

| 要素ID | 種類 | 説明 |
|--------|------|------|
| toggle_physics | Toggle | 物理デバッグ表示 |
| btn_spawn_test | Button | テストボトル生成 |
| slider_timescale | Slider | タイムスケール |
| txt_fps | Text | FPS表示 |

## 動作仕様

### パネル表示切替
- F1キー：パネル表示/非表示トグル
- ESCキー：パネル非表示

### サーバー管理
- 開始/停止でゲームサーバーを制御
- IP/ポートは起動時に自動取得

### ガチャリセット
- 選択したガチャ機のボックスを初期状態に戻す
- 確認ダイアログあり

### プレイヤー管理
- 接続中プレイヤー一覧表示
- キック、コイン付与が可能

## ショートカットキー

| キー | 機能 |
|------|------|
| F1 | 管理パネル表示/非表示 |
| F2 | QRコード表示/非表示 |
| F3 | デバッグ情報表示/非表示 |
| F5 | 画面リフレッシュ |
