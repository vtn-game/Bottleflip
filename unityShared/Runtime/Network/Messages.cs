using System;

namespace BottleFlip.Network
{
    // ========== 基本メッセージ ==========

    /// <summary>
    /// クライアント登録メッセージ
    /// </summary>
    [Serializable]
    public class RegisterMessage : NetworkMessage
    {
        public RegisterData data;

        public RegisterMessage(string clientType)
        {
            type = "register";
            data = new RegisterData { clientType = clientType };
        }
    }

    [Serializable]
    public class RegisterData
    {
        public string clientType; // "web" or "main"
    }

    /// <summary>
    /// 認証メッセージ（Webクライアント用）
    /// </summary>
    [Serializable]
    public class AuthMessage : NetworkMessage
    {
        public AuthData data;

        public AuthMessage(string playerId, string playerName)
        {
            type = "auth";
            data = new AuthData { playerId = playerId, playerName = playerName };
        }
    }

    [Serializable]
    public class AuthData
    {
        public string playerId;
        public string playerName;
    }

    // ========== ゲームプレイメッセージ ==========

    /// <summary>
    /// 加速度ベクトル（JSON シリアライズ用）
    /// </summary>
    [Serializable]
    public class AccelerationVector
    {
        public float x;
        public float y;
        public float z;

        public AccelerationVector() { }

        public AccelerationVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public AccelerationVector(UnityEngine.Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public UnityEngine.Vector3 ToVector3()
        {
            return new UnityEngine.Vector3(x, y, z);
        }

        public float Magnitude => UnityEngine.Mathf.Sqrt(x * x + y * y + z * z);
    }

    /// <summary>
    /// ボトル投げメッセージ
    /// </summary>
    [Serializable]
    public class ThrowMessage : NetworkMessage
    {
        public ThrowData data;

        public ThrowMessage(string bottleId, float intensity)
        {
            type = "throw";
            data = new ThrowData { bottleId = bottleId, intensity = intensity };
        }

        public ThrowMessage(string bottleId, UnityEngine.Vector3 acceleration)
        {
            type = "throw";
            data = new ThrowData
            {
                bottleId = bottleId,
                intensity = acceleration.magnitude,
                acceleration = new AccelerationVector(acceleration)
            };
        }
    }

    [Serializable]
    public class ThrowData
    {
        public string playerId;
        public string playerName;
        public string bottleId;
        public float intensity;
        public AccelerationVector acceleration;

        /// <summary>
        /// 加速度ベクトルをVector3として取得
        /// </summary>
        public UnityEngine.Vector3 GetAcceleration()
        {
            if (acceleration != null)
            {
                return acceleration.ToVector3();
            }
            // 後方互換: accelerationがない場合はintensityから上方向ベクトルを生成
            return new UnityEngine.Vector3(0, intensity, 0);
        }
    }

    /// <summary>
    /// 投げ結果メッセージ
    /// </summary>
    [Serializable]
    public class ThrowResultMessage : NetworkMessage
    {
        public ThrowResultData data;

        public ThrowResultMessage(string playerId, string playerName, bool success, int coinsEarned)
        {
            type = "throw_result";
            data = new ThrowResultData
            {
                playerId = playerId,
                playerName = playerName,
                success = success,
                coinsEarned = coinsEarned
            };
        }
    }

    [Serializable]
    public class ThrowResultData
    {
        public string playerId;
        public string playerName;
        public bool success;
        public int coinsEarned;
    }

    /// <summary>
    /// コメントメッセージ
    /// </summary>
    [Serializable]
    public class CommentMessage : NetworkMessage
    {
        public CommentData data;

        public CommentMessage(string text)
        {
            type = "comment";
            data = new CommentData { text = text };
        }
    }

    [Serializable]
    public class CommentData
    {
        public string playerId;
        public string playerName;
        public string text;
    }

    /// <summary>
    /// コメントスキップメッセージ
    /// </summary>
    [Serializable]
    public class SkipCommentMessage : NetworkMessage
    {
        public SkipCommentData data;

        public SkipCommentMessage()
        {
            type = "skip_comment";
            data = new SkipCommentData();
        }
    }

    [Serializable]
    public class SkipCommentData
    {
        public string playerId;
    }

    // ========== サーバーからのメッセージ ==========

    /// <summary>
    /// プレイヤー参加通知
    /// </summary>
    [Serializable]
    public class PlayerJoinedMessage : NetworkMessage
    {
        public PlayerJoinedData data;
    }

    [Serializable]
    public class PlayerJoinedData
    {
        public string playerId;
        public string playerName;
    }

    /// <summary>
    /// プレイヤー離脱通知
    /// </summary>
    [Serializable]
    public class PlayerLeftMessage : NetworkMessage
    {
        public PlayerLeftData data;
    }

    [Serializable]
    public class PlayerLeftData
    {
        public string playerId;
        public string playerName;
    }

    /// <summary>
    /// 登録成功レスポンス
    /// </summary>
    [Serializable]
    public class RegisterSuccessMessage : NetworkMessage
    {
        public RegisterSuccessData data;
    }

    [Serializable]
    public class RegisterSuccessData
    {
        public string connectionId;
        public GameStateData state;
    }

    /// <summary>
    /// ゲーム状態
    /// </summary>
    [Serializable]
    public class GameStateData
    {
        public int webClients;
        public int mainClients;
        public PlayerInfo[] players;
    }

    [Serializable]
    public class PlayerInfo
    {
        public string id;
        public string name;
    }

    // ========== メッセージパーサー ==========

    /// <summary>
    /// メッセージタイプ判定用
    /// </summary>
    [Serializable]
    public class MessageHeader
    {
        public string type;
    }

    public static class MessageParser
    {
        public static string GetMessageType(string json)
        {
            var header = UnityEngine.JsonUtility.FromJson<MessageHeader>(json);
            return header?.type ?? "";
        }

        public static T Parse<T>(string json)
        {
            return UnityEngine.JsonUtility.FromJson<T>(json);
        }
    }
}
