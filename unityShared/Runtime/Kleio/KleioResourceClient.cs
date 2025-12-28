using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// リソース取得APIクライアント
    /// </summary>
    public class KleioResourceClient
    {
        private readonly KleioConfig _config;
        private readonly KleioHttpClient _httpClient;

        public KleioResourceClient(KleioConfig config, KleioHttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// リソースマニフェストを取得
        /// </summary>
        /// <param name="resourceId">リソースID</param>
        /// <returns>マニフェスト</returns>
        public async Task<KleioResult<KleioResourceManifest>> GetManifestAsync(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentNullException(nameof(resourceId));

            var url = $"{_config.resource_endpoint}/{Uri.EscapeDataString(resourceId)}/manifest";
            Debug.Log($"[Kleio] Fetching manifest: {url}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccess)
            {
                return KleioResult<KleioResourceManifest>.Failure(
                    GetErrorCode(response.StatusCode),
                    response.Error
                );
            }

            try
            {
                var manifest = JsonConvert.DeserializeObject<KleioResourceManifest>(response.Body);
                return KleioResult<KleioResourceManifest>.Success(manifest);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[Kleio] Failed to parse manifest: {ex.Message}");
                return KleioResult<KleioResourceManifest>.Failure(
                    KleioErrorCodes.InvalidRequest,
                    "Failed to parse manifest"
                );
            }
        }

        /// <summary>
        /// リソースデータ（バイナリ）を取得
        /// </summary>
        /// <param name="resourceId">リソースID</param>
        /// <returns>バイナリデータ</returns>
        public async Task<KleioResult<byte[]>> FetchResourceAsync(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentNullException(nameof(resourceId));

            var url = $"{_config.resource_endpoint}/{Uri.EscapeDataString(resourceId)}";
            Debug.Log($"[Kleio] Fetching resource: {url}");

            var response = await _httpClient.GetBinaryAsync(url);

            if (!response.IsSuccess)
            {
                return KleioResult<byte[]>.Failure(
                    GetErrorCode(response.StatusCode),
                    response.Error
                );
            }

            return KleioResult<byte[]>.Success(response.BinaryData);
        }

        /// <summary>
        /// リソースファイルを取得
        /// </summary>
        /// <param name="resourceId">リソースID</param>
        /// <param name="filePath">ファイルパス（マニフェスト内のパス）</param>
        /// <returns>バイナリデータ</returns>
        public async Task<KleioResult<byte[]>> FetchResourceFileAsync(string resourceId, string filePath)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentNullException(nameof(resourceId));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var url = $"{_config.resource_endpoint}/{Uri.EscapeDataString(resourceId)}/files/{Uri.EscapeDataString(filePath)}";
            Debug.Log($"[Kleio] Fetching file: {url}");

            var response = await _httpClient.GetBinaryAsync(url);

            if (!response.IsSuccess)
            {
                return KleioResult<byte[]>.Failure(
                    GetErrorCode(response.StatusCode),
                    response.Error
                );
            }

            return KleioResult<byte[]>.Success(response.BinaryData);
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

    /// <summary>
    /// 操作結果
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    public class KleioResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public KleioError Error { get; private set; }

        private KleioResult() { }

        public static KleioResult<T> Success(T data)
        {
            return new KleioResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static KleioResult<T> Failure(string errorCode, string message)
        {
            return new KleioResult<T>
            {
                IsSuccess = false,
                Error = new KleioError
                {
                    code = errorCode,
                    message = message
                }
            };
        }
    }
}
