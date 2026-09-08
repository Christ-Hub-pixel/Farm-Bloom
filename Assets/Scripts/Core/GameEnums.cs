namespace FarmBloom.Core
{

    public enum SpecialTileType
    {
        None = 0,
        LineHorizontal = 1,  // Alignement de 4 horizontal
        LineVertical = 2,    // Alignement de 4 vertical
        Bomb3x3 = 3,         // Forme en L ou T (Panier explosif)
        RainbowFlower = 4    // Alignement de 5 (Super fleur qui nettoie une couleur)
    }

    public enum BoosterType
    {
        Shovel = 0,            // Détruit une tuile ciblée
        Tractor = 1,           // Nettoie une ligne ou colonne entière
        RainbowFertilizer = 2, // Transforme en super-fleur
        ExtraMoves = 3         // Ajoute +5 coups en jeu
    }

    public enum GameScreen
    {
        Splash,
        Auth,
        WorldMap,
        PreLevelGoal,
        Match3Game,
        FarmView,
        BuildingCatalog,
        BarnStorage,
        Shop,
        DailyRewards,
        SpinWheel,
        ChestOpening,
        SeasonPass,
        SocialHub,
        Leaderboard,
        DailyQuests,
        Profile,
        Club,
        Settings,
        AudioSettings
    }

    public enum BuildingType
    {
        FarmHouse,   // Maison principale
        Windmill,    // Moulin (blé -> farine)
        Dairy,       // Fromagerie (lait -> fromage)
        Barn,        // Grange (stockage de récoltes)
        ChickenCoop, // Poulailler (œufs)
        Bakery       // Boulangerie (farine -> pain)
    }

    public enum AnimalType
    {
        Chicken, // Poule -> Œufs
        Cow,     // Vache -> Lait
        Goat,    // Chèvre -> Lait de chèvre
        Bee      // Abeille -> Miel
    }

    public enum PlotState
    {
        Empty = 0,
        Planted = 1,
        Growing = 2,
        ReadyToHarvest = 3
    }

    public enum RewardType
    {
        Coins,
        Gems,
        Energy,
        Booster,
        CropResource
    }
}
