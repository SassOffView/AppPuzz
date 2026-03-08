// ============================================================
// GameManager.cs
// Orchestratore centrale del gioco: coordina Grid, Energy, Timer,
// Creature e Validazione. Gestisce il flusso della partita.
// ============================================================
// STEP 7 : Timer countdown + schermata risultati.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;
using AppPuzz.Localization;
using AppPuzz.Creatures;
using AppPuzz.UI;
using AppPuzz.Utils;

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

        /// <summary>Fires when the player submits an invalid word.</summary>
        public static event System.Action OnWordInvalid;

        // ----------------------------------------------------------
        // Configurazione
        // ----------------------------------------------------------

        [Header("Timer")]
        [Tooltip("Durata della partita in secondi.")]
        public float gameDuration = 120f;

        // ----------------------------------------------------------
        // Riferimenti UI
        // ----------------------------------------------------------

        [Header("UI - Timer")]
        [Tooltip("Testo che mostra il conto alla rovescia (es. '02:00').")]
        public TextMeshProUGUI timerText;

        [Header("UI - Parola corrente")]
        public TextMeshProUGUI currentWordText;

        [Header("UI - XP Float")]
        public Canvas floatCanvas;

        [Header("UI - Risultati")]
        [Tooltip("Pannello risultati mostrato al termine della partita.")]
        public ResultsPanel resultsPanel;

        // ----------------------------------------------------------
        // Riferimenti ai sistemi
        // ----------------------------------------------------------

        [Header("Sistemi di gioco")]
        public GridManager   gridManager;
        public EnergyManager energyManager;

        // WordValidator viene creato via codice (non MonoBehaviour)
        private WordValidator _wordValidator;
        private float _timeRemaining;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _wordValidator = new WordValidator();
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            WordSelector.OnCurrentWordChanged  += OnWordChanged;
            EnergyManager.OnEnergyGained       += OnEnergyGained;

            // Trova ResultsPanel anche se è disattivato nel hierarchy
            if (resultsPanel == null)
                resultsPanel = FindFirstObjectByType<ResultsPanel>(FindObjectsInactive.Include);

            // Auto-find EnergyManager / GridManager se non assegnati dall'Inspector
            if (energyManager == null)
                energyManager = FindFirstObjectByType<EnergyManager>(FindObjectsInactive.Include);
            if (gridManager == null)
                gridManager = FindFirstObjectByType<AppPuzz.Grid.GridManager>(FindObjectsInactive.Include);

            // Crea WordSelector automaticamente se non è nella scena
            if (WordSelector.Instance == null)
            {
                new GameObject("WordSelector").AddComponent<WordSelector>();
                Debug.Log("[GameManager] WordSelector creato automaticamente.");
            }
        }

        private void OnDestroy()
        {
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            WordSelector.OnCurrentWordChanged  -= OnWordChanged;
            EnergyManager.OnEnergyGained       -= OnEnergyGained;
        }

        private void OnWordChanged(string word)
        {
            if (currentWordText != null) currentWordText.text = word;
        }

        private void OnEnergyGained(float amount)
        {
            SpawnXPFloat(amount);
        }

        private void SpawnXPFloat(float xp)
        {
            if (floatCanvas == null) return;
            var go = new GameObject("XPFloat");
            go.transform.SetParent(floatCanvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            float rx = UnityEngine.Random.Range(0.2f, 0.8f);
            float ry = UnityEngine.Random.Range(0.3f, 0.6f);
            rt.anchorMin = rt.anchorMax = new Vector2(rx, ry);
            rt.sizeDelta = new Vector2(240, 80);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = $"+{xp:F0} XP";
            txt.fontSize = 56;
            txt.fontStyle = FontStyles.Bold;
            txt.color = UITheme.Colors.Gold;
            txt.alignment = TextAlignmentOptions.Center;
            StartCoroutine(AnimateXPFloat(go, txt));
        }

        private static IEnumerator AnimateXPFloat(GameObject go, TextMeshProUGUI txt)
        {
            float elapsed = 0f, duration = 1.4f;
            Vector2 startPos = go.GetComponent<RectTransform>().anchoredPosition;
            Color startColor = txt.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                go.GetComponent<RectTransform>().anchoredPosition = startPos + Vector2.up * (55f * t);
                txt.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }
            Destroy(go);
        }

        private void HandleLanguageChanged(Language _)
        {
            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());
        }

        private void Start()
        {
            // Se ScreenManager esiste, non avviare la partita automaticamente:
            // sara ScreenManager a chiamare StartGame() quando mostra il GameplayPanel.
            // Se ScreenManager NON esiste (scena di sola gameplay) avvia subito.
            if (ScreenManager.Instance == null)
                StartGame();
            else
                Debug.Log("[GameManager] ScreenManager attivo. StartGame() verra chiamato dallo ScreenManager.");
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            _timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                EndGame();
            }
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Avvia una nuova partita (reset griglia, energia, timer).</summary>
        public void StartGame()
        {
            CurrentState   = GameState.Playing;
            _timeRemaining = gameDuration;

            resultsPanel?.Hide();

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
                OnWordInvalid?.Invoke();
                Debug.Log($"[GameManager] Parola '{word}' non valida.");
            }
        }

        /// <summary>Termina la partita e mostra i risultati.</summary>
        public void EndGame()
        {
            if (CurrentState == GameState.GameOver) return;

            CurrentState = GameState.GameOver;

            float finalEnergy = energyManager?.CurrentEnergy ?? 0f;
            int   finalLevel  = CreatureController.Instance?.CurrentLevel ?? 1;
            int   maxStreak   = energyManager?.MaxStreak ?? 0;

            Debug.Log($"[GameManager] Partita terminata. Energia={finalEnergy:F0}, Livello={finalLevel}, MaxStreak={maxStreak}");

            // Salva risultato nel profilo giocatore
            PlayerProfile.Instance?.RecordMatchResult(finalEnergy, maxStreak, false);

            resultsPanel?.Show(finalEnergy, finalLevel, maxStreak);
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
            int seconds = Mathf.CeilToInt(Mathf.Max(_timeRemaining, 0f));
            timerText.text = $"{seconds / 60:D2}:{seconds % 60:D2}";

            // Colore rosso quando mancano meno di 10 secondi (incluso 0)
            timerText.color = _timeRemaining <= 10f ? Color.red : Color.white;
        }
    }
}
