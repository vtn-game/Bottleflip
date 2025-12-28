using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BottleFlip.Kleio
{
    /// <summary>
    /// Kleio設定ファイルローダー
    /// </summary>
    public static class KleioConfigLoader
    {
        /// <summary>
        /// YAMLファイルから設定を読み込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>設定オブジェクト</returns>
        public static KleioConfig LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Config file not found: {filePath}");

            var yaml = File.ReadAllText(filePath);
            return LoadFromYaml(yaml);
        }

        /// <summary>
        /// StreamingAssetsから設定を読み込む
        /// </summary>
        /// <param name="fileName">ファイル名（デフォルト: kleio.yaml）</param>
        /// <returns>設定オブジェクト</returns>
        public static KleioConfig LoadFromStreamingAssets(string fileName = "kleio.yaml")
        {
            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            return LoadFromFile(path);
        }

        /// <summary>
        /// TextAssetから設定を読み込む
        /// </summary>
        /// <param name="textAsset">TextAsset</param>
        /// <returns>設定オブジェクト</returns>
        public static KleioConfig LoadFromTextAsset(TextAsset textAsset)
        {
            if (textAsset == null)
                throw new ArgumentNullException(nameof(textAsset));

            return LoadFromYaml(textAsset.text);
        }

        /// <summary>
        /// YAML文字列から設定を読み込む
        /// </summary>
        /// <param name="yaml">YAML文字列</param>
        /// <returns>設定オブジェクト</returns>
        public static KleioConfig LoadFromYaml(string yaml)
        {
            if (string.IsNullOrEmpty(yaml))
                throw new ArgumentNullException(nameof(yaml));

            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var root = deserializer.Deserialize<KleioConfigRoot>(yaml);

                if (root?.kleio == null)
                    throw new InvalidOperationException("Invalid config format: 'kleio' section not found");

                return root.kleio;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Kleio] Failed to parse config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// デフォルト設定を生成
        /// </summary>
        /// <returns>デフォルト設定</returns>
        public static KleioConfig CreateDefault()
        {
            return new KleioConfig
            {
                api_version = "v1",
                search_endpoint = "https://localhost/api/v1/resources/search",
                resource_endpoint = "https://localhost/api/v1/resources",
                auth = new AuthConfig { type = "none" },
                cache = new CacheConfig(),
                retry = new RetryConfig(),
                timeout = new TimeoutConfig()
            };
        }
    }
}
