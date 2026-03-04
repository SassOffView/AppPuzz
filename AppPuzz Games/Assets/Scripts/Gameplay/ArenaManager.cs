// ============================================================
// ArenaManager.cs
// Modalità Arena: partita a tempo con punteggi e rank.
// Time-attack con difficoltà crescente, obiettivi di parole,
// e animazione stile "battaglia Pokémon".
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;
using AppPuzz.Localization;
using AppPuzz.UI;
using AppPuzz.Utils;
using AppPuzz.Creatures;

namespace AppPuzz.Gameplay
{
    /// <summary>Difficoltà dell'arena.</summary>
    public enum ArenaDifficulty { Bronzo, Argento, Oro, Platino, Leggenda }

    /// <summary>
    /// Gestisce la modalità Arena (time-attack competitivo).
    /// </summary>
    public class ArenaManager : MonoBehaviour
    {
        public static ArenaManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Inspector
        // ----------------------------------------------------------

        [Header("UI Arena")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI objectiveText;
        public TextMeshProUGUI streakText;
        public Slider          timerBar;
        public Slider          energyBar;
        public Image           timerBarFill;

        [Header("Creature avversario (simulated)")]
        public Image           opponentCreatureImage;
        public Slider          opponentHealthBar;
        public TextMeshProUGUI opponentNameText;

        [Header("Animazioni")]
        public GameObject      attackFlash;      // flash bianco sull'avversario
        public GameObject      victoryEffect;
        public GameObject      defeatEffect;

        [Header("Risultati")]
        public GameObject      resultsOverlay;
        public TextMeshProUGUI resultTitle;
        public TextMeshProUGUI resultScoreText;
        public TextMeshProUGUI resultRankText;
        public Button          playAgainBtn;
        public Button          exitBtn;

        [Header("Configurazione")]
        public float baseDuration   = 90f;
        public float maxEnergy      = 3000f;
        public float targetEnergy   = 1500f;  // obiettivo per vincere

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------
        private WordValidator  _validator = new WordValidator();
        private float          _timeLeft;
        private float          _score;
        private int            _streak;
        private int            _maxStreak;
        private bool           _isPlaying;
        private ArenaDifficulty _difficulty;
        private float          _opponentHealth = 1f;

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
            if (resultsOverlay != null) resultsOverlay.SetActive(false);
        }

        private void OnEnable()
        {
            StartArena(ArenaDifficulty.Bronzo);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timeLeft -= Time.deltaTime;
            UpdateTimerUI();

            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                EndArena();
            }
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Avvia l'arena con la difficoltà indicata.</summary>
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

            GridManager.Instance?.GenerateGrid();

            if (resultsOverlay != null) resultsOverlay.SetActive(false);

            UpdateTimerUI();
            UpdateScoreUI();
            SetupOpponent(difficulty);

            if (objectiveText != null)
                objectiveText.text = $"Obiettivo: {targetEnergy:F0} energia";
            if (rankText != null)
                rankText.text = $"Rango: {difficulty}";

            Debug.Log($"[Arena] Partita avviata. Difficoltà={difficulty}");
        }

        /// <summary>Parola sottomessa dal giocatore in arena.</summary>
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

                // "Danno" all'avversario
                float damage = gained / maxEnergy;
                _opponentHealth = Mathf.Clamp01(_opponentHealth - damage);
                UpdateOpponentUI();

                // Bonus tempo per parole leggendarie
                if (legendary) _timeLeft += 5f;

                StartCoroutine(AttackFlashEffect());
            }
            else
            {
                _streak = 0;
                // Penalità tempo per errori in difficoltà alte
                if (_difficulty >= ArenaDifficulty.Oro)
                    _timeLeft = Mathf.Max(0f, _timeLeft - 2f);
            }

            UpdateScoreUI();

            // Vittoria anticipata se obiettivo raggiunto
            if (_score >= targetEnergy)
                EndArena();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------
        private void WireButtons()
        {
            playAgainBtn?.onClick.AddListener(() => StartArena(_difficulty));
            exitBtn?.onClick.AddListener(() =>
            {
                _isPlaying = false;
                ScreenManager.Instance?.ShowScreen(ScreenID.Home);
            });
        }

        private void EndArena()
        {
            _isPlaying = false;
            bool won = _score >= targetEnergy;

            PlayerProfile.Instance?.RecordMatchResult(_score, _maxStreak, won);

            // Aggiorna rank arena
            if (won && PlayerProfile.Instance != null)
            {
                int newRank = Mathf.Min(PlayerProfile.Instance.ArenaRank + 1, 4);
                PlayerProfile.Instance.ArenaRank = newRank;
            }

            ShowResults(won);
            Debug.Log($"[Arena] Fine. Score={_score:F0}, Won={won}");
        }

        private void ShowResults(bool won)
        {
            if (resultsOverlay != null) resultsOverlay.SetActive(true);
            if (resultTitle   != null) resultTitle.text    = won ? "✦ VITTORIA! ✦" : "SCONFITTA";
            if (resultTitle   != null) resultTitle.color   = won ? UITheme.Colors.Gold : UITheme.Colors.TextDanger;
            if (resultScoreText!= null) resultScoreText.text = $"Energia: {_score:F0}";
            if (resultRankText!= null) resultRankText.text  = $"Rango: {PlayerProfile.Instance?.ArenaRankName ?? "Bronzo"}";

            if (won && victoryEffect != null) victoryEffect.SetActive(true);
            if (!won && defeatEffect != null) defeatEffect.SetActive(true);
        }

        private void SetupOpponent(ArenaDifficulty d)
        {
            string[] names = { "Lupo Grigio", "Drago Bronzeo", "Serpente d'Oro", "Drago Cristallino", "Bestia Leggendaria" };
            if (opponentNameText != null) opponentNameText.text = names[(int)d];
            _opponentHealth = 1f;
            UpdateOpponentUI();
        }

        private void UpdateOpponentUI()
        {
            if (opponentHealthBar != null) opponentHealthBar.value = _opponentHealth;
            if (opponentHealthBar != null)
            {
                var fill = opponentHealthBar.fillRect?.GetComponent<Image>();
                if (fill != null) fill.color = UITheme.EnergyBarColor(_opponentHealth);
            }
        }

        private void UpdateTimerUI()
        {
            float total = GetDuration(_difficulty);
            float norm  = _timeLeft / total;

            if (timerBar != null) timerBar.value = Mathf.Clamp01(norm);
            if (timerBarFill != null) timerBarFill.color = UITheme.EnergyBarColor(norm);

            int s = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f));
            if (timerText != null)
            {
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
            ArenaDifficulty.Bronzo   => baseDuration,
            ArenaDifficulty.Argento  => baseDuration * 0.9f,
            ArenaDifficulty.Oro      => baseDuration * 0.75f,
            ArenaDifficulty.Platino  => baseDuration * 0.6f,
            ArenaDifficulty.Leggenda => baseDuration * 0.5f,
            _                        => baseDuration,
        };
    }
}
