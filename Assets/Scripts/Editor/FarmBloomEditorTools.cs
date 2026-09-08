#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using FarmBloom;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Farm;
using FarmBloom.Match3;
using FarmBloom.UI;
using FarmBloom.Utils;

namespace FarmBloom.Editor
{
    public static class FarmBloomEditorTools
    {
        [MenuItem("Farm Bloom/1. Initialiser la Scène Complète (Hiérarchie & UI)", false, 1)]
        public static void SetupSceneHierarchy()
        {
            // Vérifier Caméra
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.2f, 0.45f, 0.25f);
                camObj.transform.position = new Vector3(0f, 0f, -10f);
                Undo.RegisterCreatedObjectUndo(camObj, "Create Camera");
            }

            // Vérifier Root
            var root = Object.FindAnyObjectByType<FarmBloomGameInitializer>();
            if (root == null)
            {
                var rootObj = new GameObject("FarmBloom_Root");
                rootObj.AddComponent<FarmBloomGameInitializer>();
                Undo.RegisterCreatedObjectUndo(rootObj, "Create FarmBloom Root");
            }

            // Vérifier Canvas
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("GameCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.AddComponent<UIBuilder>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            Debug.Log("<b>[Farm Bloom]</b> Scène initialisée avec succès ! Cliquez sur ▶ Play pour lancer le jeu.");
            EditorUtility.DisplayDialog("Farm Bloom", "La scène Farm Bloom a été configurée avec succès !\n\nVous pouvez désormais cliquer sur Play (▶) pour jouer et naviguer à travers les 50 interfaces.", "Super !");
        }

        [MenuItem("Farm Bloom/2. Économie/Ajouter 10 000 Pièces & 500 Gemmes", false, 20)]
        public static void AddCurrenciesCheat()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(10000);
                CurrencyManager.Instance.AddGems(500);
            }
            else if (SaveManager.Instance?.Data != null)
            {
                SaveManager.Instance.Data.coins += 10000;
                SaveManager.Instance.Data.gems += 500;
                SaveManager.Instance.Save();
            }
            Debug.Log("<b>[Farm Bloom]</b> +10 000 Pièces & +500 Gemmes ajoutées !");
        }

        [MenuItem("Farm Bloom/2. Économie/Débloquer tous les niveaux (1 à 12)", false, 21)]
        public static void UnlockAllLevelsCheat()
        {
            if (SaveManager.Instance?.Data != null)
            {
                SaveManager.Instance.Data.highestUnlockedLevel = 12;
                for (int i = 1; i <= 12; i++)
                {
                    var existing = SaveManager.Instance.Data.levelProgression.Find(e => e.level == i);
                    if (existing != null) existing.stars = 3;
                    else SaveManager.Instance.Data.levelProgression.Add(new LevelStarEntry { level = i, stars = 3, highScore = 50000 });
                }
                SaveManager.Instance.Save();
                Debug.Log("<b>[Farm Bloom]</b> Tous les niveaux ont été débloqués avec 3 étoiles !");
            }
        }

        [MenuItem("Farm Bloom/3. Données/Réinitialiser la Sauvegarde", false, 30)]
        public static void ResetSaveCheat()
        {
            if (EditorUtility.DisplayDialog("Réinitialiser", "Voulez-vous vraiment réinitialiser toutes les données de sauvegarde de Farm Bloom ?", "Oui", "Annuler"))
            {
                SaveManager.Instance?.ResetSave();
                Debug.Log("<b>[Farm Bloom]</b> Données de sauvegarde réinitialisées aux valeurs d'usine.");
            }
        }

        [MenuItem("Farm Bloom/4. Voir les Maquettes de Référence", false, 40)]
        public static void OpenMockupReference()
        {
            string path = "Assets/Art/References/FarmBloom_50_Interfaces.jpg";
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
    }
}
#endif
