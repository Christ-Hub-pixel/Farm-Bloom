using UnityEngine;

public class Tile : MonoBehaviour
{
    public CropType cropType;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(CropType type, Sprite sprite)
    {
        cropType = type;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
