using System;
using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;

namespace FarmBloom.Farm
{
    [Serializable]
    public class CropInfo
    {
        public CropType Crop;
        public string NameFr;
        public string NameEn;
        public int SeedPriceCoins;
        public int GrowthDurationSeconds;
        public int HarvestYield;
        public int XpReward;
        public int SellPriceCoins;
    }

    public static class FarmCropDatabase
    {
        private static readonly Dictionary<CropType, CropInfo> _crops = new Dictionary<CropType, CropInfo>
        {
            {
                CropType.Tomato,
                new CropInfo
                {
                    Crop = CropType.Tomato,
                    NameFr = "Tomate",
                    NameEn = "Tomato",
                    SeedPriceCoins = 25,
                    GrowthDurationSeconds = 60,
                    HarvestYield = 3,
                    XpReward = 15,
                    SellPriceCoins = 18
                }
            },
            {
                CropType.Carrot,
                new CropInfo
                {
                    Crop = CropType.Carrot,
                    NameFr = "Carotte",
                    NameEn = "Carrot",
                    SeedPriceCoins = 15,
                    GrowthDurationSeconds = 30,
                    HarvestYield = 2,
                    XpReward = 10,
                    SellPriceCoins = 12
                }
            },
            {
                CropType.Corn,
                new CropInfo
                {
                    Crop = CropType.Corn,
                    NameFr = "Maïs",
                    NameEn = "Corn",
                    SeedPriceCoins = 35,
                    GrowthDurationSeconds = 90,
                    HarvestYield = 4,
                    XpReward = 25,
                    SellPriceCoins = 22
                }
            },
            {
                CropType.Banana,
                new CropInfo
                {
                    Crop = CropType.Banana,
                    NameFr = "Banane",
                    NameEn = "Banana",
                    SeedPriceCoins = 50,
                    GrowthDurationSeconds = 120,
                    HarvestYield = 5,
                    XpReward = 35,
                    SellPriceCoins = 30
                }
            },
            {
                CropType.SweetPotato,
                new CropInfo
                {
                    Crop = CropType.SweetPotato,
                    NameFr = "Patate douce",
                    NameEn = "Sweet Potato",
                    SeedPriceCoins = 40,
                    GrowthDurationSeconds = 80,
                    HarvestYield = 3,
                    XpReward = 20,
                    SellPriceCoins = 26
                }
            }
        };

        public static CropInfo Get(CropType crop)
        {
            if (_crops.TryGetValue(crop, out var info)) return info;
            return _crops[CropType.Carrot];
        }

        public static IEnumerable<CropInfo> GetAll() => _crops.Values;
    }
}
