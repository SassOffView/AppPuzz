// ============================================================
// LegendManager.cs — Modalita Leggenda: 50 sfide progressive
// con narrazione, griglie crescenti e premi.
//
// Sfide 1-9:   griglia 4x4, 60 secondi
// Sfide 10-24: griglia 5x5, 45 secondi
// Sfide 25-44: griglia 6x6, 40 secondi
// Sfide 45-50: griglia 7x7, 30 secondi
//
// Premi: carte (ATT/DEF/SPE) + skin/accessori creatura
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;
using AppPuzz.Localization;
using AppPuzz.UI;
using AppPuzz.Utils;
using AppPuzz.Creatures;

namespace AppPuzz.Gameplay
{
    public class LegendManager : MonoBehaviour
    {
        public static LegendManager Instance { get; private set; }

        [Header("UI Legend")]
        public TextMeshProUGUI currentWordText;
        public TextMeshProUGUI feedbackText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI wordsFoundText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI challengeTitle;
        public TextMeshProUGUI narrativeText;
        public Slider          timerBar;
        public Slider          energyBar;

        [Header("Pulsanti")]
        public Button exitButton;

        [Header("Griglia")]
        public Transform gridContainer;

        [Header("Fine Sfida")]
        public GameObject       resultOverlay;
        public TextMeshProUGUI  resultTitle;
        public TextMeshProUGUI  resultScoreText;
        public TextMeshProUGUI  resultRewardText;
        public TextMeshProUGUI  resultWordsText;
        public Button           nextChallengeBtn;
        public Button           resultMenuBtn;

        [Header("XP Float")]
        public Canvas floatCanvas;

        // Sfida corrente
        private int   _challengeLevel;
        private float _timeLeft;
        private float _score;
        private int   _wordsFound;
        private int   _targetScore;
        private bool  _isPlaying;
        private readonly WordValidator _validator = new WordValidator();
        private readonly List<string> _foundWords = new List<string>();

        // ── Narrativa per le sfide ──
        private static readonly string[] NARRATIVES = {
            "La tua creatura si risveglia... Il viaggio inizia.",
            "Le prime parole risuonano nella foresta.",
            "Un sentiero di lettere si apre davanti a te.",
            "La nebbia si dirada, rivelando nuove parole.",
            "Un antico portale richiede il tuo sapere.",
            "Le rune iniziano a brillare piu intensamente.",
            "La creatura cresce, avida di conoscenza.",
            "Un enigma ancestrale ti attende.",
            "I cristalli risuonano con le tue parole.",
            "La prima evoluzione e vicina!",                      // 10 - grid 5x5
            "La griglia si espande. Piu parole, piu potere.",
            "Un guardiano ti sfida: trova le parole nascoste!",
            "La creatura impara nuove abilita.",
            "Le stelle guidano il tuo cammino di lettere.",
            "Un bivio: solo le parole giuste apriranno la via.",
            "La biblioteca perduta rivela i suoi segreti.",
            "Le catene si spezzano parola dopo parola.",
            "Un drago di parole protegge il passaggio.",
            "La pietra filosofale si manifesta nelle lettere.",
            "Il secondo risveglio e imminente.",                   // 20
            "Le parole diventano armi piu potenti.",
            "Un labirinto di lettere ti circonda.",
            "La creatura ruggisce di potenza!",
            "Le ombre si ritirano davanti al tuo sapere.",
            "La griglia si evolve ancora!",                        // 25 - grid 6x6
            "Un mondo nuovo di possibilita si apre.",
            "I nemici tremano al suono delle tue parole.",
            "La creatura raggiunge una forma superiore.",
            "Le parole forgiano la tua leggenda.",
            "Un torneo tra le stelle ti attende.",                  // 30
            "Le costellazioni formano nuove parole.",
            "Il potere cresce ad ogni sfida superata.",
            "Un oracolo predice la tua vittoria.",
            "Le parole si intrecciano come magia.",
            "La creatura brilla di luce propria.",                  // 35
            "Un esercito di lettere marcia al tuo comando.",
            "Le mura del castello cedono alle tue parole.",
            "Il trono delle parole e quasi tuo.",
            "La creatura raggiunge la sua forma finale!",
            "Le ultime prove si avvicinano.",                       // 40
            "Solo i piu forti arrivano fin qui.",
            "Le parole diventano puro potere.",
            "La leggenda si scrive lettera per lettera.",
            "Il destino e nelle tue mani.",
            "La griglia suprema si rivela!",                        // 45 - grid 7x7
            "Ogni parola vale oro puro.",
            "La creatura trascende ogni limite.",
            "Le ultime parole forgiano l'eternita.",
            "Un ultimo sforzo per la gloria!",
            "SEI DIVENTATO UNA LEGGENDA!"                           // 50
        };

        // ── Config sfide ──
        public static int GetGridSize(int level)
        {
            if (level < 10) return 4;
            if (level < 25) return 5;
            if (level < 45) return 6;
            return 7;
        }

        public static float GetTimeLimit(int level)
        {
            if (level < 10) return 60f;
            if (level < 25) return 45f;
            if (level < 45) return 40f;
            return 30f;
        }

        public static int GetTargetScore(int level)
        {
            // Punteggio obiettivo crescente
            return 50 + level * 20;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            WordSelector.OnCurrentWordChanged += OnWordChanged;
            _challengeLevel = PlayerProfile.Instance?.LegendLevel ?? 1;
            StartChallenge(_challengeLevel);
        }

        private void OnDisable()
        {
            WordSelector.OnCurrentWordChanged -= OnWordChanged;
        }

        private void OnWordChanged(string word)
        {
            if (currentWordText != null) currentWordText.text = word;
        }

        private void Update()
        {
            HandleClickInput();
            if (!_isPlaying) return;
            _timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            if (_timeLeft <= 0f) { _timeLeft = 0f; EndChallenge(false); }
        }

        // -------- Avvio sfida --------
        public void StartChallenge(int level)
        {
            _challengeLevel = Mathf.Clamp(level, 1, 50);
            _timeLeft    = GetTimeLimit(_challengeLevel);
            _score       = 0f;
            _wordsFound  = 0;
            _targetScore = GetTargetScore(_challengeLevel);
            _isPlaying   = true;
            _foundWords.Clear();

            if (LanguageManager.Instance != null)
                _validator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            // Configura griglia
            int gridSize = GetGridSize(_challengeLevel);
            ConfigureGrid(gridSize);

            if (gridContainer != null && GridManager.Instance != null)
                GridManager.Instance.gridContainer = gridContainer;
            GridManager.Instance?.SetValidator(_validator);
            GridManager.Instance?.GenerateGrid();

            if (resultOverlay != null) resultOverlay.SetActive(false);

            // UI
            if (challengeTitle != null)
                challengeTitle.text = $"SFIDA {_challengeLevel}/50";
            if (narrativeText != null)
            {
                int idx = Mathf.Clamp(_challengeLevel - 1, 0, NARRATIVES.Length - 1);
                narrativeText.text = NARRATIVES[idx];
            }
            if (currentWordText != null) currentWordText.text = "";
            if (feedbackText != null) feedbackText.text = "";

            UpdateUI();
            UpdateTimerUI();
            Debug.Log($"[LegendManager] Sfida {_challengeLevel} avviata. Grid {gridSize}x{gridSize}, {_timeLeft}s, target {_targetScore}");
        }

        private void ConfigureGrid(int gridSize)
        {
            // Aggiorna GridManager.GRID_SIZE non e possibile (const), quindi
            // cambiamo constraintCount nel GridLayoutGroup
            if (gridContainer == null) return;
            var glg = gridContainer.GetComponent<GridLayoutGroup>();
            if (glg != null)
                glg.constraintCount = gridSize;

            // Aggiorna temporaneamente GRID_SIZE nel GridManager
            // Siccome GRID_SIZE e const, usiamo un approccio diverso:
            // impostiamo una variabile pubblica opzionale
            if (GridManager.Instance != null)
                GridManager.Instance.overrideGridSize = gridSize;
        }

        // -------- Submit parola --------
        public void SubmitWord(string word, List<LetterCell> cells = null)
        {
            if (!_isPlaying) return;
            var result = _validator.Validate(word.ToLower());
            bool legendary = result == ValidationResult.Legendary;

            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;

            if (result != ValidationResult.Invalid)
            {
                float baseScore = LetterScoring.CalculateBaseScore(word, isItalian);
                if (legendary) baseScore *= 3f;
                _score += baseScore;
                _wordsFound++;
                _foundWords.Add(word.ToUpper());

                ShowFeedback(legendary ? "LEGGENDARIO!" : "Ottimo!",
                    legendary ? UITheme.Colors.Legendary : UITheme.Colors.TextSuccess);
                SpawnXPFloat(baseScore);

                if (cells != null && cells.Count > 0)
                {
                    string creatureType = CreatureController.Instance?.SelectedCreatureTypeName ?? "MentalDragon";
                    Color lightColor = UITheme.LightningColor(creatureType);
                    StartCoroutine(LightningAndDamageSequence(cells, lightColor));
                }

                // Controllo vittoria
                if (_score >= _targetScore)
                    EndChallenge(true);
            }
            else
            {
                ShowFeedback("Non trovata", UITheme.Colors.TextDanger);
                if (cells != null && cells.Count > 0)
                    StartCoroutine(WrongWordEffect(cells));
            }
            UpdateUI();
        }

        private IEnumerator LightningAndDamageSequence(List<LetterCell> cells, Color lightColor)
        {
            foreach (var cell in cells)
            {
                if (cell != null) yield return StartCoroutine(cell.FlashLightning(lightColor, 0.09f));
                yield return new WaitForSeconds(0.04f);
            }
            yield return new WaitForSeconds(0.05f);

            foreach (var cell in cells)
            {
                if (cell == null) continue;
                cell.ApplyCorrectHit();
            }
            GridManager.Instance?.DecrementAllFrozenCells();
        }

        private IEnumerator WrongWordEffect(List<LetterCell> cells)
        {
            var flashes = new List<Coroutine>();
            foreach (var cell in cells)
                if (cell != null) flashes.Add(StartCoroutine(cell.FlashWrong(0.14f)));
            foreach (var f in flashes) yield return f;

            foreach (var cell in cells)
            {
                if (cell == null) continue;
                cell.ApplyWrongHit();
            }
        }

        // -------- Fine sfida --------
        private void EndChallenge(bool won)
        {
            _isPlaying = false;

            var allWords = GridSolver.FindAllWords(GridManager.Instance, _validator);

            if (resultOverlay != null) resultOverlay.SetActive(true);

            if (resultTitle != null)
            {
                resultTitle.text = won ? "SFIDA SUPERATA!" : "TEMPO SCADUTO";
                resultTitle.color = won ? UITheme.Colors.Gold : UITheme.Colors.TextDanger;
            }

            if (resultScoreText != null)
                resultScoreText.text = $"Punteggio: {_score:F0} / {_targetScore}\nParole: {_wordsFound}";

            // Premi
            string rewardText = "";
            if (won)
            {
                // Sblocca prossima sfida
                if (PlayerProfile.Instance != null && _challengeLevel >= PlayerProfile.Instance.LegendLevel)
                {
                    PlayerProfile.Instance.LegendLevel = _challengeLevel + 1;
                }

                // Calcola premi proporzionati al livello
                int cardReward = 1 + _challengeLevel / 10;
                int attCards = Random.Range(0, cardReward + 1);
                int defCards = Random.Range(0, cardReward + 1);
                int speCards = cardReward - Mathf.Min(attCards, cardReward);
                PlayerProfile.Instance?.AddLegendCards(attCards, defCards, speCards);

                // XP
                float xpReward = 10f + _challengeLevel * 5f;
                if (PlayerProfile.Instance != null)
                {
                    PlayerProfile.Instance.CurrentXP += xpReward;
                    PlayerProfile.Instance.TotalXP += xpReward;
                    PlayerProfile.Instance.RecalculateLevel();
                }

                rewardText = $"PREMI:\n+{xpReward:F0} XP\nCarte: ATT x{attCards} | DEF x{defCards} | SPE x{speCards}";

                if (_challengeLevel % 5 == 0)
                    rewardText += "\n+ Accessorio Creatura sbloccato!";
            }
            else
            {
                rewardText = "Riprova per superare la sfida!";
            }

            if (resultRewardText != null)
                resultRewardText.text = rewardText;

            // Parole trovate/trovabili
            if (resultWordsText != null)
            {
                var sb = new System.Text.StringBuilder();
                if (_foundWords.Count > 0)
                {
                    sb.AppendLine($"<b>LE TUE PAROLE ({_foundWords.Count}):</b>");
                    foreach (var w in _foundWords)
                        sb.AppendLine($"  {w}  ({w.Length} lettere)");
                    sb.AppendLine();
                }
                if (allWords.Count > 0)
                {
                    sb.AppendLine($"<b>PAROLE NELLA GRIGLIA ({allWords.Count}):</b>");
                    var foundSet = new HashSet<string>(_foundWords, System.StringComparer.OrdinalIgnoreCase);
                    foreach (var w in allWords)
                    {
                        string mark = foundSet.Contains(w.ToUpper()) ? " [OK]" : "";
                        sb.AppendLine($"  {w.ToUpper()}  ({w.Length} lettere){mark}");
                    }
                }
                resultWordsText.text = sb.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    resultWordsText.transform.parent.parent as RectTransform);
            }

            if (nextChallengeBtn != null)
                nextChallengeBtn.gameObject.SetActive(won && _challengeLevel < 50);
        }

        // -------- XP float --------
        private void SpawnXPFloat(float xp)
        {
            if (floatCanvas == null) return;
            var go = new GameObject("XPFloat");
            go.transform.SetParent(floatCanvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.5f, 0.8f));
            rt.sizeDelta = new Vector2(200, 60);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = $"+{xp:F0}";
            txt.fontSize = 48;
            txt.color = UITheme.Colors.Gold;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            StartCoroutine(AnimateXPFloat(go, txt));
        }

        private IEnumerator AnimateXPFloat(GameObject go, TextMeshProUGUI txt)
        {
            float elapsed = 0f, duration = 1.2f;
            var sc = txt.color;
            var rt = go.GetComponent<RectTransform>();
            Vector2 sp = rt.anchoredPosition;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = sp + Vector2.up * (50f * t);
                txt.color = new Color(sc.r, sc.g, sc.b, 1f - t);
                yield return null;
            }
            Destroy(go);
        }

        // -------- UI --------
        private void UpdateTimerUI()
        {
            float total = GetTimeLimit(_challengeLevel);
            if (timerText != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f));
                timerText.text = $"{s / 60:D2}:{s % 60:D2}";
                timerText.color = _timeLeft <= 10f ? UITheme.Colors.TextDanger : UITheme.Colors.TextPrimary;
            }
            if (timerBar != null)
                timerBar.value = Mathf.Clamp01(_timeLeft / total);
        }

        private void UpdateUI()
        {
            if (scoreText != null) scoreText.text = $"Punteggio: {_score:F0} / {_targetScore}";
            if (wordsFoundText != null) wordsFoundText.text = $"Parole: {_wordsFound}";
            if (energyBar != null) energyBar.value = Mathf.Clamp01(_score / _targetScore);
        }

        private void ShowFeedback(string msg, Color color)
        {
            if (feedbackText == null) return;
            feedbackText.text = msg;
            feedbackText.color = color;
            StopCoroutine("FadeFeedback");
            StartCoroutine("FadeFeedback");
        }

        private IEnumerator FadeFeedback()
        {
            yield return new WaitForSeconds(2f);
            if (feedbackText != null) { feedbackText.text = ""; feedbackText.color = UITheme.Colors.TextPrimary; }
        }

        // -------- Click --------
        private void HandleClickInput()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) clicked = true;
            if (!clicked) return;
            var es = EventSystem.current;
            if (es == null) return;
            Vector2 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            var ptr = new PointerEventData(es) { position = pos };
            var hits = new List<RaycastResult>();
            es.RaycastAll(ptr, hits);
            foreach (var r in hits)
            {
                var go = r.gameObject;
                if (IsUnder(go, exitButton))        { OnExit();            return; }
                if (IsUnder(go, nextChallengeBtn))  { OnNextChallenge();   return; }
                if (IsUnder(go, resultMenuBtn))     { OnMenu();            return; }
            }
        }

        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }

        private void OnExit()
        {
            _isPlaying = false;
            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }

        private void OnNextChallenge()
        {
            StartChallenge(_challengeLevel + 1);
        }

        private void OnMenu()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }
    }
}
