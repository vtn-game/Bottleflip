using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// Kleio用HTTPクライアント
    /// リトライ、認証、タイムアウトを処理
    /// </summary>
    public class KleioHttpClient
    {
        private readonly KleioConfig _config;
        private readonly string _apiKey;

        public KleioHttpClient(KleioConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // APIキーを取得（設定ファイルまたは環境変数から）
            if (_config.auth != null && _config.auth.type == "api_key")
            {
                _apiKey = !string.IsNullOrEmpty(_config.auth.key)
                    ? _config.auth.key
                    : Environment.GetEnvironmentVariable("KLEIO_API_KEY");
            }
        }

        /// <summary>
        /// GETリクエストを送信
        /// </summary>
        public async Task<KleioHttpResponse> GetAsync(string url)
        {
            return await SendRequestWithRetryAsync(url, "GET", null);
        }

        /// <summary>
        /// バイナリデータを取得
        /// </summary>
        public async Task<KleioHttpResponse> GetBinaryAsync(string url)
        {
            return await SendRequestWithRetryAsync(url, "GET", null, expectBinary: true);
        }

        /// <summary>
        /// リトライ付きリクエスト送信
        /// </summary>
        private async Task<KleioHttpResponse> SendRequestWithRetryAsync(
            string url,
            string method,
            string body,
            bool expectBinary = false)
        {
            var retryConfig = _config.retry ?? new RetryConfig();
            int attempts = 0;
            int delay = retryConfig.delay_ms;

            while (attempts < retryConfig.max_attempts)
            {
                attempts++;

                try
                {
                    var response = await SendRequestAsync(url, method, body, expectBinary);

                    // 成功またはリトライ不可能なエラー
                    if (response.IsSuccess || !IsRetryableError(response.StatusCode))
                    {
                        return response;
                    }

                    Debug.LogWarning($"[Kleio] Request failed (attempt {attempts}/{retryConfig.max_attempts}): {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Kleio] Request exception (attempt {attempts}/{retryConfig.max_attempts}): {ex.Message}");

                    if (attempts >= retryConfig.max_attempts)
                    {
                        return new KleioHttpResponse
                        {
                            StatusCode = 0,
                            Error = ex.Message,
                            IsSuccess = false
                        };
                    }
                }

                // リトライ前に待機
                if (attempts < retryConfig.max_attempts)
                {
                    await Task.Delay(delay);
                    delay = (int)(delay * retryConfig.backoff_multiplier);
                }
            }

            return new KleioHttpResponse
            {
                StatusCode = 0,
                Error = "Max retry attempts exceeded",
                IsSuccess = false
            };
        }

        /// <summary>
        /// 単一リクエスト送信
        /// </summary>
        private async Task<KleioHttpResponse> SendRequestAsync(
            string url,
            string method,
            string body,
            bool expectBinary)
        {
            using var request = new UnityWebRequest(url, method);

            // タイムアウト設定
            var timeoutConfig = _config.timeout ?? new TimeoutConfig();
            request.timeout = timeoutConfig.read_ms / 1000;

            // ボディ設定
            if (!string.IsNullOrEmpty(body))
            {
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            // ダウンロードハンドラー
            request.downloadHandler = new DownloadHandlerBuffer();

            // 認証ヘッダー
            AddAuthHeaders(request);

            // リクエスト送信
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            var response = new KleioHttpResponse
            {
                StatusCode = (int)request.responseCode,
                IsSuccess = request.result == UnityWebRequest.Result.Success
            };

            if (response.IsSuccess)
            {
                if (expectBinary)
                {
                    response.BinaryData = request.downloadHandler.data;
                }
                else
                {
                    response.Body = request.downloadHandler.text;
                }
            }
            else
            {
                response.Error = request.error ?? request.downloadHandler.text;
            }

            return response;
        }

        /// <summary>
        /// 認証ヘッダーを追加
        /// </summary>
        private void AddAuthHeaders(UnityWebRequest request)
        {
            if (_config.auth == null) return;

            switch (_config.auth.type)
            {
                case "api_key":
                    if (!string.IsNullOrEmpty(_apiKey))
                    {
                        var headerName = _config.auth.header_name ?? "X-API-Key";
                        request.SetRequestHeader(headerName, _apiKey);
                    }
                    break;

                case "bearer":
                    if (!string.IsNullOrEmpty(_apiKey))
                    {
                        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
                    }
                    break;
            }
        }

        /// <summary>
        /// リトライ可能なエラーかどうか
        /// </summary>
        private bool IsRetryableError(int statusCode)
        {
            // 5xx系エラー、429（レート制限）、0（ネットワークエラー）
            return statusCode == 0 ||
                   statusCode == 429 ||
                   (statusCode >= 500 && statusCode < 600);
        }
    }

    /// <summary>
    /// HTTPレスポンス
    /// </summary>
    public class KleioHttpResponse
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string Body { get; set; }
        public byte[] BinaryData { get; set; }
        public string Error { get; set; }
    }
}
