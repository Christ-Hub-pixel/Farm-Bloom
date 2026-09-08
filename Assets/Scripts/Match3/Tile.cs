using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    public CropType cropType;

    private SpriteRenderer spriteRenderer;
    private BoardManager boardManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        col.size = new Vector2(1f, 1f);
    }

    public void Setup(CropType type, Sprite sprite, BoardManager manager)
    {
        cropType = type;
        boardManager = manager;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void OnMouseDown()
    {
        if (boardManager != null)
        {
            boardManager.SelectTile(this);
        }
    }
}
