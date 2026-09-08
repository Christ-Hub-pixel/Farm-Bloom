using System;
using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;

namespace FarmBloom.Match3
{
    [Serializable]
    public class CropTarget
    {
        public CropType Crop;
        public int TargetCount;
        public int CurrentCollected;
        public bool IsCompleted => CurrentCollected >= TargetCount;
    }

    [CreateAssetMenu(fileName = "Level_01", menuName = "FarmBloom/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int LevelNumber = 1;
        public int MaxMoves = 25;
        public int OneStarScore = 10000;
        public int TwoStarScore = 25000;
        public int ThreeStarScore = 45000;

        public List<CropTarget> Targets = new List<CropTarget>();
        public List<CropType> AvailableCrops = new List<CropType>();

        public static LevelData CreateDefaultLevel(int level)
        {
            var data = ScriptableObject.CreateInstance<LevelData>();
            data.LevelNumber = level;
            data.MaxMoves = Mathf.Max(18, 25 - (level / 5));
            data.OneStarScore = 8000 + level * 2000;
            data.TwoStarScore = 20000 + level * 5000;
            data.ThreeStarScore = 40000 + level * 10000;

            data.AvailableCrops.Add(CropType.Tomato);
            data.AvailableCrops.Add(CropType.Corn);
            data.AvailableCrops.Add(CropType.Carrot);
            data.AvailableCrops.Add(CropType.Banana);
            data.AvailableCrops.Add(CropType.Flower);

            // Objectifs dynamiques inspirés de la maquette (ex: 20 tomates, 15 carottes)
            data.Targets.Add(new CropTarget { Crop = CropType.Tomato, TargetCount = 15 + (level * 3) });
            data.Targets.Add(new CropTarget { Crop = CropType.Carrot, TargetCount = 10 + (level * 2) });
            if (level >= 3)
            {
                data.Targets.Add(new CropTarget { Crop = CropType.Corn, TargetCount = 12 });
            }

            return data;
        }
    }
}
