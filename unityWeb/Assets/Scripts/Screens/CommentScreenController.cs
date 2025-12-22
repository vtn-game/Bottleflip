using UnityEngine;
using UnityEngine.UI;
using BottleFlip.Web.Core;

namespace BottleFlip.Web.Screens
{
    /// <summary>
    /// コメント画面（投げ結果表示＆コメント入力）のコントローラー
    /// オーバーレイとしてメイン画面の上に表示
    /// </summary>
    public class CommentScreenController : OverlayScreen
    {
        [Header("Result Display")]
        [SerializeField] private Text resultTitleText;
        [SerializeField] private Text resultMessageText;
        [SerializeField] private Text coinsEarnedText;
        [SerializeField] private GameObject successEffectObject;
        [SerializeField] private GameObject failEffectObject;

        [Header("Comment Input")]
        [SerializeField] private InputField commentInputField;
        [SerializeField] private Text characterCountText;
        [SerializeField] private int maxCharacterCount = 50;

        [Header("Buttons")]
        [SerializeField] private Button sendCommentButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button closeButton;

        private bool _lastThrowSuccess;
        private int _lastThrowCoins;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            SetupInputField();
        }

        private void OnEnable()
        {
            // 結果を読み込み
            _lastThrowSuccess = PlayerPrefs.GetInt("LastThrowSuccess", 0) == 1;
            _lastThrowCoins = PlayerPrefs.GetInt("LastThrowCoins", 0);

            UpdateResultDisplay();
            ClearComment();
        }

        private void SetupButtons()
        {
            if (sendCommentButton != null)
            {
                sendCommentButton.onClick.AddListener(OnSendCommentClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        private void SetupInputField()
        {
            if (commentInputField != null)
            {
                commentInputField.onValueChanged.AddListener(OnCommentChanged);
                commentInputField.characterLimit = maxCharacterCount;
            }
        }

        private void UpdateResultDisplay()
        {
            if (_lastThrowSuccess)
            {
                // 成功時
                if (resultTitleText != null)
                {
                    resultTitleText.text = "成功！";
                    resultTitleText.color = new Color(1f, 0.84f, 0f); // 金色
                }

                if (resultMessageText != null)
                {
                    resultMessageText.text = "ボトルが立ちました！";
                }

                if (coinsEarnedText != null)
                {
                    coinsEarnedText.text = $"+{_lastThrowCoins} コイン";
                    coinsEarnedText.gameObject.SetActive(true);
                }

                if (successEffectObject != null)
                    successEffectObject.SetActive(true);
                if (failEffectObject != null)
                    failEffectObject.SetActive(false);
            }
            else
            {
                // 失敗時
                if (resultTitleText != null)
                {
                    resultTitleText.text = "残念...";
                    resultTitleText.color = Color.white;
                }

                if (resultMessageText != null)
                {
                    resultMessageText.text = "ボトルが倒れてしまいました";
                }

                if (coinsEarnedText != null)
                {
                    coinsEarnedText.gameObject.SetActive(false);
                }

                if (successEffectObject != null)
                    successEffectObject.SetActive(false);
                if (failEffectObject != null)
                    failEffectObject.SetActive(true);
            }

            Debug.Log($"[Comment] Result - Success: {_lastThrowSuccess}, Coins: {_lastThrowCoins}");
        }

        private void OnCommentChanged(string text)
        {
            // 文字数カウント更新
            if (characterCountText != null)
            {
                characterCountText.text = $"{text.Length}/{maxCharacterCount}";

                // 文字数に応じて色を変更
                if (text.Length >= maxCharacterCount)
                {
                    characterCountText.color = Color.red;
                }
                else if (text.Length >= maxCharacterCount * 0.8f)
                {
                    characterCountText.color = new Color(1f, 0.6f, 0f); // オレンジ
                }
                else
                {
                    characterCountText.color = Color.white;
                }
            }

            // 送信ボタンの有効/無効
            if (sendCommentButton != null)
            {
                sendCommentButton.interactable = !string.IsNullOrWhiteSpace(text);
            }
        }

        private void OnSendCommentClicked()
        {
            string comment = commentInputField?.text?.Trim();

            if (string.IsNullOrEmpty(comment))
            {
                Debug.Log("[Comment] Empty comment, skipping");
                CloseAndReturn();
                return;
            }

            // コメント送信
            SendComment(comment);
        }

        private void SendComment(string comment)
        {
            // サーバーに送信
            WebNetworkManager.Instance?.SendComment(comment);

            Debug.Log($"[Comment] Sent: {comment}");

            CloseAndReturn();
        }

        private void OnSkipClicked()
        {
            Debug.Log("[Comment] Skipped");
            CloseAndReturn();
        }

        private void CloseAndReturn()
        {
            // 画面を閉じる
            Close();

            // メイン画面の投げを再有効化
            var mainScreen = FindObjectOfType<MainScreenController>();
            if (mainScreen != null)
            {
                mainScreen.EnableThrow();
            }
        }

        private void ClearComment()
        {
            if (commentInputField != null)
            {
                commentInputField.text = "";
            }
            OnCommentChanged("");
        }

        protected override void OnOpenComplete()
        {
            base.OnOpenComplete();
            Debug.Log("[Comment] Screen opened");
        }

        protected override void OnCloseComplete()
        {
            base.OnCloseComplete();
            ClearComment();
            Debug.Log("[Comment] Screen closed");
        }
    }
}
