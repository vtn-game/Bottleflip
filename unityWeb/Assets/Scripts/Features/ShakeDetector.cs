using System;
using UnityEngine;
using BottleFlip.Web.Platform;

namespace BottleFlip.Web.Features
{
    /// <summary>
    /// 振り検知結果
    /// </summary>
    public struct ShakeResult
    {
        /// <summary>
        /// 振りの強さ（0-1に正規化）
        /// </summary>
        public float Intensity;

        /// <summary>
        /// ピーク時の加速度ベクトル
        /// </summary>
        public Vector3 Acceleration;

        /// <summary>
        /// 振り継続時間
        /// </summary>
        public float Duration;
    }

    /// <summary>
    /// スマホの振りを検知するクラス（WebGL対応）
    /// </summary>
    public class ShakeDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float shakeThreshold = 2.0f;
        [SerializeField] private float cooldownTime = 1.0f;
        [SerializeField] private float minShakeDuration = 0.1f;

        [Header("Platform Settings")]
        [Tooltip("WebGL用のDeviceMotionを自動的に初期化するか")]
        [SerializeField] private bool autoInitializeWebGL = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        private float lastShakeTime;
        private float shakeStartTime;
        private bool isShaking = false;
        private float peakIntensity = 0f;
        private Vector3 peakAcceleration;
        private Vector3 lastAcceleration;

        private WebGLDeviceMotion webGLMotion;
        private bool isWebGLInitialized = false;

        /// <summary>
        /// 振り検知イベント（加速度ベクトル付き）
        /// </summary>
        public event Action<ShakeResult> OnShakeDetectedWithVector;

        /// <summary>
        /// 振り検知イベント（後方互換）
        /// </summary>
        public event Action<float> OnShakeDetected;

        public event Action OnShakeStarted;
        public event Action OnShakeEnded;

        /// <summary>
        /// パーミッション要求イベント（iOSで必要）
        /// </summary>
        public event Action OnPermissionRequired;

        /// <summary>
        /// パーミッション結果イベント
        /// </summary>
        public event Action<bool> OnPermissionResult;

        /// <summary>
        /// 現在の加速度の大きさ
        /// </summary>
        public float CurrentMagnitude { get; private set; }

        /// <summary>
        /// 現在の加速度ベクトル
        /// </summary>
        public Vector3 CurrentAcceleration { get; private set; }

        /// <summary>
        /// シェイク中かどうか
        /// </summary>
        public bool IsShaking => isShaking;

        /// <summary>
        /// 加速度センサーが利用可能か
        /// </summary>
        public bool IsAccelerometerAvailable { get; private set; }

        /// <summary>
        /// パーミッションが必要か（iOS）
        /// </summary>
        public bool NeedsPermission => webGLMotion != null && webGLMotion.RequiresPermission();

        private void Start()
        {
            InitializePlatform();
        }

        private void Update()
        {
            if (IsAccelerometerAvailable)
            {
                DetectShake();
            }
        }

        /// <summary>
        /// プラットフォーム別の初期化
        /// </summary>
        private void InitializePlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (autoInitializeWebGL)
            {
                InitializeWebGL();
            }
#else
            // エディタまたは非WebGL
            lastAcceleration = Input.acceleration;
            IsAccelerometerAvailable = SystemInfo.supportsAccelerometer;

            if (enableDebugLog)
            {
                Debug.Log($"[ShakeDetector] Non-WebGL mode. Accelerometer: {IsAccelerometerAvailable}");
            }
#endif
        }

        /// <summary>
        /// WebGL用の初期化
        /// </summary>
        public void InitializeWebGL()
        {
            if (isWebGLInitialized) return;

            // WebGLDeviceMotionを探すか作成
            webGLMotion = WebGLDeviceMotion.Instance;
            if (webGLMotion == null)
            {
                var go = new GameObject("WebGLDeviceMotion");
                webGLMotion = go.AddComponent<WebGLDeviceMotion>();
            }

            webGLMotion.OnInitialized += OnWebGLInitialized;
            webGLMotion.OnPermissionResult += OnWebGLPermissionResult;

            isWebGLInitialized = true;

            if (enableDebugLog)
            {
                Debug.Log("[ShakeDetector] WebGL mode initialized");
            }
        }

        private void OnWebGLInitialized()
        {
            IsAccelerometerAvailable = webGLMotion.IsSupported;

            if (webGLMotion.RequiresPermission())
            {
                // iOS 13+ ではユーザーアクションによるパーミッション要求が必要
                OnPermissionRequired?.Invoke();

                if (enableDebugLog)
                {
                    Debug.Log("[ShakeDetector] Permission required. Call RequestPermission() on user action.");
                }
            }
            else
            {
                // パーミッション不要な場合は自動で要求
                webGLMotion.RequestPermission();
            }
        }

        private void OnWebGLPermissionResult(bool granted)
        {
            IsAccelerometerAvailable = granted && webGLMotion.IsSupported;
            OnPermissionResult?.Invoke(granted);

            if (enableDebugLog)
            {
                Debug.Log($"[ShakeDetector] Permission result: {granted}");
            }
        }

        /// <summary>
        /// パーミッションを要求（ユーザーアクション時に呼び出し）
        /// </summary>
        public void RequestPermission()
        {
            if (webGLMotion != null)
            {
                webGLMotion.RequestPermission();
            }
            else
            {
                // 非WebGL環境
                IsAccelerometerAvailable = true;
                OnPermissionResult?.Invoke(true);
            }
        }

        /// <summary>
        /// 振りを検知
        /// </summary>
        private void DetectShake()
        {
            // 加速度を取得
            Vector3 acceleration = GetCurrentAcceleration();
            CurrentAcceleration = acceleration;

            // 加速度の変化量を計算
            Vector3 deltaAcceleration = acceleration - lastAcceleration;
            CurrentMagnitude = deltaAcceleration.magnitude;

            lastAcceleration = acceleration;

            // クールダウン中はスキップ
            if (Time.time - lastShakeTime < cooldownTime)
            {
                return;
            }

            // しきい値を超えた場合
            if (CurrentMagnitude > shakeThreshold)
            {
                if (!isShaking)
                {
                    // シェイク開始
                    isShaking = true;
                    shakeStartTime = Time.time;
                    peakIntensity = CurrentMagnitude;
                    peakAcceleration = deltaAcceleration;
                    OnShakeStarted?.Invoke();

                    if (enableDebugLog)
                    {
                        Debug.Log($"[ShakeDetector] Shake started! Magnitude: {CurrentMagnitude:F2}");
                    }
                }
                else
                {
                    // ピーク更新
                    if (CurrentMagnitude > peakIntensity)
                    {
                        peakIntensity = CurrentMagnitude;
                        peakAcceleration = deltaAcceleration;
                    }
                }
            }
            else if (isShaking)
            {
                // シェイク終了判定
                float shakeDuration = Time.time - shakeStartTime;

                if (shakeDuration >= minShakeDuration)
                {
                    // 有効なシェイクとして処理
                    float intensity = CalculateIntensity(peakIntensity);

                    var result = new ShakeResult
                    {
                        Intensity = intensity,
                        Acceleration = peakAcceleration,
                        Duration = shakeDuration
                    };

                    if (enableDebugLog)
                    {
                        Debug.Log($"[ShakeDetector] Shake detected! Accel: {peakAcceleration}, Intensity: {intensity:F2}");
                    }

                    // イベント発火
                    OnShakeDetectedWithVector?.Invoke(result);
                    OnShakeDetected?.Invoke(intensity);
                    lastShakeTime = Time.time;
                }

                isShaking = false;
                peakIntensity = 0f;
                peakAcceleration = Vector3.zero;
                OnShakeEnded?.Invoke();
            }
        }

        /// <summary>
        /// 現在の加速度を取得（プラットフォーム別）
        /// </summary>
        private Vector3 GetCurrentAcceleration()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (webGLMotion != null && webGLMotion.IsPermissionGranted)
            {
                return webGLMotion.Acceleration;
            }
            return Vector3.zero;
#else
            return Input.acceleration;
#endif
        }

        /// <summary>
        /// 加速度から0-1の強度に変換
        /// </summary>
        private float CalculateIntensity(float magnitude)
        {
            // しきい値からの超過分を正規化
            float excess = magnitude - shakeThreshold;
            float intensity = Mathf.Clamp01(excess / 5.0f);
            return intensity;
        }

        /// <summary>
        /// 手動でシェイクをトリガー（デバッグ用）
        /// </summary>
        public void TriggerManualShake(float intensity = 0.5f)
        {
            TriggerManualShake(new Vector3(0, intensity * 5f, 0));
        }

        /// <summary>
        /// 加速度ベクトルを指定してシェイクをトリガー（デバッグ用）
        /// </summary>
        public void TriggerManualShake(Vector3 acceleration)
        {
            if (Time.time - lastShakeTime < cooldownTime)
            {
                return;
            }

            float intensity = CalculateIntensity(acceleration.magnitude);

            var result = new ShakeResult
            {
                Intensity = intensity,
                Acceleration = acceleration,
                Duration = 0.1f
            };

            OnShakeDetectedWithVector?.Invoke(result);
            OnShakeDetected?.Invoke(intensity);
            lastShakeTime = Time.time;

            Debug.Log($"[ShakeDetector] Manual shake triggered: Accel={acceleration}, Intensity={intensity:F2}");
        }

        private void OnDestroy()
        {
            if (webGLMotion != null)
            {
                webGLMotion.OnInitialized -= OnWebGLInitialized;
                webGLMotion.OnPermissionResult -= OnWebGLPermissionResult;
            }
        }
    }
}
