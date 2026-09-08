using System;
using UnityEngine;
using FarmBloom.Core;
using FarmBloom.Data;
using FarmBloom.Utils;

namespace FarmBloom.Farm
{
    public class CropPlot : MonoBehaviour
    {
        [Header("Plot Identification")]
        public int PlotId;

        [Header("Plot State")]
        public PlotState CurrentState = PlotState.Empty;
        public CropType CurrentCrop = CropType.None;
        public long PlantedEpoch = 0;
        public int GrowthDuration = 60;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer _soilRenderer;
        [SerializeField] private SpriteRenderer _plantRenderer;
        [SerializeField] private TextMesh _timerText;

        public event Action<CropPlot> OnPlotClicked;
        public event Action<CropPlot, CropType, int> OnCropHarvested;

        private void Awake()
        {
            SetupVisualComponents();
        }

        private void SetupVisualComponents()
        {
            if (_soilRenderer == null)
            {
                _soilRenderer = gameObject.AddComponent<SpriteRenderer>();
                // Sol labouré brun
                _soilRenderer.color = new Color(0.48f, 0.28f, 0.14f);
            }

            if (_plantRenderer == null)
            {
                GameObject plantObj = new GameObject("PlantVisual");
                plantObj.transform.SetParent(transform);
                plantObj.transform.localPosition = new Vector3(0f, 0.2f, -0.1f);
                _plantRenderer = plantObj.AddComponent<SpriteRenderer>();
            }

            if (_timerText == null)
            {
                GameObject textObj = new GameObject("TimerText");
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = new Vector3(0f, -0.35f, -0.2f);
                _timerText = textObj.AddComponent<TextMesh>();
                _timerText.characterSize = 0.08f;
                _timerText.fontSize = 32;
                _timerText.alignment = TextAlignment.Center;
                _timerText.anchor = TextAnchor.MiddleCenter;
                _timerText.color = Color.white;
            }
        }

        public void LoadFromData(PlotSaveData data)
        {
            PlotId = data.plotId;
            CurrentState = data.state;
            CurrentCrop = data.crop;
            PlantedEpoch = data.plantedEpochSeconds;
            GrowthDuration = data.growthDurationSeconds;
            UpdateVisuals();
        }

        public void SaveToData(PlotSaveData data)
        {
            data.plotId = PlotId;
            data.state = CurrentState;
            data.crop = CurrentCrop;
            data.plantedEpochSeconds = PlantedEpoch;
            data.growthDurationSeconds = GrowthDuration;
        }

        private void Update()
        {
            if (CurrentState == PlotState.Growing || CurrentState == PlotState.Planted)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long elapsed = now - PlantedEpoch;

                if (elapsed >= GrowthDuration)
                {
                    CurrentState = PlotState.ReadyToHarvest;
                    UpdateVisuals();
                    SaveManager.Instance?.Save();
                }
                else
                {
                    long remaining = GrowthDuration - elapsed;
                    _timerText.text = $"{remaining / 60:D2}:{remaining % 60:D2}";
                }
            }
        }

        public void Plant(CropType crop)
        {
            var info = FarmCropDatabase.Get(crop);
            if (CurrencyManager.Instance.Coins < info.SeedPriceCoins)
            {
                Debug.LogWarning("Pas assez de pièces pour acheter cette semence !");
                return;
            }

            CurrencyManager.Instance.SpendCoins(info.SeedPriceCoins);
            CurrentCrop = crop;
            CurrentState = PlotState.Growing;
            PlantedEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            GrowthDuration = info.GrowthDurationSeconds;

            SoundManager.Instance?.PlayButtonClick();
            UpdateVisuals();
            SaveManager.Instance?.Save();
        }

        public void Harvest()
        {
            if (CurrentState != PlotState.ReadyToHarvest) return;

            var info = FarmCropDatabase.Get(CurrentCrop);

            // Ajout dans la grange
            if (SaveManager.Instance?.Data != null)
            {
                var barn = SaveManager.Instance.Data.barnInventory;
                var item = barn.Find(i => i.crop == CurrentCrop);
                if (item != null)
                {
                    item.count += info.HarvestYield;
                }
                else
                {
                    barn.Add(new InventoryItemEntry { crop = CurrentCrop, count = info.HarvestYield });
                }

                // Gain XP et points de ferme
                SaveManager.Instance.Data.playerXp += info.XpReward;
                SaveManager.Instance.Save();
            }

            SoundManager.Instance?.PlayHarvest();
            OnCropHarvested?.Invoke(this, CurrentCrop, info.HarvestYield);

            CurrentState = PlotState.Empty;
            CurrentCrop = CropType.None;
            PlantedEpoch = 0;
            UpdateVisuals();
            SaveManager.Instance?.Save();
        }

        public void OnMouseDown()
        {
            if (CurrentState == PlotState.ReadyToHarvest)
            {
                Harvest();
            }
            else if (CurrentState == PlotState.Empty)
            {
                OnPlotClicked?.Invoke(this);
            }
            else
            {
                SoundManager.Instance?.PlayButtonClick();
            }
        }

        public void UpdateVisuals()
        {
            if (SpriteFactory.Instance != null && _soilRenderer.sprite == null)
            {
                _soilRenderer.sprite = SpriteFactory.Instance.GetUIPanelSprite(120, 90, new Color(0.45f, 0.26f, 0.12f), new Color(0.35f, 0.18f, 0.08f));
            }

            switch (CurrentState)
            {
                case PlotState.Empty:
                    _plantRenderer.sprite = null;
                    _timerText.text = "Semer";
                    _timerText.color = new Color(0.9f, 0.9f, 0.7f);
                    break;

                case PlotState.Planted:
                case PlotState.Growing:
                    if (SpriteFactory.Instance != null && CurrentCrop != CropType.None)
                    {
                        _plantRenderer.sprite = SpriteFactory.Instance.GetCropSprite(CurrentCrop);
                        _plantRenderer.transform.localScale = Vector3.one * 0.45f;
                    }
                    _timerText.color = Color.white;
                    break;

                case PlotState.ReadyToHarvest:
                    if (SpriteFactory.Instance != null && CurrentCrop != CropType.None)
                    {
                        _plantRenderer.sprite = SpriteFactory.Instance.GetCropSprite(CurrentCrop);
                        _plantRenderer.transform.localScale = Vector3.one * 0.85f;
                    }
                    _timerText.text = "Récolter !";
                    _timerText.color = new Color(0.2f, 1f, 0.3f);
                    break;
            }
        }
    }
}
