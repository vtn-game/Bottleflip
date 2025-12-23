# 入力定義

## unityWeb（スマホ側）

### 振り検知
- **入力源**: 加速度センサー（DeviceMotionAPI）
- **しきい値**: 2.0G以上で振りと判定
- **クールダウン**: 1秒間は再検知しない

### タッチ入力
- ボタンタップ: 標準のUnity UI Button
- スクロール: ScrollRect

### プラットフォーム別

#### WebGL
- JavaScript経由でDeviceMotionEventを取得
- `WebGLDeviceMotion` クラスでブリッジ

#### iOS Safari
- iOS 13+ではパーミッション要求が必要
- ユーザーアクション（ボタンタップ）をトリガーにリクエスト

#### Editor
- `Input.acceleration` を使用（シミュレート）
- デバッグボタンでマニュアル投げ可能

---

## unityMain（母艦側）

### キーボード入力
| キー | アクション |
|------|-----------|
| Space | ボトルを投げる（スタンドアロンテスト用） |
| Escape | 終了確認 |

### マウス入力
- UI操作のみ

### 設定可能パラメータ

```csharp
[Header("Standalone Input")]
[SerializeField] private bool enableStandaloneInput = true;
[SerializeField] private KeyCode throwKey = KeyCode.Space;
[SerializeField] private float defaultThrowIntensity = 5.0f;
[SerializeField] private float throwIntensityRandomRange = 2.0f;
```

---

## 入力の無効化条件

### unityWeb
- サーバー未接続時
- オーバーレイ画面表示中
- 投げ結果待機中（canThrow = false）
- パーミッション未許可時（iOS）

### unityMain
- `enableStandaloneInput = false` 時
- 判定中（isJudging = true）
