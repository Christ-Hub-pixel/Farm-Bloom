using System;
using UnityEngine;
using FarmBloom.Core;
using FarmBloom.Utils;

namespace FarmBloom.Match3
{
    public class Match3Tile : MonoBehaviour
    {
        [Header("Grid Coordinates")]
        public int GridX;
        public int GridY;

        [Header("Tile Properties")]
        public CropType Crop = CropType.None;
        public SpecialTileType Special = SpecialTileType.None;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlightRing;

        private Vector3 _targetPosition;
        private bool _isMoving = false;
        private float _moveSpeed = 12f;

        public bool IsSelected { get; private set; } = false;

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
                if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (_isMoving)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, _moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.localPosition, _targetPosition) < 0.005f)
                {
                    transform.localPosition = _targetPosition;
                    _isMoving = false;
                }
            }
        }

        public void Init(int x, int y, CropType crop, SpecialTileType special = SpecialTileType.None)
        {
            GridX = x;
            GridY = y;
            Crop = crop;
            Special = special;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (SpriteFactory.Instance != null && Crop != CropType.None)
            {
                _renderer.sprite = SpriteFactory.Instance.GetCropSprite(Crop, Special);
            }
            transform.localScale = Vector3.one;
        }

        public void MoveToPosition(Vector3 worldPos, float speed = 14f)
        {
            _targetPosition = worldPos;
            _moveSpeed = speed;
            _isMoving = true;
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (selected)
            {
                transform.localScale = Vector3.one * 1.15f;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        public void PlayBounceAnimation()
        {
            StartCoroutine(BounceCoroutine());
        }

        private System.Collections.IEnumerator BounceCoroutine()
        {
            Vector3 startScale = Vector3.one;
            Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);
            Vector3 stretchScale = new Vector3(0.9f, 1.15f, 1f);

            float t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, squashScale, t / 0.08f);
                yield return null;
            }

            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(squashScale, stretchScale, t / 0.1f);
                yield return null;
            }

            t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(stretchScale, startScale, t / 0.08f);
                yield return null;
            }
            transform.localScale = startScale;
        }
    }
}
