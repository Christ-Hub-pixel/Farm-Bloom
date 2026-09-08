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
        [MenuItem("Farm Bloom/⭐ 0. Configurer Moteur Match-3 (Plateau 6x6, Sprites & Prefab)", false, 0)]
        public static void SetupMatch3Engine()
        {
            // 1. S'assurer des dossiers
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Sprites"))
                AssetDatabase.CreateFolder("Assets/Art", "Sprites");

            // 2. Générer les 5 Sprites PNG
            CreateAndSaveSprite("Assets/Art/Sprites/Strawberry.png", new Color(0.95f, 0.15f, 0.25f), "Fraise");
            CreateAndSaveSprite("Assets/Art/Sprites/Carrot.png", new Color(1f, 0.55f, 0.05f), "Carotte");
            CreateAndSaveSprite("Assets/Art/Sprites/Corn.png", new Color(1f, 0.85f, 0.1f), "Maïs");
            CreateAndSaveSprite("Assets/Art/Sprites/Tomato.png", new Color(0.95f, 0.25f, 0.15f), "Tomate");
            CreateAndSaveSprite("Assets/Art/Sprites/Potato.png", new Color(0.72f, 0.52f, 0.35f), "PommeDeTerre");

            AssetDatabase.Refresh();

            // 3. Créer le Prefab Tile (Tile -> SpriteRenderer)
            string prefabPath = "Assets/Prefabs/Tile.prefab";
            GameObject tempTileObj = new GameObject("Tile");
            tempTileObj.AddComponent<SpriteRenderer>();
            tempTileObj.AddComponent<Tile>();

            GameObject tilePrefab = PrefabUtility.SaveAsPrefabAsset(tempTileObj, prefabPath);
            Object.DestroyImmediate(tempTileObj);

            // 4. Configurer la Caméra
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(2.5f, 2.5f, -10f);
            cam.orthographicSize = 4.5f;
            cam.backgroundColor = new Color(0.18f, 0.45f, 0.22f); // Vert herbe Farm Bloom

            // 5. Configurer l'objet BoardManager dans la scène
            var boardManagerObj = GameObject.Find("BoardManager");
            if (boardManagerObj == null) boardManagerObj = new GameObject("BoardManager");

            var bm = boardManagerObj.GetComponent<BoardManager>();
            if (bm == null) bm = boardManagerObj.AddComponent<BoardManager>();

            SerializedObject so = new SerializedObject(bm);
            so.FindProperty("tilePrefab").objectReferenceValue = tilePrefab.GetComponent<Tile>();
            so.FindProperty("strawberrySprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Strawberry.png");
            so.FindProperty("carrotSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Carrot.png");
            so.FindProperty("cornSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Corn.png");
            so.FindProperty("tomatoSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Tomato.png");
            so.FindProperty("potatoSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Potato.png");
            so.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("<b>[Farm Bloom]</b> Moteur Match-3 6x6 configuré avec succès (BoardManager + Prefab + Sprites) !");
            EditorUtility.DisplayDialog("Match-3 6x6 Prêt !", "La scène a été configurée avec le plateau 6x6, le Prefab Tile et les 5 Sprites (Fraise 🍓, Carotte 🥕, Maïs 🌽, Tomate 🍅, Pomme de terre 🥔).\n\nAppuyez sur Play (▶) pour lancer le Match-3 !", "C'est parti !");
        }

        private static void CreateAndSaveSprite(string path, Color mainColor, string type)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] px = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float light = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center + new Vector2(-12f, 12f)) / (radius * 1.5f)) * 0.35f;
                        px[y * size + x] = new Color(Mathf.Min(1f, mainColor.r + light), Mathf.Min(1f, mainColor.g + light), Mathf.Min(1f, mainColor.b + light), 1f);
                    }
                    else
                    {
                        px[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(px);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Farm Bloom/1. Initialiser la Scène Complète (Hiérarchie & UI)", false, 10)]
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
