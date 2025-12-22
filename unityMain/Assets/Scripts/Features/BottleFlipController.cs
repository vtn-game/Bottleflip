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

        [Header("Physics Settings - Velocity")]
        [Tooltip("加速度から速度への変換係数")]
        [SerializeField] private float accelerationToVelocityScale = 1.5f;
        [Tooltip("上方向への基本速度")]
        [SerializeField] private float baseUpwardSpeed = 3.0f;
        [Tooltip("上方向への追加速度係数")]
        [SerializeField] private float upwardSpeedMultiplier = 2.0f;
        [Tooltip("水平方向の速度スケール")]
        [SerializeField] private float horizontalScale = 0.5f;
        [Tooltip("最大水平速度")]
        [SerializeField] private float maxHorizontalSpeed = 3.0f;
        [Tooltip("最大垂直速度")]
        [SerializeField] private float maxVerticalSpeed = 10.0f;

        [Header("Physics Settings - Rotation")]
        [Tooltip("基本回転速度 (rad/s)")]
        [SerializeField] private float baseRotationSpeed = 6.0f;
        [Tooltip("加速度による追加回転係数")]
        [SerializeField] private float rotationMultiplier = 3.0f;
        [Tooltip("ランダム回転の範囲")]
        [SerializeField] private float randomRotationRange = 1.5f;

        [Header("Judge Settings")]
        [Tooltip("成功と判定する最大傾き角度")]
        [SerializeField] private float uprightThreshold = 15f;
        [Tooltip("安定と判定する速度しきい値")]
        [SerializeField] private float stableVelocityThreshold = 0.1f;
        [Tooltip("安定状態を維持する必要時間")]
        [SerializeField] private float stableTime = 0.5f;
        [Tooltip("判定タイムアウト時間")]
        [SerializeField] private float judgeTimeout = 10f;

        [Header("References")]
        [SerializeField] private MainNetworkManager networkManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool logDebugInfo = true;

        [Header("Standalone Input")]
        [Tooltip("母艦アプリ単体でボトルを投げる機能を有効にする")]
        [SerializeField] private bool enableStandaloneInput = true;
        [Tooltip("投げる時のキー")]
        [SerializeField] private KeyCode throwKey = KeyCode.Space;
        [Tooltip("デフォルトの投げる強さ")]
        [SerializeField] private float defaultThrowIntensity = 5.0f;
        [Tooltip("投げる強さのランダム範囲")]
        [SerializeField] private float throwIntensityRandomRange = 2.0f;

        // 現在のボトル情報
        private GameObject currentBottle;
        private Rigidbody currentBottleRb;
        private string currentPlayerId;
        private string currentPlayerName;
        private string currentBottleId;
        private bool isJudging = false;

        // デバッグ用
        private Vector3 lastAppliedVelocity;
        private Vector3 lastAppliedAngularVelocity;
        private Vector3 lastAcceleration;

        // イベント
        public event Action<bool, int> OnFlipResult;
        public event Action<string, string> OnBottleSpawned;

        private void Start()
        {
            if (networkManager == null)
            {
                networkManager = MainNetworkManager.Instance;
            }

            if (networkManager != null)
            {
                networkManager.OnThrowReceived += HandleThrowReceived;
            }
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnThrowReceived -= HandleThrowReceived;
            }
        }

        private void Update()
        {
            // 母艦アプリ単体でのボトル投げ入力
            if (enableStandaloneInput && Input.GetKeyDown(throwKey))
            {
                ThrowBottleStandalone();
            }
        }

        /// <summary>
        /// 母艦アプリ単体でボトルを投げる
        /// </summary>
        public void ThrowBottleStandalone()
        {
            if (isJudging)
            {
                if (logDebugInfo)
                {
                    Debug.Log("[BottleFlip] Cannot throw - already judging a flip");
                }
                return;
            }

            // ランダムな加速度を生成（上方向メインで少し水平方向も）
            float intensity = defaultThrowIntensity + UnityEngine.Random.Range(-throwIntensityRandomRange, throwIntensityRandomRange);
            float horizontalX = UnityEngine.Random.Range(-1f, 1f);
            float horizontalZ = UnityEngine.Random.Range(-0.5f, 0.5f);
            Vector3 acceleration = new Vector3(horizontalX, intensity, horizontalZ);

            ThrowBottleWithAcceleration(acceleration, "Standalone");

            if (logDebugInfo)
            {
                Debug.Log($"[BottleFlip] Standalone throw - Intensity: {intensity:F2}, Accel: {acceleration}");
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
            if (prefab == null)
            {
                Debug.LogError("[BottleFlip] No bottle prefab assigned!");
                return;
            }

            // 生成
            currentBottle = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            currentBottleRb = currentBottle.GetComponent<Rigidbody>();

            if (currentBottleRb == null)
            {
                currentBottleRb = currentBottle.AddComponent<Rigidbody>();
                currentBottleRb.mass = 0.5f;
                currentBottleRb.drag = 0.5f;
                currentBottleRb.angularDrag = 0.5f;
            }

            // プレイヤー情報保存
            currentPlayerId = data.playerId;
            currentPlayerName = data.playerName;
            currentBottleId = data.bottleId;

            // 加速度ベクトルを取得して初速設定
            Vector3 acceleration = data.GetAcceleration();
            ApplyInitialForceFromAcceleration(acceleration, data.intensity);

            // 判定開始
            StartCoroutine(JudgeFlipCoroutine());

            OnBottleSpawned?.Invoke(data.playerName, data.bottleId);

            if (logDebugInfo)
            {
                Debug.Log($"[BottleFlip] Bottle spawned for {data.playerName}, Accel: {acceleration}, Intensity: {data.intensity:F2}");
            }
        }

        /// <summary>
        /// 加速度ベクトルに応じた初速を適用
        /// </summary>
        private void ApplyInitialForceFromAcceleration(Vector3 acceleration, float intensity)
        {
            lastAcceleration = acceleration;

            // === 速度計算 ===
            // スマホの加速度座標系をゲーム空間に変換
            // スマホ: X=右, Y=上, Z=前（画面から手前へ）
            // Unity: X=右, Y=上, Z=前

            // 加速度の大きさから基本速度を算出
            float accelMagnitude = acceleration.magnitude;

            // 上方向速度: 基本速度 + 加速度の大きさに応じた追加
            float upwardSpeed = baseUpwardSpeed + accelMagnitude * upwardSpeedMultiplier;
            upwardSpeed = Mathf.Clamp(upwardSpeed, baseUpwardSpeed, maxVerticalSpeed);

            // 水平方向速度: 加速度のXZ成分から算出
            // スマホを振る方向によって水平移動が変わる
            float horizontalX = acceleration.x * horizontalScale * accelerationToVelocityScale;
            float horizontalZ = acceleration.z * horizontalScale * accelerationToVelocityScale;

            // 水平速度をクランプ
            Vector2 horizontal = new Vector2(horizontalX, horizontalZ);
            if (horizontal.magnitude > maxHorizontalSpeed)
            {
                horizontal = horizontal.normalized * maxHorizontalSpeed;
            }

            Vector3 velocity = new Vector3(horizontal.x, upwardSpeed, horizontal.y);

            // === 回転計算 ===
            // 前方向への回転（X軸回転）がメインのフリップ
            // 加速度が大きいほど回転が速い
            float flipRotation = baseRotationSpeed + accelMagnitude * rotationMultiplier;

            // 加速度の水平成分から回転軸を微調整
            // 左右に振られた場合は少しひねりが入る
            float twistY = acceleration.x * randomRotationRange;
            float tiltZ = UnityEngine.Random.Range(-randomRotationRange * 0.5f, randomRotationRange * 0.5f);

            Vector3 angularVelocity = new Vector3(flipRotation, twistY, tiltZ);

            // === 適用 ===
            currentBottleRb.velocity = velocity;
            currentBottleRb.angularVelocity = angularVelocity;

            // デバッグ用に保存
            lastAppliedVelocity = velocity;
            lastAppliedAngularVelocity = angularVelocity;

            if (logDebugInfo)
            {
                Debug.Log($"[BottleFlip] Applied - Velocity: {velocity}, AngularVelocity: {angularVelocity}");
            }
        }

        /// <summary>
        /// 加速度ベクトルから直接ボトルを投げる（テスト用）
        /// </summary>
        public void ThrowBottleWithAcceleration(Vector3 acceleration, string testPlayerName = "TestPlayer")
        {
            var testData = new ThrowData
            {
                playerId = "test",
                playerName = testPlayerName,
                bottleId = "B001",
                intensity = acceleration.magnitude,
                acceleration = new AccelerationVector(acceleration)
            };

            SpawnAndFlipBottle(testData);
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

                if (currentBottle == null)
                {
                    isJudging = false;
                    yield break;
                }

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
            if (currentBottle == null) return false;

            // 下方向にレイキャストして接地判定
            // ボトルの底面からレイを飛ばす
            var collider = currentBottle.GetComponent<Collider>();
            float rayLength = collider != null ? collider.bounds.extents.y + 0.1f : 0.5f;

            return Physics.Raycast(currentBottle.transform.position, Vector3.down, rayLength);
        }

        private bool IsUpright()
        {
            if (currentBottle == null) return false;

            float angle = Vector3.Angle(currentBottle.transform.up, Vector3.up);
            return angle <= uprightThreshold;
        }

        private bool IsStable()
        {
            if (currentBottleRb == null) return false;

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
            if (networkManager != null)
            {
                networkManager.SendThrowResult(currentPlayerId, currentPlayerName, success, coinsEarned);
            }

            OnFlipResult?.Invoke(success, coinsEarned);

            if (logDebugInfo)
            {
                Debug.Log($"[BottleFlip] Result: {(success ? "SUCCESS" : "FAIL")}, Coins: {coinsEarned}");
            }
        }

        /// <summary>
        /// 現在のボトルを取得（コメント表示用）
        /// </summary>
        public GameObject GetCurrentBottle()
        {
            return currentBottle;
        }

        /// <summary>
        /// 判定中かどうか
        /// </summary>
        public bool IsJudging => isJudging;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // スポーンポイント表示
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 0.5f);
            }

            // 最後に適用した速度を表示
            if (currentBottle != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(currentBottle.transform.position,
                    currentBottle.transform.position + lastAppliedVelocity * 0.3f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(currentBottle.transform.position,
                    currentBottle.transform.position + lastAcceleration * 0.3f);
            }
        }
#endif
    }
}
