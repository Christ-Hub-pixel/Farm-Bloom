using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;

namespace FarmBloom.Match3
{
    public class MatchGroup
    {
        public List<Match3Tile> Tiles = new List<Match3Tile>();
        public CropType Crop;
        public SpecialTileType SpecialToCreate = SpecialTileType.None;
        public Match3Tile SpecialCreationTile = null;
    }

    public static class MatchDetector
    {
        public static List<MatchGroup> FindMatches(Match3Tile[,] grid, int width, int height, Match3Tile swappedTile = null)
        {
            var results = new List<MatchGroup>();
            var matchedSet = new HashSet<Match3Tile>();

            var horizontalRuns = new List<List<Match3Tile>>();
            var verticalRuns = new List<List<Match3Tile>>();

            // 1. Détection Horizontale
            for (int y = 0; y < height; y++)
            {
                int runLength = 1;
                for (int x = 0; x < width; x++)
                {
                    Match3Tile current = grid[x, y];
                    bool hasNext = (x < width - 1);
                    Match3Tile next = hasNext ? grid[x + 1, y] : null;

                    if (hasNext && current != null && next != null && current.Crop != CropType.None && current.Crop == next.Crop)
                    {
                        runLength++;
                    }
                    else
                    {
                        if (runLength >= 3 && current != null && current.Crop != CropType.None)
                        {
                            var run = new List<Match3Tile>();
                            for (int r = 0; r < runLength; r++)
                            {
                                run.Add(grid[x - r, y]);
                            }
                            horizontalRuns.Add(run);
                        }
                        runLength = 1;
                    }
                }
            }

            // 2. Détection Verticale
            for (int x = 0; x < width; x++)
            {
                int runLength = 1;
                for (int y = 0; y < height; y++)
                {
                    Match3Tile current = grid[x, y];
                    bool hasNext = (y < height - 1);
                    Match3Tile next = hasNext ? grid[x, y + 1] : null;

                    if (hasNext && current != null && next != null && current.Crop != CropType.None && current.Crop == next.Crop)
                    {
                        runLength++;
                    }
                    else
                    {
                        if (runLength >= 3 && current != null && current.Crop != CropType.None)
                        {
                            var run = new List<Match3Tile>();
                            for (int r = 0; r < runLength; r++)
                            {
                                run.Add(grid[x, y - r]);
                            }
                            verticalRuns.Add(run);
                        }
                        runLength = 1;
                    }
                }
            }

            // 3. Fusion des intersections (L ou T -> Bombe 3x3)
            var processedH = new bool[horizontalRuns.Count];
            var processedV = new bool[verticalRuns.Count];

            for (int i = 0; i < horizontalRuns.Count; i++)
            {
                for (int j = 0; j < verticalRuns.Count; j++)
                {
                    if (horizontalRuns[i][0].Crop == verticalRuns[j][0].Crop)
                    {
                        // Vérifier si une tuile est commune
                        Match3Tile intersection = null;
                        foreach (var th in horizontalRuns[i])
                        {
                            if (verticalRuns[j].Contains(th))
                            {
                                intersection = th;
                                break;
                            }
                        }

                        if (intersection != null)
                        {
                            var combined = new MatchGroup();
                            combined.Crop = intersection.Crop;
                            combined.SpecialToCreate = SpecialTileType.Bomb3x3;
                            combined.SpecialCreationTile = (swappedTile != null && (horizontalRuns[i].Contains(swappedTile) || verticalRuns[j].Contains(swappedTile))) ? swappedTile : intersection;

                            foreach (var th in horizontalRuns[i])
                            {
                                if (!combined.Tiles.Contains(th)) combined.Tiles.Add(th);
                            }
                            foreach (var tv in verticalRuns[j])
                            {
                                if (!combined.Tiles.Contains(tv)) combined.Tiles.Add(tv);
                            }

                            results.Add(combined);
                            processedH[i] = true;
                            processedV[j] = true;
                        }
                    }
                }
            }

            // 4. Ajout des séries horizontales restantes
            for (int i = 0; i < horizontalRuns.Count; i++)
            {
                if (processedH[i]) continue;
                var run = horizontalRuns[i];
                var group = new MatchGroup();
                group.Crop = run[0].Crop;
                group.Tiles.AddRange(run);

                if (run.Count >= 5)
                {
                    group.SpecialToCreate = SpecialTileType.RainbowFlower;
                    group.SpecialCreationTile = (swappedTile != null && run.Contains(swappedTile)) ? swappedTile : run[run.Count / 2];
                }
                else if (run.Count == 4)
                {
                    group.SpecialToCreate = SpecialTileType.LineVertical; // balaye verticalement
                    group.SpecialCreationTile = (swappedTile != null && run.Contains(swappedTile)) ? swappedTile : run[run.Count / 2];
                }

                results.Add(group);
            }

            // 5. Ajout des séries verticales restantes
            for (int j = 0; j < verticalRuns.Count; j++)
            {
                if (processedV[j]) continue;
                var run = verticalRuns[j];
                var group = new MatchGroup();
                group.Crop = run[0].Crop;
                group.Tiles.AddRange(run);

                if (run.Count >= 5)
                {
                    group.SpecialToCreate = SpecialTileType.RainbowFlower;
                    group.SpecialCreationTile = (swappedTile != null && run.Contains(swappedTile)) ? swappedTile : run[run.Count / 2];
                }
                else if (run.Count == 4)
                {
                    group.SpecialToCreate = SpecialTileType.LineHorizontal; // balaye horizontalement
                    group.SpecialCreationTile = (swappedTile != null && run.Contains(swappedTile)) ? swappedTile : run[run.Count / 2];
                }

                results.Add(group);
            }

            return results;
        }

        public static bool HasPossibleMoves(Match3Tile[,] grid, int width, int height)
        {
            // Vérifie les swaps horizontaux et verticaux simulés
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Swap avec droite
                    if (x < width - 1)
                    {
                        SwapSimulated(grid, x, y, x + 1, y);
                        bool matches = FindMatches(grid, width, height).Count > 0;
                        SwapSimulated(grid, x, y, x + 1, y); // Annuler
                        if (matches) return true;
                    }
                    // Swap avec haut
                    if (y < height - 1)
                    {
                        SwapSimulated(grid, x, y, x, y + 1);
                        bool matches = FindMatches(grid, width, height).Count > 0;
                        SwapSimulated(grid, x, y, x, y + 1); // Annuler
                        if (matches) return true;
                    }
                }
            }
            return false;
        }

        private static void SwapSimulated(Match3Tile[,] grid, int x1, int y1, int x2, int y2)
        {
            Match3Tile temp = grid[x1, y1];
            grid[x1, y1] = grid[x2, y2];
            grid[x2, y2] = temp;
        }
    }
}
