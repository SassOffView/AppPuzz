// ============================================================
// GameManager.cs
// Orchestratore centrale del gioco: coordina Grid, Energy, Timer,
// Creature e Validazione. Gestisce il flusso della partita.
// ============================================================
// STEP 1 : stub compilabile — flusso completo nello STEP 7.
// ============================================================

using UnityEngine;
using AppPuzz.Gameplay;
using AppPuzz.Grid;
using AppPuzz.Localization;

namespace AppPuzz.Gameplay
{
    /// <summary>Fasi del ciclo di vita di una partita.</summary>
    public enum GameState
    {
        Idle,       // Prima dell'avvio (es. menu)
        Playing,    // Partita in corso
        Paused,     // Pausa (futuro)
        GameOver    // Tempo scaduto, mostra risultati
    }

    /// <summary>
    /// Singleton MonoBehaviour che orchestra tutti i sistemi di gioco.
    /// Punto di ingresso principale della scena Gameplay.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static GameManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Stato della partita
        // ----------------------------------------------------------

        /// <summary>Stato corrente del flusso di gioco.</summary>
        public GameState CurrentState { get; private set; } = GameState.Idle;

        // ----------------------------------------------------------
        // Riferimenti ai sistemi (assegnati dall'Inspector o trovati in Awake)
        // ----------------------------------------------------------

        [Header("Sistemi di gioco")]
        public GridManager   gridManager;
        public EnergyManager energyManager;

        // WordValidator viene creato via codice (non MonoBehaviour)
        private WordValidator _wordValidator;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _wordValidator = new WordValidator();
        }

        private void Start()
        {
            StartGame(); // test — verrà gestito meglio allo STEP 7
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Avvia una nuova partita (reset griglia, energia, timer).</summary>
        public void StartGame()
        {
            CurrentState = GameState.Playing;

            // Carica dizionari in base alla lingua corrente
            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            // Genera la griglia
            if (gridManager != null)
                gridManager.GenerateGrid();

            // Azzera energia
            if (energyManager != null)
                energyManager.ResetEnergy();

            Debug.Log("[GameManager] Partita avviata.");
        }

        /// <summary>Chiamato quando il giocatore conferma la parola selezionata.</summary>
        public void SubmitWord(string word)
        {
            if (CurrentState != GameState.Playing) return;

            ValidationResult result = _wordValidator.Validate(word.ToLower());
            bool legendary = result == ValidationResult.Legendary;

            if (result != ValidationResult.Invalid)
            {
                energyManager?.AddEnergy(word, legendary);
                Debug.Log($"[GameManager] Parola '{word}' valida. Legendary={legendary}");
            }
            else
            {
                energyManager?.ResetStreak();
                Debug.Log($"[GameManager] Parola '{word}' non valida.");
            }
        }

        /// <summary>Termina la partita (time-up o trigger esterno).</summary>
        public void EndGame()
        {
            CurrentState = GameState.GameOver;
            Debug.Log($"[GameManager] Partita terminata. Energia finale: {energyManager?.CurrentEnergy}");
            // TODO (STEP 7): mostrare schermata risultati
        }
    }
}
