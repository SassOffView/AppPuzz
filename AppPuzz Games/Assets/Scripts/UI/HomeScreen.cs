// ============================================================
// HomeScreen.cs
// Schermata principale stile Pokémon:
// - Mostra creatura scelta con animazione idle
// - Pulsanti: Gioca, Allenamento, Arena, Evoluzione, Impostazioni
// - Statistiche giocatore (livello, XP, rango arena)
// - Daily challenge badge
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using AppPuzz.Utils;
using AppPuzz.Creatures;

namespace AppPuzz.UI
{
    /// <summary>Controller della schermata Home.</summary>
    public class HomeScreen : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti UI (da assegnare nell'Inspector)
        // ----------------------------------------------------------

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI playerLevelText;
        public Slider          xpBar;
        public TextMeshProUGUI xpText;

        [Header("Creatura")]
        public Image           creatureImage;
        public TextMeshProUGUI creatureNameText;
        public TextMeshProUGUI creatureTypeText;
        public Image           creatureTypeBadge;

        [Header("Statistiche")]
        public TextMeshProUGUI bestScoreText;
        public TextMeshProUGUI totalMatchesText;
        public TextMeshProUGUI arenaRankText;
        public Image           arenaRankIcon;

        [Header("Pulsanti navigazione")]
        public Button playButton;
        public Button trainingButton;
        public Button arenaButton;
        public Button evolutionButton;
        public Button settingsButton;

        [Header("Daily Challenge")]
        public GameObject      dailyChallengePanel;
        public TextMeshProUGUI dailyChallengeText;
        public Button          dailyChallengeButton;

        [Header("Notifiche")]
        public GameObject      evolutionNotificationBadge;  // badge "Puoi evolvere!"

        // ----------------------------------------------------------
        // Colori tipi (sprite badge)
        // ----------------------------------------------------------
        private static readonly Color[] TYPE_COLORS =
        {
            UITheme.Colors.TypeDragon,
            UITheme.Colors.TypeWolf,
            UITheme.Colors.TypeSerpent
        };
        private static readonly string[] CREATURE_NAMES =
        {
            "Mental Dragon",
            "Astral Wolf",
            "Ethereal Serpent"
        };
        private static readonly string[] CREATURE_TYPE_LABELS =
        {
            "DRAGO MENTALE",
            "LUPO ASTRALE",
            "SERPENTE ETEREO"
        };

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void OnEnable()
        {
            RefreshUI();
        }

        private void Start()
        {
            WireButtons();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void WireButtons()
        {
            playButton?.onClick.AddListener(OnPlay);
            trainingButton?.onClick.AddListener(OnTraining);
            arenaButton?.onClick.AddListener(OnArena);
            evolutionButton?.onClick.AddListener(OnEvolution);
            settingsButton?.onClick.AddListener(OnSettings);
            dailyChallengeButton?.onClick.AddListener(OnDailyChallenge);
        }

        /// <summary>Aggiorna tutti i valori UI dal PlayerProfile.</summary>
        private void RefreshUI()
        {
            PlayerProfile p = PlayerProfile.Instance;
            if (p == null) return;

            // ---- Header ----
            if (titleText      != null) titleText.text      = UITheme.Strings.AppTitle;
            if (playerLevelText!= null) playerLevelText.text = $"LV. {p.PlayerLevel}";
            if (xpBar          != null) xpBar.value         = p.LevelProgress();
            if (xpText         != null)
            {
                float toNext = p.XPToNextLevel();
                xpText.text = toNext >= 0 ? $"{toNext:F0} XP al prossimo livello" : "MAX LEVEL";
            }

            // ---- Creatura ----
            int creatureIdx = (int)p.SelectedCreature;
            if (creatureNameText != null) creatureNameText.text = CREATURE_NAMES[creatureIdx];
            if (creatureTypeText != null) creatureTypeText.text = CREATURE_TYPE_LABELS[creatureIdx];
            if (creatureTypeBadge != null) creatureTypeBadge.color = TYPE_COLORS[creatureIdx];

            // ---- Statistiche ----
            if (bestScoreText   != null) bestScoreText.text    = $"Record: {p.BestScore:F0}";
            if (totalMatchesText!= null) totalMatchesText.text = $"Partite: {p.TotalMatches}";
            if (arenaRankText   != null) arenaRankText.text    = p.ArenaRankName.ToUpper();

            // ---- Daily Challenge ----
            bool dailyAvailable = IsDailyAvailable();
            if (dailyChallengePanel  != null) dailyChallengePanel.SetActive(dailyAvailable);
            if (dailyChallengeText   != null && dailyAvailable)
                dailyChallengeText.text = "Sfida del giorno disponibile!";

            // ---- Notifica evoluzione ----
            bool canEvolve = p.CurrentXP >= 50f;   // soglia semplice; CreatureController ha i threshold reali
            if (evolutionNotificationBadge != null) evolutionNotificationBadge.SetActive(canEvolve);
        }

        private bool IsDailyAvailable()
        {
            string last = PlayerPrefs.GetString("last_daily", "");
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            return last != today;
        }

        // ---- Handler pulsanti ----
        private void OnPlay()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Gameplay);
        }

        private void OnTraining()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Training);
        }

        private void OnArena()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Arena);
        }

        private void OnEvolution()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Evolution);
        }

        private void OnSettings()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Settings);
        }

        private void OnDailyChallenge()
        {
            PlayerPrefs.SetString("last_daily", DateTime.Today.ToString("yyyy-MM-dd"));
            ScreenManager.Instance?.ShowScreen(ScreenID.Gameplay);
        }
    }
}
