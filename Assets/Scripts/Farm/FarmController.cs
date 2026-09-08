using System;
using System.Collections.Generic;
using UnityEngine;
using FarmBloom.Core;
using FarmBloom.Data;

namespace FarmBloom.Farm
{
    public class FarmController : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform _plotsContainer;
        [SerializeField] private Transform _buildingsContainer;
        [SerializeField] private Transform _animalsContainer;

        private readonly List<CropPlot> _plots = new List<CropPlot>();
        private readonly List<FarmBuilding> _buildings = new List<FarmBuilding>();

        public event Action<CropPlot> OnPlotPlantRequested;
        public event Action<FarmBuilding> OnBuildingDetailsRequested;

        private void Awake()
        {
            if (_plotsContainer == null)
            {
                var go = new GameObject("PlotsContainer");
                go.transform.SetParent(transform);
                _plotsContainer = go.transform;
            }

            if (_buildingsContainer == null)
            {
                var go = new GameObject("BuildingsContainer");
                go.transform.SetParent(transform);
                _buildingsContainer = go.transform;
            }

            if (_animalsContainer == null)
            {
                var go = new GameObject("AnimalsContainer");
                go.transform.SetParent(transform);
                _animalsContainer = go.transform;
            }
        }

        private void Start()
        {
            InitFarm();
        }

        public void InitFarm()
        {
            ClearFarm();
            SpawnPlots();
            SpawnBuildings();
            SpawnAnimals();
        }

        private void ClearFarm()
        {
            foreach (Transform child in _plotsContainer) Destroy(child.gameObject);
            foreach (Transform child in _buildingsContainer) Destroy(child.gameObject);
            foreach (Transform child in _animalsContainer) Destroy(child.gameObject);
            _plots.Clear();
            _buildings.Clear();
        }

        private void SpawnPlots()
        {
            var data = SaveManager.Instance?.Data;
            if (data == null) return;

            int cols = 3;
            float spacingX = 1.4f;
            float spacingY = 1.1f;
            Vector3 centerOffset = new Vector3(-0.7f, -1.8f, 0f);

            for (int i = 0; i < data.plots.Count; i++)
            {
                var pData = data.plots[i];
                GameObject go = new GameObject($"Plot_{i}");
                go.transform.SetParent(_plotsContainer);

                int col = i % cols;
                int row = i / cols;
                go.transform.localPosition = centerOffset + new Vector3(col * spacingX, -row * spacingY, 0f);

                CropPlot plot = go.AddComponent<CropPlot>();
                plot.LoadFromData(pData);
                plot.OnPlotClicked += HandlePlotClicked;
                _plots.Add(plot);
            }
        }

        private void SpawnBuildings()
        {
            var data = SaveManager.Instance?.Data;
            if (data == null) return;

            foreach (var bData in data.buildings)
            {
                GameObject go = new GameObject($"Building_{bData.buildingType}");
                go.transform.SetParent(_buildingsContainer);
                go.transform.localPosition = new Vector3(bData.posX, bData.posY, 0f);

                FarmBuilding building = go.AddComponent<FarmBuilding>();
                building.Init(bData.buildingType, bData.level, bData.isConstructed);
                building.OnBuildingClicked += HandleBuildingClicked;
                _buildings.Add(building);
            }
        }

        private void SpawnAnimals()
        {
            var data = SaveManager.Instance?.Data;
            if (data == null) return;

            // Spawns animés légers pour les animaux
            for (int i = 0; i < 4; i++)
            {
                GameObject animal = new GameObject($"Chicken_{i}");
                animal.transform.SetParent(_animalsContainer);
                animal.transform.localPosition = new Vector3(1.5f + (i * 0.45f), -0.2f + ((i % 2) * 0.3f), 0f);

                var sr = animal.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.95f, 0.85f); // Mascotte poulet
                animal.AddComponent<AnimalRoam>();
            }
        }

        private void HandlePlotClicked(CropPlot plot)
        {
            OnPlotPlantRequested?.Invoke(plot);
        }

        private void HandleBuildingClicked(FarmBuilding building)
        {
            OnBuildingDetailsRequested?.Invoke(building);
        }
    }

    public class AnimalRoam : MonoBehaviour
    {
        private Vector3 _startPos;
        private float _timeOffset;

        private void Start()
        {
            _startPos = transform.localPosition;
            _timeOffset = UnityEngine.Random.Range(0f, 10f);
        }

        private void Update()
        {
            float t = Time.time * 2f + _timeOffset;
            float bobbing = Mathf.Abs(Mathf.Sin(t)) * 0.08f;
            transform.localPosition = _startPos + new Vector3(Mathf.Cos(t * 0.5f) * 0.15f, bobbing, 0f);
        }
    }
}
