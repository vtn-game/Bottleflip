using System;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// Kleio設定のルートクラス
    /// </summary>
    [Serializable]
    public class KleioConfigRoot
    {
        public KleioConfig kleio;
    }

    /// <summary>
    /// Kleioの設定
    /// </summary>
    [Serializable]
    public class KleioConfig
    {
        /// <summary>
        /// APIバージョン
        /// </summary>
        public string api_version = "v1";

        /// <summary>
        /// 検索APIエンドポイント
        /// </summary>
        public string search_endpoint;

        /// <summary>
        /// リソース取得APIエンドポイント
        /// </summary>
        public string resource_endpoint;

        /// <summary>
        /// 認証設定
        /// </summary>
        public AuthConfig auth;

        /// <summary>
        /// キャッシュ設定
        /// </summary>
        public CacheConfig cache;

        /// <summary>
        /// リトライ設定
        /// </summary>
        public RetryConfig retry;

        /// <summary>
        /// タイムアウト設定
        /// </summary>
        public TimeoutConfig timeout;
    }

    /// <summary>
    /// 認証設定
    /// </summary>
    [Serializable]
    public class AuthConfig
    {
        /// <summary>
        /// 認証タイプ（none, api_key, bearer）
        /// </summary>
        public string type = "none";

        /// <summary>
        /// APIキーヘッダー名
        /// </summary>
        public string header_name = "X-API-Key";

        /// <summary>
        /// APIキー（環境変数から取得する場合は空）
        /// </summary>
        public string key;
    }

    /// <summary>
    /// キャッシュ設定
    /// </summary>
    [Serializable]
    public class CacheConfig
    {
        /// <summary>
        /// キャッシュ有効/無効
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// キャッシュディレクトリ名
        /// </summary>
        public string directory = "KleioCache";

        /// <summary>
        /// 最大キャッシュサイズ（MB）
        /// </summary>
        public int max_size_mb = 500;

        /// <summary>
        /// キャッシュ有効期限（時間）
        /// </summary>
        public int ttl_hours = 24;
    }

    /// <summary>
    /// リトライ設定
    /// </summary>
    [Serializable]
    public class RetryConfig
    {
        /// <summary>
        /// 最大リトライ回数
        /// </summary>
        public int max_attempts = 3;

        /// <summary>
        /// リトライ間隔（ミリ秒）
        /// </summary>
        public int delay_ms = 1000;

        /// <summary>
        /// バックオフ倍率
        /// </summary>
        public float backoff_multiplier = 2.0f;
    }

    /// <summary>
    /// タイムアウト設定
    /// </summary>
    [Serializable]
    public class TimeoutConfig
    {
        /// <summary>
        /// 接続タイムアウト（ミリ秒）
        /// </summary>
        public int connect_ms = 5000;

        /// <summary>
        /// 読み取りタイムアウト（ミリ秒）
        /// </summary>
        public int read_ms = 30000;
    }
}
