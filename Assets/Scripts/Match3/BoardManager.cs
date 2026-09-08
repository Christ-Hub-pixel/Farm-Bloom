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

    // Étape 2 : Sélection et Échange
    private Tile selectedTile;
    private bool isSwapping = false;

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
        cam.transform.position = new Vector3(centerX, centerY - 0.2f, -10f);
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.45f, 0.22f); // Vert prairie Farm Bloom

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float boardWidth = width * tileSize + 1.2f;
        if (aspect < 1f) // Mode portrait mobile (9:16)
        {
            cam.orthographicSize = (boardWidth / aspect) * 0.5f;
        }
        else // Mode paysage / éditeur
        {
            cam.orthographicSize = Mathf.Max(width, height) * tileSize * 0.75f;
        }
    }

    private void EnsureDefaults()
    {
        if (tilePrefab == null)
        {
            GameObject fallbackPrefab = new GameObject("DefaultTilePrefab");
            fallbackPrefab.transform.SetParent(transform);
            fallbackPrefab.AddComponent<SpriteRenderer>();
            fallbackPrefab.AddComponent<BoxCollider2D>().size = new Vector2(tileSize, tileSize);
            tilePrefab = fallbackPrefab.AddComponent<Tile>();
            fallbackPrefab.SetActive(false);
        }

        // Chargement direct des sprites PNG s'ils existent
        strawberrySprite ??= LoadPngSprite("Art/Sprites/Strawberry.png");
        carrotSprite ??= LoadPngSprite("Art/Sprites/Carrot.png");
        cornSprite ??= LoadPngSprite("Art/Sprites/Corn.png");
        tomatoSprite ??= LoadPngSprite("Art/Sprites/Tomato.png");
        potatoSprite ??= LoadPngSprite("Art/Sprites/Potato.png");

        // Secours procédural si nécessaire
        strawberrySprite ??= CreateCropSprite(new Color(0.95f, 0.15f, 0.25f), "Fraise");
        carrotSprite ??= CreateCropSprite(new Color(1f, 0.55f, 0.05f), "Carotte");
        cornSprite ??= CreateCropSprite(new Color(1f, 0.85f, 0.1f), "Maïs");
        tomatoSprite ??= CreateCropSprite(new Color(0.95f, 0.25f, 0.15f), "Tomate");
        potatoSprite ??= CreateCropSprite(new Color(0.72f, 0.52f, 0.35f), "PommeDeTerre");
    }

    private Sprite LoadPngSprite(string relativePath)
    {
        try
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Bilinear;
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 280f);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BoardManager] Impossible de charger le sprite {relativePath} : {e.Message}");
        }
        return null;
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
                tile.Setup(crop, GetSprite(crop), this);

                board[x, y] = tile;
            }
        }

        Debug.Log($"<b>[Farm Bloom]</b> Plateau {width}x{height} généré avec succès ({width * height} cases) !");
    }

    // ==================== ÉTAPE 2 : SÉLECTION & ÉCHANGE ====================

    public void SelectTile(Tile tile)
    {
        if (isSwapping)
            return;

        if (selectedTile == null)
        {
            selectedTile = tile;

            Debug.Log("Première récolte sélectionnée : " + tile.cropType);
            HighlightTile(tile);

            return;
        }

        if (selectedTile == tile)
        {
            ClearSelection();
            return;
        }

        if (AreAdjacent(selectedTile, tile))
        {
            SwapTiles(selectedTile, tile);
        }
        else
        {
            ClearSelection();

            selectedTile = tile;
            HighlightTile(tile);
        }
    }

    private bool AreAdjacent(Tile a, Tile b)
    {
        Vector3 difference = a.transform.position - b.transform.position;

        float distance = Mathf.Abs(difference.x) + Mathf.Abs(difference.y);

        return Mathf.Abs(distance - tileSize) < 0.05f || Mathf.Approximately(distance, tileSize);
    }

    private void SwapTiles(Tile a, Tile b)
    {
        isSwapping = true;

        Vector3 positionA = a.transform.position;
        Vector3 positionB = b.transform.position;

        a.transform.position = positionB;
        b.transform.position = positionA;

        // Mise à jour du tableau interne
        int xA = -1, yA = -1, xB = -1, yB = -1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == a) { xA = x; yA = y; }
                if (board[x, y] == b) { xB = x; yB = y; }
            }
        }
        if (xA != -1 && xB != -1)
        {
            board[xA, yA] = b;
            board[xB, yB] = a;
        }

        ClearSelection();

        isSwapping = false;

        Debug.Log($"<color=cyan><b>[Match-3]</b> Échange effectué entre {a.cropType} et {b.cropType} !</color>");
    }

    private void HighlightTile(Tile tile)
    {
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            renderer.color = new Color(0.75f, 0.75f, 0.75f); // Teinte visuelle pour marquer la sélection
        }
    }

    private void ClearSelection()
    {
        selectedTile = null;

        if (board == null) return;

        foreach (Tile tile in board)
        {
            if (tile != null)
            {
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    renderer.color = Color.white;
                }
            }
        }
    }

    private CropType GetSafeRandomCrop(int x, int y)
    {
        List<CropType> available = new List<CropType>(cropTypes);

        if (x >= 2)
        {
            CropType left1 = board[x - 1, y].cropType;
            CropType left2 = board[x - 2, y].cropType;

            if (left1 == left2)
            {
                available.Remove(left1);
            }
        }

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
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
