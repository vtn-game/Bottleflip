var DeviceMotionPlugin = {

    // 加速度データを保持
    $DeviceMotionState: {
        accelerationX: 0,
        accelerationY: 0,
        accelerationZ: 0,
        accelerationIncludingGravityX: 0,
        accelerationIncludingGravityY: 0,
        accelerationIncludingGravityZ: 0,
        rotationAlpha: 0,
        rotationBeta: 0,
        rotationGamma: 0,
        isSupported: false,
        isPermissionGranted: false,
        callbackObjectName: null,
        callbackMethodName: null
    },

    // 初期化
    DeviceMotion_Initialize: function(objectNamePtr, methodNamePtr) {
        var objectName = UTF8ToString(objectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);

        DeviceMotionState.callbackObjectName = objectName;
        DeviceMotionState.callbackMethodName = methodName;

        // DeviceMotionEvent がサポートされているか確認
        if (window.DeviceMotionEvent) {
            DeviceMotionState.isSupported = true;
            console.log("[DeviceMotion] DeviceMotionEvent is supported");
        } else {
            DeviceMotionState.isSupported = false;
            console.warn("[DeviceMotion] DeviceMotionEvent is NOT supported");
        }

        return DeviceMotionState.isSupported ? 1 : 0;
    },

    // パーミッション要求 (iOS 13+ で必要)
    DeviceMotion_RequestPermission: function() {
        // iOS 13+ では明示的なパーミッション要求が必要
        if (typeof DeviceMotionEvent !== 'undefined' &&
            typeof DeviceMotionEvent.requestPermission === 'function') {

            DeviceMotionEvent.requestPermission()
                .then(function(permissionState) {
                    if (permissionState === 'granted') {
                        DeviceMotionState.isPermissionGranted = true;
                        DeviceMotionPlugin.DeviceMotion_StartListening();
                        console.log("[DeviceMotion] Permission granted");

                        // Unity にコールバック
                        if (DeviceMotionState.callbackObjectName) {
                            SendMessage(DeviceMotionState.callbackObjectName,
                                'OnPermissionResult', 'granted');
                        }
                    } else {
                        DeviceMotionState.isPermissionGranted = false;
                        console.warn("[DeviceMotion] Permission denied");

                        if (DeviceMotionState.callbackObjectName) {
                            SendMessage(DeviceMotionState.callbackObjectName,
                                'OnPermissionResult', 'denied');
                        }
                    }
                })
                .catch(function(error) {
                    console.error("[DeviceMotion] Permission error:", error);

                    if (DeviceMotionState.callbackObjectName) {
                        SendMessage(DeviceMotionState.callbackObjectName,
                            'OnPermissionResult', 'error');
                    }
                });
        } else {
            // iOS 13未満または Android - パーミッション不要
            DeviceMotionState.isPermissionGranted = true;
            DeviceMotionPlugin.DeviceMotion_StartListening();
            console.log("[DeviceMotion] No permission required");

            if (DeviceMotionState.callbackObjectName) {
                SendMessage(DeviceMotionState.callbackObjectName,
                    'OnPermissionResult', 'granted');
            }
        }
    },

    // リスニング開始
    DeviceMotion_StartListening: function() {
        window.addEventListener('devicemotion', function(event) {
            // 加速度 (重力を除く)
            if (event.acceleration) {
                DeviceMotionState.accelerationX = event.acceleration.x || 0;
                DeviceMotionState.accelerationY = event.acceleration.y || 0;
                DeviceMotionState.accelerationZ = event.acceleration.z || 0;
            }

            // 加速度 (重力を含む)
            if (event.accelerationIncludingGravity) {
                DeviceMotionState.accelerationIncludingGravityX =
                    event.accelerationIncludingGravity.x || 0;
                DeviceMotionState.accelerationIncludingGravityY =
                    event.accelerationIncludingGravity.y || 0;
                DeviceMotionState.accelerationIncludingGravityZ =
                    event.accelerationIncludingGravity.z || 0;
            }

            // 回転速度
            if (event.rotationRate) {
                DeviceMotionState.rotationAlpha = event.rotationRate.alpha || 0;
                DeviceMotionState.rotationBeta = event.rotationRate.beta || 0;
                DeviceMotionState.rotationGamma = event.rotationRate.gamma || 0;
            }

        }, true);

        console.log("[DeviceMotion] Started listening");
    },

    // 加速度取得 (重力を除く)
    DeviceMotion_GetAccelerationX: function() {
        return DeviceMotionState.accelerationX;
    },

    DeviceMotion_GetAccelerationY: function() {
        return DeviceMotionState.accelerationY;
    },

    DeviceMotion_GetAccelerationZ: function() {
        return DeviceMotionState.accelerationZ;
    },

    // 加速度取得 (重力を含む)
    DeviceMotion_GetAccelerationWithGravityX: function() {
        return DeviceMotionState.accelerationIncludingGravityX;
    },

    DeviceMotion_GetAccelerationWithGravityY: function() {
        return DeviceMotionState.accelerationIncludingGravityY;
    },

    DeviceMotion_GetAccelerationWithGravityZ: function() {
        return DeviceMotionState.accelerationIncludingGravityZ;
    },

    // 回転速度取得
    DeviceMotion_GetRotationAlpha: function() {
        return DeviceMotionState.rotationAlpha;
    },

    DeviceMotion_GetRotationBeta: function() {
        return DeviceMotionState.rotationBeta;
    },

    DeviceMotion_GetRotationGamma: function() {
        return DeviceMotionState.rotationGamma;
    },

    // サポート状況確認
    DeviceMotion_IsSupported: function() {
        return DeviceMotionState.isSupported ? 1 : 0;
    },

    // パーミッション状況確認
    DeviceMotion_IsPermissionGranted: function() {
        return DeviceMotionState.isPermissionGranted ? 1 : 0;
    }
};

autoAddDeps(DeviceMotionPlugin, '$DeviceMotionState');
mergeInto(LibraryManager.library, DeviceMotionPlugin);
