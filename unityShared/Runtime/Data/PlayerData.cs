using System;
using UnityEngine;

namespace BottleFlip.Data
{
    /// <summary>
    /// プレイヤーデータ
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string id;
        public string name;
        public int coins;
        public string selectedBottleId;
        public string[] ownedBottleIds;

        public PlayerData()
        {
            id = Guid.NewGuid().ToString();
            coins = 0;
            selectedBottleId = "B001";
            ownedBottleIds = new string[] { "B001" };
        }
    }

    /// <summary>
    /// ローカルストレージ用プレイヤーデータ
    /// </summary>
    [Serializable]
    public class LocalPlayerData
    {
        public string playerId;
        public string playerName;
        public string selectedBottleId;

        private const string PREFS_KEY = "BottleFlip_LocalPlayer";

        public static LocalPlayerData Load()
        {
            var json = PlayerPrefs.GetString(PREFS_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonUtility.FromJson<LocalPlayerData>(json);
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            PlayerPrefs.Save();
        }
    }
}
