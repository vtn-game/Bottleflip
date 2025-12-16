using UnityEngine;
using UnityEngine.SceneManagement;

namespace BottleFlip.Web.Core
{
    /// <summary>
    /// アプリ起動時の初期化処理
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string nameInputScene = "NameInput";
        [SerializeField] private string mainScene = "Main";

        [Header("Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject networkManagerPrefab;

        private void Awake()
        {
            // シングルトン生成
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }

            if (WebNetworkManager.Instance == null && networkManagerPrefab != null)
            {
                Instantiate(networkManagerPrefab);
            }
        }

        private void Start()
        {
            // 初期画面判定
            if (GameManager.Instance.IsRegistered)
            {
                // 登録済み → メイン画面へ
                LoadScene(mainScene);
            }
            else
            {
                // 未登録 → 名前入力画面へ
                LoadScene(nameInputScene);
            }
        }

        private void LoadScene(string sceneName)
        {
            Debug.Log($"[Bootstrap] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
