using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BottleFlip.Network;
using BottleFlip.Main.Core;

namespace BottleFlip.Main.Features
{
    /// <summary>
    /// ボトル上のコメント表示を管理
    /// </summary>
    public class CommentDisplay : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject commentBubblePrefab;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 30f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private float offsetY = 0.5f;
        [SerializeField] private int maxComments = 20;

        [Header("References")]
        [SerializeField] private MainNetworkManager networkManager;
        [SerializeField] private BottleFlipController bottleFlipController;

        private int currentCommentCount = 0;

        private void Start()
        {
            if (networkManager == null)
            {
                networkManager = MainNetworkManager.Instance;
            }

            networkManager.OnCommentReceived += HandleCommentReceived;
            networkManager.OnCommentSkipped += HandleCommentSkipped;
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnCommentReceived -= HandleCommentReceived;
                networkManager.OnCommentSkipped -= HandleCommentSkipped;
            }
        }

        private void HandleCommentReceived(CommentData data)
        {
            var bottle = bottleFlipController?.GetCurrentBottle();
            if (bottle != null && !string.IsNullOrEmpty(data.text))
            {
                ShowComment(bottle.transform, data.playerName, data.text);
            }
        }

        private void HandleCommentSkipped(string playerId)
        {
            // コメントなしの場合、ボトルを削除
            var bottle = bottleFlipController?.GetCurrentBottle();
            if (bottle != null)
            {
                StartCoroutine(FadeOutAndDestroy(bottle));
            }
        }

        /// <summary>
        /// コメントを表示
        /// </summary>
        public void ShowComment(Transform bottleTransform, string playerName, string commentText)
        {
            if (currentCommentCount >= maxComments)
            {
                Debug.LogWarning("[CommentDisplay] Max comments reached");
                return;
            }

            var bubble = Instantiate(commentBubblePrefab, bottleTransform);
            bubble.transform.localPosition = new Vector3(0, offsetY, 0);

            // テキスト設定
            var nameText = bubble.transform.Find("NameText")?.GetComponent<Text>();
            var commentTextComponent = bubble.transform.Find("CommentText")?.GetComponent<Text>();

            if (nameText != null) nameText.text = playerName;
            if (commentTextComponent != null) commentTextComponent.text = commentText;

            // ビルボード追加
            var billboard = bubble.AddComponent<Billboard>();

            // フェードインと自動削除
            StartCoroutine(CommentLifecycle(bubble, bottleTransform.gameObject));

            currentCommentCount++;
            Debug.Log($"[CommentDisplay] Showing comment: {playerName}: {commentText}");
        }

        private IEnumerator CommentLifecycle(GameObject bubble, GameObject bottle)
        {
            var canvasGroup = bubble.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bubble.AddComponent<CanvasGroup>();
            }

            // フェードイン
            canvasGroup.alpha = 0;
            float elapsed = 0;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            canvasGroup.alpha = 1;

            // 表示期間
            yield return new WaitForSeconds(displayDuration);

            // フェードアウト
            elapsed = 0;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1 - (elapsed / fadeOutDuration);
                yield return null;
            }

            // 削除
            currentCommentCount--;
            Destroy(bubble);

            // ボトルも削除
            if (bottle != null)
            {
                Destroy(bottle);
            }
        }

        private IEnumerator FadeOutAndDestroy(GameObject obj)
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(obj);
        }
    }

    /// <summary>
    /// 常にカメラの方を向くコンポーネント
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(
                    transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }
    }
}
