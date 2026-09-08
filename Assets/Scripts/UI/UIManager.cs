using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Farm;
using FarmBloom.Match3;
using FarmBloom.Utils;

namespace FarmBloom.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        [Header("Canvas & Root")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private CanvasScaler _canvasScaler;

        [Header("Screens & Views")]
        public GameObject SplashScreen;
        public GameObject AuthScreen;
        public GameObject WorldMapScreen;
        public GameObject Match3GameScreen;
        public GameObject FarmViewScreen;
        public GameObject ShopScreen;
        public GameObject SocialScreen;

        [Header("Popups / Modals")]
        public GameObject PreLevelModal;
        public GameObject PauseModal;
        public GameObject VictoryModal;
        public GameObject DefeatModal;
        public GameObject ComboModal;
        public GameObject DailyRewardModal;
        public GameObject SpinWheelModal;
        public GameObject ChestModal;
        public GameObject BarnModal;
        public GameObject PlantSeedModal;
        public GameObject BuildingUpgradeModal;
        public GameObject AnimalsModal;
        public GameObject SeasonPassModal;
        public GameObject PiggyBankModal;
        public GameObject ProfileModal;
        public GameObject DailyQuestsModal;
        public GameObject LeaderboardModal;
        public GameObject ClubModal;
        public GameObject SettingsModal;

        [Header("HUD Elements")]
        public Text CoinsText;
        public Text GemsText;
        public Text EnergyText;
        public Text LevelTitleText;
        public Text MovesText;
        public Text ScoreText;
        public Transform TargetsContainer;

        private CropPlot _selectedPlotForPlanting;
        private FarmBuilding _selectedBuildingForUpgrade;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            EnsureCanvasSetup();
        }

        private void Start()
        {
            SubscribeEvents();
            UpdateHUDCurrencies();
        }

        private void EnsureCanvasSetup()
        {
            if (_mainCanvas == null)
            {
                _mainCanvas = FindAnyObjectByType<Canvas>();
                if (_mainCanvas == null)
                {
                    GameObject canvasGo = new GameObject("MainCanvas");
                    _mainCanvas = canvasGo.AddComponent<Canvas>();
                    _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _canvasScaler = canvasGo.AddComponent<CanvasScaler>();
                    _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    _canvasScaler.referenceResolution = new Vector2(1080, 1920);
                    _canvasScaler.matchWidthOrHeight = 0.5f;
                    canvasGo.AddComponent<GraphicRaycaster>();
                }
            }
        }

        private void SubscribeEvents()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCoinsChanged += (c) => UpdateCoins(c);
                CurrencyManager.Instance.OnGemsChanged += (g) => UpdateGems(g);
                CurrencyManager.Instance.OnEnergyChanged += (cur, max) => UpdateEnergy(cur, max);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScreenChanged += HandleScreenChanged;
            }
        }

        public void UpdateHUDCurrencies()
        {
            if (CurrencyManager.Instance != null)
            {
                UpdateCoins(CurrencyManager.Instance.Coins);
                UpdateGems(CurrencyManager.Instance.Gems);
                UpdateEnergy(CurrencyManager.Instance.Energy, CurrencyManager.Instance.MaxEnergy);
            }
        }

        private void UpdateCoins(int amount)
        {
            if (CoinsText != null) CoinsText.text = amount.ToString("N0");
        }

        private void UpdateGems(int amount)
        {
            if (GemsText != null) GemsText.text = amount.ToString("N0");
        }

        private void UpdateEnergy(int current, int max)
        {
            if (EnergyText != null) EnergyText.text = $"{current}/{max}";
        }

        private void HandleScreenChanged(GameScreen screen)
        {
            CloseAllModals();

            if (SplashScreen != null) SplashScreen.SetActive(screen == GameScreen.Splash);
            if (AuthScreen != null) AuthScreen.SetActive(screen == GameScreen.Auth);
            if (WorldMapScreen != null) WorldMapScreen.SetActive(screen == GameScreen.WorldMap);
            if (Match3GameScreen != null) Match3GameScreen.SetActive(screen == GameScreen.Match3Game);
            if (FarmViewScreen != null) FarmViewScreen.SetActive(screen == GameScreen.FarmView);
            if (ShopScreen != null) ShopScreen.SetActive(screen == GameScreen.Shop);
            if (SocialScreen != null) SocialScreen.SetActive(screen == GameScreen.SocialHub || screen == GameScreen.Leaderboard || screen == GameScreen.Club);
        }

        public void CloseAllModals()
        {
            if (PreLevelModal != null) PreLevelModal.SetActive(false);
            if (PauseModal != null) PauseModal.SetActive(false);
            if (VictoryModal != null) VictoryModal.SetActive(false);
            if (DefeatModal != null) DefeatModal.SetActive(false);
            if (ComboModal != null) ComboModal.SetActive(false);
            if (DailyRewardModal != null) DailyRewardModal.SetActive(false);
            if (SpinWheelModal != null) SpinWheelModal.SetActive(false);
            if (ChestModal != null) ChestModal.SetActive(false);
            if (BarnModal != null) BarnModal.SetActive(false);
            if (PlantSeedModal != null) PlantSeedModal.SetActive(false);
            if (BuildingUpgradeModal != null) BuildingUpgradeModal.SetActive(false);
            if (AnimalsModal != null) AnimalsModal.SetActive(false);
            if (SeasonPassModal != null) SeasonPassModal.SetActive(false);
            if (PiggyBankModal != null) PiggyBankModal.SetActive(false);
            if (ProfileModal != null) ProfileModal.SetActive(false);
            if (DailyQuestsModal != null) DailyQuestsModal.SetActive(false);
            if (LeaderboardModal != null) LeaderboardModal.SetActive(false);
            if (ClubModal != null) ClubModal.SetActive(false);
            if (SettingsModal != null) SettingsModal.SetActive(false);
        }

        // ==================== NAVIGATION MODALES ====================

        public void ShowPreLevelModal(int levelNumber)
        {
            CloseAllModals();
            if (PreLevelModal != null) PreLevelModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowPauseModal()
        {
            if (PauseModal != null)
            {
                PauseModal.SetActive(true);
                GameManager.Instance?.PauseGame(true);
            }
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ResumeFromPause()
        {
            if (PauseModal != null) PauseModal.SetActive(false);
            GameManager.Instance?.PauseGame(false);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowVictoryModal(int score, int stars)
        {
            if (VictoryModal != null) VictoryModal.SetActive(true);
        }

        public void ShowDefeatModal()
        {
            if (DefeatModal != null) DefeatModal.SetActive(true);
        }

        public void ShowComboBonus(int multiplier, int scoreBonus)
        {
            if (ComboModal != null)
            {
                ComboModal.SetActive(true);
                StartCoroutine(AutoCloseModalCoroutine(ComboModal, 1.4f));
            }
        }

        public void ShowDailyRewardModal()
        {
            CloseAllModals();
            if (DailyRewardModal != null) DailyRewardModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowSpinWheelModal()
        {
            CloseAllModals();
            if (SpinWheelModal != null) SpinWheelModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowChestModal()
        {
            CloseAllModals();
            if (ChestModal != null) ChestModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowBarnModal()
        {
            CloseAllModals();
            if (BarnModal != null) BarnModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void OpenPlantSeedModal(CropPlot plot)
        {
            _selectedPlotForPlanting = plot;
            if (PlantSeedModal != null) PlantSeedModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ConfirmPlantSeed(CropType crop)
        {
            if (_selectedPlotForPlanting != null)
            {
                _selectedPlotForPlanting.Plant(crop);
                _selectedPlotForPlanting = null;
            }
            if (PlantSeedModal != null) PlantSeedModal.SetActive(false);
        }

        public void OpenBuildingUpgradeModal(FarmBuilding building)
        {
            _selectedBuildingForUpgrade = building;
            if (BuildingUpgradeModal != null) BuildingUpgradeModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ConfirmBuildingUpgrade()
        {
            if (_selectedBuildingForUpgrade != null)
            {
                _selectedBuildingForUpgrade.Upgrade();
            }
            if (BuildingUpgradeModal != null) BuildingUpgradeModal.SetActive(false);
        }

        public void ShowAnimalsModal()
        {
            CloseAllModals();
            if (AnimalsModal != null) AnimalsModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowSeasonPassModal()
        {
            CloseAllModals();
            if (SeasonPassModal != null) SeasonPassModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowPiggyBankModal()
        {
            CloseAllModals();
            if (PiggyBankModal != null) PiggyBankModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowProfileModal()
        {
            CloseAllModals();
            if (ProfileModal != null) ProfileModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowDailyQuestsModal()
        {
            CloseAllModals();
            if (DailyQuestsModal != null) DailyQuestsModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowLeaderboardModal()
        {
            CloseAllModals();
            if (LeaderboardModal != null) LeaderboardModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowClubModal()
        {
            CloseAllModals();
            if (ClubModal != null) ClubModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        public void ShowSettingsModal()
        {
            CloseAllModals();
            if (SettingsModal != null) SettingsModal.SetActive(true);
            SoundManager.Instance?.PlayButtonClick();
        }

        private IEnumerator AutoCloseModalCoroutine(GameObject modal, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (modal != null) modal.SetActive(false);
        }
    }
}
