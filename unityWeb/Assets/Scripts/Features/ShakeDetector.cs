using System;
using UnityEngine;

namespace BottleFlip.Web.Features
{
    /// <summary>
    /// スマホの振りを検知するクラス
    /// </summary>
    public class ShakeDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float shakeThreshold = 2.0f;
        [SerializeField] private float cooldownTime = 1.0f;
        [SerializeField] private float minShakeDuration = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        private float lastShakeTime;
        private float shakeStartTime;
        private bool isShaking = false;
        private float peakIntensity = 0f;

        private Vector3 lastAcceleration;

        public event Action<float> OnShakeDetected;
        public event Action OnShakeStarted;
        public event Action OnShakeEnded;

        /// <summary>
        /// 現在の加速度の大きさ
        /// </summary>
        public float CurrentMagnitude { get; private set; }

        /// <summary>
        /// シェイク中かどうか
        /// </summary>
        public bool IsShaking => isShaking;

        private void Start()
        {
            lastAcceleration = Input.acceleration;

            // WebGLでの加速度センサー確認
#if UNITY_WEBGL && !UNITY_EDITOR
            CheckAccelerometerPermission();
#endif
        }

        private void Update()
        {
            DetectShake();
        }

        private void DetectShake()
        {
            Vector3 acceleration = Input.acceleration;

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

                    if (enableDebugLog)
                    {
                        Debug.Log($"[ShakeDetector] Shake detected! Peak: {peakIntensity:F2}, Intensity: {intensity:F2}");
                    }

                    OnShakeDetected?.Invoke(intensity);
                    lastShakeTime = Time.time;
                }

                isShaking = false;
                peakIntensity = 0f;
                OnShakeEnded?.Invoke();
            }
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

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// WebGLでの加速度センサーパーミッション確認
        /// iOS 13+では明示的な許可が必要
        /// </summary>
        private void CheckAccelerometerPermission()
        {
            // JavaScript経由でDeviceMotionEventの許可を要求
            // 実際のプロジェクトではjslibで実装
            Debug.Log("[ShakeDetector] Checking accelerometer permission for WebGL");
        }
#endif

        /// <summary>
        /// 手動でシェイクをトリガー（デバッグ用）
        /// </summary>
        public void TriggerManualShake(float intensity = 0.5f)
        {
            if (Time.time - lastShakeTime < cooldownTime)
            {
                return;
            }

            OnShakeDetected?.Invoke(intensity);
            lastShakeTime = Time.time;
            Debug.Log($"[ShakeDetector] Manual shake triggered: {intensity:F2}");
        }
    }
}
