using System;
using UnityEngine;

namespace FarmBloom.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        private static CurrencyManager _instance;
        public static CurrencyManager Instance => _instance;

        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;
        public event Action<int, int> OnEnergyChanged; // current, max
        public event Action<int> OnPiggyBankChanged;

        private const int ENERGY_RECHARGE_SECONDS = 1200; // 20 minutes par vie

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

        private void Start()
        {
            UpdateEnergyRecharge();
        }

        private void Update()
        {
            // Vérification périodique de recharge d'énergie
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                var data = SaveManager.Instance.Data;
                if (data.energy < data.maxEnergy)
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (now - data.lastEnergyRefillSeconds >= ENERGY_RECHARGE_SECONDS)
                    {
                        data.energy++;
                        data.lastEnergyRefillSeconds = now;
                        SaveManager.Instance.Save();
                        OnEnergyChanged?.Invoke(data.energy, data.maxEnergy);
                    }
                }
            }
        }

        public int Coins => SaveManager.Instance?.Data?.coins ?? 0;
        public int Gems => SaveManager.Instance?.Data?.gems ?? 0;
        public int Energy => SaveManager.Instance?.Data?.energy ?? 0;
        public int MaxEnergy => SaveManager.Instance?.Data?.maxEnergy ?? 5;
        public int PiggyBank => SaveManager.Instance?.Data?.piggyBankCoins ?? 0;

        public void AddCoins(int amount)
        {
            if (amount <= 0 || SaveManager.Instance?.Data == null) return;
            SaveManager.Instance.Data.coins += amount;
            SaveManager.Instance.Save();
            OnCoinsChanged?.Invoke(SaveManager.Instance.Data.coins);
        }

        public bool SpendCoins(int amount)
        {
            if (SaveManager.Instance?.Data == null) return false;
            if (SaveManager.Instance.Data.coins < amount) return false;

            SaveManager.Instance.Data.coins -= amount;
            SaveManager.Instance.Save();
            OnCoinsChanged?.Invoke(SaveManager.Instance.Data.coins);
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0 || SaveManager.Instance?.Data == null) return;
            SaveManager.Instance.Data.gems += amount;
            SaveManager.Instance.Save();
            OnGemsChanged?.Invoke(SaveManager.Instance.Data.gems);
        }

        public bool SpendGems(int amount)
        {
            if (SaveManager.Instance?.Data == null) return false;
            if (SaveManager.Instance.Data.gems < amount) return false;

            SaveManager.Instance.Data.gems -= amount;
            SaveManager.Instance.Save();
            OnGemsChanged?.Invoke(SaveManager.Instance.Data.gems);
            return true;
        }

        public bool ConsumeEnergy(int amount = 1)
        {
            if (SaveManager.Instance?.Data == null) return false;
            var data = SaveManager.Instance.Data;
            if (data.energy < amount) return false;

            data.energy -= amount;
            if (data.energy == data.maxEnergy - amount)
            {
                data.lastEnergyRefillSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            SaveManager.Instance.Save();
            OnEnergyChanged?.Invoke(data.energy, data.maxEnergy);
            return true;
        }

        public void RefillEnergy()
        {
            if (SaveManager.Instance?.Data == null) return;
            var data = SaveManager.Instance.Data;
            data.energy = data.maxEnergy;
            SaveManager.Instance.Save();
            OnEnergyChanged?.Invoke(data.energy, data.maxEnergy);
        }

        public void AddToPiggyBank(int amount)
        {
            if (SaveManager.Instance?.Data == null) return;
            SaveManager.Instance.Data.piggyBankCoins += amount;
            SaveManager.Instance.Save();
            OnPiggyBankChanged?.Invoke(SaveManager.Instance.Data.piggyBankCoins);
        }

        public void UpdateEnergyRecharge()
        {
            if (SaveManager.Instance?.Data == null) return;
            var data = SaveManager.Instance.Data;
            if (data.energy < data.maxEnergy && data.lastEnergyRefillSeconds > 0)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long elapsed = now - data.lastEnergyRefillSeconds;
                int gained = (int)(elapsed / ENERGY_RECHARGE_SECONDS);
                if (gained > 0)
                {
                    data.energy = Mathf.Min(data.maxEnergy, data.energy + gained);
                    data.lastEnergyRefillSeconds = now;
                    SaveManager.Instance.Save();
                    OnEnergyChanged?.Invoke(data.energy, data.maxEnergy);
                }
            }
        }
    }
}
