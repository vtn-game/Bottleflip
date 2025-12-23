# パフォーマンステスト仕様

## 目標値

### レスポンス
| 項目 | 目標 | 許容最大 |
|------|------|---------|
| 振り→投げ送信 | 50ms | 100ms |
| 投げ→結果受信 | 500ms | 1000ms |
| 画面遷移 | 100ms | 300ms |
| ガチャアニメーション | 2000ms | 3000ms |

### フレームレート
| プラットフォーム | 目標 | 最低 |
|-----------------|------|------|
| unityWeb (WebGL) | 60fps | 30fps |
| unityMain (Standalone) | 60fps | 30fps |

### メモリ
| プラットフォーム | 目標 | 最大 |
|-----------------|------|------|
| unityWeb | 128MB | 256MB |
| unityMain | 256MB | 512MB |

---

## テスト項目

### 通信遅延テスト
```
1. 振り検知からWebSocket送信までの時間を計測
2. 送信から結果受信までのRTTを計測
3. 100回試行して平均・最大・最小を記録
```

### 負荷テスト
```
1. 連続投げ（1秒間隔で100回）
2. 同時接続数（10, 50, 100クライアント）
3. メモリリークチェック（1時間連続稼働）
```

### WebGL固有
```
1. 初回ロード時間
2. アセットバンドルロード時間
3. LocalStorage読み書き速度
```

---

## 計測方法

### Unity Profiler
- CPU使用率
- メモリ使用量
- GC Alloc

### カスタム計測
```csharp
public class PerformanceLogger : MonoBehaviour
{
    private Stopwatch _stopwatch = new Stopwatch();

    public void StartMeasure(string label)
    {
        _stopwatch.Restart();
        Debug.Log($"[Perf] {label} started");
    }

    public void EndMeasure(string label)
    {
        _stopwatch.Stop();
        Debug.Log($"[Perf] {label}: {_stopwatch.ElapsedMilliseconds}ms");
    }
}
```

### ネットワーク計測
```csharp
// 送信時にタイムスタンプを付与
var message = new ThrowMessage
{
    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// 受信時にRTT計算
var rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - result.originalTimestamp;
```

---

## パフォーマンス改善チェックリスト

### 共通
- [ ] Update()内で重い処理をしていないか
- [ ] 不要なGetComponent呼び出しがないか
- [ ] GC Allocが発生していないか（文字列結合等）
- [ ] 未使用のイベント購読を解除しているか

### WebGL固有
- [ ] 非同期処理がブロッキングしていないか
- [ ] 大きなアセットを遅延ロードしているか
- [ ] テクスチャサイズが適切か

### ネットワーク
- [ ] メッセージサイズが最小化されているか
- [ ] 不要な再送がないか
- [ ] 接続プール/再利用ができているか
