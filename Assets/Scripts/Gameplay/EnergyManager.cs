// ============================================================
// EnergyManager.cs
// Gestisce l'energia del giocatore: calcolo, accumulo, streak,
// bonus lettere rare, bonus leggendario, aggiornamento UI.
// ============================================================
// STEP 1 : stub compilabile — formula completa nello STEP 5.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

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
        public float maxEnergy = 200f;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------

        /// <summary>Energia corrente (0 … maxEnergy).</summary>
        public float CurrentEnergy { get; private set; }

        /// <summary>Numero di parole valide consecutive (per il bonus streak).</summary>
        public int CurrentStreak { get; private set; }

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
            // TODO (STEP 5): implementare la formula completa
            // Per ora aggiunge 1 per far compilare
            CurrentEnergy = Mathf.Clamp(CurrentEnergy + 1f, 0f, maxEnergy);
            UpdateUI();
            Debug.Log($"[EnergyManager] AddEnergy('{word}', legendary={isLegendary}) — formula nello STEP 5.");
        }

        /// <summary>
        /// Resetta streak e incrementa il contatore (chiamato da GameManager su parola invalid).
        /// </summary>
        public void ResetStreak()
        {
            CurrentStreak = 0;
        }

        /// <summary>Azzera energia e streak a inizio partita.</summary>
        public void ResetEnergy()
        {
            CurrentEnergy  = 0f;
            CurrentStreak  = 0;
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
