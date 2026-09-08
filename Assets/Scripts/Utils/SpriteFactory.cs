using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;

namespace FarmBloom.Utils
{
    public class SpriteFactory : MonoBehaviour
    {
        private static SpriteFactory _instance;
        public static SpriteFactory Instance => _instance;

        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

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

        public Sprite GetCropSprite(CropType crop, SpecialTileType special = SpecialTileType.None)
        {
            string key = $"crop_{crop}_{special}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            // Remplissage transparent
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.38f;

            Color primaryColor = GetCropColor(crop);
            Color secondaryColor = GetCropSecondaryColor(crop);

            switch (crop)
            {
                case CropType.Tomato:
                    DrawCircle(pixels, size, center, radius, primaryColor, secondaryColor);
                    // Feuilles vertes au sommet
                    DrawLeaf(pixels, size, new Vector2(center.x - 12, center.y + radius - 4), 16, new Color(0.2f, 0.8f, 0.25f));
                    DrawLeaf(pixels, size, new Vector2(center.x + 12, center.y + radius - 4), 16, new Color(0.2f, 0.8f, 0.25f));
                    DrawStem(pixels, size, new Vector2(center.x, center.y + radius), 12, new Color(0.15f, 0.6f, 0.18f));
                    break;

                case CropType.Corn:
                    // Épi ovale jaune
                    DrawOval(pixels, size, center, radius * 0.7f, radius * 1.05f, primaryColor, secondaryColor);
                    // Feuilles d'épi vertes à la base
                    DrawLeaf(pixels, size, new Vector2(center.x - 22, center.y - 15), 24, new Color(0.35f, 0.75f, 0.2f));
                    DrawLeaf(pixels, size, new Vector2(center.x + 22, center.y - 15), 24, new Color(0.35f, 0.75f, 0.2f));
                    // Grains de maïs (détail)
                    DrawCornKernels(pixels, size, center, radius * 0.6f);
                    break;

                case CropType.Carrot:
                    // Forme conique orange
                    DrawCarrot(pixels, size, center, radius, primaryColor, secondaryColor);
                    // Fanette verte
                    DrawTuft(pixels, size, new Vector2(center.x, center.y + radius * 0.8f), 24, new Color(0.18f, 0.85f, 0.25f));
                    break;

                case CropType.Banana:
                    // Banane arquée jaune vive
                    DrawBanana(pixels, size, center, radius, primaryColor, secondaryColor);
                    break;

                case CropType.SweetPotato:
                    // Ovale violet doux
                    DrawOval(pixels, size, center, radius * 1.1f, radius * 0.7f, primaryColor, secondaryColor);
                    DrawHighlight(pixels, size, new Vector2(center.x - 10, center.y + 10), radius * 0.35f);
                    break;

                case CropType.Flower:
                    // Fleur style marguerite blanche/rose coeur doré
                    DrawFlower(pixels, size, center, radius, primaryColor, secondaryColor);
                    break;

                case CropType.Cocoa:
                    // Cabosse de cacao marron avec rayures
                    DrawOval(pixels, size, center, radius * 0.75f, radius * 1.05f, primaryColor, secondaryColor);
                    DrawStripes(pixels, size, center, radius * 0.65f, new Color(0.28f, 0.12f, 0.05f));
                    break;

                default:
                    DrawCircle(pixels, size, center, radius, primaryColor, secondaryColor);
                    break;
            }

            // Effets spéciaux (ligne balayeuse, bombe, arc-en-ciel)
            if (special == SpecialTileType.LineHorizontal || special == SpecialTileType.LineVertical)
            {
                DrawGlowLines(pixels, size, center, special == SpecialTileType.LineHorizontal);
            }
            else if (special == SpecialTileType.Bomb3x3)
            {
                DrawBombSpark(pixels, size, center);
            }
            else if (special == SpecialTileType.RainbowFlower)
            {
                DrawRainbowRing(pixels, size, center, radius * 1.1f);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _spriteCache[key] = sprite;
            return sprite;
        }

        public Sprite GetCurrencySprite(RewardType type)
        {
            string key = $"currency_{type}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            switch (type)
            {
                case RewardType.Coins:
                    // Pièce d'or éclatante
                    DrawCircle(pixels, size, center, size * 0.42f, new Color(1f, 0.85f, 0.1f), new Color(0.85f, 0.55f, 0.05f));
                    DrawCircle(pixels, size, center, size * 0.32f, new Color(1f, 0.92f, 0.25f), new Color(0.95f, 0.7f, 0.1f));
                    DrawStar(pixels, size, center, size * 0.18f, new Color(1f, 1f, 0.6f));
                    break;

                case RewardType.Gems:
                    // Gemme violette facettée
                    DrawGem(pixels, size, center, size * 0.4f, new Color(0.72f, 0.25f, 0.95f), new Color(0.45f, 0.1f, 0.75f));
                    break;

                case RewardType.Energy:
                    // Coeur rouge vif
                    DrawHeart(pixels, size, center, size * 0.4f, new Color(1f, 0.2f, 0.3f), new Color(0.75f, 0.08f, 0.15f));
                    break;

                default:
                    DrawCircle(pixels, size, center, size * 0.4f, Color.white, Color.gray);
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _spriteCache[key] = sprite;
            return sprite;
        }

        public Sprite GetStarSprite(bool earned)
        {
            string key = $"star_{earned}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Color starCol = earned ? new Color(1f, 0.82f, 0.05f) : new Color(0.35f, 0.35f, 0.4f, 0.8f);
            Color starBorder = earned ? new Color(0.85f, 0.5f, 0.02f) : new Color(0.2f, 0.2f, 0.25f, 0.8f);

            DrawStar(pixels, size, center, size * 0.45f, starBorder);
            DrawStar(pixels, size, center, size * 0.38f, starCol);

            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _spriteCache[key] = sprite;
            return sprite;
        }

        public Sprite GetUIPanelSprite(int width = 256, int height = 256, Color? baseColor = null, Color? borderColor = null)
        {
            Color baseCol = baseColor ?? new Color(0.96f, 0.88f, 0.72f); // Bois beige chaleureux
            Color borderCol = borderColor ?? new Color(0.55f, 0.32f, 0.15f); // Bordure bois brun

            string key = $"ui_panel_{width}_{height}_{baseCol}_{borderCol}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            int borderWidth = 8;
            int cornerRadius = 24;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    float distCorner = GetCornerDistance(x, y, width, height, cornerRadius);

                    if (distCorner > cornerRadius)
                    {
                        pixels[idx] = Color.clear;
                    }
                    else if (distCorner > cornerRadius - borderWidth || x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth)
                    {
                        pixels[idx] = borderCol;
                    }
                    else
                    {
                        // Léger dégradé vertical
                        float gradient = (float)y / height * 0.15f;
                        pixels[idx] = new Color(baseCol.r + gradient, baseCol.g + gradient, baseCol.b + gradient, baseCol.a);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            _spriteCache[key] = sprite;
            return sprite;
        }

        public Sprite GetUIButtonSprite(int width = 220, int height = 70, Color? buttonColor = null)
        {
            Color col = buttonColor ?? new Color(0.35f, 0.78f, 0.15f); // Vert vif Farm Bloom
            Color darkCol = new Color(col.r * 0.65f, col.g * 0.65f, col.b * 0.65f);

            string key = $"ui_btn_{width}_{height}_{col}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            int radius = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    float distCorner = GetPillDistance(x, y, width, height, radius);

                    if (distCorner > radius)
                    {
                        pixels[idx] = Color.clear;
                    }
                    else if (distCorner > radius - 4 || y < 6)
                    {
                        pixels[idx] = darkCol; // Ombrage bas
                    }
                    else
                    {
                        float shine = (float)y / height * 0.25f;
                        pixels[idx] = new Color(Mathf.Min(1f, col.r + shine), Mathf.Min(1f, col.g + shine), Mathf.Min(1f, col.b + shine));
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            _spriteCache[key] = sprite;
            return sprite;
        }

        // ==================== ALGORITHMES GRAPHIQUES INTERNES ====================

        private Color GetCropColor(CropType crop)
        {
            switch (crop)
            {
                case CropType.Tomato: return new Color(0.96f, 0.22f, 0.18f);      // Rouge tomate
                case CropType.Corn: return new Color(1f, 0.85f, 0.12f);           // Jaune maïs
                case CropType.Carrot: return new Color(1f, 0.55f, 0.05f);         // Orange carotte
                case CropType.Banana: return new Color(1f, 0.88f, 0.18f);         // Jaune banane
                case CropType.SweetPotato: return new Color(0.72f, 0.22f, 0.65f); // Violet patate
                case CropType.Flower: return new Color(1f, 0.45f, 0.72f);          // Rose fleur
                case CropType.Cocoa: return new Color(0.48f, 0.24f, 0.12f);       // Brun cacao
                default: return Color.white;
            }
        }

        private Color GetCropSecondaryColor(CropType crop)
        {
            Color c = GetCropColor(crop);
            return new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f);
        }

        private void DrawCircle(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            float sqrRadius = radius * radius;
            float sqrInner = (radius - 3f) * (radius - 3f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= sqrRadius)
                    {
                        int idx = y * size + x;
                        if (distSq > sqrInner)
                        {
                            pixels[idx] = border;
                        }
                        else
                        {
                            // Éclairage sphérique
                            float lightDist = (x - (center.x - radius * 0.3f)) * (x - (center.x - radius * 0.3f)) +
                                              (y - (center.y + radius * 0.3f)) * (y - (center.y + radius * 0.3f));
                            float highlight = Mathf.Clamp01(1f - (lightDist / (radius * radius * 1.5f))) * 0.35f;
                            pixels[idx] = new Color(Mathf.Min(1f, fill.r + highlight), Mathf.Min(1f, fill.g + highlight), Mathf.Min(1f, fill.b + highlight), 1f);
                        }
                    }
                }
            }
        }

        private void DrawOval(Color[] pixels, int size, Vector2 center, float radiusX, float radiusY, Color fill, Color border)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center.x) / radiusX;
                    float ny = (y - center.y) / radiusY;
                    float normDist = nx * nx + ny * ny;

                    if (normDist <= 1f)
                    {
                        int idx = y * size + x;
                        if (normDist > 0.85f)
                        {
                            pixels[idx] = border;
                        }
                        else
                        {
                            float highlight = Mathf.Clamp01(1f - ((x - center.x + radiusX * 0.3f) * (x - center.x + radiusX * 0.3f) + (y - center.y - radiusY * 0.3f) * (y - center.y - radiusY * 0.3f)) / (radiusX * radiusY)) * 0.3f;
                            pixels[idx] = new Color(Mathf.Min(1f, fill.r + highlight), Mathf.Min(1f, fill.g + highlight), Mathf.Min(1f, fill.b + highlight), 1f);
                        }
                    }
                }
            }
        }

        private void DrawCarrot(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            for (int y = 0; y < size; y++)
            {
                float relativeY = (y - (center.y - radius)) / (radius * 1.8f);
                if (relativeY < 0f || relativeY > 1f) continue;

                float widthAtY = Mathf.Lerp(3f, radius * 0.85f, relativeY);

                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center.x);
                    if (dx <= widthAtY)
                    {
                        int idx = y * size + x;
                        if (dx > widthAtY - 2.5f || relativeY > 0.95f)
                        {
                            pixels[idx] = border;
                        }
                        else
                        {
                            float hl = (1f - (dx / widthAtY)) * 0.25f;
                            pixels[idx] = new Color(Mathf.Min(1f, fill.r + hl), Mathf.Min(1f, fill.g + hl), Mathf.Min(1f, fill.b + hl), 1f);
                        }
                    }
                }
            }
        }

        private void DrawBanana(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float arcCenter = center.x + Mathf.Sin((y - center.y) / radius * 1.5f) * 16f;
                    float dx = Mathf.Abs(x - arcCenter);
                    float dy = Mathf.Abs(y - center.y);

                    if (dy < radius * 0.9f && dx < radius * 0.38f * (1f - (dy / radius) * 0.4f))
                    {
                        int idx = y * size + x;
                        if (dx > radius * 0.32f || dy > radius * 0.85f)
                            pixels[idx] = border;
                        else
                            pixels[idx] = fill;
                    }
                }
            }
        }

        private void DrawFlower(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            // 5 Pétales
            for (int p = 0; p < 5; p++)
            {
                float angle = p * (Mathf.PI * 2f / 5f);
                Vector2 petalCenter = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * 0.55f);
                DrawCircle(pixels, size, petalCenter, radius * 0.4f, fill, border);
            }
            // Coeur jaune d'or
            DrawCircle(pixels, size, center, radius * 0.32f, new Color(1f, 0.85f, 0.1f), new Color(0.85f, 0.55f, 0.05f));
        }

        private void DrawStar(Color[] pixels, int size, Vector2 center, float radius, Color col)
        {
            int points = 5;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius) continue;

                    float angle = Mathf.Atan2(dy, dx) + Mathf.PI * 0.5f;
                    float m = Mathf.Cos(points * angle);
                    float starR = radius * (0.55f + 0.45f * m);

                    if (dist <= starR)
                    {
                        pixels[y * size + x] = col;
                    }
                }
            }
        }

        private void DrawHeart(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center.x) / radius;
                    float ny = (y - center.y) / radius;

                    float a = nx * nx + ny * ny - 1f;
                    if (a * a * a - nx * nx * ny * ny * ny <= 0f)
                    {
                        pixels[y * size + x] = fill;
                    }
                }
            }
        }

        private void DrawGem(Color[] pixels, int size, Vector2 center, float radius, Color fill, Color border)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center.x);
                    float dy = Mathf.Abs(y - center.y);

                    if (dx / radius + dy / radius <= 1f)
                    {
                        pixels[y * size + x] = (dx / radius + dy / radius > 0.85f) ? border : fill;
                    }
                }
            }
        }

        private void DrawLeaf(Color[] pixels, int size, Vector2 pos, float leafSize, Color col)
        {
            DrawCircle(pixels, size, pos, leafSize * 0.45f, col, new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f));
        }

        private void DrawStem(Color[] pixels, int size, Vector2 pos, float stemH, Color col)
        {
            for (int y = (int)pos.y; y < pos.y + stemH && y < size; y++)
            {
                for (int x = (int)pos.x - 2; x <= pos.x + 2 && x < size; x++)
                {
                    if (x >= 0 && y >= 0) pixels[y * size + x] = col;
                }
            }
        }

        private void DrawTuft(Color[] pixels, int size, Vector2 pos, float height, Color col)
        {
            DrawLeaf(pixels, size, new Vector2(pos.x - 10, pos.y + height * 0.4f), 14, col);
            DrawLeaf(pixels, size, new Vector2(pos.x + 10, pos.y + height * 0.4f), 14, col);
            DrawLeaf(pixels, size, new Vector2(pos.x, pos.y + height * 0.7f), 16, col);
        }

        private void DrawCornKernels(Color[] pixels, int size, Vector2 center, float radius)
        {
            Color dot = new Color(0.85f, 0.65f, 0.05f);
            for (int row = -3; row <= 3; row++)
            {
                for (int col = -2; col <= 2; col++)
                {
                    int kx = (int)(center.x + col * 9);
                    int ky = (int)(center.y + row * 11);
                    if (kx >= 0 && kx < size && ky >= 0 && ky < size)
                    {
                        pixels[ky * size + kx] = dot;
                    }
                }
            }
        }

        private void DrawStripes(Color[] pixels, int size, Vector2 center, float radius, Color stripeCol)
        {
            for (int s = -2; s <= 2; s++)
            {
                float xOffset = s * (radius * 0.35f);
                for (int y = (int)(center.y - radius); y <= center.y + radius && y < size; y++)
                {
                    int x = (int)(center.x + xOffset);
                    if (x >= 0 && x < size && y >= 0)
                    {
                        if (pixels[y * size + x].a > 0.5f) pixels[y * size + x] = stripeCol;
                    }
                }
            }
        }

        private void DrawHighlight(Color[] pixels, int size, Vector2 pos, float rad)
        {
            DrawCircle(pixels, size, pos, rad, new Color(1f, 1f, 1f, 0.4f), Color.clear);
        }

        private void DrawGlowLines(Color[] pixels, int size, Vector2 center, bool horizontal)
        {
            Color glow = new Color(1f, 1f, 1f, 0.9f);
            if (horizontal)
            {
                for (int y = (int)center.y - 4; y <= center.y + 4; y++)
                {
                    for (int x = 10; x < size - 10; x++)
                        if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = glow;
                }
            }
            else
            {
                for (int x = (int)center.x - 4; x <= center.x + 4; x++)
                {
                    for (int y = 10; y < size - 10; y++)
                        if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = glow;
                }
            }
        }

        private void DrawBombSpark(Color[] pixels, int size, Vector2 center)
        {
            Color spark = new Color(1f, 0.95f, 0.2f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (Mathf.PI / 4f);
                int sx = (int)(center.x + Mathf.Cos(angle) * 36f);
                int sy = (int)(center.y + Mathf.Sin(angle) * 36f);
                if (sx >= 0 && sx < size && sy >= 0 && sy < size) pixels[sy * size + sx] = spark;
            }
        }

        private void DrawRainbowRing(Color[] pixels, int size, Vector2 center, float radius)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > radius - 6f && dist <= radius)
                    {
                        float angle = (Mathf.Atan2(y - center.y, x - center.x) + Mathf.PI) / (Mathf.PI * 2f);
                        pixels[y * size + x] = Color.HSVToRGB(angle, 0.85f, 1f);
                    }
                }
            }
        }

        private float GetCornerDistance(int x, int y, int width, int height, int cornerRadius)
        {
            int cx = (x < cornerRadius) ? cornerRadius : (x >= width - cornerRadius ? width - cornerRadius - 1 : x);
            int cy = (y < cornerRadius) ? cornerRadius : (y >= height - cornerRadius ? height - cornerRadius - 1 : y);
            return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        }

        private float GetPillDistance(int x, int y, int width, int height, int radius)
        {
            int cx = Mathf.Clamp(x, radius, width - radius);
            int cy = height / 2;
            return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        }
    }
}
