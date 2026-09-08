using System.Collections;
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
    private bool isBusy = false;

    // Interaction & Sélection
    private Tile selectedTile;
    private Vector2 touchStartPos;
    private const float MinSwipeDistance = 30f;

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

    private void Update()
    {
        if (isBusy || board == null) return;

        HandleInput();
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
        cam.clearFlags = CameraClearFlags.SolidColor; // Fond uni vert naturel
        cam.backgroundColor = new Color(0.18f, 0.45f, 0.22f); // Vert prairie Farm Bloom

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
    }

    private void EnsureDefaults()
    {
        if (tilePrefab == null)
        {
            GameObject fallbackPrefab = new GameObject("DefaultTilePrefab");
            fallbackPrefab.transform.SetParent(transform);
            fallbackPrefab.AddComponent<SpriteRenderer>();
            tilePrefab = fallbackPrefab.AddComponent<Tile>();
            fallbackPrefab.SetActive(false);
        }

        // Chargement direct des véritables sprites PNG illustrés (Fraise, Carotte, Maïs, Tomate, Pomme de terre)
        strawberrySprite ??= LoadPngSprite("Art/Sprites/Strawberry.png");
        carrotSprite ??= LoadPngSprite("Art/Sprites/Carrot.png");
        cornSprite ??= LoadPngSprite("Art/Sprites/Corn.png");
        tomatoSprite ??= LoadPngSprite("Art/Sprites/Tomato.png");
        potatoSprite ??= LoadPngSprite("Art/Sprites/Potato.png");

        // Secours procédural si les fichiers PNG sont absents
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
                    // 280 pixels par unité donne une taille de ~0.91 unité (espacement parfait dans une case 1x1)
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
                tile.Setup(crop, GetSprite(crop));
                tile.SetGridPosition(x, y);

                board[x, y] = tile;
            }
        }

        Debug.Log($"<b>[Farm Bloom]</b> Plateau {width}x{height} généré avec succès ! Prêt pour le jeu.");
    }

    // ==================== INTERACTION & ÉCHANGE ====================

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Tile clickedTile = GetTileAtScreenPosition(Input.mousePosition);
            if (clickedTile != null)
            {
                // Si on avait déjà sélectionné une tuile voisine par clic
                if (selectedTile != null && IsNeighbor(selectedTile, clickedTile))
                {
                    Tile prev = selectedTile;
                    selectedTile.SetSelected(false);
                    selectedTile = null;
                    StartCoroutine(TrySwapRoutine(prev, clickedTile));
                    return;
                }

                if (selectedTile != null)
                {
                    selectedTile.SetSelected(false);
                }

                selectedTile = clickedTile;
                selectedTile.SetSelected(true);
                touchStartPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0) && selectedTile != null)
        {
            Vector2 delta = (Vector2)Input.mousePosition - touchStartPos;

            if (delta.magnitude >= MinSwipeDistance)
            {
                // Détection de la direction du glissement (Swipe)
                Vector2Int dir = Vector2Int.zero;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                }
                else
                {
                    dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
                }

                int targetX = selectedTile.x + dir.x;
                int targetY = selectedTile.y + dir.y;

                if (IsValidGridPosition(targetX, targetY))
                {
                    Tile neighbor = board[targetX, targetY];
                    Tile current = selectedTile;
                    selectedTile.SetSelected(false);
                    selectedTile = null;
                    StartCoroutine(TrySwapRoutine(current, neighbor));
                    return;
                }
            }

            // Simple clic : on garde la sélection active pour permettre un second clic sur le voisin
        }
    }

    private bool IsNeighbor(Tile a, Tile b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
    }

    private bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private Tile GetTileAtScreenPosition(Vector3 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.y / tileSize);

        if (IsValidGridPosition(x, y))
        {
            return board[x, y];
        }

        return null;
    }

    private IEnumerator TrySwapRoutine(Tile a, Tile b)
    {
        isBusy = true;

        // 1. Sauvegarder positions d'origine
        int xA = a.x, yA = a.y;
        int xB = b.x, yB = b.y;

        // 2. Échanger dans le tableau de données
        board[xA, yA] = b;
        board[xB, yB] = a;
        a.SetGridPosition(xB, yB);
        b.SetGridPosition(xA, yA);

        // 3. Animation fluide de l'échange
        float swapDuration = 0.2f;
        a.MoveTo(new Vector3(xB * tileSize, yB * tileSize, 0f), swapDuration);
        b.MoveTo(new Vector3(xA * tileSize, yA * tileSize, 0f), swapDuration);

        yield return new WaitForSeconds(swapDuration + 0.05f);

        // 4. Vérifier si un alignement de 3 récoltes (ou plus) a été formé
        List<Tile> matches = FindAllMatches();

        if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatchesRoutine(matches));
        }
        else
        {
            // Aucun alignement : annuler l'échange (retour à la place initiale)
            Debug.Log("<color=orange><b>[Match-3]</b> Pas d'alignement, retour à la position initiale.</color>");

            board[xA, yA] = a;
            board[xB, yB] = b;
            a.SetGridPosition(xA, yA);
            b.SetGridPosition(xB, yB);

            a.MoveTo(new Vector3(xA * tileSize, yA * tileSize, 0f), swapDuration);
            b.MoveTo(new Vector3(xB * tileSize, yB * tileSize, 0f), swapDuration);

            yield return new WaitForSeconds(swapDuration + 0.05f);
            isBusy = false;
        }
    }

    // ==================== SUPPRESSION, GRAVITÉ & REMPLISSAGE ====================

    private IEnumerator ProcessMatchesRoutine(List<Tile> initialMatches)
    {
        List<Tile> currentMatches = initialMatches;

        while (currentMatches != null && currentMatches.Count > 0)
        {
            Debug.Log($"<color=green><b>[Match-3]</b> 💥 Destruction de {currentMatches.Count} récoltes !</color>");

            // 1. Destruction animée des récoltes alignées
            foreach (Tile tile in currentMatches)
            {
                if (tile != null)
                {
                    board[tile.x, tile.y] = null;
                    tile.Disappear(0.18f);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 2. Gravité : faire descendre les tuiles restantes
            yield return StartCoroutine(ApplyGravityRoutine());

            // 3. Remplissage : spawner de nouvelles récoltes en haut de chaque colonne
            yield return StartCoroutine(RefillBoardRoutine());

            // 4. Cascades : vérifier si de nouveaux alignements se sont formés
            currentMatches = FindAllMatches();
            if (currentMatches.Count > 0)
            {
                Debug.Log($"<color=yellow><b>[Match-3]</b> ✨ Combo en cascade ! {currentMatches.Count} nouvelles récoltes alignées !</color>");
                yield return new WaitForSeconds(0.12f);
            }
        }

        isBusy = false;
    }

    private IEnumerator ApplyGravityRoutine()
    {
        bool anyTileMoved = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    // Trouver la première tuile au-dessus
                    for (int aboveY = y + 1; aboveY < height; aboveY++)
                    {
                        if (board[x, aboveY] != null)
                        {
                            Tile fallingTile = board[x, aboveY];
                            board[x, y] = fallingTile;
                            board[x, aboveY] = null;

                            fallingTile.SetGridPosition(x, y);
                            fallingTile.MoveTo(new Vector3(x * tileSize, y * tileSize, 0f), 0.2f);
                            anyTileMoved = true;
                            break;
                        }
                    }
                }
            }
        }

        if (anyTileMoved)
        {
            yield return new WaitForSeconds(0.22f);
        }
    }

    private IEnumerator RefillBoardRoutine()
    {
        float dropDuration = 0.25f;

        for (int x = 0; x < width; x++)
        {
            int emptySpacesInCol = 0;

            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    emptySpacesInCol++;
                    CropType crop = cropTypes[Random.Range(0, cropTypes.Length)];

                    // Apparition au-dessus de la grille
                    Vector3 spawnPos = new Vector3(x * tileSize, (height + emptySpacesInCol) * tileSize, 0f);
                    Vector3 targetPos = new Vector3(x * tileSize, y * tileSize, 0f);

                    Tile newTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
                    newTile.gameObject.SetActive(true);
                    newTile.name = $"Tile_{x}_{y}";
                    newTile.Setup(crop, GetSprite(crop));
                    newTile.SetGridPosition(x, y);

                    board[x, y] = newTile;
                    newTile.MoveTo(targetPos, dropDuration);
                }
            }
        }

        yield return new WaitForSeconds(dropDuration + 0.05f);
    }

    // ==================== DÉTECTION DES ALIGNEMENTS ====================

    public List<Tile> FindAllMatches()
    {
        HashSet<Tile> matchedTiles = new HashSet<Tile>();

        // 1. Alignements Horizontaux
        for (int y = 0; y < height; y++)
        {
            int matchCount = 1;
            for (int x = 0; x < width; x++)
            {
                if (x < width - 1 && board[x, y] != null && board[x + 1, y] != null && board[x, y].cropType == board[x + 1, y].cropType)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            if (board[x - i, y] != null) matchedTiles.Add(board[x - i, y]);
                        }
                    }
                    matchCount = 1;
                }
            }
        }

        // 2. Alignements Verticaux
        for (int x = 0; x < width; x++)
        {
            int matchCount = 1;
            for (int y = 0; y < height; y++)
            {
                if (y < height - 1 && board[x, y] != null && board[x, y + 1] != null && board[x, y].cropType == board[x, y + 1].cropType)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            if (board[x, y - i] != null) matchedTiles.Add(board[x, y - i]);
                        }
                    }
                    matchCount = 1;
                }
            }
        }

        return new List<Tile>(matchedTiles);
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
