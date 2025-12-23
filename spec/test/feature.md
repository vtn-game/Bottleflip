# 機能テスト仕様

## 優先度
1. 通信処理（WebSocket）
2. 物理演算（ボトルフリップ判定）
3. コイン計算（獲得・消費ロジック）

---

## unityWeb テスト

### 振り検知テスト
| テストケース | 入力 | 期待結果 |
|-------------|------|---------|
| 振り検知_正常 | 加速度 > 2.0G | OnShakeDetected発火 |
| 振り検知_しきい値以下 | 加速度 < 2.0G | イベント発火なし |
| 振り検知_クールダウン中 | 連続振り | 1回目のみ検知 |
| パーミッション_許可 | ユーザー許可 | 振り検知有効化 |
| パーミッション_拒否 | ユーザー拒否 | エラー表示 |

### ネットワークテスト
| テストケース | 入力 | 期待結果 |
|-------------|------|---------|
| 接続_成功 | 有効なURL | OnConnected発火 |
| 接続_失敗 | 無効なURL | OnError発火 |
| 投げ送信 | ThrowData | サーバーにメッセージ送信 |
| 結果受信 | ThrowResultData | OnThrowResult発火 |
| 切断_再接続 | 切断後 | 自動再接続試行 |

### ガチャテスト
| テストケース | 入力 | 期待結果 |
|-------------|------|---------|
| 単発_コイン十分 | 100コイン以上 | ガチャ実行、100コイン消費 |
| 単発_コイン不足 | 100コイン未満 | ボタン無効、実行不可 |
| 10連_天井保証 | 10連実行 | 最低1つ★★★以上 |
| 確率_検証 | 1000回試行 | 各レアリティが確率通り |

---

## unityMain テスト

### 物理演算テスト
| テストケース | 入力 | 期待結果 |
|-------------|------|---------|
| 投げ_正常 | ThrowData | ボトル生成、初速適用 |
| 成功判定_直立 | 傾き<15°, 速度<0.1, 0.5秒継続 | success=true |
| 失敗判定_傾き | 傾き>15°, 安定 | success=false |
| 失敗判定_タイムアウト | 10秒経過 | success=false |
| スタンドアロン投げ | スペースキー | ボトル生成 |

### コイン計算テスト
| テストケース | 入力 | 期待結果 |
|-------------|------|---------|
| 成功時_通常 | success=true | +50コイン |
| 失敗時 | success=false | +0コイン |
| ボーナス_2倍 | 2倍ボトル使用 | +100コイン |

---

## 共通 テスト

### 通信プロトコルテスト
| テストケース | 送信 | 期待受信 |
|-------------|------|---------|
| 認証フロー | c2s_auth | s2c_auth_result |
| 投げフロー | c2s_throw | s2c_throw_result |
| コメント送信 | c2s_comment | （確認応答なし） |

### データ永続化テスト
| テストケース | 操作 | 期待結果 |
|-------------|------|---------|
| コイン保存 | AddCoins(100) | PlayerPrefs["coins"]=100 |
| コイン読込 | 再起動後 | 保存値が復元 |
| ボトル選択保存 | SetSelectedBottle("B002") | 保存・復元 |

---

## テスト実行方法

### Unity Test Framework
```csharp
[Test]
public void ShakeDetector_WhenAccelerationExceedsThreshold_ShouldFireEvent()
{
    // Arrange
    var detector = new ShakeDetector();
    bool eventFired = false;
    detector.OnShakeDetected += () => eventFired = true;

    // Act
    detector.ProcessAcceleration(new Vector3(0, 3.0f, 0));

    // Assert
    Assert.IsTrue(eventFired);
}
```

### モック使用
- ネットワーク: `IWebSocketClient` インターフェースでモック
- 時間: `Time.deltaTime` をラップしてテスト可能に
