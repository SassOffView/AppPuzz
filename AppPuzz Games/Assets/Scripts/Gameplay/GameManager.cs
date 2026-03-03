// ============================================================
// GameManager.cs
// Orchestratore centrale del gioco: coordina Grid, Energy, Timer,
// Creature e Validazione. Gestisce il flusso della partita.
// ============================================================
// STEP 7 : Timer countdown + schermata risultati.
// ============================================================

using UnityEngine;
using TMPro;
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

        /// <summary>Secondi rimanenti nella partita corrente.</summary>
        public float TimeRemaining { get; private set; }

        // ----------------------------------------------------------
        // Configurazione
        // ----------------------------------------------------------

        [Header("Timer")]
        [Tooltip("Durata di una partita in secondi.")]
        public float gameDuration = 90f;

        // ----------------------------------------------------------
        // Riferimenti UI
        // ----------------------------------------------------------

        [Header("UI - Timer")]
        [Tooltip("Testo che mostra i secondi rimanenti (es. '01:30').")]
        public TextMeshProUGUI timerText;

        [Header("UI - Risultati")]
        [Tooltip("Pannello mostrato a fine partita.")]
        public GameObject resultsPanel;

        [Tooltip("Testo del punteggio finale nel pannello risultati.")]
        public TextMeshProUGUI finalScoreText;

        [Tooltip("Testo del livello creatura raggiunto nel pannello risultati.")]
        public TextMeshProUGUI finalLevelText;

        // ----------------------------------------------------------
        // Riferimenti ai sistemi
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
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(Language _)
        {
            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());
        }

        private void Start()
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            StartGame();
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            TimeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (TimeRemaining <= 0f)
                EndGame();
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Avvia una nuova partita (reset griglia, energia, timer).</summary>
        public void StartGame()
        {
            CurrentState = GameState.Playing;
            TimeRemaining = gameDuration;

            // Nascondi pannello risultati
            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            // Carica dizionari in base alla lingua corrente
            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            // Genera la griglia
            if (gridManager != null)
                gridManager.GenerateGrid();

            // Azzera energia
            if (energyManager != null)
                energyManager.ResetEnergy();

            UpdateTimerUI();
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

        /// <summary>Termina la partita e mostra i risultati.</summary>
        public void EndGame()
        {
            if (CurrentState == GameState.GameOver) return;

            CurrentState = GameState.GameOver;
            TimeRemaining = 0f;
            UpdateTimerUI();

            float finalEnergy = energyManager != null ? energyManager.CurrentEnergy : 0f;
            Debug.Log($"[GameManager] Partita terminata. Energia finale: {finalEnergy:F0}");

            ShowResults(finalEnergy);
        }

        /// <summary>Riavvia la partita — collegare al bottone "Gioca ancora".</summary>
        public void RestartGame()
        {
            StartGame();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void UpdateTimerUI()
        {
            if (timerText == null) return;
            int seconds = Mathf.CeilToInt(Mathf.Max(TimeRemaining, 0f));
            int mm = seconds / 60;
            int ss = seconds % 60;
            timerText.text = $"{mm:D2}:{ss:D2}";

            // Colore rosso quando mancano meno di 10 secondi
            timerText.color = (TimeRemaining <= 10f && TimeRemaining > 0f)
                ? Color.red
                : Color.white;
        }

        private void ShowResults(float finalEnergy)
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = $"{finalEnergy:F0}";

            if (finalLevelText != null && energyManager != null)
            {
                // Livello leggibile in base all'energia
                string levelLabel = finalEnergy >= 120f ? "Leggendario" :
                                    finalEnergy >=  50f ? "Evoluto"     :
                                                          "Base";
                finalLevelText.text = levelLabel;
            }
        }
    }
}
