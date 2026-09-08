using UnityEngine;

namespace FarmBloom.Core
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;
        public static SoundManager Instance => _instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudioSources();
        }

        private void InitAudioSources()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            ApplyVolumes();
        }

        public void ApplyVolumes()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                _musicSource.volume = SaveManager.Instance.Data.musicVolume;
                _sfxSource.volume = SaveManager.Instance.Data.soundVolume;
            }
            else
            {
                _musicSource.volume = 0.7f;
                _sfxSource.volume = 1f;
            }
        }

        public void SetMusicVolume(float val)
        {
            if (_musicSource != null) _musicSource.volume = val;
            if (SaveManager.Instance?.Data != null)
            {
                SaveManager.Instance.Data.musicVolume = val;
                SaveManager.Instance.Save();
            }
        }

        public void SetSoundVolume(float val)
        {
            if (_sfxSource != null) _sfxSource.volume = val;
            if (SaveManager.Instance?.Data != null)
            {
                SaveManager.Instance.Data.soundVolume = val;
                SaveManager.Instance.Save();
            }
        }

        public void PlayButtonClick()
        {
            PlayProceduralTone(440f, 0.05f, 0.3f);
        }

        public void PlayTileSwap()
        {
            PlayProceduralTone(520f, 0.08f, 0.4f);
        }

        public void PlayMatch(int combo = 1)
        {
            float baseFreq = 440f + (combo * 70f);
            PlayProceduralTone(baseFreq, 0.15f, 0.5f);
        }

        public void PlaySpecialCreate()
        {
            PlayProceduralTone(780f, 0.25f, 0.6f);
        }

        public void PlayExplosion()
        {
            PlayProceduralNoise(0.3f, 0.6f);
        }

        public void PlayVictory()
        {
            StartCoroutine(VictoryFanfareCoroutine());
        }

        public void PlayDefeat()
        {
            PlayProceduralTone(220f, 0.4f, 0.5f);
        }

        public void PlayCoinEarn()
        {
            PlayProceduralTone(880f, 0.08f, 0.4f);
        }

        public void PlayHarvest()
        {
            PlayProceduralTone(600f, 0.12f, 0.5f);
        }

        public void TriggerHaptic()
        {
            if (SaveManager.Instance?.Data?.vibrations == true)
            {
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }
        }

        private System.Collections.IEnumerator VictoryFanfareCoroutine()
        {
            float[] notes = { 440f, 554.37f, 659.25f, 880f };
            foreach (var note in notes)
            {
                PlayProceduralTone(note, 0.12f, 0.5f);
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Générateur procédural d'effets sonores en mémoire vive (pas de dépendances de fichiers)
        private void PlayProceduralTone(float frequency, float duration, float volume)
        {
            if (_sfxSource == null || _sfxSource.volume <= 0.01f) return;

            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / sampleCount; // Décroissance linéaire
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("ProcTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            _sfxSource.PlayOneShot(clip);
        }

        private void PlayProceduralNoise(float duration, float volume)
        {
            if (_sfxSource == null || _sfxSource.volume <= 0.01f) return;

            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rand = new System.Random();
            for (int i = 0; i < sampleCount; i++)
            {
                float envelope = Mathf.Pow(1f - (float)i / sampleCount, 2f);
                samples[i] = ((float)rand.NextDouble() * 2f - 1f) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("ProcNoise", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            _sfxSource.PlayOneShot(clip);
        }
    }
}
