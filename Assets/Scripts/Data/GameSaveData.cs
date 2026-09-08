using System;
using System.Collections.Generic;
using FarmBloom.Core;

namespace FarmBloom.Data
{
    [Serializable]
    public class LevelStarEntry
    {
        public int level;
        public int stars;
        public int highScore;
    }

    [Serializable]
    public class InventoryItemEntry
    {
        public CropType crop;
        public int count;
    }

    [Serializable]
    public class PlotSaveData
    {
        public int plotId;
        public PlotState state;
        public CropType crop;
        public long plantedEpochSeconds;
        public int growthDurationSeconds;
    }

    [Serializable]
    public class BuildingSaveData
    {
        public BuildingType buildingType;
        public int level;
        public float posX;
        public float posY;
        public bool isConstructed;
    }

    [Serializable]
    public class AnimalSaveData
    {
        public AnimalType animalType;
        public int count;
        public long lastFedEpochSeconds;
    }

    [Serializable]
    public class BoosterEntry
    {
        public BoosterType boosterType;
        public int count;
    }

    [Serializable]
    public class DailyQuestEntry
    {
        public int questId;
        public string titleFr;
        public string titleEn;
        public int currentProgress;
        public int targetGoal;
        public int coinReward;
        public bool isClaimed;
    }

    [Serializable]
    public class GameSaveData
    {
        // Devises & Vies
        public int coins = 1250;
        public int gems = 60;
        public int energy = 5;
        public int maxEnergy = 5;
        public long lastEnergyRefillSeconds = 0;

        // Progression Niveaux Match-3
        public int highestUnlockedLevel = 1;
        public List<LevelStarEntry> levelProgression = new List<LevelStarEntry>();

        // Profil Joueur
        public string playerName = "Fermier Bloom";
        public int avatarId = 0;
        public int playerLevel = 3;
        public int playerXp = 450;

        // Ferme & Simulation
        public int barnCapacity = 100;
        public List<InventoryItemEntry> barnInventory = new List<InventoryItemEntry>();
        public List<PlotSaveData> plots = new List<PlotSaveData>();
        public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
        public List<AnimalSaveData> animals = new List<AnimalSaveData>();

        // Boosters en réserve
        public List<BoosterEntry> boosters = new List<BoosterEntry>();

        // Quotidien, Roue & Coffres
        public int dailyRewardStreak = 1;
        public string lastDailyRewardDate = "";
        public bool freeSpinAvailable = true;
        public int piggyBankCoins = 450;
        public bool isVipActive = false;
        public List<DailyQuestEntry> dailyQuests = new List<DailyQuestEntry>();

        // Paramètres
        public string language = "fr";
        public float soundVolume = 1f;
        public float musicVolume = 0.8f;
        public bool vibrations = true;
        public bool notifications = true;

        public static GameSaveData CreateDefault()
        {
            var data = new GameSaveData();

            // Inventaire de départ
            data.barnInventory.Add(new InventoryItemEntry { crop = CropType.Tomato, count = 25 });
            data.barnInventory.Add(new InventoryItemEntry { crop = CropType.Corn, count = 18 });
            data.barnInventory.Add(new InventoryItemEntry { crop = CropType.Carrot, count = 14 });
            data.barnInventory.Add(new InventoryItemEntry { crop = CropType.Banana, count = 8 });

            // 6 Parcelles par défaut
            for (int i = 0; i < 6; i++)
            {
                data.plots.Add(new PlotSaveData
                {
                    plotId = i,
                    state = (i < 2) ? PlotState.ReadyToHarvest : (i < 4 ? PlotState.Growing : PlotState.Empty),
                    crop = (i < 2) ? CropType.Tomato : (i < 4 ? CropType.Carrot : CropType.None),
                    plantedEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (i < 2 ? 600 : 30),
                    growthDurationSeconds = 120
                });
            }

            // Bâtiments de départ
            data.buildings.Add(new BuildingSaveData { buildingType = BuildingType.FarmHouse, level = 1, posX = 0f, posY = 2f, isConstructed = true });
            data.buildings.Add(new BuildingSaveData { buildingType = BuildingType.Windmill, level = 1, posX = -2.5f, posY = 1.2f, isConstructed = true });
            data.buildings.Add(new BuildingSaveData { buildingType = BuildingType.Barn, level = 1, posX = 2.5f, posY = 1.2f, isConstructed = true });
            data.buildings.Add(new BuildingSaveData { buildingType = BuildingType.Dairy, level = 0, posX = -2.5f, posY = -1f, isConstructed = false });
            data.buildings.Add(new BuildingSaveData { buildingType = BuildingType.ChickenCoop, level = 1, posX = 2.5f, posY = -1f, isConstructed = true });

            // Animaux de départ
            data.animals.Add(new AnimalSaveData { animalType = AnimalType.Chicken, count = 6, lastFedEpochSeconds = 0 });
            data.animals.Add(new AnimalSaveData { animalType = AnimalType.Cow, count = 2, lastFedEpochSeconds = 0 });
            data.animals.Add(new AnimalSaveData { animalType = AnimalType.Goat, count = 1, lastFedEpochSeconds = 0 });
            data.animals.Add(new AnimalSaveData { animalType = AnimalType.Bee, count = 4, lastFedEpochSeconds = 0 });

            // Boosters initiaux
            data.boosters.Add(new BoosterEntry { boosterType = BoosterType.Shovel, count = 3 });
            data.boosters.Add(new BoosterEntry { boosterType = BoosterType.Tractor, count = 2 });
            data.boosters.Add(new BoosterEntry { boosterType = BoosterType.RainbowFertilizer, count = 1 });
            data.boosters.Add(new BoosterEntry { boosterType = BoosterType.ExtraMoves, count = 2 });

            // Quêtes quotidiennes
            data.dailyQuests.Add(new DailyQuestEntry { questId = 1, titleFr = "Récolte 50 carottes", titleEn = "Harvest 50 carrots", currentProgress = 20, targetGoal = 50, coinReward = 200, isClaimed = false });
            data.dailyQuests.Add(new DailyQuestEntry { questId = 2, titleFr = "Gagne 3 niveaux Match-3", titleEn = "Win 3 Match-3 levels", currentProgress = 1, targetGoal = 3, coinReward = 350, isClaimed = false });
            data.dailyQuests.Add(new DailyQuestEntry { questId = 3, titleFr = "Fais 2 combos géniaux", titleEn = "Make 2 awesome combos", currentProgress = 2, targetGoal = 2, coinReward = 500, isClaimed = true });

            return data;
        }
    }
}
