using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Farm;
using FarmBloom.Match3;
using FarmBloom.Utils;

#pragma warning disable IDE0130 // Désactive l'info IDE0130 (Namespace conventionnel Unity)
namespace FarmBloom.UI
{
    public class UIBuilder : MonoBehaviour
    {
        private Canvas _canvas;
        private UIManager _uiManager;
        private Font _defaultFont;

        private void Awake()
        {
            _uiManager = GetComponent<UIManager>();
            _uiManager ??= gameObject.AddComponent<UIManager>();

            _canvas = GetComponentInParent<Canvas>();
            _canvas ??= FindAnyObjectByType<Canvas>();

            // Police par défaut
            _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _defaultFont ??= Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        private void Start()
        {
            BuildAllInterfaces();
        }

        public void BuildAllInterfaces()
        {
            if (_canvas == null) return;

            Transform root = _canvas.transform;

            // 1. Top HUD persistant
            CreateTopHUD(root);

            // 2. Bottom Navigation persistant
            CreateBottomNav(root);

            // 3. Écrans Principaux
            _uiManager.SplashScreen = CreateSplashScreen(root);
            _uiManager.AuthScreen = CreateAuthScreen(root);
            _uiManager.WorldMapScreen = CreateWorldMapScreen(root);
            _uiManager.Match3GameScreen = CreateMatch3Screen(root);
            _uiManager.FarmViewScreen = CreateFarmViewScreen(root);
            _uiManager.ShopScreen = CreateShopScreen(root);
            _uiManager.SocialScreen = CreateSocialScreen(root);

            // 4. Modales & Popups
            _uiManager.PreLevelModal = CreatePreLevelModal(root);
            _uiManager.PauseModal = CreatePauseModal(root);
            _uiManager.VictoryModal = CreateVictoryModal(root);
            _uiManager.DefeatModal = CreateDefeatModal(root);
            _uiManager.ComboModal = CreateComboModal(root);
            _uiManager.DailyRewardModal = CreateDailyRewardModal(root);
            _uiManager.SpinWheelModal = CreateSpinWheelModal(root);
            _uiManager.ChestModal = CreateChestModal(root);
            _uiManager.BarnModal = CreateBarnModal(root);
            _uiManager.PlantSeedModal = CreatePlantSeedModal(root);
            _uiManager.BuildingUpgradeModal = CreateBuildingUpgradeModal(root);
            _uiManager.AnimalsModal = CreateAnimalsModal(root);
            _uiManager.SeasonPassModal = CreateSeasonPassModal(root);
            _uiManager.PiggyBankModal = CreatePiggyBankModal(root);
            _uiManager.ProfileModal = CreateProfileModal(root);
            _uiManager.DailyQuestsModal = CreateDailyQuestsModal(root);
            _uiManager.LeaderboardModal = CreateLeaderboardModal(root);
            _uiManager.ClubModal = CreateClubModal(root);
            _uiManager.SettingsModal = CreateSettingsModal(root);

            // Fermer toutes les modales au démarrage et afficher l'écran d'accueil
            _uiManager.CloseAllModals();
            _uiManager.UpdateHUDCurrencies();
            GameManager.Instance?.SwitchScreen(GameScreen.Splash);
        }

        // ==================== 1. TOP HUD ====================

        private GameObject CreateTopHUD(Transform parent)
        {
            GameObject hud = CreateUIObject("TopHUD", parent);
            RectTransform rt = hud.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 110f);
            rt.anchoredPosition = Vector2.zero;

            // Fond doux
            Image bg = hud.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.22f, 0.12f, 0.85f);

            // Profil Avatar (Gauche)
            GameObject avatarBtn = CreateButton("AvatarBtn", hud.transform, new Vector2(-420f, 0f), new Vector2(90f, 90f), () => _uiManager.ShowProfileModal());
            Image avatarImg = avatarBtn.GetComponent<Image>();
            avatarImg.color = new Color(0.95f, 0.75f, 0.35f);
            CreateLabel("AvatarText", avatarBtn.transform, "Niv.3", 22, Color.black, TextAnchor.MiddleCenter);

            // Compteur Pièces (Centre Gauche)
            GameObject coinsObj = CreateButton("CoinsMeter", hud.transform, new Vector2(-220f, 0f), new Vector2(180f, 65f), () => GameManager.Instance?.SwitchScreen(GameScreen.Shop));
            coinsObj.GetComponent<Image>().color = new Color(0.25f, 0.18f, 0.08f, 0.9f);
            _uiManager.CoinsText = CreateLabel("CoinsVal", coinsObj.transform, "1 250", 26, new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleCenter);

            // Compteur Gemmes (Centre Droite)
            GameObject gemsObj = CreateButton("GemsMeter", hud.transform, new Vector2(-20f, 0f), new Vector2(160f, 65f), () => GameManager.Instance?.SwitchScreen(GameScreen.Shop));
            gemsObj.GetComponent<Image>().color = new Color(0.2f, 0.1f, 0.28f, 0.9f);
            _uiManager.GemsText = CreateLabel("GemsVal", gemsObj.transform, "60", 26, new Color(0.85f, 0.45f, 1f), TextAnchor.MiddleCenter);

            // Compteur Vies / Énergie (Droite)
            GameObject energyObj = CreateButton("EnergyMeter", hud.transform, new Vector2(170f, 0f), new Vector2(160f, 65f), () => CurrencyManager.Instance.RefillEnergy());
            energyObj.GetComponent<Image>().color = new Color(0.28f, 0.1f, 0.1f, 0.9f);
            _uiManager.EnergyText = CreateLabel("EnergyVal", energyObj.transform, "5/5", 26, new Color(1f, 0.35f, 0.45f), TextAnchor.MiddleCenter);

            // Tirelire / Coffre-fort (Bouton doré)
            GameObject vaultBtn = CreateButton("VaultBtn", hud.transform, new Vector2(340f, 0f), new Vector2(75f, 75f), () => _uiManager.ShowPiggyBankModal());
            vaultBtn.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f);
            CreateLabel("VaultTxt", vaultBtn.transform, "Tirelire", 16, Color.black, TextAnchor.MiddleCenter);

            // Bouton Paramètres (Extrême Droite)
            GameObject settBtn = CreateButton("SettingsBtn", hud.transform, new Vector2(450f, 0f), new Vector2(75f, 75f), () => _uiManager.ShowSettingsModal());
            settBtn.GetComponent<Image>().color = new Color(0.45f, 0.55f, 0.65f);
            CreateLabel("SettTxt", settBtn.transform, "⚙", 36, Color.white, TextAnchor.MiddleCenter);

            return hud;
        }

        // ==================== 2. BOTTOM NAV ====================

        private GameObject CreateBottomNav(Transform parent)
        {
            GameObject nav = CreateUIObject("BottomNav", parent);
            RectTransform rt = nav.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 130f);
            rt.anchoredPosition = Vector2.zero;

            Image bg = nav.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.12f, 0.08f, 0.95f); // Bois sombre chaleureux

            // 5 Boutons de menu
            string[] tabs = { "Ferme", "Puzzles", "Grange", "Boutique", "Social" };
            Action[] actions = {
                () => GameManager.Instance?.SwitchScreen(GameScreen.FarmView),
                () => GameManager.Instance?.SwitchScreen(GameScreen.WorldMap),
                () => _uiManager.ShowBarnModal(),
                () => GameManager.Instance?.SwitchScreen(GameScreen.Shop),
                () => _uiManager.ShowDailyQuestsModal()
            };

            float startX = -400f;
            float stepX = 200f;

            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                GameObject btn = CreateButton($"NavBtn_{tabs[i]}", nav.transform, new Vector2(startX + i * stepX, 0f), new Vector2(170f, 90f), actions[idx]);
                btn.GetComponent<Image>().color = (i == 0 || i == 1) ? new Color(0.35f, 0.72f, 0.18f) : new Color(0.32f, 0.25f, 0.18f);
                CreateLabel("NavTxt", btn.transform, tabs[i], 24, Color.white, TextAnchor.MiddleCenter);
            }

            return nav;
        }

        // ==================== 3. ÉCRANS PRINCIPAUX ====================

        private GameObject CreateSplashScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("SplashScreen", parent, new Color(0.15f, 0.45f, 0.15f));

            // Titre & Logo Farm Bloom
            CreateLabel("Title", screen.transform, "FARM BLOOM", 84, new Color(1f, 0.92f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0f, 250f));
            CreateLabel("Tagline", screen.transform, "RÉCOLTE • CONSTRUIS • PROGRESSE", 32, new Color(0.95f, 0.95f, 0.9f), TextAnchor.MiddleCenter, new Vector2(0f, 170f));

            // Mascotte Poussin
            GameObject mascot = CreateUIObject("MascotBox", screen.transform);
            mascot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);
            mascot.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 260f);
            Image mImg = mascot.AddComponent<Image>();
            mImg.color = new Color(1f, 0.92f, 0.5f);
            CreateLabel("MascotTxt", mascot.transform, "🐥\nBienvenue à la Ferme !", 34, new Color(0.4f, 0.25f, 0.1f), TextAnchor.MiddleCenter);

            // Bouton JOUER
            GameObject playBtn = CreateButton("PlayBtn", screen.transform, new Vector2(0f, -260f), new Vector2(380f, 110f), () => GameManager.Instance?.SwitchScreen(GameScreen.WorldMap));
            playBtn.GetComponent<Image>().color = new Color(0.35f, 0.85f, 0.18f);
            CreateLabel("PlayTxt", playBtn.transform, "JOUER !", 46, Color.white, TextAnchor.MiddleCenter);

            // Bouton Se connecter
            GameObject loginBtn = CreateButton("LoginBtn", screen.transform, new Vector2(0f, -380f), new Vector2(260f, 65f), () => GameManager.Instance?.SwitchScreen(GameScreen.Auth));
            loginBtn.GetComponent<Image>().color = new Color(0.2f, 0.35f, 0.2f, 0.8f);
            CreateLabel("LoginTxt", loginBtn.transform, "Se connecter", 24, Color.white, TextAnchor.MiddleCenter);

            return screen;
        }

        private GameObject CreateAuthScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("AuthScreen", parent, new Color(0.12f, 0.25f, 0.12f));

            CreateLabel("AuthTitle", screen.transform, "Bienvenue !", 56, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 320f));
            CreateLabel("AuthSub", screen.transform, "Connecte-toi pour sauvegarder ta progression", 26, new Color(0.8f, 0.9f, 0.8f), TextAnchor.MiddleCenter, new Vector2(0f, 250f));

            string[] authMethods = { "Continuer avec Google", "Continuer avec Facebook", "Email & Mot de passe", "Mode Invité" };
            Color[] authColors = { new(0.85f, 0.25f, 0.2f), new(0.25f, 0.45f, 0.85f), new(0.35f, 0.35f, 0.35f), new(0.35f, 0.72f, 0.25f) };

            for (int i = 0; i < authMethods.Length; i++)
            {
                int idx = i;
                GameObject btn = CreateButton($"AuthBtn_{i}", screen.transform, new Vector2(0f, 110f - i * 110f), new Vector2(460f, 85f), () => GameManager.Instance?.SwitchScreen(GameScreen.WorldMap));
                btn.GetComponent<Image>().color = authColors[idx];
                CreateLabel("AuthTxt", btn.transform, authMethods[idx], 26, Color.white, TextAnchor.MiddleCenter);
            }

            return screen;
        }

        private GameObject CreateWorldMapScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("WorldMapScreen", parent, new Color(0.25f, 0.65f, 0.35f));

            // Bannière Région
            GameObject regionBanner = CreateUIObject("RegionBanner", screen.transform);
            regionBanner.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 420f);
            regionBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 90f);
            regionBanner.AddComponent<Image>().color = new Color(0.55f, 0.32f, 0.15f);
            CreateLabel("RegionTxt", regionBanner.transform, "Région 1 : Prairies Vertes", 32, new Color(1f, 0.95f, 0.7f), TextAnchor.MiddleCenter);

            // Boutons d'accès rapide : Roue & Récompense 7 jours
            GameObject spinBtn = CreateButton("SpinBtn", screen.transform, new Vector2(-420f, 320f), new Vector2(140f, 90f), () => _uiManager.ShowSpinWheelModal());
            spinBtn.GetComponent<Image>().color = new Color(0.85f, 0.45f, 0.15f);
            CreateLabel("SpinTxt", spinBtn.transform, "🎡\nRoue", 22, Color.white, TextAnchor.MiddleCenter);

            GameObject dailyBtn = CreateButton("DailyBtn", screen.transform, new Vector2(-420f, 210f), new Vector2(140f, 90f), () => _uiManager.ShowDailyRewardModal());
            dailyBtn.GetComponent<Image>().color = new Color(0.25f, 0.65f, 0.85f);
            CreateLabel("DailyTxt", dailyBtn.transform, "📅\n7 Jours", 22, Color.white, TextAnchor.MiddleCenter);

            // Grille de niveaux (1 à 12)
            float startX = -250f;
            float startY = 240f;
            float stepX = 170f;
            float stepY = 150f;

            for (int i = 1; i <= 12; i++)
            {
                int lvl = i;
                int col = (i - 1) % 4;
                int row = (i - 1) / 4;
                Vector2 pos = new(startX + col * stepX, startY - row * stepY);

                GameObject lvlBtn = CreateButton($"LevelNode_{lvl}", screen.transform, pos, new Vector2(115f, 115f), () => _uiManager.ShowPreLevelModal(lvl));
                Image img = lvlBtn.GetComponent<Image>();
                img.color = (lvl <= (SaveManager.Instance?.Data?.highestUnlockedLevel ?? 1)) ? new Color(0.95f, 0.82f, 0.2f) : new Color(0.4f, 0.4f, 0.45f);
                CreateLabel("LvlTxt", lvlBtn.transform, $"{lvl}\n★★★", 24, Color.black, TextAnchor.MiddleCenter);
            }

            return screen;
        }

        private GameObject CreateMatch3Screen(Transform parent)
        {
            GameObject screen = CreateFullScreen("Match3GameScreen", parent, new Color(0.18f, 0.28f, 0.2f, 0f));

            // Panneau d'objectifs supérieur
            GameObject topBar = CreateUIObject("Match3TopBar", screen.transform);
            RectTransform rt = topBar.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 400f);
            rt.sizeDelta = new Vector2(850f, 120f);
            topBar.AddComponent<Image>().color = new Color(0.15f, 0.25f, 0.15f, 0.92f);

            // Compteur de coups (au centre du bandeau)
            GameObject movesBox = CreateUIObject("MovesBox", topBar.transform);
            movesBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            movesBox.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 90f);
            movesBox.AddComponent<Image>().color = new Color(0.85f, 0.35f, 0.15f);
            _uiManager.MovesText = CreateLabel("MovesVal", movesBox.transform, "Coups\n25", 26, Color.white, TextAnchor.MiddleCenter);

            // Score (à gauche)
            _uiManager.ScoreText = CreateLabel("ScoreVal", topBar.transform, "Score : 0", 28, new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleLeft, new Vector2(-280f, 0f));

            // Bouton Pause (à droite)
            GameObject pauseBtn = CreateButton("PauseBtn", topBar.transform, new Vector2(340f, 0f), new Vector2(75f, 75f), () => _uiManager.ShowPauseModal());
            pauseBtn.GetComponent<Image>().color = new Color(0.35f, 0.45f, 0.55f);
            CreateLabel("PauseTxt", pauseBtn.transform, "❚❚", 32, Color.white, TextAnchor.MiddleCenter);

            // Barre inférieure des boosters de match-3
            GameObject boosterBar = CreateUIObject("BoosterBar", screen.transform);
            RectTransform bRt = boosterBar.GetComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, -440f);
            bRt.sizeDelta = new Vector2(800f, 100f);
            boosterBar.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.15f, 0.85f);

            string[] boosterNames = { "Pelle", "Tracteur", "Super Fleur", "+5 Coups" };
            BoosterType[] bTypes = { BoosterType.Shovel, BoosterType.Tractor, BoosterType.RainbowFertilizer, BoosterType.ExtraMoves };

            for (int b = 0; b < 4; b++)
            {
                int bIdx = b;
                GameObject bBtn = CreateButton($"Booster_{b}", boosterBar.transform, new Vector2(-300f + b * 200f, 0f), new Vector2(160f, 75f), () =>
                {
                    Match3Board board = FindAnyObjectByType<Match3Board>();
                    if (bTypes[bIdx] == BoosterType.ExtraMoves) board?.AddExtraMoves(5);
                    else board?.SetActiveBooster(bTypes[bIdx]);
                });
                bBtn.GetComponent<Image>().color = new Color(0.35f, 0.65f, 0.25f);
                CreateLabel("BTxt", bBtn.transform, boosterNames[bIdx], 20, Color.white, TextAnchor.MiddleCenter);
            }

            return screen;
        }

        private GameObject CreateFarmViewScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("FarmViewScreen", parent, new Color(0.3f, 0.7f, 0.35f, 0f));

            // Panneau d'infos rapide ferme
            GameObject farmHud = CreateUIObject("FarmInfoHUD", screen.transform);
            farmHud.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 380f);
            farmHud.GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 80f);
            farmHud.AddComponent<Image>().color = new Color(0.2f, 0.35f, 0.15f, 0.88f);
            CreateLabel("FarmTitle", farmHud.transform, "🌾 Ma Ferme - Touche une parcelle ou un bâtiment !", 26, Color.white, TextAnchor.MiddleCenter);

            // Bouton d'accès rapide aux animaux
            GameObject animBtn = CreateButton("FarmAnimBtn", screen.transform, new Vector2(400f, 260f), new Vector2(140f, 80f), () => _uiManager.ShowAnimalsModal());
            animBtn.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.25f);
            CreateLabel("AnimTxt", animBtn.transform, "🐮 Animaux", 22, Color.white, TextAnchor.MiddleCenter);

            return screen;
        }

        private GameObject CreateShopScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("ShopScreen", parent, new Color(0.18f, 0.12f, 0.25f));

            CreateLabel("ShopTitle", screen.transform, "Boutique Farm Bloom", 48, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0f, 360f));

            // Pack du fermier
            GameObject starterPack = CreateUIObject("StarterPack", screen.transform);
            starterPack.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 220f);
            starterPack.GetComponent<RectTransform>().sizeDelta = new Vector2(750f, 150f);
            starterPack.AddComponent<Image>().color = new Color(0.85f, 0.45f, 0.15f);
            CreateLabel("PackTxt", starterPack.transform, "⭐ Pack du Fermier (Offre Spéciale !)\n5 000 Pièces + 250 Gemmes + 3 Boosters\nTarif : 4,99 €", 26, Color.white, TextAnchor.MiddleLeft, new Vector2(-50f, 0f));
            CreateButton("BuyPackBtn", starterPack.transform, new Vector2(250f, 0f), new Vector2(180f, 75f), () =>
            {
                CurrencyManager.Instance?.AddCoins(5000);
                CurrencyManager.Instance?.AddGems(250);
            }).GetComponent<Image>().color = new Color(0.35f, 0.8f, 0.2f);

            // Packs de Pièces
            int[] coinPacks = { 1000, 5000, 25000 };
            float[] prices = { 0.99f, 3.99f, 14.99f };
            for (int i = 0; i < 3; i++)
            {
                int pIdx = i;
                GameObject cBox = CreateUIObject($"CoinPack_{i}", screen.transform);
                cBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f + i * 260f, 30f);
                cBox.GetComponent<RectTransform>().sizeDelta = new Vector2(230f, 160f);
                cBox.AddComponent<Image>().color = new Color(0.45f, 0.32f, 0.15f);
                CreateLabel("CTxt", cBox.transform, $"💰 {coinPacks[i]:N0}\nPièces\n{prices[i]} €", 24, Color.white, TextAnchor.MiddleCenter);
                CreateButton("BuyBtn", cBox.transform, new Vector2(0f, -50f), new Vector2(160f, 45f), () => CurrencyManager.Instance?.AddCoins(coinPacks[pIdx])).GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.2f);
            }

            // Packs de Gemmes
            int[] gemPacks = { 50, 250, 1200 };
            float[] gPrices = { 1.99f, 7.99f, 29.99f };
            for (int i = 0; i < 3; i++)
            {
                int gIdx = i;
                GameObject gBox = CreateUIObject($"GemPack_{i}", screen.transform);
                gBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f + i * 260f, -170f);
                gBox.GetComponent<RectTransform>().sizeDelta = new Vector2(230f, 160f);
                gBox.AddComponent<Image>().color = new Color(0.35f, 0.15f, 0.45f);
                CreateLabel("GTxt", gBox.transform, $"💎 {gemPacks[i]:N0}\nGemmes\n{gPrices[i]} €", 24, Color.white, TextAnchor.MiddleCenter);
                CreateButton("BuyGBtn", gBox.transform, new Vector2(0f, -50f), new Vector2(160f, 45f), () => CurrencyManager.Instance?.AddGems(gemPacks[gIdx])).GetComponent<Image>().color = new Color(0.75f, 0.3f, 0.85f);
            }

            // Bouton Pass Saisonnier
            GameObject passBtn = CreateButton("PassBtn", screen.transform, new Vector2(0f, -320f), new Vector2(480f, 85f), () => _uiManager.ShowSeasonPassModal());
            passBtn.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f);
            CreateLabel("PassTxt", passBtn.transform, "🎖️ Voir le Pass Saisonnier", 28, Color.black, TextAnchor.MiddleCenter);

            return screen;
        }

        private GameObject CreateSocialScreen(Transform parent)
        {
            GameObject screen = CreateFullScreen("SocialScreen", parent, new Color(0.15f, 0.25f, 0.35f));

            CreateLabel("SocialTitle", screen.transform, "Communauté & Amis", 48, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 360f));

            // Boutons de sous-sections
            CreateButton("QuestsBtn", screen.transform, new Vector2(0f, 200f), new Vector2(450f, 80f), () => _uiManager.ShowDailyQuestsModal()).GetComponent<Image>().color = new Color(0.25f, 0.65f, 0.35f);
            CreateLabel("QTxt", screen.transform, "🎯 Défis & Missions Quotidiennes", 26, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 200f));

            CreateButton("LeaderboardBtn", screen.transform, new Vector2(0f, 80f), new Vector2(450f, 80f), () => _uiManager.ShowLeaderboardModal()).GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f);
            CreateLabel("LTxt", screen.transform, "🏆 Classement Mondial des Fermiers", 26, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 80f));

            CreateButton("ClubBtn", screen.transform, new Vector2(0f, -40f), new Vector2(450f, 80f), () => _uiManager.ShowClubModal()).GetComponent<Image>().color = new Color(0.35f, 0.45f, 0.75f);
            CreateLabel("CTxt", screen.transform, "👥 Club des Fermiers", 26, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, -40f));

            return screen;
        }

        // ==================== 4. MODALES POPUPS ====================

        private GameObject CreatePreLevelModal(Transform parent)
        {
            GameObject modal = CreateModalBase("PreLevelModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Niveau 1 - Objectif", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 260f));

            // Cibles illustrées
            GameObject targetsBox = CreateUIObject("TargetsBox", modal.transform);
            targetsBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 80f);
            targetsBox.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 180f);
            targetsBox.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.75f);
            CreateLabel("TargetsTxt", targetsBox.transform, "Récolte les éléments :\n\n🍅 x 15 Tomates\n🥕 x 10 Carottes", 30, new Color(0.35f, 0.2f, 0.1f), TextAnchor.MiddleCenter);

            // Choix des boosters
            CreateLabel("BoostersLabel", modal.transform, "Sélectionne tes boosters :", 24, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, -60f));

            // Bouton JOUER
            GameObject playBtn = CreateButton("PlayLevelBtn", modal.transform, new Vector2(0f, -220f), new Vector2(360f, 95f), () =>
            {
                modal.SetActive(false);
                GameManager.Instance?.StartLevel(GameManager.Instance.CurrentPlayingLevel);
                FindAnyObjectByType<Match3Board>()?.InitBoard(GameManager.Instance.CurrentPlayingLevel);
            });
            playBtn.GetComponent<Image>().color = new Color(0.35f, 0.82f, 0.18f);
            CreateLabel("PlayTxt", playBtn.transform, "JOUER !", 38, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreatePauseModal(Transform parent)
        {
            GameObject modal = CreateModalBase("PauseModal", parent, 550f, 650f);
            CreateLabel("Title", modal.transform, "Pause", 44, new Color(0.4f, 0.2f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 220f));

            CreateButton("ResumeBtn", modal.transform, new Vector2(0f, 90f), new Vector2(340f, 80f), () => _uiManager.ResumeFromPause()).GetComponent<Image>().color = new Color(0.35f, 0.78f, 0.18f);
            CreateLabel("ResTxt", modal.transform, "Reprendre", 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 90f));

            CreateButton("RestartBtn", modal.transform, new Vector2(0f, -10f), new Vector2(340f, 80f), () =>
            {
                modal.SetActive(false);
                GameManager.Instance?.StartLevel(GameManager.Instance.CurrentPlayingLevel);
                FindAnyObjectByType<Match3Board>()?.InitBoard(GameManager.Instance.CurrentPlayingLevel);
            }).GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f);
            CreateLabel("RstTxt", modal.transform, "Recommencer", 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, -10f));

            CreateButton("QuitBtn", modal.transform, new Vector2(0f, -110f), new Vector2(340f, 80f), () =>
            {
                modal.SetActive(false);
                GameManager.Instance?.SwitchScreen(GameScreen.WorldMap);
            }).GetComponent<Image>().color = new Color(0.75f, 0.25f, 0.2f);
            CreateLabel("QuitTxt", modal.transform, "Quitter", 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, -110f));

            return modal;
        }

        private GameObject CreateVictoryModal(Transform parent)
        {
            GameObject modal = CreateModalBase("VictoryModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Bravo !", 54, new Color(0.95f, 0.75f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, 260f));

            // Étoiles
            CreateLabel("Stars", modal.transform, "⭐⭐⭐", 64, new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 150f));

            // Score
            CreateLabel("ScoreTitle", modal.transform, "Score Obtenu :", 28, new Color(0.4f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 50f));
            CreateLabel("ScoreVal", modal.transform, "45 230", 48, new Color(0.2f, 0.6f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, -10f));

            // Récompense
            CreateLabel("RewardTxt", modal.transform, "Récompenses : +200 Pièces  +35 Tirelire", 24, new Color(0.5f, 0.35f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, -80f));

            // Bouton Continuer
            GameObject contBtn = CreateButton("ContBtn", modal.transform, new Vector2(0f, -220f), new Vector2(360f, 95f), () =>
            {
                modal.SetActive(false);
                GameManager.Instance?.SwitchScreen(GameScreen.WorldMap);
            });
            contBtn.GetComponent<Image>().color = new Color(0.35f, 0.82f, 0.18f);
            CreateLabel("ContTxt", contBtn.transform, "Continuer", 36, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateDefeatModal(Transform parent)
        {
            GameObject modal = CreateModalBase("DefeatModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Manque de coups !", 44, new Color(0.85f, 0.25f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0f, 260f));

            CreateLabel("MascotSad", modal.transform, "🐔💦\nOh non ! Il te manquait peu...", 36, new Color(0.4f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 110f));

            // Bouton +5 Coups
            GameObject extraBtn = CreateButton("ExtraMovesBtn", modal.transform, new Vector2(0f, -40f), new Vector2(420f, 85f), () =>
            {
                if (CurrencyManager.Instance.SpendGems(10))
                {
                    modal.SetActive(false);
                    FindAnyObjectByType<Match3Board>()?.AddExtraMoves(5);
                }
            });
            extraBtn.GetComponent<Image>().color = new Color(0.75f, 0.35f, 0.85f);
            CreateLabel("ExtraTxt", extraBtn.transform, "+5 Coups pour 10 💎", 28, Color.white, TextAnchor.MiddleCenter);

            // Bouton Réessayer
            GameObject retryBtn = CreateButton("RetryBtn", modal.transform, new Vector2(0f, -170f), new Vector2(360f, 80f), () =>
            {
                modal.SetActive(false);
                GameManager.Instance?.StartLevel(GameManager.Instance.CurrentPlayingLevel);
                FindAnyObjectByType<Match3Board>()?.InitBoard(GameManager.Instance.CurrentPlayingLevel);
            });
            retryBtn.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f);
            CreateLabel("RetryTxt", retryBtn.transform, "Réessayer", 30, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateComboModal(Transform parent)
        {
            GameObject modal = CreateUIObject("ComboModal", parent);
            RectTransform rt = modal.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 100f);
            rt.sizeDelta = new Vector2(600f, 160f);
            modal.AddComponent<Image>().color = new Color(0.95f, 0.45f, 0.05f, 0.92f);
            CreateLabel("ComboTxt", modal.transform, "🌟 COMBO GÉNIAL ! 🌟\nx5  (+250)", 38, Color.white, TextAnchor.MiddleCenter);
            return modal;
        }

        private GameObject CreateDailyRewardModal(Transform parent)
        {
            GameObject modal = CreateModalBase("DailyRewardModal", parent, 700f, 800f);
            CreateLabel("Title", modal.transform, "Récompense Quotidienne", 42, new Color(0.4f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 300f));

            // 7 Jours
            int[] rewards = { 100, 150, 200, 250, 300, 400, 1000 };
            for (int d = 0; d < 7; d++)
            {
                int rIdx = d;
                GameObject dayBox = CreateUIObject($"Day_{d + 1}", modal.transform);
                float x = (d < 4) ? (-225f + d * 150f) : (-180f + (d - 4) * 180f);
                float y = (d < 4) ? 140f : -10f;
                dayBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                dayBox.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 130f);
                dayBox.AddComponent<Image>().color = (d == 0) ? new Color(0.35f, 0.78f, 0.2f) : new Color(0.85f, 0.78f, 0.65f);
                CreateLabel("DayTxt", dayBox.transform, $"Jour {d + 1}\n💰 {rewards[rIdx]}", 22, (d == 0) ? Color.white : Color.black, TextAnchor.MiddleCenter);
            }

            // Bouton Récolter
            GameObject claimBtn = CreateButton("ClaimDailyBtn", modal.transform, new Vector2(0f, -240f), new Vector2(360f, 90f), () =>
            {
                CurrencyManager.Instance?.AddCoins(250);
                modal.SetActive(false);
            });
            claimBtn.GetComponent<Image>().color = new Color(0.35f, 0.82f, 0.18f);
            CreateLabel("ClaimTxt", claimBtn.transform, "RÉCOLTER !", 36, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateSpinWheelModal(Transform parent)
        {
            GameObject modal = CreateModalBase("SpinWheelModal", parent, 700f, 800f);
            CreateLabel("Title", modal.transform, "Roue de Récompenses", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 300f));

            // Cadran circulaire illustré
            GameObject wheel = CreateUIObject("WheelGraphic", modal.transform);
            wheel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);
            wheel.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 360f);
            wheel.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.25f);
            CreateLabel("WheelTxt", wheel.transform, "🎡\n100 💰 | 20 💎\n500 💰 | Pelle\n300 💰 | Tracteur", 26, new Color(0.35f, 0.2f, 0.1f), TextAnchor.MiddleCenter);

            // Bouton Tourner
            GameObject spinBtn = CreateButton("SpinBtn", modal.transform, new Vector2(0f, -240f), new Vector2(360f, 90f), () =>
            {
                CurrencyManager.Instance?.AddCoins(300);
                CurrencyManager.Instance?.AddGems(10);
                SoundManager.Instance?.PlayCoinEarn();
                modal.SetActive(false);
            });
            spinBtn.GetComponent<Image>().color = new Color(0.85f, 0.45f, 0.15f);
            CreateLabel("SpinTxt", spinBtn.transform, "TOURNER (Gratuit)", 32, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateChestModal(Transform parent)
        {
            GameObject modal = CreateModalBase("ChestModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Coffre Épique !", 46, new Color(0.85f, 0.65f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, 260f));
            CreateLabel("ChestIcon", modal.transform, "🎁📦", 72, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 80f));
            CreateLabel("LootDesc", modal.transform, "Contenu du coffre :\n• 1 500 Pièces d'or\n• 50 Gemmes\n• 2 Boosters Tracteur", 28, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, -80f));

            GameObject openBtn = CreateButton("OpenBtn", modal.transform, new Vector2(0f, -220f), new Vector2(340f, 85f), () =>
            {
                CurrencyManager.Instance?.AddCoins(1500);
                CurrencyManager.Instance?.AddGems(50);
                modal.SetActive(false);
            });
            openBtn.GetComponent<Image>().color = new Color(0.35f, 0.82f, 0.18f);
            CreateLabel("OpenTxt", openBtn.transform, "OUVRIR !", 36, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateBarnModal(Transform parent)
        {
            GameObject modal = CreateModalBase("BarnModal", parent, 700f, 800f);
            CreateLabel("Title", modal.transform, "Grange & Stocks", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 300f));
            CreateLabel("CapTxt", modal.transform, "Capacité : 65 / 100", 28, new Color(0.2f, 0.5f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0f, 230f));

            // Affichage des stocks
            string[] stockItems = { "🍅 Tomates : 25", "🌽 Maïs : 18", "🥕 Carottes : 14", "🍌 Bananes : 8" };
            for (int s = 0; s < stockItems.Length; s++)
            {
                GameObject sBox = CreateUIObject($"Stock_{s}", modal.transform);
                sBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 130f - s * 70f);
                sBox.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 55f);
                sBox.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.72f);
                CreateLabel("STxt", sBox.transform, stockItems[s], 26, Color.black, TextAnchor.MiddleCenter);
            }

            // Bouton Améliorer Grange
            GameObject upBtn = CreateButton("UpgradeBarnBtn", modal.transform, new Vector2(0f, -240f), new Vector2(460f, 85f), () =>
            {
                if (CurrencyManager.Instance.SpendCoins(500))
                {
                    if (SaveManager.Instance?.Data != null) SaveManager.Instance.Data.barnCapacity += 50;
                    modal.SetActive(false);
                }
            });
            upBtn.GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.2f);
            CreateLabel("UpTxt", upBtn.transform, "Améliorer Capacité (+50) : 500 💰", 24, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreatePlantSeedModal(Transform parent)
        {
            GameObject modal = CreateModalBase("PlantSeedModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Choisir une Semence", 40, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 270f));

            CropType[] crops = { CropType.Carrot, CropType.Tomato, CropType.Corn, CropType.Banana };
            string[] cNames = { "🥕 Carotte (15 💰 - 30s)", "🍅 Tomate (25 💰 - 60s)", "🌽 Maïs (35 💰 - 90s)", "🍌 Banane (50 💰 - 120s)" };

            for (int i = 0; i < crops.Length; i++)
            {
                int cIdx = i;
                GameObject cBtn = CreateButton($"CropBtn_{i}", modal.transform, new Vector2(0f, 140f - i * 95f), new Vector2(480f, 75f), () =>
                {
                    _uiManager.ConfirmPlantSeed(crops[cIdx]);
                });
                cBtn.GetComponent<Image>().color = new Color(0.45f, 0.75f, 0.25f);
                CreateLabel("CTxt", cBtn.transform, cNames[cIdx], 24, Color.white, TextAnchor.MiddleCenter);
            }

            return modal;
        }

        private GameObject CreateBuildingUpgradeModal(Transform parent)
        {
            GameObject modal = CreateModalBase("BuildingUpgradeModal", parent, 600f, 650f);
            CreateLabel("Title", modal.transform, "Améliorer Bâtiment", 40, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 220f));
            CreateLabel("Desc", modal.transform, "Moulin à Farine\n\nNiveau 1  ➔  Niveau 2\n\n+30% Vitesse de production\n+20 Points d'expérience", 28, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, 50f));

            GameObject upBtn = CreateButton("ConfirmUpBtn", modal.transform, new Vector2(0f, -180f), new Vector2(380f, 85f), () =>
            {
                _uiManager.ConfirmBuildingUpgrade();
            });
            upBtn.GetComponent<Image>().color = new Color(0.35f, 0.8f, 0.2f);
            CreateLabel("UpTxt", upBtn.transform, "Améliorer (600 💰)", 30, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateAnimalsModal(Transform parent)
        {
            GameObject modal = CreateModalBase("AnimalsModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Mes Animaux de Ferme", 40, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 270f));

            string[] animals = { "🐔 Poulets : 12  ➔  Production d'œufs", "🐮 Vaches : 8  ➔  Production de lait", "🐐 Chèvres : 5  ➔  Fromage fermier", "🐝 Abeilles : 10  ➔  Miel artisanal" };
            for (int a = 0; a < animals.Length; a++)
            {
                GameObject aBox = CreateUIObject($"AnimBox_{a}", modal.transform);
                aBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 130f - a * 80f);
                aBox.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 65f);
                aBox.AddComponent<Image>().color = new Color(0.92f, 0.85f, 0.72f);
                CreateLabel("ATxt", aBox.transform, animals[a], 24, Color.black, TextAnchor.MiddleCenter);
            }

            GameObject feedBtn = CreateButton("FeedBtn", modal.transform, new Vector2(0f, -220f), new Vector2(400f, 85f), () =>
            {
                CurrencyManager.Instance?.AddCoins(250);
                SoundManager.Instance?.PlayCoinEarn();
                modal.SetActive(false);
            });
            feedBtn.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f);
            CreateLabel("FeedTxt", feedBtn.transform, "Nourrir & Récolter (+250 💰)", 26, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateSeasonPassModal(Transform parent)
        {
            GameObject modal = CreateModalBase("SeasonPassModal", parent, 700f, 850f);
            CreateLabel("Title", modal.transform, "Pass Saisonnier", 44, new Color(0.85f, 0.65f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, 320f));
            CreateLabel("Sub", modal.transform, "Débloque des récompenses exclusives et des boosters !", 24, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, 260f));

            CreateButton("ActivatePassBtn", modal.transform, new Vector2(0f, -280f), new Vector2(440f, 90f), () => modal.SetActive(false)).GetComponent<Image>().color = new Color(0.95f, 0.65f, 0.1f);
            CreateLabel("ActTxt", modal.transform, "Activer le Pass (9,99 €)", 30, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, -280f));

            return modal;
        }

        private GameObject CreatePiggyBankModal(Transform parent)
        {
            GameObject modal = CreateModalBase("PiggyBankModal", parent, 600f, 700f);
            CreateLabel("Title", modal.transform, "Tirelire / Coffre-fort", 40, new Color(0.85f, 0.55f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, 240f));
            CreateLabel("BankIcon", modal.transform, "🐷💰", 72, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 90f));
            CreateLabel("BankVal", modal.transform, "1 450 Pièces Épargnées !", 34, new Color(0.2f, 0.6f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, -30f));

            GameObject breakBtn = CreateButton("BreakBankBtn", modal.transform, new Vector2(0f, -180f), new Vector2(380f, 85f), () =>
            {
                CurrencyManager.Instance?.AddCoins(1450);
                modal.SetActive(false);
            });
            breakBtn.GetComponent<Image>().color = new Color(0.85f, 0.45f, 0.15f);
            CreateLabel("BreakTxt", breakBtn.transform, "Ouvrir pour 2,99 €", 30, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateProfileModal(Transform parent)
        {
            GameObject modal = CreateModalBase("ProfileModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Profil Joueur", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 270f));
            CreateLabel("Avatar", modal.transform, "🤠", 84, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 130f));
            CreateLabel("Stats", modal.transform, "Nom : Fermier Bloom\nNiveau de Ferme : 3\nÉtoiles Totales : 36 ⭐\nID Joueur : #4507", 28, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, -50f));

            return modal;
        }

        private GameObject CreateDailyQuestsModal(Transform parent)
        {
            GameObject modal = CreateModalBase("DailyQuestsModal", parent, 700f, 800f);
            CreateLabel("Title", modal.transform, "Défis Quotidiens", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 300f));

            string[] quests = { "🎯 Récolte 50 carottes (20/50)", "🏆 Gagne 3 niveaux Match-3 (1/3)", "🌟 Fais 2 combos géniaux (2/2 - Terminé !)" };
            for (int q = 0; q < quests.Length; q++)
            {
                GameObject qBox = CreateUIObject($"Quest_{q}", modal.transform);
                qBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 150f - q * 110f);
                qBox.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 85f);
                qBox.AddComponent<Image>().color = new Color(0.92f, 0.88f, 0.75f);
                CreateLabel("QTxt", qBox.transform, quests[q], 24, Color.black, TextAnchor.MiddleCenter);
            }

            return modal;
        }

        private GameObject CreateLeaderboardModal(Transform parent)
        {
            GameObject modal = CreateModalBase("LeaderboardModal", parent, 700f, 800f);
            CreateLabel("Title", modal.transform, "Classement Mondial", 42, new Color(0.85f, 0.65f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0f, 300f));

            string[] top = { "🥇 1. Emma - 125 450 pts", "🥈 2. Lucas - 105 200 pts", "🥉 3. Noah - 98 450 pts", "4. Mia - 85 120 pts", "5. Vous - 45 230 pts" };
            for (int l = 0; l < top.Length; l++)
            {
                GameObject lBox = CreateUIObject($"Rank_{l}", modal.transform);
                lBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 170f - l * 75f);
                lBox.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 60f);
                lBox.AddComponent<Image>().color = (l == 4) ? new Color(0.95f, 0.85f, 0.35f) : new Color(0.9f, 0.85f, 0.75f);
                CreateLabel("LTxt", lBox.transform, top[l], 24, Color.black, TextAnchor.MiddleCenter);
            }

            return modal;
        }

        private GameObject CreateClubModal(Transform parent)
        {
            GameObject modal = CreateModalBase("ClubModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Club des Fermiers", 42, new Color(0.35f, 0.45f, 0.75f), TextAnchor.MiddleCenter, new Vector2(0f, 270f));
            CreateLabel("ClubInfo", modal.transform, "Nom : Les Moissonneurs Dorés\nMembres : 28 / 30\n\nChat & Dons de vies actifs !", 28, Color.black, TextAnchor.MiddleCenter, new Vector2(0f, 60f));

            GameObject donateBtn = CreateButton("DonateBtn", modal.transform, new Vector2(0f, -160f), new Vector2(360f, 80f), () =>
            {
                CurrencyManager.Instance?.AddCoins(100);
                modal.SetActive(false);
            });
            donateBtn.GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.25f);
            CreateLabel("DonTxt", donateBtn.transform, "Envoyer 5 Vies (+100 💰)", 26, Color.white, TextAnchor.MiddleCenter);

            return modal;
        }

        private GameObject CreateSettingsModal(Transform parent)
        {
            GameObject modal = CreateModalBase("SettingsModal", parent, 650f, 750f);
            CreateLabel("Title", modal.transform, "Paramètres & Audio", 42, new Color(0.45f, 0.25f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 270f));

            // Curseur Musique
            CreateLabel("MusTxt", modal.transform, "Musique", 26, Color.black, TextAnchor.MiddleLeft, new Vector2(-150f, 150f));
            CreateButton("MusToggle", modal.transform, new Vector2(150f, 150f), new Vector2(120f, 50f), () =>
            {
                float newVol = (SoundManager.Instance != null && SaveManager.Instance?.Data?.musicVolume > 0.1f) ? 0f : 0.8f;
                SoundManager.Instance?.SetMusicVolume(newVol);
            }).GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.25f);

            // Curseur Bruitages
            CreateLabel("SfxTxt", modal.transform, "Effets Sonores", 26, Color.black, TextAnchor.MiddleLeft, new Vector2(-150f, 60f));
            CreateButton("SfxToggle", modal.transform, new Vector2(150f, 60f), new Vector2(120f, 50f), () =>
            {
                float newVol = (SoundManager.Instance != null && SaveManager.Instance?.Data?.soundVolume > 0.1f) ? 0f : 1f;
                SoundManager.Instance?.SetSoundVolume(newVol);
            }).GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.25f);

            // Langue
            CreateLabel("LangTxt", modal.transform, "Langue (FR / EN / ES / PT)", 26, Color.black, TextAnchor.MiddleLeft, new Vector2(-150f, -30f));
            CreateButton("LangBtn", modal.transform, new Vector2(150f, -30f), new Vector2(120f, 50f), () =>
            {
                string cur = LocalizationManager.Instance?.CurrentLanguage ?? "fr";
                string next = (cur == "fr") ? "en" : (cur == "en" ? "es" : (cur == "es" ? "pt" : "fr"));
                LocalizationManager.Instance?.SetLanguage(next);
            }).GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f);

            return modal;
        }

        // ==================== OUTILS GRAPHIQUES ====================

        private GameObject CreateFullScreen(string name, Transform parent, Color bgColor)
        {
            GameObject screen = CreateUIObject(name, parent);
            RectTransform rt = screen.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            Image img = screen.AddComponent<Image>();
            img.color = bgColor;
            return screen;
        }

        private GameObject CreateModalBase(string name, Transform parent, float width, float height)
        {
            // Voile sombre d'arrière-plan
            GameObject overlay = CreateUIObject($"{name}_Overlay", parent);
            RectTransform oRt = overlay.GetComponent<RectTransform>();
            oRt.anchorMin = Vector2.zero;
            oRt.anchorMax = Vector2.one;
            oRt.sizeDelta = Vector2.zero;
            overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            // Panneau modal
            GameObject modal = CreateUIObject(name, overlay.transform);
            RectTransform rt = modal.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = Vector2.zero;

            Image img = modal.AddComponent<Image>();
            img.color = new Color(0.96f, 0.90f, 0.78f); // Bois crème chaleureux

            // Bouton de fermeture (Croix rouge en haut à droite)
            GameObject closeBtn = CreateButton("CloseBtn", modal.transform, new Vector2(width * 0.5f - 40f, height * 0.5f - 40f), new Vector2(60f, 60f), () => overlay.SetActive(false));
            closeBtn.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.2f);
            CreateLabel("X", closeBtn.transform, "✕", 32, Color.white, TextAnchor.MiddleCenter);

            return overlay;
        }

        private GameObject CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, Action onClick)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btnObj.AddComponent<Image>();
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButtonClick();
                onClick?.Invoke();
            });

            return btnObj;
        }

        private Text CreateLabel(string name, Transform parent, string text, int fontSize, Color color, TextAnchor alignment, Vector2? pos = null)
        {
            GameObject txtObj = CreateUIObject(name, parent);
            RectTransform rt = txtObj.GetComponent<RectTransform>();
            if (pos.HasValue) rt.anchoredPosition = pos.Value;
            else
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }

            Text txt = txtObj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.font = _defaultFont;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            return txt;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }
    }
}
