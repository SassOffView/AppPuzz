// ============================================================
// GameManager.cs
// Orchestratore centrale del gioco: coordina Grid, Energy, Timer,
// Creature e Validazione. Gestisce il flusso della partita.
// ============================================================
// Include: preview punteggio real-time, fulmine sulla creatura,
//          sistema danno tessere a 6 livelli, meccanica ghiaccio.
// ============================================================

using System.Collections;
using System.Collections.Generic;
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
        Idle,
        Playing,
        Paused,
        GameOver
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
        public GameState CurrentState { get; private set; } = GameState.Idle;

        /// <summary>Fires when the player submits an invalid word.</summary>
        public static event System.Action OnWordInvalid;

        // ----------------------------------------------------------
        // Configurazione
        // ----------------------------------------------------------

        [Header("Timer")]
        public float gameDuration = 120f;

        // ----------------------------------------------------------
        // Riferimenti UI
        // ----------------------------------------------------------

        [Header("UI - Timer")]
        public TextMeshProUGUI timerText;

        [Header("UI - Parola corrente")]
        public TextMeshProUGUI currentWordText;

        [Header("UI - Preview Punteggio")]
        [Tooltip("Mostra il punteggio potenziale durante la selezione delle lettere.")]
        public TextMeshProUGUI scorePreviewText;

        [Header("UI - XP Float")]
        public Canvas floatCanvas;

        [Header("UI - Risultati")]
        public ResultsPanel resultsPanel;

        // ----------------------------------------------------------
        // Riferimenti ai sistemi
        // ----------------------------------------------------------

        [Header("Sistemi di gioco")]
        public GridManager   gridManager;
        public EnergyManager energyManager;

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------
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

            if (resultsPanel == null)
                resultsPanel = FindFirstObjectByType<ResultsPanel>(FindObjectsInactive.Include);
            if (energyManager == null)
                energyManager = FindFirstObjectByType<EnergyManager>(FindObjectsInactive.Include);
            if (gridManager == null)
                gridManager = FindFirstObjectByType<AppPuzz.Grid.GridManager>(FindObjectsInactive.Include);

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

        // ── Aggiornamento word + preview punteggio ─────────────────
        private void OnWordChanged(string word)
        {
            if (currentWordText != null) currentWordText.text = word;
            UpdateScorePreview(word);
        }

        private void UpdateScorePreview(string word)
        {
            if (scorePreviewText == null) return;

            if (word.Length < 2)
            {
                scorePreviewText.text = "";
                return;
            }

            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;

            float baseScore  = LetterScoring.CalculateBaseScore(word, isItalian);
            float lengthMult = LetterScoring.GetLengthMultiplier(word.Length);
            int   letterSum  = LetterScoring.CalculateLetterSum(word, isItalian);

            // Mostra: somma-lettere × moltiplicatore-lunghezza
            string multStr = lengthMult > 1f ? $" ×{lengthMult:F2}" : "";
            scorePreviewText.text = $"{letterSum}{multStr} = {baseScore:F0} pt";
            scorePreviewText.color = word.Length >= 7 ? UITheme.Colors.Gold : UITheme.Colors.TextSecondary;
        }

        private void OnEnergyGained(float amount)
        {
            SpawnXPFloat(amount);
        }

        private void HandleLanguageChanged(Language _)
        {
            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());
            // Aggiorna i valori punti su tutte le celle
            if (gridManager != null)
                foreach (var cell in gridManager.GetAllCells())
                    cell.RefreshPointValue();
        }

        private void Start()
        {
            if (ScreenManager.Instance == null)
                StartGame();
            else
                Debug.Log("[GameManager] ScreenManager attivo. StartGame() verrà chiamato dallo ScreenManager.");
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

        public void StartGame()
        {
            CurrentState   = GameState.Playing;
            _timeRemaining = gameDuration;

            resultsPanel?.Hide();

            if (LanguageManager.Instance != null)
                _wordValidator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            gridManager?.SetValidator(_wordValidator);

            if (gridManager != null)
                gridManager.GenerateGrid();

            if (energyManager != null)
                energyManager.ResetEnergy();

            if (scorePreviewText != null) scorePreviewText.text = "";

            UpdateTimerUI();
            Debug.Log("[GameManager] Partita avviata.");
        }

        /// <summary>
        /// Chiamato da WordSelector con la parola e le celle selezionate.
        /// Gestisce scoring, fulmine, danno e ghiaccio.
        /// </summary>
        public void SubmitWord(string word, List<LetterCell> cells)
        {
            if (CurrentState != GameState.Playing) return;

            ValidationResult result    = _wordValidator.Validate(word.ToLower());
            bool             legendary = result == ValidationResult.Legendary;

            if (result != ValidationResult.Invalid)
            {
                energyManager?.AddEnergy(word, legendary);
                Debug.Log($"[GameManager] '{word}' valida. Legendary={legendary}");

                // Fulmine + danno corretto + sblocco freeze
                string creatureType = CreatureController.Instance?.SelectedCreatureTypeName ?? "MentalDragon";
                Color  lightColor   = UITheme.LightningColor(creatureType);
                StartCoroutine(LightningAndDamageSequence(cells, lightColor, correct: true));
            }
            else
            {
                energyManager?.ResetStreak();
                OnWordInvalid?.Invoke();
                Debug.Log($"[GameManager] '{word}' non valida.");

                // Flash rosso + danno errato
                StartCoroutine(WrongWordEffect(cells));
            }
        }

        public void EndGame()
        {
            if (CurrentState == GameState.GameOver) return;
            CurrentState = GameState.GameOver;

            float finalEnergy = energyManager?.CurrentEnergy ?? 0f;
            int   finalLevel  = CreatureController.Instance?.CurrentLevel ?? 1;
            int   maxStreak   = energyManager?.MaxStreak ?? 0;

            PlayerProfile.Instance?.RecordMatchResult(finalEnergy, maxStreak, false);
            resultsPanel?.Show(finalEnergy, finalLevel, maxStreak);
            Debug.Log($"[GameManager] Partita terminata. Energia={finalEnergy:F0}");
        }

        public void RestartGame() => StartGame();

        // ----------------------------------------------------------
        // Effetto fulmine e danno (parola corretta)
        // ----------------------------------------------------------

        private IEnumerator LightningAndDamageSequence(List<LetterCell> cells, Color lightColor, bool correct)
        {
            if (cells == null || cells.Count == 0) yield break;

            // Colpisci le lettere in sequenza con il flash fulmine
            foreach (var cell in cells)
            {
                if (cell != null)
                    yield return StartCoroutine(cell.FlashLightning(lightColor, 0.09f));
                yield return new WaitForSeconds(0.04f);
            }

            // Applica danno + verifica rottura e sblocco freeze
            yield return new WaitForSeconds(0.05f);

            List<(int row, int col)> broken = new List<(int, int)>();
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                bool isBroken = cell.ApplyCorrectHit();
                if (isBroken)
                    broken.Add((cell.Row, cell.Col));
            }

            // Decrementa il freeze su tutte le celle ghiacciate (1 parola corretta = 1 turno)
            gridManager?.DecrementAllFrozenCells();

            // Sostituisci le celle rotte (breve pausa per mostrare lo stato finale)
            if (broken.Count > 0)
            {
                yield return new WaitForSeconds(0.3f);
                foreach (var (row, col) in broken)
                    gridManager?.ReplaceCell(row, col);
            }
        }

        // ----------------------------------------------------------
        // Effetto parola errata (flash rosso + danno ×2)
        // ----------------------------------------------------------

        private IEnumerator WrongWordEffect(List<LetterCell> cells)
        {
            if (cells == null || cells.Count == 0) yield break;

            // Flash rosso + shake simultaneo su tutte le celle
            var flashes = new List<Coroutine>();
            foreach (var cell in cells)
            {
                if (cell != null)
                {
                    flashes.Add(StartCoroutine(cell.FlashWrong(0.14f)));
                    cell.ShakeWrong();
                }
            }
            foreach (var f in flashes) yield return f;

            // Applica danno da parola errata (2 hit) e controlla freeze
            List<(int row, int col)> broken = new List<(int, int)>();
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                cell.ApplyWrongHit(); // gestisce anche il freeze interno
                if (cell.IsLetterBroken)
                    broken.Add((cell.Row, cell.Col));
            }

            // Sostituisci eventuali celle rotte
            if (broken.Count > 0)
            {
                yield return new WaitForSeconds(0.3f);
                foreach (var (row, col) in broken)
                    gridManager?.ReplaceCell(row, col);
            }
        }

        // ----------------------------------------------------------
        // XP float
        // ----------------------------------------------------------

        private void SpawnXPFloat(float xp)
        {
            if (floatCanvas == null) return;
            var go = new GameObject("XPFloat");
            go.transform.SetParent(floatCanvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            float rx = Random.Range(0.2f, 0.8f);
            float ry = Random.Range(0.3f, 0.6f);
            rt.anchorMin = rt.anchorMax = new Vector2(rx, ry);
            rt.sizeDelta = new Vector2(280, 90);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text      = $"+{xp:F0} pt";
            txt.fontSize  = 72;
            txt.fontStyle = FontStyles.Bold;
            txt.color     = Color.white; // flash bianco iniziale
            txt.alignment = TextAlignmentOptions.Center;
            StartCoroutine(AnimateXPFloat(go, txt));
        }

        private static IEnumerator AnimateXPFloat(GameObject go, TextMeshProUGUI txt)
        {
            float elapsed = 0f, duration = 1.4f;
            var rt = go.GetComponent<RectTransform>();
            Vector2 startPos   = rt.anchoredPosition;
            Color   goldColor  = UITheme.Colors.Gold;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale: 0.8 → 1.2 nei primi 0.3s, poi 1.0
                float scaleT = Mathf.Clamp01(elapsed / 0.3f);
                float scale = scaleT < 1f ? Mathf.Lerp(0.8f, 1.2f, scaleT) : 1f;
                rt.localScale = Vector3.one * scale;

                // Colore: bianco → oro nei primi 0.2s, poi fade out
                float colorT = Mathf.Clamp01(elapsed / 0.2f);
                Color c = Color.Lerp(Color.white, goldColor, colorT);
                c.a = 1f - t;
                txt.color = c;

                // Posizione: drift verso l'alto
                rt.anchoredPosition = startPos + Vector2.up * (65f * t);
                yield return null;
            }
            Destroy(go);
        }

        // ----------------------------------------------------------
        // Timer UI
        // ----------------------------------------------------------

        private void UpdateTimerUI()
        {
            if (timerText == null) return;
            int seconds = Mathf.CeilToInt(Mathf.Max(_timeRemaining, 0f));
            timerText.text  = $"{seconds / 60:D2}:{seconds % 60:D2}";
            timerText.color = _timeRemaining <= 10f ? Color.red : Color.white;

            // Pulsazione quando ≤ 10s (scale 1.0 → 1.05 → 1.0 ogni 0.5s)
            if (_timeRemaining <= 10f && _timeRemaining > 0f)
            {
                float pulse = 1f + 0.05f * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / 0.5f));
                timerText.transform.localScale = Vector3.one * pulse;
            }
            else
            {
                timerText.transform.localScale = Vector3.one;
            }
        }
    }
}
