using UnityEngine;
using UnityEngine.UI;
using BottleFlip.Main.Core;
using BottleFlip.Main.Features;

namespace BottleFlip.Main.Screens
{
    /// <summary>
    /// 母艦アプリのメインビュー画面コントローラー
    /// </summary>
    public class MainViewController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text playerCountText;
        [SerializeField] private Text connectionUrlText;
        [SerializeField] private RawImage qrCodeImage;

        [Header("Activity Log")]
        [SerializeField] private ScrollRect activityScrollRect;
        [SerializeField] private Text activityLogText;
        [SerializeField] private int maxLogLines = 50;

        [Header("Status")]
        [SerializeField] private Text statusText;
        [SerializeField] private Image connectionIndicator;

        [Header("References")]
        [SerializeField] private BottleFlipController bottleFlipController;

        private string serverUrl;

        private void Start()
        {
            SetupUI();
            SetupEvents();
        }

        private void OnDestroy()
        {
            CleanupEvents();
        }

        private void SetupUI()
        {
            if (titleText != null)
            {
                titleText.text = "ボトルフリップ！";
            }

            UpdatePlayerCount(0);
            UpdateConnectionStatus(false);

            // サーバーURL取得（実際は設定から）
            serverUrl = $"http://{GetLocalIPAddress()}:8080";
            if (connectionUrlText != null)
            {
                connectionUrlText.text = serverUrl;
            }

            // QRコード生成
            GenerateQRCode(serverUrl);

            ClearActivityLog();
            AddActivityLog("システム起動...");
        }

        private void SetupEvents()
        {
            // ゲームマネージャーイベント
            if (MainGameManager.Instance != null)
            {
                MainGameManager.Instance.OnPlayerCountChanged += UpdatePlayerCount;
                MainGameManager.Instance.OnPlayerJoined += OnPlayerJoined;
                MainGameManager.Instance.OnPlayerLeft += OnPlayerLeft;
            }

            // ネットワークイベント
            if (MainNetworkManager.Instance != null)
            {
                MainNetworkManager.Instance.OnConnected += () => UpdateConnectionStatus(true);
                MainNetworkManager.Instance.OnDisconnected += () => UpdateConnectionStatus(false);
            }

            // ボトルフリップイベント
            if (bottleFlipController != null)
            {
                bottleFlipController.OnBottleSpawned += OnBottleSpawned;
                bottleFlipController.OnFlipResult += OnFlipResult;
            }
        }

        private void CleanupEvents()
        {
            if (MainGameManager.Instance != null)
            {
                MainGameManager.Instance.OnPlayerCountChanged -= UpdatePlayerCount;
                MainGameManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                MainGameManager.Instance.OnPlayerLeft -= OnPlayerLeft;
            }
        }

        private void UpdatePlayerCount(int count)
        {
            if (playerCountText != null)
            {
                playerCountText.text = $"接続中: {count}人";
            }
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (statusText != null)
            {
                statusText.text = connected ? "サーバー接続中" : "サーバー未接続";
            }

            if (connectionIndicator != null)
            {
                connectionIndicator.color = connected ? Color.green : Color.red;
            }

            AddActivityLog(connected ? "サーバーに接続しました" : "サーバーから切断されました");
        }

        private void OnPlayerJoined(PlayerInfo player)
        {
            AddActivityLog($"→ {player.name} が参加しました");
        }

        private void OnPlayerLeft(PlayerInfo player)
        {
            AddActivityLog($"← {player.name} が退出しました");
        }

        private void OnBottleSpawned(string playerName, string bottleId)
        {
            AddActivityLog($"🍾 {playerName} がボトルを投げた！");
        }

        private void OnFlipResult(bool success, int coins)
        {
            if (success)
            {
                AddActivityLog($"✓ 成功！ (+{coins}コイン)");
            }
            else
            {
                AddActivityLog("✗ ざんねん...");
            }
        }

        private void AddActivityLog(string message)
        {
            if (activityLogText == null) return;

            var time = System.DateTime.Now.ToString("HH:mm:ss");
            var newLine = $"[{time}] {message}\n";

            activityLogText.text += newLine;

            // 行数制限
            var lines = activityLogText.text.Split('\n');
            if (lines.Length > maxLogLines)
            {
                var newText = "";
                for (int i = lines.Length - maxLogLines; i < lines.Length; i++)
                {
                    newText += lines[i] + "\n";
                }
                activityLogText.text = newText;
            }

            // スクロール
            if (activityScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                activityScrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void ClearActivityLog()
        {
            if (activityLogText != null)
            {
                activityLogText.text = "";
            }
        }

        private void GenerateQRCode(string url)
        {
            // TODO: QRコード生成ライブラリを使用
            // 現在はプレースホルダー
            Debug.Log($"[MainView] QR Code URL: {url}");
        }

        private string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }
    }
}
