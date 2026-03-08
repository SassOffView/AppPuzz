// ============================================================
// EnergyManager.cs
// Gestisce l'energia del giocatore: calcolo, accumulo, streak,
// bonus lettere rare, bonus leggendario, aggiornamento UI.
// ============================================================
// STEP 1 : stub compilabile — formula completa nello STEP 5.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using AppPuzz.Creatures;
using AppPuzz.Localization;

namespace AppPuzz.Gameplay
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce il punteggio-energia.
    /// Aggiorna la barra UI e tiene traccia dello streak di parole valide.
    /// </summary>
    public class EnergyManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static EnergyManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Configurazione (dall'Inspector)
        // ----------------------------------------------------------

        [Header("UI")]
        [Tooltip("La Slider o Image fill che rappresenta la barra energia.")]
        public Slider energyBar;

        [Header("Limiti")]
        [Tooltip("Energia massima raggiungibile.")]
        public float maxEnergy = 5000f;

        [Header("Streak")]
        [Tooltip("Secondi massimi tra una parola valida e la successiva per mantenere lo streak.")]
        public float streakTimeWindow = 1.5f;

        [Tooltip("Numero di parole consecutive prima che inizi il bonus streak.")]
        public int streakBonusThreshold = 3;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------

        /// <summary>Fires whenever energy is gained: argument = amount gained.</summary>
        public static event System.Action<float> OnEnergyGained;

        /// <summary>Energia corrente (0 … maxEnergy).</summary>
        public float CurrentEnergy { get; private set; }

        /// <summary>Numero di parole valide consecutive (per il bonus streak).</summary>
        public int CurrentStreak { get; private set; }

        /// <summary>Streak massimo raggiunto durante la partita.</summary>
        public int MaxStreak { get; private set; }

        // Accumulatore streak in virgola mobile (parole 2 lettere = +0.5)
        private float _streakValue = 0f;

        // Timestamp dell'ultima parola valida (per il controllo finestra temporale)
        private float _lastWordTime = float.NegativeInfinity;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            ResetEnergy();
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Aggiunge energia in base alla parola validata.
        /// </summary>
        /// <param name="word">Parola valida appena trovata.</param>
        /// <param name="isLegendary">True se è una parola fantasy.</param>
        public void AddEnergy(string word, bool isLegendary = false)
        {
            // Reset streak se è passato troppo tempo dall'ultima parola valida
            if (Time.time - _lastWordTime > streakTimeWindow)
            {
                CurrentStreak = 0;
                _streakValue = 0f;
            }

            _lastWordTime = Time.time;
            // Parole di 2 lettere contribuiscono 0.5 allo streak invece di 1
            _streakValue += (word.Length <= 2) ? 0.5f : 1f;
            CurrentStreak = (int)_streakValue;
            if (CurrentStreak > MaxStreak) MaxStreak = CurrentStreak;

            // Punteggio base: somma valori lettere × moltiplicatore lunghezza
            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;
            float baseScore = LetterScoring.CalculateBaseScore(word, isItalian);

            // Bonus streak: +20% per ogni parola oltre la soglia (default: dalla 4ª), cap 2.5×
            int bonusStreak = Mathf.Max(0, CurrentStreak - streakBonusThreshold);
            float streakMultiplier = Mathf.Min(1f + bonusStreak * 0.2f, 2.5f);

            // Bonus leggendario: punteggio triplicato
            float legendaryMultiplier = isLegendary ? 3f : 1f;

            float gained = baseScore * streakMultiplier * legendaryMultiplier;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy + gained, 0f, maxEnergy);
            OnEnergyGained?.Invoke(gained);
            UpdateUI();
            CreatureController.Instance?.OnEnergyChanged(CurrentEnergy);

            Debug.Log($"[EnergyManager] +{gained:F1} energia " +
                      $"(parola='{word}', base={baseScore:F1}, streak={CurrentStreak}, legendary={isLegendary})");
        }

        /// <summary>
        /// Resetta streak e incrementa il contatore (chiamato da GameManager su parola invalid).
        /// </summary>
        public void ResetStreak()
        {
            CurrentStreak = 0;
            _streakValue = 0f;
        }

        /// <summary>Azzera energia e streak a inizio partita.</summary>
        public void ResetEnergy()
        {
            CurrentEnergy  = 0f;
            CurrentStreak  = 0;
            _streakValue   = 0f;
            MaxStreak      = 0;
            _lastWordTime  = float.NegativeInfinity;
            UpdateUI();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void UpdateUI()
        {
            if (energyBar != null)
                energyBar.value = CurrentEnergy / maxEnergy;
        }
    }
}
