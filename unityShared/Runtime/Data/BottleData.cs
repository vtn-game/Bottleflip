using System;
using UnityEngine;

namespace BottleFlip.Data
{
    /// <summary>
    /// ボトル難易度
    /// </summary>
    public enum BottleDifficulty
    {
        Easy,       // かんたん
        Normal,     // ふつう
        Hard,       // むずかしい
        VeryHard    // げきむず
    }

    /// <summary>
    /// ボトルマスタデータ
    /// </summary>
    [Serializable]
    public class BottleMaster
    {
        public string id;
        public string name;
        public int rarity;              // 1-5
        public BottleDifficulty difficulty;
        public int baseCoin;
        public float mass;
        public float centerOfMassOffset;
        public string prefabPath;
        public string spritePath;

        public string DifficultyText
        {
            get
            {
                switch (difficulty)
                {
                    case BottleDifficulty.Easy: return "かんたん";
                    case BottleDifficulty.Normal: return "ふつう";
                    case BottleDifficulty.Hard: return "むずかしい";
                    case BottleDifficulty.VeryHard: return "げきむず";
                    default: return "";
                }
            }
        }

        public string RarityText
        {
            get
            {
                var stars = "";
                for (int i = 0; i < 5; i++)
                {
                    stars += i < rarity ? "★" : "☆";
                }
                return stars;
            }
        }

        /// <summary>
        /// 難易度による物理補正係数
        /// </summary>
        public float DifficultyModifier
        {
            get
            {
                switch (difficulty)
                {
                    case BottleDifficulty.Easy: return 1.0f;
                    case BottleDifficulty.Normal: return 1.1f;
                    case BottleDifficulty.Hard: return 1.25f;
                    case BottleDifficulty.VeryHard: return 1.5f;
                    default: return 1.0f;
                }
            }
        }
    }

    /// <summary>
    /// ボトルマスタデータベース（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "BottleDatabase", menuName = "BottleFlip/Bottle Database")]
    public class BottleDatabase : ScriptableObject
    {
        public BottleMaster[] bottles;

        public BottleMaster GetBottle(string id)
        {
            foreach (var bottle in bottles)
            {
                if (bottle.id == id) return bottle;
            }
            return null;
        }

        public BottleMaster[] GetBottlesByRarity(int rarity)
        {
            return System.Array.FindAll(bottles, b => b.rarity == rarity);
        }
    }
}
