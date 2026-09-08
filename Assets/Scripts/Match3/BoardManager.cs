using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Dimensions")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float tileSize = 1f;

    [Header("Prefab Tile")]
    [SerializeField] private Tile tilePrefab;

    [Header("Sprites des Récoltes")]
    [SerializeField] private Sprite strawberrySprite;
    [SerializeField] private Sprite carrotSprite;
    [SerializeField] private Sprite cornSprite;
    [SerializeField] private Sprite tomatoSprite;
    [SerializeField] private Sprite potatoSprite;

    private Tile[,] board;

    private readonly CropType[] cropTypes =
    {
        CropType.Strawberry,
        CropType.Carrot,
        CropType.Corn,
        CropType.Tomato,
        CropType.Potato
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateBoardManager()
    {
        if (FindAnyObjectByType<BoardManager>() == null)
        {
            GameObject go = new GameObject("BoardManager");
            go.AddComponent<BoardManager>();
        }
    }

    private void Awake()
    {
        EnsureDefaults();
    }

    private void Start()
    {
        CenterCamera();
        CreateBoard();
    }

    private void CenterCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        float centerX = (width - 1) * tileSize * 0.5f;
        float centerY = (height - 1) * tileSize * 0.5f;
        cam.transform.position = new Vector3(centerX, centerY, -10f);
        cam.orthographic = true;

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float boardWidth = width * tileSize + 1.2f;
        if (aspect < 1f) // Portrait (9:16)
        {
            cam.orthographicSize = (boardWidth / aspect) * 0.5f;
        }
        else // Paysage (16:10 / 16:9)
        {
            cam.orthographicSize = Mathf.Max(width, height) * tileSize * 0.75f;
        }

        cam.backgroundColor = new Color(0.18f, 0.45f, 0.22f); // Vert herbe prairie Farm Bloom
    }

    private void EnsureDefaults()
    {
        // Création de secours automatique du prefab si non assigné
        if (tilePrefab == null)
        {
            GameObject fallbackPrefab = new GameObject("DefaultTilePrefab");
            fallbackPrefab.transform.SetParent(transform);
            fallbackPrefab.AddComponent<SpriteRenderer>();
            tilePrefab = fallbackPrefab.AddComponent<Tile>();
            fallbackPrefab.SetActive(false);
        }

        // Génération automatique des 5 sprites nets avec couleurs caractéristiques si non assignés
        strawberrySprite ??= CreateCropSprite(new Color(0.95f, 0.15f, 0.25f), "Fraise");
        carrotSprite ??= CreateCropSprite(new Color(1f, 0.55f, 0.05f), "Carotte");
        cornSprite ??= CreateCropSprite(new Color(1f, 0.85f, 0.1f), "Maïs");
        tomatoSprite ??= CreateCropSprite(new Color(0.95f, 0.25f, 0.15f), "Tomate");
        potatoSprite ??= CreateCropSprite(new Color(0.72f, 0.52f, 0.35f), "PommeDeTerre");
    }

    private void CreateBoard()
    {
        board = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CropType crop = GetSafeRandomCrop(x, y);

                Tile tile = Instantiate(
                    tilePrefab,
                    new Vector3(x * tileSize, y * tileSize, 0f),
                    Quaternion.identity,
                    transform
                );

                tile.gameObject.SetActive(true);
                tile.name = $"Tile_{x}_{y}";
                tile.Setup(crop, GetSprite(crop));

                board[x, y] = tile;
            }
        }

        Debug.Log($"<b>[Farm Bloom]</b> Plateau Match-3 {width}x{height} généré avec succès ({width * height} récoltes, 0 alignement au départ) !");
    }

    private CropType GetSafeRandomCrop(int x, int y)
    {
        List<CropType> available = new List<CropType>(cropTypes);

        // Évite 3 identiques consécutifs à l'horizontale
        if (x >= 2)
        {
            CropType left1 = board[x - 1, y].cropType;
            CropType left2 = board[x - 2, y].cropType;

            if (left1 == left2)
            {
                available.Remove(left1);
            }
        }

        // Évite 3 identiques consécutifs à la verticale
        if (y >= 2)
        {
            CropType down1 = board[x, y - 1].cropType;
            CropType down2 = board[x, y - 2].cropType;

            if (down1 == down2)
            {
                available.Remove(down1);
            }
        }

        return available[Random.Range(0, available.Count)];
    }

    private Sprite GetSprite(CropType crop)
    {
        return crop switch
        {
            CropType.Strawberry => strawberrySprite,
            CropType.Carrot => carrotSprite,
            CropType.Corn => cornSprite,
            CropType.Tomato => tomatoSprite,
            CropType.Potato => potatoSprite,
            _ => null
        };
    }

    private Sprite CreateCropSprite(Color mainColor, string cropName)
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
                    // Dégradé d'éclairage 3D casual arrondi
                    float light = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center + new Vector2(-15f, 15f)) / (radius * 1.6f)) * 0.35f;
                    float border = dist > radius - 3f ? 0.75f : 1f; // contour doux
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
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
