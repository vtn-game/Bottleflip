using System;
using System.Collections.Generic;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// 検索パラメータ
    /// </summary>
    [Serializable]
    public class KleioSearchParams
    {
        /// <summary>
        /// 検索タグ
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// リソースタイプ（bottle, effect, sound等）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// レアリティレベル（1-5）
        /// </summary>
        public int? Rarity { get; set; }

        /// <summary>
        /// 取得件数上限
        /// </summary>
        public int Limit { get; set; } = 20;

        /// <summary>
        /// オフセット（ページング用）
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// クエリ文字列を生成
        /// </summary>
        public string ToQueryString()
        {
            var parts = new List<string>();

            if (Tags != null && Tags.Length > 0)
                parts.Add($"tags={string.Join(",", Tags)}");

            if (!string.IsNullOrEmpty(Type))
                parts.Add($"type={Uri.EscapeDataString(Type)}");

            if (Rarity.HasValue)
                parts.Add($"rarity={Rarity.Value}");

            parts.Add($"limit={Limit}");
            parts.Add($"offset={Offset}");

            return string.Join("&", parts);
        }
    }

    /// <summary>
    /// 検索レスポンス
    /// </summary>
    [Serializable]
    public class KleioSearchResponse
    {
        public bool success;
        public int total;
        public List<KleioResourceInfo> results;
        public KleioError error;
    }

    /// <summary>
    /// リソース情報（検索結果）
    /// </summary>
    [Serializable]
    public class KleioResourceInfo
    {
        public string id;
        public string name;
        public string type;
        public string[] tags;
        public int rarity;
        public string thumbnail_url;
        public KleioResourceMetadata metadata;
    }

    /// <summary>
    /// リソースメタデータ
    /// </summary>
    [Serializable]
    public class KleioResourceMetadata
    {
        public string author;
        public string version;
        public long size_bytes;
    }

    /// <summary>
    /// リソースマニフェスト
    /// </summary>
    [Serializable]
    public class KleioResourceManifest
    {
        public string id;
        public string name;
        public string type;
        public string version;
        public List<KleioResourceFile> files;
        public string[] dependencies;
        public KleioPhysicsData physics;
    }

    /// <summary>
    /// リソースファイル情報
    /// </summary>
    [Serializable]
    public class KleioResourceFile
    {
        public string path;
        public string hash;
        public long size;
    }

    /// <summary>
    /// 物理パラメータ（ボトル用）
    /// </summary>
    [Serializable]
    public class KleioPhysicsData
    {
        public float mass = 0.5f;
        public float[] center_of_mass;
        public float drag = 0.5f;
        public float angular_drag = 0.5f;
    }

    /// <summary>
    /// エラー情報
    /// </summary>
    [Serializable]
    public class KleioError
    {
        public string code;
        public string message;
        public Dictionary<string, object> details;
    }

    /// <summary>
    /// 汎用レスポンス
    /// </summary>
    [Serializable]
    public class KleioResponse<T>
    {
        public bool success;
        public T data;
        public KleioError error;
    }

    /// <summary>
    /// エラーコード定義
    /// </summary>
    public static class KleioErrorCodes
    {
        public const string InvalidRequest = "INVALID_REQUEST";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
        public const string RateLimited = "RATE_LIMITED";
        public const string ServerError = "SERVER_ERROR";
        public const string NetworkError = "NETWORK_ERROR";
        public const string Timeout = "TIMEOUT";
        public const string CacheError = "CACHE_ERROR";
    }
}
