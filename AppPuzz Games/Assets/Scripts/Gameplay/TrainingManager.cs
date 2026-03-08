// ============================================================
// TrainingManager.cs — Allenamento: timer 2 min, suggerimenti,
// parola corrente in tempo reale, fine-sessione con parole trovabili.
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

namespace AppPuzz.Gameplay
{
    public class TrainingManager : MonoBehaviour
    {
        public static TrainingManager Instance { get; private set; }

        [Header("UI Training")]
        public TextMeshProUGUI currentWordText;   // parola in formazione
        public TextMeshProUGUI feedbackText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI wordsFoundText;
        public TextMeshProUGUI hintText;
        public TextMeshProUGUI timerText;         // NUOVO: countdown
        public Slider          energyBar;
        public Slider          timerBar;          // NUOVO

        [Header("Pulsanti")]
        public Button hintButton;
        public Button newGridButton;
        public Button exitButton;

        [Header("Griglia")]
        public Transform gridContainer;

        [Header("Fine Sessione")]
        public GameObject       endSessionOverlay;     // NUOVO: overlay fine partita
        public TextMeshProUGUI  endSessionTitle;
        public TextMeshProUGUI  endWordsScrollText;    // lista parole trovabili
        public Button           endCloseButton;

        [Header("XP Float")]
        public Canvas floatCanvas;                     // canvas per i label XP flottanti

        [Header("Configurazione")]
        public int   hintsPerSession = 3;
        public float xpMultiplier   = 0.5f;
        public float sessionDuration = 120f;           // 2 minuti

        private readonly WordValidator _validator = new WordValidator();
        private float _score;
        private int   _wordsFound;
        private int   _hintsLeft;
        private bool  _isActive;
        private float _timeLeft;

        private const float MAX_ENERGY = 500f;

        private static readonly string[] FB_OK  = {"Ottimo!","Bravo!","Perfetto!","Fantastico!"};
        private static readonly string[] FB_LEG  = {"★ LEGGENDARIO! ★","★ INCREDIBILE! ★","★ EPICO! ★"};
        private static readonly string[] FB_BAD  = {"Non trovata","Riprova!","Non è nel dizionario"};

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            WordSelector.OnCurrentWordChanged += OnWordChanged;
            StartTraining();
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

            if (!_isActive) return;
            _timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            if (_timeLeft <= 0f) { _timeLeft = 0f; EndSession(); }
        }

        // -------- API pubblica --------
        public void StartTraining()
        {
            _score      = 0f;
            _wordsFound = 0;
            _hintsLeft  = hintsPerSession;
            _isActive   = true;
            _timeLeft   = sessionDuration;

            if (LanguageManager.Instance != null)
                _validator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            if (gridContainer != null && GridManager.Instance != null)
                GridManager.Instance.gridContainer = gridContainer;
            // Passa il validator per garantire i requisiti minimi di parole
            GridManager.Instance?.SetValidator(_validator);
            GridManager.Instance?.GenerateGrid();

            if (endSessionOverlay != null) endSessionOverlay.SetActive(false);

            UpdateUI();
            UpdateTimerUI();
            if (feedbackText != null) feedbackText.text = "Trova parole sulla griglia!";
            if (hintText     != null) hintText.text     = "";
            if (currentWordText != null) currentWordText.text = "";
        }

        public void SubmitWord(string word)
        {
            if (!_isActive) return;
            var result = _validator.Validate(word.ToLower());
            bool legendary = result == ValidationResult.Legendary;

            if (result != ValidationResult.Invalid)
            {
                float baseScore = word.Length * 2f;
                if (legendary) baseScore *= 3f;
                _score += baseScore;
                _wordsFound++;

                ShowFeedback(legendary
                    ? FB_LEG[Random.Range(0, FB_LEG.Length)]
                    : FB_OK [Random.Range(0, FB_OK.Length)],
                    legendary ? UITheme.Colors.Legendary : UITheme.Colors.TextSuccess);

                SpawnXPFloat(baseScore);
            }
            else
            {
                ShowFeedback(FB_BAD[Random.Range(0, FB_BAD.Length)], UITheme.Colors.TextDanger);
            }
            UpdateUI();
        }

        // -------- Fine sessione --------
        private void EndSession()
        {
            _isActive = false;
            PlayerProfile.Instance?.RecordMatchResult(_score, 0, false);

            // Trova le parole che si potevano formare
            var allWords = GridSolver.FindAllWords(GridManager.Instance, _validator);

            ShowEndOverlay(allWords);
        }

        private void ShowEndOverlay(List<string> allWords)
        {
            if (endSessionOverlay == null) return;
            endSessionOverlay.SetActive(true);

            if (endSessionTitle != null)
                endSessionTitle.text = $"Tempo scaduto!\nParole trovate: {_wordsFound} | Punteggio: {_score:F0}";

            if (endWordsScrollText != null)
            {
                if (allWords.Count == 0)
                {
                    endWordsScrollText.text = "(nessuna parola trovabile)";
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Parole trovabili ({allWords.Count}):");
                    sb.AppendLine();
                    foreach (var w in allWords)
                        sb.AppendLine($"  {w.ToUpper()}  ({w.Length} lettere)");
                    endWordsScrollText.text = sb.ToString();
                }
            }
        }

        // -------- XP float --------
        private void SpawnXPFloat(float xp)
        {
            if (floatCanvas == null) return;
            var go = new GameObject("XPFloat");
            go.transform.SetParent(floatCanvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            // Posizione casuale nella metà superiore dello schermo
            rt.anchorMin = rt.anchorMax = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.5f, 0.8f));
            rt.sizeDelta = new Vector2(200, 60);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = $"+{xp:F0} XP";
            txt.fontSize = 48;
            txt.color = UITheme.Colors.Gold;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            StartCoroutine(AnimateXPFloat(go, txt));
        }

        private IEnumerator AnimateXPFloat(GameObject go, TextMeshProUGUI txt)
        {
            float elapsed = 0f;
            float duration = 1.4f;
            var startColor = txt.color;
            var rt = go.GetComponent<RectTransform>();
            Vector2 startPos = rt.anchoredPosition;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = startPos + Vector2.up * (60f * t);
                txt.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }
            Destroy(go);
        }

        // -------- UI --------
        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f));
                timerText.text  = $"{s / 60:D2}:{s % 60:D2}";
                timerText.color = _timeLeft <= 15f ? UITheme.Colors.TextDanger : UITheme.Colors.TextPrimary;
            }
            if (timerBar != null)
                timerBar.value = Mathf.Clamp01(_timeLeft / sessionDuration);
        }

        private void UpdateUI()
        {
            if (scoreText      != null) scoreText.text      = $"Energia: {_score:F0}";
            if (wordsFoundText != null) wordsFoundText.text = $"Parole: {_wordsFound}";
            if (energyBar      != null) energyBar.value     = Mathf.Clamp01(_score / MAX_ENERGY);
            if (hintButton != null)
            {
                var lbl = hintButton.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl != null) lbl.text = $"Suggerimento ({_hintsLeft})";
                hintButton.interactable = _hintsLeft > 0;
            }
        }

        private void ShowFeedback(string msg, Color color)
        {
            if (feedbackText == null) return;
            feedbackText.text  = msg;
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
                if (IsUnder(go, hintButton))    { ShowHint();                  return; }
                if (IsUnder(go, newGridButton)) { OnNewGrid();                 return; }
                if (IsUnder(go, exitButton))    { OnExit();                    return; }
                if (IsUnder(go, endCloseButton)){ OnEndClose();                return; }
            }
        }

        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }

        private void ShowHint()
        {
            if (_hintsLeft <= 0) { ShowFeedback("Nessun suggerimento rimasto!", UITheme.Colors.TextDanger); return; }
            _hintsLeft--;
            if (hintText != null)
                hintText.text = $"Suggerimento: cerca parole di {Random.Range(4,7)} lettere. ({_hintsLeft} rimasti)";
            UpdateUI();
        }

        private void OnNewGrid()
        {
            if (gridContainer != null && GridManager.Instance != null)
                GridManager.Instance.gridContainer = gridContainer;
            GridManager.Instance?.SetValidator(_validator);
            GridManager.Instance?.GenerateGrid();
            if (feedbackText != null) feedbackText.text = "Nuova griglia!";
        }

        private void OnExit()
        {
            _isActive = false;
            PlayerProfile.Instance?.RecordMatchResult(_score, 0, false);
            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }

        private void OnEndClose()
        {
            if (endSessionOverlay != null) endSessionOverlay.SetActive(false);
            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }
    }
}
