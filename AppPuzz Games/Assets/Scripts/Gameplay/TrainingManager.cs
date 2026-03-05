// ============================================================
// TrainingManager.cs
// Modalità Allenamento: partita senza timer, con suggerimenti
// e feedback dettagliato. XP ridotto rispetto alla modalità normale.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;
using AppPuzz.Localization;
using AppPuzz.UI;
using AppPuzz.Utils;

namespace AppPuzz.Gameplay
{
    /// <summary>
    /// Gestisce la modalità Allenamento.
    /// Aggiungere a un GameObject "TrainingManager" nella scena.
    /// </summary>
    public class TrainingManager : MonoBehaviour
    {
        public static TrainingManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Riferimenti Inspector
        // ----------------------------------------------------------

        [Header("UI Training")]
        public TextMeshProUGUI   currentWordText;
        public TextMeshProUGUI   feedbackText;
        public TextMeshProUGUI   scoreText;
        public TextMeshProUGUI   wordsFoundText;
        public TextMeshProUGUI   hintText;
        public Slider            energyBar;

        [Header("Pulsanti")]
        public Button hintButton;
        public Button newGridButton;
        public Button exitButton;

        [Header("Configurazione")]
        public int   hintsPerSession  = 3;
        public float xpMultiplier     = 0.5f;   // XP ridotto in allenamento

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------
        private WordValidator _validator  = new WordValidator();
        private float         _score      = 0f;
        private int           _wordsFound = 0;
        private int           _hintsLeft;
        private bool          _isActive;
        private const float   MAX_ENERGY  = 500f;

        private static readonly string[] FEEDBACK_VALID     = { "Ottimo!", "Bravo!", "Perfetto!", "Fantastico!" };
        private static readonly string[] FEEDBACK_LEGENDARY = { "*** LEGGENDARIO! ***", "*** INCREDIBILE! ***", "*** EPICO! ***" };
        private static readonly string[] FEEDBACK_INVALID   = { "Non trovata", "Riprova!", "Non è nel dizionario" };

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
            WireButtons();
        }

        private void OnEnable()
        {
            StartTraining();
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Avvia una sessione di allenamento.</summary>
        public void StartTraining()
        {
            _score      = 0f;
            _wordsFound = 0;
            _hintsLeft  = hintsPerSession;
            _isActive   = true;

            // Carica dizionari
            if (LanguageManager.Instance != null)
                _validator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            // Genera griglia
            GridManager.Instance?.GenerateGrid();

            UpdateUI();
            if (feedbackText != null) feedbackText.text = "Trova parole sulla griglia!";
            if (hintText     != null) hintText.text     = "";

            Debug.Log("[Training] Sessione avviata.");
        }

        /// <summary>Chiamato da WordSelector quando il giocatore completa una selezione.</summary>
        public void SubmitWord(string word)
        {
            if (!_isActive) return;

            var result = _validator.Validate(word.ToLower());
            bool legendary = result == ValidationResult.Legendary;

            if (result != ValidationResult.Invalid)
            {
                float baseScore = word.Length * 2f;
                if (legendary) baseScore *= 3f;
                _score      += baseScore;
                _wordsFound++;

                ShowFeedback(legendary
                    ? FEEDBACK_LEGENDARY[Random.Range(0, FEEDBACK_LEGENDARY.Length)]
                    : FEEDBACK_VALID[Random.Range(0, FEEDBACK_VALID.Length)],
                    legendary ? UITheme.Colors.Legendary : UITheme.Colors.TextSuccess);
            }
            else
            {
                ShowFeedback(FEEDBACK_INVALID[Random.Range(0, FEEDBACK_INVALID.Length)],
                    UITheme.Colors.TextDanger);
            }

            UpdateUI();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void WireButtons()
        {
            hintButton?.onClick.AddListener(ShowHint);
            newGridButton?.onClick.AddListener(OnNewGrid);
            exitButton?.onClick.AddListener(OnExit);
        }

        private void ShowHint()
        {
            if (_hintsLeft <= 0)
            {
                ShowFeedback("Nessun suggerimento rimasto!", UITheme.Colors.TextDanger);
                return;
            }
            _hintsLeft--;
            if (hintText != null)
                hintText.text = $"Suggerimento: cerca parole di {Random.Range(4, 7)} lettere.\n({_hintsLeft} rimasti)";
            UpdateUI();
        }

        private void OnNewGrid()
        {
            GridManager.Instance?.GenerateGrid();
            if (feedbackText != null) feedbackText.text = "Nuova griglia!";
        }

        private void OnExit()
        {
            // Salva XP guadagnato
            float xpGained = _score * xpMultiplier;
            PlayerProfile.Instance?.RecordMatchResult(_score, 0, false);

            _isActive = false;
            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }

        private void UpdateUI()
        {
            if (scoreText      != null) scoreText.text      = $"Energia: {_score:F0}";
            if (wordsFoundText != null) wordsFoundText.text = $"Parole: {_wordsFound}";
            if (energyBar      != null) energyBar.value     = Mathf.Clamp01(_score / MAX_ENERGY);
            if (hintButton     != null)
            {
                var label = hintButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = $"Suggerimento ({_hintsLeft})";
                hintButton.interactable = _hintsLeft > 0;
            }
        }

        private void ShowFeedback(string msg, Color color)
        {
            if (feedbackText == null) return;
            feedbackText.text  = msg;
            feedbackText.color = color;
            StopAllCoroutines();
            StartCoroutine(FadeFeedback());
        }

        private IEnumerator FadeFeedback()
        {
            yield return new WaitForSeconds(2f);
            if (feedbackText != null)
            {
                feedbackText.text  = "";
                feedbackText.color = UITheme.Colors.TextPrimary;
            }
        }
    }
}
