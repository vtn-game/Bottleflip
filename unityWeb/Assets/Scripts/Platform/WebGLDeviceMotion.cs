using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BottleFlip.Web.Platform
{
    /// <summary>
    /// WebGL用の加速度センサーブリッジ
    /// JavaScriptのDeviceMotionEvent APIと連携
    /// </summary>
    public class WebGLDeviceMotion : MonoBehaviour
    {
        public static WebGLDeviceMotion Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int DeviceMotion_Initialize(string objectName, string methodName);

        [DllImport("__Internal")]
        private static extern void DeviceMotion_RequestPermission();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationX();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationY();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationZ();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationWithGravityX();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationWithGravityY();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetAccelerationWithGravityZ();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetRotationAlpha();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetRotationBeta();

        [DllImport("__Internal")]
        private static extern float DeviceMotion_GetRotationGamma();

        [DllImport("__Internal")]
        private static extern int DeviceMotion_IsSupported();

        [DllImport("__Internal")]
        private static extern int DeviceMotion_IsPermissionGranted();
#endif

        /// <summary>
        /// 加速度センサーがサポートされているか
        /// </summary>
        public bool IsSupported { get; private set; }

        /// <summary>
        /// パーミッションが許可されているか
        /// </summary>
        public bool IsPermissionGranted { get; private set; }

        /// <summary>
        /// 初期化済みか
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 現在の加速度（重力を除く）
        /// </summary>
        public Vector3 Acceleration { get; private set; }

        /// <summary>
        /// 現在の加速度（重力を含む）
        /// </summary>
        public Vector3 AccelerationWithGravity { get; private set; }

        /// <summary>
        /// 回転速度
        /// </summary>
        public Vector3 RotationRate { get; private set; }

        // イベント
        public event Action<bool> OnPermissionResult;
        public event Action OnInitialized;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (IsInitialized && IsPermissionGranted)
            {
                UpdateAccelerationData();
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int supported = DeviceMotion_Initialize(gameObject.name, "OnPermissionResultCallback");
            IsSupported = supported == 1;
            IsInitialized = true;

            Debug.Log($"[WebGLDeviceMotion] Initialized. Supported: {IsSupported}");
            OnInitialized?.Invoke();
#else
            // エディタまたは非WebGL環境
            IsSupported = SystemInfo.supportsAccelerometer;
            IsPermissionGranted = true;
            IsInitialized = true;
            Debug.Log($"[WebGLDeviceMotion] Non-WebGL mode. Using Input.acceleration");
            OnInitialized?.Invoke();
#endif
        }

        /// <summary>
        /// パーミッション要求（iOS 13+で必要）
        /// </summary>
        public void RequestPermission()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsSupported)
            {
                Debug.LogWarning("[WebGLDeviceMotion] DeviceMotion not supported");
                OnPermissionResult?.Invoke(false);
                return;
            }

            Debug.Log("[WebGLDeviceMotion] Requesting permission...");
            DeviceMotion_RequestPermission();
#else
            // 非WebGL環境ではパーミッション不要
            IsPermissionGranted = true;
            OnPermissionResult?.Invoke(true);
#endif
        }

        /// <summary>
        /// JavaScript からのコールバック
        /// </summary>
        public void OnPermissionResultCallback(string result)
        {
            IsPermissionGranted = result == "granted";
            Debug.Log($"[WebGLDeviceMotion] Permission result: {result}");
            OnPermissionResult?.Invoke(IsPermissionGranted);
        }

        /// <summary>
        /// 加速度データを更新
        /// </summary>
        private void UpdateAccelerationData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: JavaScript から取得
            Acceleration = new Vector3(
                DeviceMotion_GetAccelerationX(),
                DeviceMotion_GetAccelerationY(),
                DeviceMotion_GetAccelerationZ()
            );

            AccelerationWithGravity = new Vector3(
                DeviceMotion_GetAccelerationWithGravityX(),
                DeviceMotion_GetAccelerationWithGravityY(),
                DeviceMotion_GetAccelerationWithGravityZ()
            );

            RotationRate = new Vector3(
                DeviceMotion_GetRotationAlpha(),
                DeviceMotion_GetRotationBeta(),
                DeviceMotion_GetRotationGamma()
            );
#else
            // エディタ/非WebGL: Unity の Input.acceleration を使用
            Acceleration = Input.acceleration;
            AccelerationWithGravity = Input.acceleration;
            RotationRate = Vector3.zero;
#endif
        }

        /// <summary>
        /// パーミッションが必要かどうか（iOS 13+判定）
        /// </summary>
        public bool RequiresPermission()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // iOS Safari 13+ でパーミッションが必要
            // 実際の判定は JavaScript 側で行われる
            return IsSupported && !IsPermissionGranted;
#else
            return false;
#endif
        }
    }
}
