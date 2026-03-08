// ============================================================
// ArenaManager.cs — Arena: time-attack con punteggio e rank.
// Aggiunto: parola corrente in RT, lista parole a fine partita,
// XP float animation.
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
    public enum ArenaDifficulty { Bronzo, Argento, Oro, Platino, Leggenda }

    public class ArenaManager : MonoBehaviour
    {
        public static ArenaManager Instance { get; private set; }

        [Header("UI Arena")]
        public TextMeshProUGUI currentWordText;   // parola in formazione
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI objectiveText;
        public TextMeshProUGUI streakText;
        public Slider          timerBar;
        public Slider          energyBar;
        public Image           timerBarFill;

        [Header("Avversario simulato")]
        public Image           opponentCreatureImage;
        public Slider          opponentHealthBar;
        public TextMeshProUGUI opponentNameText;

        [Header("Animazioni")]
        public GameObject attackFlash;
        public GameObject victoryEffect;
        public GameObject defeatEffect;

        [Header("Risultati")]
        public GameObject      resultsOverlay;
        public TextMeshProUGUI resultTitle;
        public TextMeshProUGUI resultScoreText;
        public TextMeshProUGUI resultRankText;
        public TextMeshProUGUI resultWordsText;    // NUOVO: parole trovabili
        public Button          playAgainBtn;
        public Button          exitBtn;

        [Header("Uscita anticipata")]
        public Button     quitButton;
        public GameObject quitPopup;
        public Button     confirmQuitBtn;
        public Button     cancelQuitBtn;

        [Header("Griglia")]
        public Transform gridContainer;

        [Header("XP Float")]
        public Canvas floatCanvas;

        [Header("Configurazione")]
        public float baseDuration  = 90f;
        public float maxEnergy     = 3000f;
        public float targetEnergy  = 1500f;

        private readonly WordValidator _validator = new WordValidator();
        private float          _timeLeft;
        private float          _score;
        private int            _streak;
        private int            _maxStreak;
        private bool           _isPlaying;
        private ArenaDifficulty _difficulty;
        private float          _opponentHealth = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            WordSelector.OnCurrentWordChanged += OnWordChanged;
            StartArena(ArenaDifficulty.Bronzo);
        }

        private void OnDisable()
        {
            WordSelector.OnCurrentWordChanged -= OnWordChanged;
        }

        private void OnWordChanged(string word)
        {
            if (currentWordText != null) currentWordText.text = word;
        }

        private void Start()
        {
            if (resultsOverlay != null) resultsOverlay.SetActive(false);
            if (quitPopup      != null) quitPopup.SetActive(false);
        }

        private void Update()
        {
            HandleClickInput();
            if (!_isPlaying) return;
            _timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            if (_timeLeft <= 0f) { _timeLeft = 0f; EndArena(); }
        }

        // -------- API pubblica --------
        public void StartArena(ArenaDifficulty difficulty)
        {
            _difficulty = difficulty;
            _timeLeft   = GetDuration(difficulty);
            _score      = 0f;
            _streak     = 0;
            _maxStreak  = 0;
            _isPlaying  = true;
            _opponentHealth = 1f;

            if (LanguageManager.Instance != null)
                _validator.LoadDictionaries(LanguageManager.Instance.GetDictionaryFileName());

            if (gridContainer != null && GridManager.Instance != null)
                GridManager.Instance.gridContainer = gridContainer;
            GridManager.Instance?.SetValidator(_validator);
            GridManager.Instance?.GenerateGrid();

            if (resultsOverlay != null) resultsOverlay.SetActive(false);
            if (quitPopup      != null) quitPopup.SetActive(false);
            if (quitButton     != null) quitButton.gameObject.SetActive(true);
            if (currentWordText!= null) currentWordText.text = "";

            UpdateTimerUI();
            UpdateScoreUI();
            SetupOpponent(difficulty);

            if (objectiveText != null) objectiveText.text = $"Obiettivo: {targetEnergy:F0} energia";
            if (rankText      != null) rankText.text      = $"Rango: {difficulty}";
        }

        public void SubmitWord(string word)
        {
            if (!_isPlaying) return;
            var result = _validator.Validate(word.ToLower());
            bool legendary = result == ValidationResult.Legendary;

            if (result != ValidationResult.Invalid)
            {
                _streak++;
                _maxStreak = Mathf.Max(_maxStreak, _streak);
                float streakMult = Mathf.Min(1f + (_streak - 1) * 0.15f, 2.5f);
                float legMult    = legendary ? 3f : 1f;
                float gained     = word.Length * 2f * streakMult * legMult;
                _score += gained;

                _opponentHealth = Mathf.Clamp01(_opponentHealth - gained / maxEnergy);
                UpdateOpponentUI();

                if (legendary) _timeLeft += 5f;
                StartCoroutine(AttackFlashEffect());
                SpawnXPFloat(gained);

                if (_score >= targetEnergy) EndArena();
            }
            else
            {
                _streak = 0;
                if (_difficulty >= ArenaDifficulty.Oro)
                    _timeLeft = Mathf.Max(0f, _timeLeft - 2f);
            }
            UpdateScoreUI();
        }

        // -------- XP float --------
        private void SpawnXPFloat(float xp)
        {
            if (floatCanvas == null) return;
            var go = new GameObject("XPFloat");
            go.transform.SetParent(floatCanvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.5f, 0.75f));
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

        // -------- Fine partita --------
        private void EndArena()
        {
            _isPlaying = false;
            bool won = _score >= targetEnergy;

            PlayerProfile.Instance?.RecordMatchResult(_score, _maxStreak, won);
            if (won && PlayerProfile.Instance != null)
            {
                int newRank = Mathf.Min(PlayerProfile.Instance.ArenaRank + 1, 4);
                PlayerProfile.Instance.ArenaRank = newRank;
            }

            // Trova le parole che si potevano formare
            var allWords = GridSolver.FindAllWords(GridManager.Instance, _validator);
            ShowResults(won, allWords);
        }

        private void ShowQuitPopup(bool show)
        {
            if (quitPopup  != null) quitPopup.SetActive(show);
            if (quitButton != null) quitButton.gameObject.SetActive(!show);
        }

        private void ShowResults(bool won, List<string> allWords)
        {
            if (resultsOverlay != null) resultsOverlay.SetActive(true);
            if (quitButton     != null) quitButton.gameObject.SetActive(false);
            if (resultTitle    != null)
            {
                resultTitle.text  = won ? "★ VITTORIA! ★" : "SCONFITTA";
                resultTitle.color = won ? UITheme.Colors.Gold : UITheme.Colors.TextDanger;
            }
            if (resultScoreText != null) resultScoreText.text = $"Energia: {_score:F0}";
            if (resultRankText  != null) resultRankText.text  = $"Rango: {PlayerProfile.Instance?.ArenaRankName ?? "Bronzo"}";

            if (resultWordsText != null && allWords != null)
            {
                var sb = new System.Text.StringBuilder();
                int show = Mathf.Min(allWords.Count, 20);
                sb.AppendLine($"Parole trovabili ({allWords.Count}):");
                for (int i = 0; i < show; i++)
                    sb.AppendLine($"{allWords[i].ToUpper()} ({allWords[i].Length})");
                if (allWords.Count > show) sb.AppendLine("...");
                resultWordsText.text = sb.ToString();
            }
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
                if (IsUnder(go, playAgainBtn))   { StartArena(_difficulty);                           return; }
                if (IsUnder(go, exitBtn))         { _isPlaying = false; ScreenManager.Instance?.ShowScreen(ScreenID.Home); return; }
                if (IsUnder(go, quitButton))      { ShowQuitPopup(true);                              return; }
                if (IsUnder(go, confirmQuitBtn))  { ShowQuitPopup(false); _isPlaying = false; ScreenManager.Instance?.ShowScreen(ScreenID.Home); return; }
                if (IsUnder(go, cancelQuitBtn))   { ShowQuitPopup(false);                             return; }
            }
        }

        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }

        // -------- Helpers UI --------
        private void SetupOpponent(ArenaDifficulty d)
        {
            string[] names = {"Lupo Grigio","Drago Bronzeo","Serpente d'Oro","Drago Cristallino","Bestia Leggendaria"};
            if (opponentNameText != null) opponentNameText.text = names[(int)d];
            _opponentHealth = 1f;
            UpdateOpponentUI();
        }

        private void UpdateOpponentUI()
        {
            if (opponentHealthBar != null) opponentHealthBar.value = _opponentHealth;
            var fill = opponentHealthBar?.fillRect?.GetComponent<Image>();
            if (fill != null) fill.color = UITheme.EnergyBarColor(_opponentHealth);
        }

        private void UpdateTimerUI()
        {
            float total = GetDuration(_difficulty);
            float norm  = _timeLeft / total;
            if (timerBar     != null) timerBar.value = Mathf.Clamp01(norm);
            if (timerBarFill != null) timerBarFill.color = UITheme.EnergyBarColor(norm);
            if (timerText    != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f));
                timerText.text  = $"{s / 60:D2}:{s % 60:D2}";
                timerText.color = _timeLeft <= 10f ? UITheme.Colors.TextDanger : UITheme.Colors.TextPrimary;
            }
        }

        private void UpdateScoreUI()
        {
            if (scoreText  != null) scoreText.text  = $"{_score:F0}";
            if (streakText != null) streakText.text = $"Streak ×{_streak}";
            if (energyBar  != null) energyBar.value = Mathf.Clamp01(_score / maxEnergy);
        }

        private IEnumerator AttackFlashEffect()
        {
            if (attackFlash == null) yield break;
            attackFlash.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            attackFlash.SetActive(false);
        }

        private float GetDuration(ArenaDifficulty d) => d switch
        {
            ArenaDifficulty.Argento  => baseDuration * 0.9f,
            ArenaDifficulty.Oro      => baseDuration * 0.75f,
            ArenaDifficulty.Platino  => baseDuration * 0.6f,
            ArenaDifficulty.Leggenda => baseDuration * 0.5f,
            _                        => baseDuration,
        };
    }
}
