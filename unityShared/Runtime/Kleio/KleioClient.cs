using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// Kleioメインクライアント
    /// リソースの検索と取得を統合的に提供
    /// </summary>
    public class KleioClient : IDisposable
    {
        private readonly KleioConfig _config;
        private readonly KleioHttpClient _httpClient;
        private readonly KleioSearchClient _searchClient;
        private readonly KleioResourceClient _resourceClient;

        private bool _disposed = false;

        /// <summary>
        /// 設定オブジェクトから初期化
        /// </summary>
        public KleioClient(KleioConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new KleioHttpClient(config);
            _searchClient = new KleioSearchClient(config, _httpClient);
            _resourceClient = new KleioResourceClient(config, _httpClient);

            Debug.Log($"[Kleio] Initialized with API version: {config.api_version}");
        }

        /// <summary>
        /// YAMLファイルパスから初期化
        /// </summary>
        public KleioClient(string configPath)
            : this(KleioConfigLoader.LoadFromFile(configPath))
        {
        }

        /// <summary>
        /// TextAssetから初期化
        /// </summary>
        public KleioClient(TextAsset configAsset)
            : this(KleioConfigLoader.LoadFromTextAsset(configAsset))
        {
        }

        /// <summary>
        /// 検索クライアント
        /// </summary>
        public KleioSearchClient Search => _searchClient;

        /// <summary>
        /// リソースクライアント
        /// </summary>
        public KleioResourceClient Resources => _resourceClient;

        /// <summary>
        /// 現在の設定
        /// </summary>
        public KleioConfig Config => _config;

        #region 便利メソッド

        /// <summary>
        /// リソースを検索
        /// </summary>
        public Task<KleioSearchResponse> SearchAsync(KleioSearchParams searchParams)
        {
            return _searchClient.SearchAsync(searchParams);
        }

        /// <summary>
        /// タグで検索
        /// </summary>
        public Task<KleioSearchResponse> SearchByTagsAsync(params string[] tags)
        {
            return _searchClient.SearchByTagsAsync(tags);
        }

        /// <summary>
        /// タイプで検索
        /// </summary>
        public Task<KleioSearchResponse> SearchByTypeAsync(string type, int limit = 20)
        {
            return _searchClient.SearchByTypeAsync(type, limit);
        }

        /// <summary>
        /// リソースマニフェストを取得
        /// </summary>
        public Task<KleioResult<KleioResourceManifest>> GetManifestAsync(string resourceId)
        {
            return _resourceClient.GetManifestAsync(resourceId);
        }

        /// <summary>
        /// リソースデータを取得
        /// </summary>
        public Task<KleioResult<byte[]>> FetchResourceAsync(string resourceId)
        {
            return _resourceClient.FetchResourceAsync(resourceId);
        }

        /// <summary>
        /// 検索して最初の結果を取得
        /// </summary>
        public async Task<KleioResult<byte[]>> SearchAndFetchFirstAsync(KleioSearchParams searchParams)
        {
            var searchResult = await _searchClient.SearchAsync(searchParams);

            if (!searchResult.success)
            {
                return KleioResult<byte[]>.Failure(
                    searchResult.error?.code ?? KleioErrorCodes.ServerError,
                    searchResult.error?.message ?? "Search failed"
                );
            }

            if (searchResult.results == null || searchResult.results.Count == 0)
            {
                return KleioResult<byte[]>.Failure(
                    KleioErrorCodes.ResourceNotFound,
                    "No resources found"
                );
            }

            var firstResource = searchResult.results[0];
            return await _resourceClient.FetchResourceAsync(firstResource.id);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Debug.Log("[Kleio] Disposed");
            }

            _disposed = true;
        }

        #endregion
    }
}
