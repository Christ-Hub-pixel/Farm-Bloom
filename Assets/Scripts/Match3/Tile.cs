using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public CropType cropType;
    public int x;
    public int y;

    private SpriteRenderer spriteRenderer;
    private Coroutine moveCoroutine;

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

    public void SetGridPosition(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
    }

    public void MoveTo(Vector3 targetPos, float duration)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos, duration));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Courbe fluide (smoothstep)
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        moveCoroutine = null;
    }

    public void Disappear(float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        StartCoroutine(DisappearRoutine(duration));
    }

    private IEnumerator DisappearRoutine(float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
