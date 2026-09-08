#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FarmBloom.Editor
{
    public static class FarmBloomEditorTools
    {
        [MenuItem("Farm Bloom/⭐ 1. Recommencer à Zéro (Configurer Scène Match-3 6x6)", false, 0)]
        public static void SetupCleanMatch3Scene()
        {
            // 1. S'assurer des dossiers
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
                AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Sprites"))
                AssetDatabase.CreateFolder("Assets/Art", "Sprites");

            // 2. Créer et enregistrer les 5 sprites PNG
            CreateAndSaveSprite("Assets/Art/Sprites/Strawberry.png", new Color(0.95f, 0.15f, 0.25f), "Fraise");
            CreateAndSaveSprite("Assets/Art/Sprites/Carrot.png", new Color(1f, 0.55f, 0.05f), "Carotte");
            CreateAndSaveSprite("Assets/Art/Sprites/Corn.png", new Color(1f, 0.85f, 0.1f), "Maïs");
            CreateAndSaveSprite("Assets/Art/Sprites/Tomato.png", new Color(0.95f, 0.25f, 0.15f), "Tomate");
            CreateAndSaveSprite("Assets/Art/Sprites/Potato.png", new Color(0.72f, 0.52f, 0.35f), "PommeDeTerre");

            AssetDatabase.Refresh();

            // 3. Créer ou mettre à jour le Prefab Tile
            string prefabPath = "Assets/Prefabs/Tile.prefab";
            GameObject tempTileObj = new GameObject("Tile");
            tempTileObj.AddComponent<SpriteRenderer>();
            tempTileObj.AddComponent<Tile>();

            GameObject tilePrefab = PrefabUtility.SaveAsPrefabAsset(tempTileObj, prefabPath);
            Object.DestroyImmediate(tempTileObj);

            // 4. Nettoyer la scène active : supprimer tout vieux Canvas ou objet UI parasite
            var canvasObjects = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in canvasObjects)
            {
                Object.DestroyImmediate(c.gameObject);
            }

            var leftoverRoots = GameObject.Find("FarmBloom_Root");
            if (leftoverRoots != null) Object.DestroyImmediate(leftoverRoots);

            // 5. Configurer la Caméra
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(2.5f, 2.3f, -10f);
            cam.orthographicSize = 6.2f;
            cam.backgroundColor = new Color(0.18f, 0.45f, 0.22f); // Vert prairie Farm Bloom
            cam.clearFlags = CameraClearFlags.SolidColor;

            // 6. Configurer l'objet BoardManager
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

            // Marquer la scène sale et sauvegarder
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<b>[Farm Bloom]</b> Scène réinitialisée à 0 avec succès ! Le plateau 6x6 est prêt. Cliquez sur ▶ Play.");
            EditorUtility.DisplayDialog(
                "Farm Bloom - Remis à Zéro !",
                "Tout a été remis à zéro avec succès !\n\n✓ Scène nettoyée (aucune interface parasite)\n✓ Plateau 6x6 configuré\n✓ 5 récoltes créées (🍓 🥕 🌽 🍅 🥔)\n\nCliquez sur Play (▶) pour voir le plateau 6x6 !",
                "C'est parti !"
            );
        }

        private static void CreateAndSaveSprite(string path, Color mainColor, string type)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] px = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float light = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center + new Vector2(-15f, 15f)) / (radius * 1.6f)) * 0.35f;
                        float border = dist > radius - 3f ? 0.75f : 1f;
                        px[y * size + x] = new Color(
                            Mathf.Min(1f, mainColor.r + light) * border,
                            Mathf.Min(1f, mainColor.g + light) * border,
                            Mathf.Min(1f, mainColor.b + light) * border,
                            1f
                        );
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
    }
}
#endif
