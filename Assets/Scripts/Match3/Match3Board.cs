using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;
using FarmBloom.Utils;

namespace FarmBloom.Match3
{
    public class Match3Board : MonoBehaviour
    {
        [Header("Board Dimensions")]
        public int Width = 6;
        public int Height = 6;
        public float TileSpacing = 0.95f;

        [Header("Level Configuration")]
        public LevelData CurrentLevel;

        // Events
        public event Action<int> OnMovesChanged;
        public event Action<int> OnScoreChanged;
        public event Action<List<CropTarget>> OnTargetsUpdated;
        public event Action<int, int> OnComboBonus; // comboMultiplier, bonusScore
        public event Action OnLevelWon;
        public event Action OnLevelLost;

        private Match3Tile[,] _grid;
        private bool _isProcessing = false;
        private Match3Tile _selectedTile = null;
        private int _movesRemaining = 25;
        private int _currentScore = 0;
        private List<CropTarget> _targets = new List<CropTarget>();

        public bool IsProcessing => _isProcessing;
        public int MovesRemaining => _movesRemaining;
        public int CurrentScore => _currentScore;
        public List<CropTarget> Targets => _targets;

        private Camera _mainCam;
        private BoosterType? _activeBooster = null;

        private void Awake()
        {
            _mainCam = Camera.main;
        }

        public void InitBoard(int levelNumber)
        {
            CurrentLevel = LevelData.CreateDefaultLevel(levelNumber);
            _movesRemaining = CurrentLevel.MaxMoves;
            _currentScore = 0;
            _targets.Clear();

            foreach (var t in CurrentLevel.Targets)
            {
                _targets.Add(new CropTarget
                {
                    Crop = t.Crop,
                    TargetCount = t.TargetCount,
                    CurrentCollected = 0
                });
            }

            OnMovesChanged?.Invoke(_movesRemaining);
            OnScoreChanged?.Invoke(_currentScore);
            OnTargetsUpdated?.Invoke(_targets);

            ClearBoard();
            GenerateBoard();
        }

        public void SetActiveBooster(BoosterType? booster)
        {
            _activeBooster = booster;
        }

        private void ClearBoard()
        {
            if (_grid != null)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (_grid[x, y] != null)
                        {
                            Destroy(_grid[x, y].gameObject);
                            _grid[x, y] = null;
                        }
                    }
                }
            }
            _grid = new Match3Tile[Width, Height];
        }

        private void GenerateBoard()
        {
            Vector3 origin = GetBoardOrigin();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CropType crop = GetRandomCropWithoutInitialMatch(x, y);
                    SpawnTile(x, y, crop, SpecialTileType.None);
                }
            }
        }

        private Vector3 GetBoardOrigin()
        {
            float totalW = (Width - 1) * TileSpacing;
            float totalH = (Height - 1) * TileSpacing;
            return new Vector3(-totalW * 0.5f, -totalH * 0.5f - 0.3f, 0f);
        }

        public Vector3 GridToWorld(int x, int y)
        {
            Vector3 origin = GetBoardOrigin();
            return origin + new Vector3(x * TileSpacing, y * TileSpacing, 0f);
        }

        private Match3Tile SpawnTile(int x, int y, CropType crop, SpecialTileType special)
        {
            GameObject go = new GameObject($"Tile_{x}_{y}");
            go.transform.SetParent(transform);
            go.transform.localPosition = GridToWorld(x, y);

            Match3Tile tile = go.AddComponent<Match3Tile>();
            tile.Init(x, y, crop, special);
            _grid[x, y] = tile;
            return tile;
        }

        private CropType GetRandomCropWithoutInitialMatch(int x, int y)
        {
            var crops = (CurrentLevel != null && CurrentLevel.AvailableCrops.Count > 0)
                ? CurrentLevel.AvailableCrops
                : new List<CropType> { CropType.Tomato, CropType.Corn, CropType.Carrot, CropType.Banana, CropType.Flower };

            var validCrops = new List<CropType>(crops);

            // Éviter 3 à l'horizontale
            if (x >= 2 && _grid[x - 1, y] != null && _grid[x - 2, y] != null && _grid[x - 1, y].Crop == _grid[x - 2, y].Crop)
            {
                validCrops.Remove(_grid[x - 1, y].Crop);
            }

            // Éviter 3 à la verticale
            if (y >= 2 && _grid[x, y - 1] != null && _grid[x, y - 2] != null && _grid[x, y - 1].Crop == _grid[x, y - 2].Crop)
            {
                validCrops.Remove(_grid[x, y - 1].Crop);
            }

            if (validCrops.Count == 0) return crops[UnityEngine.Random.Range(0, crops.Count)];
            return validCrops[UnityEngine.Random.Range(0, validCrops.Count)];
        }

        private void Update()
        {
            if (_isProcessing || GameManager.Instance?.IsGamePaused == true) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPt = _mainCam.ScreenToWorldPoint(Input.mousePosition);
                Match3Tile clicked = GetTileAtWorldPoint(worldPt);

                if (clicked != null)
                {
                    // Si un booster est actif
                    if (_activeBooster.HasValue)
                    {
                        ApplyBooster(_activeBooster.Value, clicked);
                        _activeBooster = null;
                        return;
                    }

                    if (_selectedTile == null)
                    {
                        _selectedTile = clicked;
                        _selectedTile.SetSelected(true);
                        SoundManager.Instance?.PlayTileSwap();
                    }
                    else
                    {
                        if (AreNeighbors(_selectedTile, clicked))
                        {
                            _selectedTile.SetSelected(false);
                            StartCoroutine(SwapAndProcessCoroutine(_selectedTile, clicked));
                            _selectedTile = null;
                        }
                        else
                        {
                            _selectedTile.SetSelected(false);
                            _selectedTile = clicked;
                            _selectedTile.SetSelected(true);
                            SoundManager.Instance?.PlayTileSwap();
                        }
                    }
                }
            }
        }

        private bool AreNeighbors(Match3Tile a, Match3Tile b)
        {
            int dx = Mathf.Abs(a.GridX - b.GridX);
            int dy = Mathf.Abs(a.GridY - b.GridY);
            return (dx + dy == 1);
        }

        private Match3Tile GetTileAtWorldPoint(Vector3 pt)
        {
            float halfSpacing = TileSpacing * 0.5f;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] != null)
                    {
                        Vector3 tilePos = _grid[x, y].transform.position;
                        if (Mathf.Abs(pt.x - tilePos.x) <= halfSpacing && Mathf.Abs(pt.y - tilePos.y) <= halfSpacing)
                        {
                            return _grid[x, y];
                        }
                    }
                }
            }
            return null;
        }

        private IEnumerator SwapAndProcessCoroutine(Match3Tile tileA, Match3Tile tileB)
        {
            _isProcessing = true;

            // Échange physique dans la grille
            int xA = tileA.GridX, yA = tileA.GridY;
            int xB = tileB.GridX, yB = tileB.GridY;

            _grid[xA, yA] = tileB;
            _grid[xB, yB] = tileA;

            tileA.GridX = xB; tileA.GridY = yB;
            tileB.GridX = xA; tileB.GridY = yA;

            tileA.MoveToPosition(GridToWorld(xB, yB), 18f);
            tileB.MoveToPosition(GridToWorld(xA, yA), 18f);

            yield return new WaitForSeconds(0.18f);

            // Cas spécial 1 : Fleur arc-en-ciel
            bool isRainbowMatch = (tileA.Special == SpecialTileType.RainbowFlower || tileB.Special == SpecialTileType.RainbowFlower);

            var matches = MatchDetector.FindMatches(_grid, Width, Height, tileA);

            if (matches.Count > 0 || isRainbowMatch)
            {
                // Coup valide !
                _movesRemaining--;
                OnMovesChanged?.Invoke(_movesRemaining);

                if (isRainbowMatch)
                {
                    CropType targetCrop = (tileA.Special == SpecialTileType.RainbowFlower) ? tileB.Crop : tileA.Crop;
                    yield return StartCoroutine(ProcessRainbowClearCoroutine(targetCrop));
                }

                yield return StartCoroutine(ProcessMatchesCascadeCoroutine(matches, tileA));

                CheckGameEndCondition();
            }
            else
            {
                // Coup invalide -> Revert
                _grid[xA, yA] = tileA;
                _grid[xB, yB] = tileB;

                tileA.GridX = xA; tileA.GridY = yA;
                tileB.GridX = xB; tileB.GridY = yB;

                tileA.MoveToPosition(GridToWorld(xA, yA), 18f);
                tileB.MoveToPosition(GridToWorld(xB, yB), 18f);

                yield return new WaitForSeconds(0.18f);
            }

            _isProcessing = false;
        }

        private IEnumerator ProcessRainbowClearCoroutine(CropType cropToClear)
        {
            SoundManager.Instance?.PlayExplosion();
            var tilesToDestroy = new List<Match3Tile>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] != null && (_grid[x, y].Crop == cropToClear || _grid[x, y].Special == SpecialTileType.RainbowFlower))
                    {
                        tilesToDestroy.Add(_grid[x, y]);
                    }
                }
            }

            AddScoreAndCollect(tilesToDestroy, 2);
            foreach (var t in tilesToDestroy)
            {
                _grid[t.GridX, t.GridY] = null;
                Destroy(t.gameObject);
            }

            yield return new WaitForSeconds(0.15f);
        }

        private IEnumerator ProcessMatchesCascadeCoroutine(List<MatchGroup> initialMatches, Match3Tile swappedTile)
        {
            int combo = 1;
            var currentMatches = initialMatches;

            while (currentMatches != null && currentMatches.Count > 0)
            {
                SoundManager.Instance?.PlayMatch(combo);

                if (combo >= 3)
                {
                    int bonusScore = combo * 250;
                    OnComboBonus?.Invoke(combo, bonusScore);
                    _currentScore += bonusScore;
                    OnScoreChanged?.Invoke(_currentScore);
                }

                // 1. Marquer et détruire les tuiles
                var allTilesToDestroy = new HashSet<Match3Tile>();
                var specialCreations = new List<(int x, int y, CropType crop, SpecialTileType special)>();

                foreach (var group in currentMatches)
                {
                    foreach (var t in group.Tiles)
                    {
                        allTilesToDestroy.Add(t);
                    }

                    // Créer la tuile spéciale si nécessaire
                    if (group.SpecialToCreate != SpecialTileType.None && group.SpecialCreationTile != null)
                    {
                        var sc = group.SpecialCreationTile;
                        specialCreations.Add((sc.GridX, sc.GridY, group.Crop, group.SpecialToCreate));
                    }
                }

                // Gérer les effets des tuiles spéciales détruites (Lignes balayeuses & Bombes)
                var expandedExplosions = ExpandSpecialTiles(allTilesToDestroy);
                foreach (var exp in expandedExplosions) allTilesToDestroy.Add(exp);

                // Ajout score et objectifs
                AddScoreAndCollect(new List<Match3Tile>(allTilesToDestroy), combo);

                // Destruction effective
                foreach (var t in allTilesToDestroy)
                {
                    if (_grid[t.GridX, t.GridY] == t)
                    {
                        _grid[t.GridX, t.GridY] = null;
                    }
                    Destroy(t.gameObject);
                }

                // Faire naître les tuiles spéciales
                foreach (var sp in specialCreations)
                {
                    SoundManager.Instance?.PlaySpecialCreate();
                    SpawnTile(sp.x, sp.y, sp.crop, sp.special);
                }

                yield return new WaitForSeconds(0.15f);

                // 2. Gravité (Chute des tuiles vers le bas)
                yield return StartCoroutine(ApplyGravityCoroutine());

                // 3. Remplissage par le haut
                yield return StartCoroutine(RefillBoardCoroutine());

                // 4. Détecter de nouveaux matchs après la chute
                yield return new WaitForSeconds(0.12f);
                combo++;
                currentMatches = MatchDetector.FindMatches(_grid, Width, Height);
            }
        }

        private HashSet<Match3Tile> ExpandSpecialTiles(HashSet<Match3Tile> tiles)
        {
            var extra = new HashSet<Match3Tile>();
            foreach (var t in tiles)
            {
                if (t.Special == SpecialTileType.LineHorizontal)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (_grid[x, t.GridY] != null) extra.Add(_grid[x, t.GridY]);
                    }
                }
                else if (t.Special == SpecialTileType.LineVertical)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (_grid[t.GridX, y] != null) extra.Add(_grid[t.GridX, y]);
                    }
                }
                else if (t.Special == SpecialTileType.Bomb3x3)
                {
                    for (int bx = Mathf.Max(0, t.GridX - 1); bx <= Mathf.Min(Width - 1, t.GridX + 1); bx++)
                    {
                        for (int by = Mathf.Max(0, t.GridY - 1); by <= Mathf.Min(Height - 1, t.GridY + 1); by++)
                        {
                            if (_grid[bx, by] != null) extra.Add(_grid[bx, by]);
                        }
                    }
                }
            }
            return extra;
        }

        private void AddScoreAndCollect(List<Match3Tile> tiles, int comboMultiplier)
        {
            int scorePerTile = 50 * comboMultiplier;
            _currentScore += tiles.Count * scorePerTile;
            OnScoreChanged?.Invoke(_currentScore);

            bool targetsChanged = false;
            foreach (var t in tiles)
            {
                var target = _targets.Find(tgt => tgt.Crop == t.Crop);
                if (target != null && target.CurrentCollected < target.TargetCount)
                {
                    target.CurrentCollected++;
                    targetsChanged = true;
                }
            }

            if (targetsChanged)
            {
                OnTargetsUpdated?.Invoke(_targets);
            }
        }

        private IEnumerator ApplyGravityCoroutine()
        {
            for (int x = 0; x < Width; x++)
            {
                int emptyY = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] != null)
                    {
                        if (y != emptyY)
                        {
                            Match3Tile t = _grid[x, y];
                            _grid[x, emptyY] = t;
                            _grid[x, y] = null;
                            t.GridY = emptyY;
                            t.MoveToPosition(GridToWorld(x, emptyY), 16f);
                        }
                        emptyY++;
                    }
                }
            }
            yield return new WaitForSeconds(0.18f);
        }

        private IEnumerator RefillBoardCoroutine()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] == null)
                    {
                        CropType crop = (CurrentLevel != null && CurrentLevel.AvailableCrops.Count > 0)
                            ? CurrentLevel.AvailableCrops[UnityEngine.Random.Range(0, CurrentLevel.AvailableCrops.Count)]
                            : CropType.Tomato;

                        Match3Tile tile = SpawnTile(x, y, crop, SpecialTileType.None);
                        // Apparition au-dessus du plateau
                        tile.transform.localPosition = GridToWorld(x, Height + 1);
                        tile.MoveToPosition(GridToWorld(x, y), 16f);
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        public void ApplyBooster(BoosterType booster, Match3Tile targetTile)
        {
            if (targetTile == null) return;

            switch (booster)
            {
                case BoosterType.Shovel:
                    // Détruit la tuile
                    _grid[targetTile.GridX, targetTile.GridY] = null;
                    AddScoreAndCollect(new List<Match3Tile> { targetTile }, 1);
                    Destroy(targetTile.gameObject);
                    SoundManager.Instance?.PlayExplosion();
                    StartCoroutine(PostBoosterFallCoroutine());
                    break;

                case BoosterType.Tractor:
                    // Détruit toute la colonne
                    var colTiles = new List<Match3Tile>();
                    for (int y = 0; y < Height; y++)
                    {
                        if (_grid[targetTile.GridX, y] != null) colTiles.Add(_grid[targetTile.GridX, y]);
                    }
                    foreach (var t in colTiles) _grid[t.GridX, t.GridY] = null;
                    AddScoreAndCollect(colTiles, 2);
                    foreach (var t in colTiles) Destroy(t.gameObject);
                    SoundManager.Instance?.PlayExplosion();
                    StartCoroutine(PostBoosterFallCoroutine());
                    break;

                case BoosterType.RainbowFertilizer:
                    targetTile.Special = SpecialTileType.RainbowFlower;
                    targetTile.UpdateVisuals();
                    SoundManager.Instance?.PlaySpecialCreate();
                    break;

                case BoosterType.ExtraMoves:
                    AddExtraMoves(5);
                    break;
            }
        }

        public void AddExtraMoves(int amount)
        {
            _movesRemaining += amount;
            OnMovesChanged?.Invoke(_movesRemaining);
            SoundManager.Instance?.PlayButtonClick();
        }

        private IEnumerator PostBoosterFallCoroutine()
        {
            _isProcessing = true;
            yield return StartCoroutine(ApplyGravityCoroutine());
            yield return StartCoroutine(RefillBoardCoroutine());
            var matches = MatchDetector.FindMatches(_grid, Width, Height);
            if (matches.Count > 0)
            {
                yield return StartCoroutine(ProcessMatchesCascadeCoroutine(matches, null));
            }
            _isProcessing = false;
            CheckGameEndCondition();
        }

        private void CheckGameEndCondition()
        {
            bool allTargetsReached = true;
            foreach (var t in _targets)
            {
                if (!t.IsCompleted)
                {
                    allTargetsReached = false;
                    break;
                }
            }

            if (allTargetsReached)
            {
                // Victoire ! Calcul des étoiles
                int stars = 1;
                if (CurrentLevel != null)
                {
                    if (_currentScore >= CurrentLevel.ThreeStarScore) stars = 3;
                    else if (_currentScore >= CurrentLevel.TwoStarScore) stars = 2;
                }
                GameManager.Instance?.CompleteLevel(stars, _currentScore);
                OnLevelWon?.Invoke();
            }
            else if (_movesRemaining <= 0)
            {
                // Défaite (manque de coups)
                GameManager.Instance?.FailLevel();
                OnLevelLost?.Invoke();
            }
        }
    }
}
