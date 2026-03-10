// ============================================================
// PlayerProfile.cs
// Dati persistenti del giocatore: creatura scelta, XP, record,
// progressione evoluzione, stato onboarding.
// Usa PlayerPrefs per la persistenza locale.
// ============================================================

using UnityEngine;
using System;
using AppPuzz.Creatures;

namespace AppPuzz.Utils
{
    /// <summary>
    /// Singleton che gestisce tutti i dati persistenti del giocatore.
    /// DontDestroyOnLoad: sopravvive tra le scene.
    /// </summary>
    public class PlayerProfile : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static PlayerProfile Instance { get; private set; }

        // ----------------------------------------------------------
        // Chiavi PlayerPrefs
        // ----------------------------------------------------------
        private const string KEY_CREATURE       = "player_creature";
        private const string KEY_XP             = "player_xp";
        private const string KEY_TOTAL_XP       = "player_total_xp";
        private const string KEY_BEST_SCORE     = "player_best_score";
        private const string KEY_TOTAL_MATCHES  = "player_total_matches";
        private const string KEY_BEST_STREAK    = "player_best_streak";
        private const string KEY_ONBOARDING     = "onboarding_complete";
        private const string KEY_LAST_PLAYED    = "last_played_date";
        private const string KEY_WINS           = "player_wins";
        private const string KEY_ARENA_RANK     = "arena_rank";
        private const string KEY_LEVEL          = "player_level";
        private const string KEY_NAME           = "player_name";
        private const string KEY_AVATAR         = "player_avatar";
        private const string KEY_LEGEND_LEVEL   = "legend_level";
        private const string KEY_LEGEND_CARDS   = "legend_cards";

        // ----------------------------------------------------------
        // Proprietà (lettura da PlayerPrefs al primo accesso)
        // ----------------------------------------------------------

        /// <summary>Tipo creatura selezionata dal giocatore.</summary>
        public CreatureType SelectedCreature
        {
            get => (CreatureType)PlayerPrefs.GetInt(KEY_CREATURE, 0);
            set => PlayerPrefs.SetInt(KEY_CREATURE, (int)value);
        }

        /// <summary>Nome (nickname) del giocatore.</summary>
        public string PlayerName
        {
            get => PlayerPrefs.GetString(KEY_NAME, "Evocatore");
            set => PlayerPrefs.SetString(KEY_NAME, value);
        }

        /// <summary>Avatar selezionato (0-9).</summary>
        public int SelectedAvatarId
        {
            get => PlayerPrefs.GetInt(KEY_AVATAR, 0);
            set => PlayerPrefs.SetInt(KEY_AVATAR, Mathf.Clamp(value, 0, 9));
        }

        /// <summary>XP corrente (per l'evoluzione della creatura).</summary>
        public float CurrentXP
        {
            get => PlayerPrefs.GetFloat(KEY_XP, 0f);
            set => PlayerPrefs.SetFloat(KEY_XP, value);
        }

        /// <summary>XP totale accumulato in tutte le sessioni.</summary>
        public float TotalXP
        {
            get => PlayerPrefs.GetFloat(KEY_TOTAL_XP, 0f);
            set => PlayerPrefs.SetFloat(KEY_TOTAL_XP, value);
        }

        /// <summary>Miglior punteggio singola partita.</summary>
        public float BestScore
        {
            get => PlayerPrefs.GetFloat(KEY_BEST_SCORE, 0f);
            set { if (value > BestScore) PlayerPrefs.SetFloat(KEY_BEST_SCORE, value); }
        }

        /// <summary>Numero totale di partite giocate.</summary>
        public int TotalMatches
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_MATCHES, 0);
            set => PlayerPrefs.SetInt(KEY_TOTAL_MATCHES, value);
        }

        /// <summary>Miglior streak di parole consecutive in assoluto.</summary>
        public int BestStreak
        {
            get => PlayerPrefs.GetInt(KEY_BEST_STREAK, 0);
            set { if (value > BestStreak) PlayerPrefs.SetInt(KEY_BEST_STREAK, value); }
        }

        /// <summary>Onboarding già completato.</summary>
        public bool OnboardingComplete
        {
            get => PlayerPrefs.GetInt(KEY_ONBOARDING, 0) == 1;
            set => PlayerPrefs.SetInt(KEY_ONBOARDING, value ? 1 : 0);
        }

        /// <summary>Numero di vittorie in arena.</summary>
        public int ArenaWins
        {
            get => PlayerPrefs.GetInt(KEY_WINS, 0);
            set => PlayerPrefs.SetInt(KEY_WINS, value);
        }

        /// <summary>Rango arena (0=Bronzo, 1=Argento, 2=Oro, 3=Platino, 4=Leggenda).</summary>
        public int ArenaRank
        {
            get => PlayerPrefs.GetInt(KEY_ARENA_RANK, 0);
            set => PlayerPrefs.SetInt(KEY_ARENA_RANK, Mathf.Clamp(value, 0, 4));
        }

        /// <summary>Livello giocatore (aumenta con TotalXP).</summary>
        public int PlayerLevel
        {
            get => PlayerPrefs.GetInt(KEY_LEVEL, 1);
            private set => PlayerPrefs.SetInt(KEY_LEVEL, value);
        }

        /// <summary>Sfida Leggenda corrente (1-50). 1 = prima sfida.</summary>
        public int LegendLevel
        {
            get => PlayerPrefs.GetInt(KEY_LEGEND_LEVEL, 1);
            set => PlayerPrefs.SetInt(KEY_LEGEND_LEVEL, Mathf.Clamp(value, 1, 50));
        }

        /// <summary>Carte collezionate (ATT|DEF|SPE conteggi separati da virgola).</summary>
        public string LegendCards
        {
            get => PlayerPrefs.GetString(KEY_LEGEND_CARDS, "0,0,0");
            set => PlayerPrefs.SetString(KEY_LEGEND_CARDS, value);
        }

        public void AddLegendCards(int att, int def, int spe)
        {
            var parts = LegendCards.Split(',');
            int a = int.Parse(parts[0]) + att;
            int d = int.Parse(parts[1]) + def;
            int s = int.Parse(parts[2]) + spe;
            LegendCards = $"{a},{d},{s}";
            PlayerPrefs.Save();
        }

        // ----------------------------------------------------------
        // Costanti progressione
        // ----------------------------------------------------------
        private static readonly float[] XP_PER_LEVEL = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500 };
        public static readonly string[] ARENA_RANK_NAMES = { "Bronzo", "Argento", "Oro", "Platino", "Leggenda" };

        public static readonly string[] EVOLUTION_TITLES = {
            "Apprendista",   // lv 1-4
            "Iniziato",      // lv 5-9
            "Praticante",    // lv 10-19
            "Esperto",       // lv 20-34
            "Maestro",       // lv 35-49
            "Arcimago",      // lv 50-74
            "Stregone",      // lv 75+
        };

        /// <summary>Titolo evoluzione basato sul livello corrente.</summary>
        public string EvolutionTitle
        {
            get
            {
                int lv = PlayerLevel;
                if (lv >= 75) return EVOLUTION_TITLES[6];
                if (lv >= 50) return EVOLUTION_TITLES[5];
                if (lv >= 35) return EVOLUTION_TITLES[4];
                if (lv >= 20) return EVOLUTION_TITLES[3];
                if (lv >= 10) return EVOLUTION_TITLES[2];
                if (lv >= 5)  return EVOLUTION_TITLES[1];
                return EVOLUTION_TITLES[0];
            }
        }

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Registra il risultato di una partita terminata.
        /// Aggiorna XP, bestScore, totalMatches, bestStreak.
        /// </summary>
        public void RecordMatchResult(float energy, int streak, bool won = false)
        {
            TotalMatches++;
            BestScore  = energy;        // setter fa il confronto
            BestStreak = streak;

            // XP guadagnato dalla partita
            float xpGained = energy * 0.5f + streak * 5f;
            if (won) xpGained *= 1.5f;

            CurrentXP += xpGained;
            TotalXP   += xpGained;

            RecalculateLevel();

            if (won) ArenaWins++;

            PlayerPrefs.SetString(KEY_LAST_PLAYED, DateTime.Today.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();

            Debug.Log($"[PlayerProfile] Partita salvata. XP+{xpGained:F0}, Livello={PlayerLevel}");
        }

        /// <summary>Ricalcola il livello giocatore in base al TotalXP.</summary>
        public void RecalculateLevel()
        {
            int lv = 1;
            for (int i = XP_PER_LEVEL.Length - 1; i >= 1; i--)
            {
                if (TotalXP >= XP_PER_LEVEL[i]) { lv = i + 1; break; }
            }
            PlayerLevel = lv;
        }

        /// <summary>Progresso XP verso il prossimo livello [0..1].</summary>
        public float LevelProgress()
        {
            int lv = PlayerLevel;
            if (lv >= XP_PER_LEVEL.Length) return 1f;
            float needed = XP_PER_LEVEL[lv] - XP_PER_LEVEL[lv - 1];
            float have   = TotalXP - XP_PER_LEVEL[lv - 1];
            return Mathf.Clamp01(have / needed);
        }

        /// <summary>XP richiesto per il prossimo livello (o -1 se già al max).</summary>
        public float XPToNextLevel()
        {
            int lv = PlayerLevel;
            if (lv >= XP_PER_LEVEL.Length) return -1f;
            return XP_PER_LEVEL[lv] - TotalXP;
        }

        /// <summary>Resetta TUTTI i dati (per debug/reset gioco).</summary>
        public void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            Debug.LogWarning("[PlayerProfile] Tutti i dati azzerati.");
        }

        /// <summary>Nome rango arena corrente.</summary>
        public string ArenaRankName => ARENA_RANK_NAMES[Mathf.Clamp(ArenaRank, 0, ARENA_RANK_NAMES.Length - 1)];
    }
}
