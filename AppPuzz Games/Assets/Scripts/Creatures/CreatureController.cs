// ============================================================
// CreatureController.cs
// Controlla la visualizzazione della creatura: quale variante
// è attiva, a quale livello di evoluzione si trova, e le reazioni
// ai cambi di energia (glow, animazioni).
// ============================================================
// STEP 1 : stub compilabile — reazioni visive nello STEP 6.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using AppPuzz.Gameplay; // GameState

namespace AppPuzz.Creatures
{
    /// <summary>Le tre varianti iniziali di creatura selezionabili dal giocatore.</summary>
    public enum CreatureType
    {
        MentalDragon,   // Drago Mentale
        AstralWolf,     // Lupo Astrale
        EtherealSerpent // Serpente Etereo
    }

    /// <summary>
    /// Gestisce la creatura associata al giocatore:
    /// - selezione variante a inizio partita
    /// - evoluzione visiva al raggiungimento delle soglie energia
    /// - reazioni (glow/animazione) quando l'energia sale
    /// </summary>
    public class CreatureController : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static CreatureController Instance { get; private set; }

        // ----------------------------------------------------------
        // Configurazione (dall'Inspector)
        // ----------------------------------------------------------

        [Header("Configurazione Evoluzione")]
        [Tooltip("L'asset EvolutionConfig creato via ScriptableObject.")]
        public EvolutionConfig evolutionConfig;

        [Header("UI Creatura")]
        [Tooltip("L'Image UI che mostra lo sprite della creatura.")]
        public Image creatureImage;

        [Header("Variante iniziale")]
        [Tooltip("Tipo di creatura selezionata per questa partita.")]
        public CreatureType selectedCreature = CreatureType.MentalDragon;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------

        private int _currentLevel = 1;

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
            UpdateVisuals(1);
            Debug.Log($"[CreatureController] Creatura: {selectedCreature}, Livello: {_currentLevel}");
        }

        // ----------------------------------------------------------
        // API pubblica — chiamata da EnergyManager quando l'energia cambia
        // ----------------------------------------------------------

        /// <summary>
        /// Controlla se l'energia corrente fa scattare una nuova soglia di evoluzione.
        /// </summary>
        /// <param name="currentEnergy">Energia corrente del giocatore.</param>
        public void OnEnergyChanged(float currentEnergy)
        {
            if (evolutionConfig == null) return;

            int newLevel = evolutionConfig.GetLevel(currentEnergy);
            if (newLevel != _currentLevel)
            {
                _currentLevel = newLevel;
                UpdateVisuals(newLevel);
                Debug.Log($"[CreatureController] Evoluzione al livello {newLevel}!");
                // TODO (STEP 6): avviare animazione evoluzione
            }
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void UpdateVisuals(int level)
        {
            if (evolutionConfig == null || creatureImage == null) return;

            // Seleziona lo sprite corrispondente al livello
            Sprite targetSprite;
            if (level == 2)
                targetSprite = evolutionConfig.spriteLevel2;
            else if (level == 3)
                targetSprite = evolutionConfig.spriteLevel3;
            else
                targetSprite = evolutionConfig.spriteLevel1;

            if (targetSprite != null)
                creatureImage.sprite = targetSprite;

            // TODO (STEP 6): cambiare colore/glow in base a selectedCreature
        }
    }
}
