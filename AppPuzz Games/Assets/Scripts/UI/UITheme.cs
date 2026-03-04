// ============================================================
// UITheme.cs
// Costanti visive stile Pokémon: colori, font, animazioni.
// Usato da tutti gli screen per un look coerente ed epico.
// ============================================================

using UnityEngine;

namespace AppPuzz.UI
{
    /// <summary>
    /// Palette e costanti visive ispirate all'estetica Pokémon.
    /// Usa UITheme.Colors.* e UITheme.Anim.* in tutti gli screen.
    /// </summary>
    public static class UITheme
    {
        // ----------------------------------------------------------
        // Palette principale
        // ----------------------------------------------------------
        public static class Colors
        {
            // Sfondi
            public static readonly Color BackgroundDeep    = HEX("#0D0D1A"); // Blu notte profondo
            public static readonly Color BackgroundPanel   = HEX("#1A1A2E"); // Pannello scuro
            public static readonly Color BackgroundCard    = HEX("#16213E"); // Card

            // Accenti oro/giallo Pokémon
            public static readonly Color Gold              = HEX("#FFD700");
            public static readonly Color GoldDark          = HEX("#B8860B");
            public static readonly Color GoldGlow          = HEX("#FFF44F");

            // Tipi creatura (ispirazione tipi Pokémon)
            public static readonly Color TypeDragon        = HEX("#6F35FC"); // Drago - viola
            public static readonly Color TypeWolf          = HEX("#4F94FF"); // Lupo - blu freddo
            public static readonly Color TypeSerpent       = HEX("#2ECC71"); // Serpente - verde

            // UI funzionale
            public static readonly Color TextPrimary       = HEX("#FFFFFF");
            public static readonly Color TextSecondary     = HEX("#B0B8D0");
            public static readonly Color TextAccent        = HEX("#FFD700");
            public static readonly Color TextDanger        = HEX("#FF4757");
            public static readonly Color TextSuccess       = HEX("#2ECC71");
            public static readonly Color TextLegendary     = HEX("#FF6B6B");

            // Bordi / outline
            public static readonly Color BorderGold        = HEX("#FFD700");
            public static readonly Color BorderBlue        = HEX("#4F94FF");
            public static readonly Color BorderPurple      = HEX("#6F35FC");

            // Barre energia / HP
            public static readonly Color EnergyFull        = HEX("#2ECC71");
            public static readonly Color EnergyMid         = HEX("#F1C40F");
            public static readonly Color EnergyLow         = HEX("#E74C3C");

            // Bottoni
            public static readonly Color ButtonPrimary     = HEX("#FFD700");
            public static readonly Color ButtonSecondary   = HEX("#2C3E6B");
            public static readonly Color ButtonDanger      = HEX("#C0392B");
            public static readonly Color ButtonDisabled    = HEX("#4A4A6A");

            // Overlay
            public static readonly Color Overlay           = new Color(0f, 0f, 0f, 0.75f);
            public static readonly Color OverlayLight      = new Color(0f, 0f, 0f, 0.4f);

            // Leggendario / Speciale
            public static readonly Color Legendary         = HEX("#FF6B6B");
            public static readonly Color LegendaryGlow     = HEX("#FFB347");
            public static readonly Color Shiny             = HEX("#F0E68C");

            private static Color HEX(string hex)
            {
                ColorUtility.TryParseHtmlString(hex, out Color c);
                return c;
            }
        }

        // ----------------------------------------------------------
        // Costanti animazioni
        // ----------------------------------------------------------
        public static class Anim
        {
            public const float FadeInDuration      = 0.35f;
            public const float FadeOutDuration     = 0.25f;
            public const float PanelSlideDuration  = 0.4f;
            public const float ButtonPressDuration = 0.1f;
            public const float EvolveFlashDuration = 0.8f;
            public const float PulseDuration       = 0.6f;
            public const float StreakBurstDuration  = 0.5f;
        }

        // ----------------------------------------------------------
        // Costanti layout
        // ----------------------------------------------------------
        public static class Layout
        {
            public const float CornerRadius    = 12f;
            public const float BorderWidth     = 2f;
            public const float CardPadding     = 16f;
            public const float ButtonHeight    = 56f;
            public const float IconSize        = 48f;
            public const float LargeIconSize   = 96f;
        }

        // ----------------------------------------------------------
        // Stringhe UI (i18n semplificata)
        // ----------------------------------------------------------
        public static class Strings
        {
            public const string AppTitle         = "WORD LEGEND";
            public const string PlayBtn          = "GIOCA";
            public const string TrainingBtn      = "ALLENAMENTO";
            public const string ArenaBtn         = "ARENA";
            public const string EvolutionBtn     = "EVOLUZIONE";
            public const string SettingsBtn      = "IMPOSTAZIONI";
            public const string PlayAgainBtn     = "GIOCA ANCORA";
            public const string MenuBtn          = "MENU";
            public const string SelectCreature   = "SCEGLI LA TUA CREATURA";
            public const string EvolveBtn        = "EVOLVI!";
            public const string Legendary        = "✦ LEGGENDARIO ✦";
            public const string WordValid        = "PAROLA VALIDA!";
            public const string WordInvalid      = "Parola non trovata";
            public const string TimesUp          = "TEMPO SCADUTO!";
            public const string NewRecord        = "✦ NUOVO RECORD ✦";
        }

        // ----------------------------------------------------------
        // Metodo helper: colore per tipo creatura
        // ----------------------------------------------------------
        public static Color CreatureTypeColor(string creatureType)
        {
            return creatureType switch
            {
                "MentalDragon"    => Colors.TypeDragon,
                "AstralWolf"      => Colors.TypeWolf,
                "EtherealSerpent" => Colors.TypeSerpent,
                _                 => Colors.Gold,
            };
        }

        /// <summary>Calcola il colore della barra energia (verde→giallo→rosso).</summary>
        public static Color EnergyBarColor(float normalized)
        {
            if (normalized > 0.5f)
                return Color.Lerp(Colors.EnergyMid,  Colors.EnergyFull, (normalized - 0.5f) * 2f);
            return Color.Lerp(Colors.EnergyLow, Colors.EnergyMid,  normalized * 2f);
        }
    }
}
