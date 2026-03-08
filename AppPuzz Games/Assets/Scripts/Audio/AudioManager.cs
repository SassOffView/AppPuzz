// ============================================================
// AudioManager.cs
// Suoni procedurali generati a runtime (nessun asset esterno).
// Stili: click UI, parola valida, parola non valida, streak,
// evoluzione leggendaria.
// ============================================================

using System.Collections;
using UnityEngine;
using AppPuzz.Gameplay;

namespace AppPuzz.Audio
{
    public class AudioManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static AudioManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Sorgenti audio
        // ----------------------------------------------------------
        private AudioSource _sfxSource;
        private AudioSource _streakSource;

        // ----------------------------------------------------------
        // Clip procedurali (generate una sola volta)
        // ----------------------------------------------------------
        private AudioClip _clickClip;
        private AudioClip _wordValidClip;
        private AudioClip _wordInvalidClip;
        private AudioClip _streakClip;
        private AudioClip _evolutionClip;

        // ----------------------------------------------------------
        // Parametri di campionamento
        // ----------------------------------------------------------
        private const int SampleRate = 44100;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Due AudioSource: uno per sfx veloci, uno per streak/evolution
            _sfxSource    = gameObject.AddComponent<AudioSource>();
            _streakSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake    = false;
            _streakSource.playOnAwake = false;

            // Genera clip a runtime
            _clickClip      = BuildClick();
            _wordValidClip  = BuildWordValid();
            _wordInvalidClip= BuildWordInvalid();
            _streakClip     = BuildStreak();
            _evolutionClip  = BuildEvolution();
        }

        private void OnEnable()
        {
            EnergyManager.OnEnergyGained += OnEnergyGained;
            GameManager.OnWordInvalid    += OnWordInvalid;
        }

        private void OnDisable()
        {
            EnergyManager.OnEnergyGained -= OnEnergyGained;
            GameManager.OnWordInvalid    -= OnWordInvalid;
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        public static void PlayClick()
        {
            Instance?._sfxSource.PlayOneShot(Instance._clickClip, 0.45f);
        }

        public void PlayWordValid()
        {
            _sfxSource.PlayOneShot(_wordValidClip, 0.7f);
        }

        public void PlayWordInvalid()
        {
            _sfxSource.PlayOneShot(_wordInvalidClip, 0.5f);
        }

        public void PlayStreak(int streakLevel)
        {
            float pitch = 1f + Mathf.Clamp((streakLevel - 3) * 0.1f, 0f, 0.5f);
            _streakSource.pitch = pitch;
            _streakSource.PlayOneShot(_streakClip, 0.8f);
        }

        public void PlayEvolution()
        {
            _streakSource.PlayOneShot(_evolutionClip, 1f);
        }

        // ----------------------------------------------------------
        // Event handlers
        // ----------------------------------------------------------

        private void OnEnergyGained(float amount)
        {
            // Se streak >= 3 suona l'arpeggio, altrimenti il chime normale
            int streak = EnergyManager.Instance?.CurrentStreak ?? 0;
            if (streak >= 3)
                PlayStreak(streak);
            else
                PlayWordValid();
        }

        private void OnWordInvalid()
        {
            PlayWordInvalid();
        }

        // ----------------------------------------------------------
        // Generatori PCM procedurali
        // ----------------------------------------------------------

        // Click UI: breve tick 1200→800 Hz, 55ms
        private static AudioClip BuildClick()
        {
            int len = (int)(SampleRate * 0.055f);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t    = (float)i / SampleRate;
                float prog = (float)i / len;
                float freq = Mathf.Lerp(1200f, 800f, prog);
                float env  = Mathf.Pow(1f - prog, 2.5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
            }
            return MakeClip("Click", data);
        }

        // Parola valida: doppio chime ascendente 660→880 Hz
        private static AudioClip BuildWordValid()
        {
            float t1 = 0.10f, t2 = 0.10f;
            int len = (int)(SampleRate * (t1 + t2 + 0.02f));
            float[] data = new float[len];
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                float t    = (float)i / SampleRate;
                float prog = (float)i / len;
                float freq;
                if (t < t1)       freq = 660f;
                else if (t < t1 + 0.01f) { phase = t; freq = 660f; }
                else              freq = 880f;

                float localProg = (t < t1) ? t / t1 : (t - t1) / t2;
                float env = Mathf.Clamp01(localProg < 0.1f ? localProg * 10f : (1f - localProg) * 1.2f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.65f;
            }
            return MakeClip("WordValid", data);
        }

        // Parola non valida: basso ronzio 180 Hz con distorsione, 130ms
        private static AudioClip BuildWordInvalid()
        {
            int len = (int)(SampleRate * 0.13f);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t    = (float)i / SampleRate;
                float prog = (float)i / len;
                float env  = Mathf.Pow(1f - prog, 1.5f);
                // Mescola 180 Hz + 270 Hz + un po' di rumore per dargli "grintosità"
                float wave = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.6f
                           + Mathf.Sin(2f * Mathf.PI * 270f * t) * 0.3f;
                data[i] = Mathf.Clamp(wave, -1f, 1f) * env * 0.55f;
            }
            return MakeClip("WordInvalid", data);
        }

        // Streak: arpeggio ascendente 4 note (440→554→659→880 Hz)
        private static AudioClip BuildStreak()
        {
            float noteDur = 0.08f;
            float[] freqs = { 440f, 554f, 659f, 880f };
            int noteLen   = (int)(SampleRate * noteDur);
            int totalLen  = noteLen * freqs.Length;
            float[] data  = new float[totalLen];

            for (int n = 0; n < freqs.Length; n++)
            {
                float freq = freqs[n];
                for (int i = 0; i < noteLen; i++)
                {
                    float t    = (float)i / SampleRate;
                    float prog = (float)i / noteLen;
                    float env  = prog < 0.1f
                        ? prog * 10f
                        : Mathf.Pow(1f - prog, 1.8f);
                    data[n * noteLen + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.60f;
                }
            }
            return MakeClip("Streak", data);
        }

        // Evoluzione: accordo maestoso (440+554+660+880 Hz) con fade-in/out, 750ms
        private static AudioClip BuildEvolution()
        {
            int len = (int)(SampleRate * 0.75f);
            float[] freqs = { 440f, 554f, 660f, 880f, 1100f };
            float[] data  = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t    = (float)i / SampleRate;
                float prog = (float)i / len;
                // Fade in 10%, fade out ultimo 25%
                float env  = prog < 0.10f
                    ? prog / 0.10f
                    : (prog > 0.75f ? (1f - prog) / 0.25f : 1f);
                env *= 0.55f;

                float wave = 0f;
                foreach (float f in freqs)
                    wave += Mathf.Sin(2f * Mathf.PI * f * t) * (1f / freqs.Length);

                data[i] = wave * env;
            }
            return MakeClip("Evolution", data);
        }

        private static AudioClip MakeClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
