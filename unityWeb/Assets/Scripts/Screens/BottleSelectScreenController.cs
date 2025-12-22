using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BottleFlip.Web.Core;

namespace BottleFlip.Web.Screens
{
    /// <summary>
    /// ボトル選択画面のコントローラー
    /// オーバーレイとしてメイン画面の上に表示
    /// </summary>
    public class BottleSelectScreenController : OverlayScreen
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Button closeButton;

        [Header("Bottle List")]
        [SerializeField] private Transform bottleListContainer;
        [SerializeField] private GameObject bottleItemPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Selected Bottle Preview")]
        [SerializeField] private Image selectedBottleImage;
        [SerializeField] private Text selectedBottleNameText;
        [SerializeField] private Text selectedBottleDescText;
        [SerializeField] private Button selectButton;

        // 所持ボトルリスト（TODO: 実際のデータソースから取得）
        private List<BottleItemData> _ownedBottles = new List<BottleItemData>();
        private List<GameObject> _bottleItemInstances = new List<GameObject>();

        private string _currentSelectedBottleId;
        private string _previewBottleId;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        private void OnEnable()
        {
            // 現在選択中のボトルを取得
            _currentSelectedBottleId = GameManager.Instance?.SelectedBottleId ?? "B001";
            _previewBottleId = _currentSelectedBottleId;

            LoadOwnedBottles();
            PopulateBottleList();
            UpdatePreview(_currentSelectedBottleId);
        }

        private void OnDisable()
        {
            ClearBottleList();
        }

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        private void LoadOwnedBottles()
        {
            // TODO: 実際のインベントリシステムから読み込み
            // 仮データ
            _ownedBottles.Clear();
            _ownedBottles.Add(new BottleItemData
            {
                bottleId = "B001",
                bottleName = "ペットボトル",
                description = "標準的なペットボトル。バランスが良い。",
                rarity = 1,
                isOwned = true
            });
            _ownedBottles.Add(new BottleItemData
            {
                bottleId = "B002",
                bottleName = "牛乳瓶",
                description = "懐かしい牛乳瓶。ちょっと重め。",
                rarity = 2,
                isOwned = true
            });
            _ownedBottles.Add(new BottleItemData
            {
                bottleId = "B003",
                bottleName = "ワインボトル",
                description = "エレガントなワインボトル。細長い形状。",
                rarity = 3,
                isOwned = true
            });

            Debug.Log($"[BottleSelect] Loaded {_ownedBottles.Count} bottles");
        }

        private void PopulateBottleList()
        {
            ClearBottleList();

            if (bottleItemPrefab == null || bottleListContainer == null)
            {
                Debug.LogWarning("[BottleSelect] Missing prefab or container");
                return;
            }

            foreach (var bottle in _ownedBottles)
            {
                var item = Instantiate(bottleItemPrefab, bottleListContainer);
                _bottleItemInstances.Add(item);

                // アイテムのUIを設定
                SetupBottleItem(item, bottle);
            }
        }

        private void SetupBottleItem(GameObject item, BottleItemData data)
        {
            // ボトル名
            var nameText = item.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = data.bottleName;
            }

            // 選択状態の表示
            var selectedIndicator = item.transform.Find("SelectedIndicator");
            if (selectedIndicator != null)
            {
                selectedIndicator.gameObject.SetActive(data.bottleId == _currentSelectedBottleId);
            }

            // レアリティ表示
            var rarityText = item.transform.Find("RarityText")?.GetComponent<Text>();
            if (rarityText != null)
            {
                rarityText.text = new string('★', data.rarity);
                rarityText.color = GetRarityColor(data.rarity);
            }

            // ボタン設定
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                string bottleId = data.bottleId;
                button.onClick.AddListener(() => OnBottleItemClicked(bottleId));
            }
        }

        private void ClearBottleList()
        {
            foreach (var item in _bottleItemInstances)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _bottleItemInstances.Clear();
        }

        private void OnBottleItemClicked(string bottleId)
        {
            _previewBottleId = bottleId;
            UpdatePreview(bottleId);
            UpdateBottleItemSelection();

            Debug.Log($"[BottleSelect] Preview: {bottleId}");
        }

        private void UpdatePreview(string bottleId)
        {
            var bottle = _ownedBottles.Find(b => b.bottleId == bottleId);
            if (bottle.bottleId == null) return;

            if (selectedBottleNameText != null)
            {
                selectedBottleNameText.text = bottle.bottleName;
            }

            if (selectedBottleDescText != null)
            {
                selectedBottleDescText.text = bottle.description;
            }

            // TODO: ボトル画像の設定
            // if (selectedBottleImage != null) { ... }

            // 選択ボタンの状態
            if (selectButton != null)
            {
                // 既に選択中なら無効化
                selectButton.interactable = bottleId != _currentSelectedBottleId;

                var buttonText = selectButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = bottleId == _currentSelectedBottleId ? "選択中" : "このボトルを使う";
                }
            }
        }

        private void UpdateBottleItemSelection()
        {
            for (int i = 0; i < _ownedBottles.Count && i < _bottleItemInstances.Count; i++)
            {
                var item = _bottleItemInstances[i];
                var bottle = _ownedBottles[i];

                // プレビュー中のハイライト
                var highlight = item.transform.Find("Highlight");
                if (highlight != null)
                {
                    highlight.gameObject.SetActive(bottle.bottleId == _previewBottleId);
                }

                // 選択済みインジケーター
                var selectedIndicator = item.transform.Find("SelectedIndicator");
                if (selectedIndicator != null)
                {
                    selectedIndicator.gameObject.SetActive(bottle.bottleId == _currentSelectedBottleId);
                }
            }
        }

        private void OnSelectButtonClicked()
        {
            if (string.IsNullOrEmpty(_previewBottleId)) return;
            if (_previewBottleId == _currentSelectedBottleId) return;

            // ボトルを選択
            _currentSelectedBottleId = _previewBottleId;
            GameManager.Instance?.SetSelectedBottle(_currentSelectedBottleId);

            // UI更新
            UpdatePreview(_currentSelectedBottleId);
            UpdateBottleItemSelection();

            Debug.Log($"[BottleSelect] Selected: {_currentSelectedBottleId}");

            // 選択後に閉じる
            Close();
        }

        private Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                5 => new Color(1f, 0.84f, 0f),     // 金
                4 => new Color(0.8f, 0.5f, 1f),    // 紫
                3 => new Color(0.3f, 0.7f, 1f),    // 青
                2 => new Color(0.3f, 0.9f, 0.3f),  // 緑
                _ => Color.white                   // 白
            };
        }

        protected override void OnOpenComplete()
        {
            base.OnOpenComplete();
            Debug.Log("[BottleSelect] Screen opened");
        }

        protected override void OnCloseComplete()
        {
            base.OnCloseComplete();
            Debug.Log("[BottleSelect] Screen closed");
        }

        /// <summary>
        /// ボトルアイテムデータ
        /// </summary>
        private struct BottleItemData
        {
            public string bottleId;
            public string bottleName;
            public string description;
            public int rarity;
            public bool isOwned;
        }
    }
}
