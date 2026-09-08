using System;
using UnityEngine;

namespace FarmBloom.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        [Header("State")]
        [SerializeField] private GameScreen _currentScreen = GameScreen.Splash;
        public GameScreen CurrentScreen => _currentScreen;

        [Header("Match-3 Session")]
        public int CurrentPlayingLevel { get; private set; } = 1;
        public int CurrentLevelMoves { get; set; } = 25;
        public int CurrentLevelScore { get; set; } = 0;
        public bool IsGamePaused { get; private set; } = false;

        public event Action<GameScreen> OnScreenChanged;
        public event Action<int> OnLevelStarted;
        public event Action<int, int, int> OnLevelCompleted; // level, score, stars
        public event Action<int, int> OnLevelFailed; // level, score

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Initialisation de démarrage
            SwitchScreen(GameScreen.Splash);
        }

        public void SwitchScreen(GameScreen newScreen)
        {
            _currentScreen = newScreen;
            IsGamePaused = false;
            OnScreenChanged?.Invoke(_currentScreen);
        }

        public void StartLevel(int level)
        {
            CurrentPlayingLevel = level;
            CurrentLevelScore = 0;
            CurrentLevelMoves = 25;
            IsGamePaused = false;

            // Consommation de vie (1 coeur)
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.ConsumeEnergy(1);
            }

            OnLevelStarted?.Invoke(level);
            SwitchScreen(GameScreen.Match3Game);
        }

        public void CompleteLevel(int stars, int score)
        {
            CurrentLevelScore = score;

            // Enregistrement de la progression
            if (SaveManager.Instance?.Data != null)
            {
                var data = SaveManager.Instance.Data;
                var existing = data.levelProgression.Find(e => e.level == CurrentPlayingLevel);
                if (existing != null)
                {
                    existing.stars = Mathf.Max(existing.stars, stars);
                    existing.highScore = Mathf.Max(existing.highScore, score);
                }
                else
                {
                    data.levelProgression.Add(new Data.LevelStarEntry
                    {
                        level = CurrentPlayingLevel,
                        stars = stars,
                        highScore = score
                    });
                }

                if (CurrentPlayingLevel >= data.highestUnlockedLevel)
                {
                    data.highestUnlockedLevel = CurrentPlayingLevel + 1;
                }

                // Récompense de niveau (ex: 150 pièces + tirelire)
                CurrencyManager.Instance.AddCoins(150 + (stars * 50));
                CurrencyManager.Instance.AddToPiggyBank(35);

                SaveManager.Instance.Save();
            }

            SoundManager.Instance?.PlayVictory();
            OnLevelCompleted?.Invoke(CurrentPlayingLevel, score, stars);
        }

        public void FailLevel()
        {
            SoundManager.Instance?.PlayDefeat();
            OnLevelFailed?.Invoke(CurrentPlayingLevel, CurrentLevelScore);
        }

        public void PauseGame(bool pause)
        {
            IsGamePaused = pause;
            Time.timeScale = pause ? 0f : 1f;
        }
    }
}
