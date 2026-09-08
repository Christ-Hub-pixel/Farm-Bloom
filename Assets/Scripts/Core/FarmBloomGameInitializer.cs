using UnityEngine;
using UnityEngine.UI;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Farm;
using FarmBloom.Match3;
using FarmBloom.UI;
using FarmBloom.Utils;

namespace FarmBloom
{
    public class FarmBloomGameInitializer : MonoBehaviour
    {
        private static bool _initialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (_initialized) return;

            // Vérifie si un Initializer est déjà présent dans la scène
            if (FindAnyObjectByType<FarmBloomGameInitializer>() == null)
            {
                GameObject root = new GameObject("FarmBloom_Root");
                root.AddComponent<FarmBloomGameInitializer>();
            }
        }

        private void Awake()
        {
            if (_initialized)
            {
                Destroy(gameObject);
                return;
            }
            _initialized = true;
            DontDestroyOnLoad(gameObject);

            SetupCoreSystems();
            SetupWorldSystems();
            SetupUISystems();
        }

        private void SetupCoreSystems()
        {
            // 1. Save Manager
            if (SaveManager.Instance == null)
            {
                GameObject go = new GameObject("SaveManager");
                go.transform.SetParent(transform);
                go.AddComponent<SaveManager>();
            }

            // 2. Currency Manager
            if (CurrencyManager.Instance == null)
            {
                GameObject go = new GameObject("CurrencyManager");
                go.transform.SetParent(transform);
                go.AddComponent<CurrencyManager>();
            }

            // 3. Sound Manager
            if (SoundManager.Instance == null)
            {
                GameObject go = new GameObject("SoundManager");
                go.transform.SetParent(transform);
                go.AddComponent<SoundManager>();
            }

            // 4. Localization Manager
            if (LocalizationManager.Instance == null)
            {
                GameObject go = new GameObject("LocalizationManager");
                go.transform.SetParent(transform);
                go.AddComponent<LocalizationManager>();
            }

            // 5. Game Manager
            if (GameManager.Instance == null)
            {
                GameObject go = new GameObject("GameManager");
                go.transform.SetParent(transform);
                go.AddComponent<GameManager>();
            }

            // 6. Sprite Factory
            if (SpriteFactory.Instance == null)
            {
                GameObject go = new GameObject("SpriteFactory");
                go.transform.SetParent(transform);
                go.AddComponent<SpriteFactory>();
            }
        }

        private void SetupWorldSystems()
        {
            // Match-3 Board
            var board = FindAnyObjectByType<Match3Board>();
            if (board == null)
            {
                GameObject match3Go = new GameObject("Match3_Board");
                match3Go.transform.SetParent(transform);
                board = match3Go.AddComponent<Match3Board>();
            }

            // Farm Controller
            var farm = FindAnyObjectByType<FarmController>();
            if (farm == null)
            {
                GameObject farmGo = new GameObject("Farm_World");
                farmGo.transform.SetParent(transform);
                farm = farmGo.AddComponent<FarmController>();
            }

            // Liaison Événements Ferme & Plateau
            farm.OnPlotPlantRequested += (plot) => UIManager.Instance?.OpenPlantSeedModal(plot);
            farm.OnBuildingDetailsRequested += (building) => UIManager.Instance?.OpenBuildingUpgradeModal(building);

            board.OnMovesChanged += (m) =>
            {
                if (UIManager.Instance?.MovesText != null) UIManager.Instance.MovesText.text = $"Coups\n{m}";
            };

            board.OnScoreChanged += (s) =>
            {
                if (UIManager.Instance?.ScoreText != null) UIManager.Instance.ScoreText.text = $"Score : {s:N0}";
            };

            board.OnComboBonus += (combo, bonus) =>
            {
                UIManager.Instance?.ShowComboBonus(combo, bonus);
            };

            board.OnLevelWon += () =>
            {
                UIManager.Instance?.ShowVictoryModal(board.CurrentScore, 3);
            };

            board.OnLevelLost += () =>
            {
                UIManager.Instance?.ShowDefeatModal();
            };
        }

        private void SetupUISystems()
        {
            // Canvas UI
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("GameCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // UI Builder
            var builder = canvas.GetComponentInChildren<UIBuilder>();
            if (builder == null)
            {
                canvas.gameObject.AddComponent<UIBuilder>();
            }

            // Caméra Principale
            if (Camera.main == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.2f, 0.45f, 0.25f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                camGo.transform.position = new Vector3(0f, 0f, -10f);
            }
        }
    }
}
