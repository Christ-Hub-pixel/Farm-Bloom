using System;
using UnityEngine;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Utils;

namespace FarmBloom.Farm
{
    public class FarmBuilding : MonoBehaviour
    {
        [Header("Building Data")]
        public BuildingType BuildingType;
        public int Level = 1;
        public bool IsConstructed = true;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private TextMesh _nameText;

        public event Action<FarmBuilding> OnBuildingClicked;

        private void Awake()
        {
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (_nameText == null)
            {
                GameObject txtObj = new GameObject("BuildingName");
                txtObj.transform.SetParent(transform);
                txtObj.transform.localPosition = new Vector3(0f, -0.65f, -0.1f);
                _nameText = txtObj.AddComponent<TextMesh>();
                _nameText.characterSize = 0.08f;
                _nameText.fontSize = 28;
                _nameText.alignment = TextAlignment.Center;
                _nameText.anchor = TextAnchor.MiddleCenter;
                _nameText.color = Color.white;
            }
        }

        public void Init(BuildingType type, int level, bool constructed)
        {
            BuildingType = type;
            Level = level;
            IsConstructed = constructed;
            UpdateVisuals();
        }

        public void Upgrade()
        {
            int cost = GetUpgradeCost();
            if (CurrencyManager.Instance.SpendCoins(cost))
            {
                Level++;
                IsConstructed = true;
                SoundManager.Instance?.PlaySpecialCreate();
                UpdateVisuals();
                SaveManager.Instance?.Save();
            }
        }

        public int GetUpgradeCost()
        {
            return 300 * Mathf.Max(1, Level * 2);
        }

        public string GetBuildingNameFr()
        {
            switch (BuildingType)
            {
                case BuildingType.FarmHouse: return "Maison de Ferme";
                case BuildingType.Windmill: return "Moulin à Farine";
                case BuildingType.Dairy: return "Fromagerie";
                case BuildingType.Barn: return "Grange";
                case BuildingType.ChickenCoop: return "Poulailler";
                case BuildingType.Bakery: return "Boulangerie";
                default: return "Bâtiment";
            }
        }

        public void UpdateVisuals()
        {
            Color col;
            switch (BuildingType)
            {
                case BuildingType.FarmHouse: col = new Color(0.95f, 0.75f, 0.3f); break; // Jaune bois
                case BuildingType.Windmill: col = new Color(0.85f, 0.85f, 0.9f); break;  // Blanc moulin
                case BuildingType.Dairy: col = new Color(0.4f, 0.7f, 0.9f); break;      // Bleu fromagerie
                case BuildingType.Barn: col = new Color(0.85f, 0.25f, 0.2f); break;     // Rouge grange
                case BuildingType.ChickenCoop: col = new Color(0.9f, 0.6f, 0.3f); break; // Bois clair
                default: col = new Color(0.6f, 0.5f, 0.4f); break;
            }

            if (SpriteFactory.Instance != null && _renderer.sprite == null)
            {
                _renderer.sprite = SpriteFactory.Instance.GetUIPanelSprite(140, 140, col, new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f));
            }

            if (_nameText != null)
            {
                _nameText.text = IsConstructed ? $"{GetBuildingNameFr()}\nNiv. {Level}" : $"{GetBuildingNameFr()}\n(À bâtir)";
            }
        }

        private void OnMouseDown()
        {
            OnBuildingClicked?.Invoke(this);
        }
    }
}
