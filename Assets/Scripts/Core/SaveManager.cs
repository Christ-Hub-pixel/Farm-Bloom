using System;
using System.IO;
using UnityEngine;
using FarmBloom.Data;

namespace FarmBloom.Core
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        private GameSaveData _currentData;
        public GameSaveData Data => _currentData;

        public event Action OnDataLoaded;
        public event Action OnDataSaved;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "farmbloom_save.json");

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _currentData = JsonUtility.FromJson<GameSaveData>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Erreur de lecture : {ex.Message}. Utilisation des données par défaut.");
            }

            if (_currentData == null)
            {
                _currentData = GameSaveData.CreateDefault();
                Save();
            }

            OnDataLoaded?.Invoke();
        }

        public void Save()
        {
            if (_currentData == null) return;

            try
            {
                string json = JsonUtility.ToJson(_currentData, true);
                File.WriteAllText(SaveFilePath, json);
                OnDataSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Échec de la sauvegarde : {ex.Message}");
            }
        }

        public void ResetSave()
        {
            _currentData = GameSaveData.CreateDefault();
            Save();
            OnDataLoaded?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
