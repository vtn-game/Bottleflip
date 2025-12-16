using System;
using System.Collections;
using UnityEngine;
using BottleFlip.Network;
using BottleFlip.Data;
using BottleFlip.Main.Core;

namespace BottleFlip.Main.Features
{
    /// <summary>
    /// ボトルフリップの物理演算と判定を管理
    /// </summary>
    public class BottleFlipController : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject defaultBottlePrefab;

        [Header("Physics Settings")]
        [SerializeField] private float baseUpwardSpeed = 5.0f;
        [SerializeField] private float intensityMultiplier = 3.0f;
        [SerializeField] private float baseRotationSpeed = 8.0f;

        [Header("Judge Settings")]
        [SerializeField] private float uprightThreshold = 15f;
        [SerializeField] private float stableVelocityThreshold = 0.1f;
        [SerializeField] private float stableTime = 0.5f;
        [SerializeField] private float judgeTimeout = 10f;

        [Header("References")]
        [SerializeField] private MainNetworkManager networkManager;

        // 現在のボトル情報
        private GameObject currentBottle;
        private Rigidbody currentBottleRb;
        private string currentPlayerId;
        private string currentPlayerName;
        private string currentBottleId;
        private bool isJudging = false;

        // イベント
        public event Action<bool, int> OnFlipResult;
        public event Action<string, string> OnBottleSpawned;

        private void Start()
        {
            if (networkManager == null)
            {
                networkManager = MainNetworkManager.Instance;
            }

            networkManager.OnThrowReceived += HandleThrowReceived;
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnThrowReceived -= HandleThrowReceived;
            }
        }

        private void HandleThrowReceived(ThrowData data)
        {
            SpawnAndFlipBottle(data);
        }

        /// <summary>
        /// ボトルを生成してフリップ
        /// </summary>
        public void SpawnAndFlipBottle(ThrowData data)
        {
            if (isJudging)
            {
                Debug.LogWarning("[BottleFlip] Already judging a flip");
                return;
            }

            // 前のボトルがあれば削除
            if (currentBottle != null)
            {
                Destroy(currentBottle);
            }

            // ボトルプレハブ取得（TODO: ボトルIDから取得）
            var prefab = defaultBottlePrefab;

            // 生成
            currentBottle = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            currentBottleRb = currentBottle.GetComponent<Rigidbody>();

            if (currentBottleRb == null)
            {
                currentBottleRb = currentBottle.AddComponent<Rigidbody>();
            }

            // プレイヤー情報保存
            currentPlayerId = data.playerId;
            currentPlayerName = data.playerName;
            currentBottleId = data.bottleId;

            // 初速設定
            ApplyInitialForce(data.intensity);

            // 判定開始
            StartCoroutine(JudgeFlipCoroutine());

            OnBottleSpawned?.Invoke(data.playerName, data.bottleId);
            Debug.Log($"[BottleFlip] Bottle spawned for {data.playerName}");
        }

        private void ApplyInitialForce(float intensity)
        {
            // 上方向速度
            float upwardSpeed = baseUpwardSpeed + intensity * intensityMultiplier;

            // 微小なブレを追加
            Vector3 velocity = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                upwardSpeed,
                UnityEngine.Random.Range(-0.2f, 0.2f)
            );

            // 回転速度
            float rotationSpeed = baseRotationSpeed + intensity * 4f;
            Vector3 angularVelocity = new Vector3(
                rotationSpeed,
                UnityEngine.Random.Range(-1f, 1f),
                0
            );

            currentBottleRb.velocity = velocity;
            currentBottleRb.angularVelocity = angularVelocity;
        }

        private IEnumerator JudgeFlipCoroutine()
        {
            isJudging = true;
            float startTime = Time.time;
            float stableStartTime = 0;
            bool wasStable = false;

            while (Time.time - startTime < judgeTimeout)
            {
                yield return new WaitForFixedUpdate();

                // まだ空中にいる場合はスキップ
                if (!IsGrounded())
                {
                    wasStable = false;
                    continue;
                }

                // 直立判定
                bool isUpright = IsUpright();
                bool isStable = IsStable();

                if (isUpright && isStable)
                {
                    if (!wasStable)
                    {
                        stableStartTime = Time.time;
                        wasStable = true;
                    }

                    // 安定時間チェック
                    if (Time.time - stableStartTime >= stableTime)
                    {
                        // 成功！
                        CompleteFlip(true);
                        yield break;
                    }
                }
                else
                {
                    wasStable = false;

                    // 完全に停止したが倒れている
                    if (IsStable() && !isUpright)
                    {
                        // 失敗
                        CompleteFlip(false);
                        yield break;
                    }
                }
            }

            // タイムアウト = 失敗
            CompleteFlip(false);
        }

        private bool IsGrounded()
        {
            // 下方向にレイキャストして接地判定
            return Physics.Raycast(currentBottle.transform.position, Vector3.down, 0.5f);
        }

        private bool IsUpright()
        {
            float angle = Vector3.Angle(currentBottle.transform.up, Vector3.up);
            return angle <= uprightThreshold;
        }

        private bool IsStable()
        {
            return currentBottleRb.velocity.magnitude < stableVelocityThreshold &&
                   currentBottleRb.angularVelocity.magnitude < stableVelocityThreshold;
        }

        private void CompleteFlip(bool success)
        {
            isJudging = false;

            // コイン計算
            int coinsEarned = 0;
            if (success)
            {
                // TODO: ボトルデータから取得
                coinsEarned = 50;
            }

            // 結果送信
            networkManager.SendThrowResult(currentPlayerId, currentPlayerName, success, coinsEarned);

            OnFlipResult?.Invoke(success, coinsEarned);
            Debug.Log($"[BottleFlip] Result: {(success ? "SUCCESS" : "FAIL")}, Coins: {coinsEarned}");
        }

        /// <summary>
        /// 現在のボトルを取得（コメント表示用）
        /// </summary>
        public GameObject GetCurrentBottle()
        {
            return currentBottle;
        }
    }
}
