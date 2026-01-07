using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// リソース検索APIクライアント
    /// </summary>
    public class KleioSearchClient
    {
        private readonly KleioConfig _config;
        private readonly KleioHttpClient _httpClient;

        public KleioSearchClient(KleioConfig config, KleioHttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// リソースを検索
        /// </summary>
        /// <param name="searchParams">検索パラメータ</param>
        /// <returns>検索結果</returns>
        public async Task<KleioSearchResponse> SearchAsync(KleioSearchParams searchParams)
        {
            if (searchParams == null)
                throw new ArgumentNullException(nameof(searchParams));

            var url = BuildSearchUrl(searchParams);
            Debug.Log($"[Kleio] Searching: {url}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccess)
            {
                return new KleioSearchResponse
                {
                    success = false,
                    error = new KleioError
                    {
                        code = GetErrorCode(response.StatusCode),
                        message = response.Error
                    }
                };
            }

            try
            {
                var result = JsonConvert.DeserializeObject<KleioSearchResponse>(response.Body);
                return result;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[Kleio] Failed to parse search response: {ex.Message}");
                return new KleioSearchResponse
                {
                    success = false,
                    error = new KleioError
                    {
                        code = KleioErrorCodes.InvalidRequest,
                        message = "Failed to parse response"
                    }
                };
            }
        }

        /// <summary>
        /// タグで検索（簡易版）
        /// </summary>
        public async Task<KleioSearchResponse> SearchByTagsAsync(params string[] tags)
        {
            return await SearchAsync(new KleioSearchParams { Tags = tags });
        }

        /// <summary>
        /// タイプで検索（簡易版）
        /// </summary>
        public async Task<KleioSearchResponse> SearchByTypeAsync(string type, int limit = 20)
        {
            return await SearchAsync(new KleioSearchParams { Type = type, Limit = limit });
        }

        /// <summary>
        /// レアリティで検索（簡易版）
        /// </summary>
        public async Task<KleioSearchResponse> SearchByRarityAsync(int rarity, int limit = 20)
        {
            return await SearchAsync(new KleioSearchParams { Rarity = rarity, Limit = limit });
        }

        /// <summary>
        /// 検索URLを構築
        /// </summary>
        private string BuildSearchUrl(KleioSearchParams searchParams)
        {
            var baseUrl = _config.search_endpoint;
            var queryString = searchParams.ToQueryString();

            if (baseUrl.Contains("?"))
            {
                return $"{baseUrl}&{queryString}";
            }

            return $"{baseUrl}?{queryString}";
        }

        /// <summary>
        /// HTTPステータスコードからエラーコードを取得
        /// </summary>
        private string GetErrorCode(int statusCode)
        {
            return statusCode switch
            {
                400 => KleioErrorCodes.InvalidRequest,
                401 => KleioErrorCodes.Unauthorized,
                403 => KleioErrorCodes.Forbidden,
                404 => KleioErrorCodes.ResourceNotFound,
                429 => KleioErrorCodes.RateLimited,
                >= 500 => KleioErrorCodes.ServerError,
                0 => KleioErrorCodes.NetworkError,
                _ => KleioErrorCodes.ServerError
            };
        }
    }
}
