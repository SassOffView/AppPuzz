// ============================================================
// UITheme.cs  — Palette visiva Word Legend
// Stile ibrido: vibranza Word Domination + creature Pokémon
// ============================================================

using UnityEngine;

namespace AppPuzz.UI
{
    public static class UITheme
    {
        // ----------------------------------------------------------
        // Palette principale
        // ----------------------------------------------------------
        public static class Colors
        {
            // ── Sfondi dark (gameplay / pannelli) ──────────────────
            public static readonly Color BackgroundDeep  = HEX("#060E1C"); // quasi nero-blu
            public static readonly Color BackgroundPanel = HEX("#0D1B3E"); // navy in-game
            public static readonly Color BackgroundCard  = HEX("#1B2F5E"); // card navy

            // ── Sky blue (home / schermate principali) ─────────────
            public static readonly Color SkyBlue        = HEX("#1BADE0"); // cielo WD
            public static readonly Color SkyBlueBright  = HEX("#4DD3F7"); // cielo chiaro
            public static readonly Color SkyBlueDark    = HEX("#0A7BA8"); // cielo scuro
            public static readonly Color TealDark       = HEX("#005F80"); // teal fondo

            // ── Tessere griglia (stile Word Domination) ────────────
            public static readonly Color TileNormal     = HEX("#FFFFFF"); // tile bianca
            public static readonly Color TileBorder     = HEX("#E09010"); // bordo amber
            public static readonly Color TileBorderSel  = HEX("#FF8C00"); // bordo selezionato
            public static readonly Color TileSelected   = HEX("#FFD040"); // tile selezionata
            public static readonly Color TileText       = HEX("#12186A"); // lettera navy scura
            public static readonly Color TileShadow     = HEX("#A06000"); // ombra bordo

            // ── Oro / Pokémon accent ───────────────────────────────
            public static readonly Color Gold           = HEX("#FFD700");
            public static readonly Color GoldDark       = HEX("#B8860B");
            public static readonly Color GoldGlow       = HEX("#FFF44F");

            // ── Tipi creatura ──────────────────────────────────────
            public static readonly Color TypeDragon     = HEX("#7B2FFF"); // viola drago
            public static readonly Color TypeWolf       = HEX("#2B8EFF"); // blu lupo
            public static readonly Color TypeSerpent    = HEX("#1EC87C"); // verde serpente

            // ── Bottoni 3D (stile WD) ──────────────────────────────
            public static readonly Color BtnGreen       = HEX("#3CC244"); // verde gioca
            public static readonly Color BtnGreenDark   = HEX("#217A2B"); // ombra verde
            public static readonly Color BtnGreenText   = HEX("#FFFFFF");
            public static readonly Color BtnPurple      = HEX("#8B4EE8"); // viola eventi
            public static readonly Color BtnPurpleDark  = HEX("#4E2599"); // ombra viola
            public static readonly Color BtnOrange      = HEX("#F57C00"); // arancio allenamento
            public static readonly Color BtnOrangeDark  = HEX("#9E4A00");

            // ── Resource bar ───────────────────────────────────────
            public static readonly Color ResourceBg     = HEX("#004F7A");
            public static readonly Color EnergyColor    = HEX("#FF6D00"); // arancio energia
            public static readonly Color GemColor       = HEX("#E91E8C"); // fucsia gemme
            public static readonly Color CoinColor      = HEX("#FFD700"); // oro monete

            // ── Pop-up / modal (stile viola WD) ────────────────────
            public static readonly Color PopupBg        = HEX("#5B2DAA");
            public static readonly Color PopupBorder    = HEX("#9B52FF");
            public static readonly Color PopupInner     = HEX("#3A1A7A");
            public static readonly Color PopupCTA       = HEX("#3CC244"); // verde CTA

            // ── Nav bar ────────────────────────────────────────────
            public static readonly Color NavBarBg       = HEX("#00214A");
            public static readonly Color NavBarActive   = HEX("#FFD700");

            // ── Card con bordo ─────────────────────────────────────
            public static readonly Color CardBg         = HEX("#1A3A6B");
            public static readonly Color CardBorderGold = HEX("#E8A000");
            public static readonly Color CardBorderTeal = HEX("#00B8D9");
            public static readonly Color CardBorderPurp = HEX("#9B52FF");

            // ── UI funzionale ──────────────────────────────────────
            public static readonly Color TextPrimary    = HEX("#FFFFFF");
            public static readonly Color TextSecondary  = HEX("#B0C8E8");
            public static readonly Color TextAccent     = HEX("#FFD700");
            public static readonly Color TextDanger     = HEX("#FF4757");
            public static readonly Color TextSuccess    = HEX("#2ECC71");
            public static readonly Color TextDark       = HEX("#12186A"); // testo su sfondo chiaro
            public static readonly Color TextLegendary  = HEX("#FF9F43");

            // ── Bordi ─────────────────────────────────────────────
            public static readonly Color BorderGold     = HEX("#FFD700");
            public static readonly Color BorderBlue     = HEX("#4F94FF");
            public static readonly Color BorderPurple   = HEX("#6F35FC");

            // ── Barre HP/energia ───────────────────────────────────
            public static readonly Color EnergyFull     = HEX("#3CC244");
            public static readonly Color EnergyMid      = HEX("#F1C40F");
            public static readonly Color EnergyLow      = HEX("#E74C3C");
            public static readonly Color EnergyBarBg    = HEX("#1A3050");

            // ── Bottoni legacy (compatibilità) ─────────────────────
            public static readonly Color ButtonPrimary  = HEX("#FFD700");
            public static readonly Color ButtonSecondary= HEX("#1A3A6B");
            public static readonly Color ButtonDanger   = HEX("#C0392B");
            public static readonly Color ButtonDisabled = HEX("#4A4A6A");

            // ── Overlay ────────────────────────────────────────────
            public static readonly Color Overlay        = new Color(0f, 0f, 0f, 0.80f);
            public static readonly Color OverlayLight   = new Color(0f, 0f, 0f, 0.45f);

            // ── Leggendario ────────────────────────────────────────
            public static readonly Color Legendary      = HEX("#FF9F43");
            public static readonly Color LegendaryGlow  = HEX("#FFD700");
            public static readonly Color Shiny          = HEX("#F0E68C");

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
            public const float FadeInDuration      = 0.30f;
            public const float FadeOutDuration     = 0.20f;
            public const float PanelSlideDuration  = 0.35f;
            public const float ButtonPressDuration = 0.08f;
            public const float EvolveFlashDuration = 0.8f;
            public const float PulseDuration       = 1.8f;  // idle creature pulse
            public const float StreakBurstDuration  = 0.5f;
            public const float TileSelectScale     = 1.12f; // tile scale when selected
        }

        // ----------------------------------------------------------
        // Layout
        // ----------------------------------------------------------
        public static class Layout
        {
            public const float CornerRadius    = 12f;
            public const float BorderWidth     = 4f;   // tile border
            public const float CardPadding     = 14f;
            public const float ButtonHeight    = 64f;
            public const float IconSize        = 52f;
            public const float LargeIconSize   = 96f;
            public const float NavBarHeight    = 0.09f;
            public const float ResourceBarH    = 0.075f;
        }

        // ----------------------------------------------------------
        // Stringhe UI
        // ----------------------------------------------------------
        public static class Strings
        {
            public const string AppTitle       = "WORD LEGEND";
            public const string PlayBtn        = "GIOCA";
            public const string TrainingBtn    = "ALLENAMENTO";
            public const string ArenaBtn       = "ARENA";
            public const string EvolutionBtn   = "EVOLUZIONE";
            public const string SettingsBtn    = "IMPOSTAZIONI";
            public const string PlayAgainBtn   = "GIOCA ANCORA";
            public const string MenuBtn        = "MENU";
            public const string SelectCreature = "SCEGLI LA TUA CREATURA";
            public const string EvolveBtn      = "EVOLVI!";
            public const string Legendary      = "★ LEGGENDARIO ★";
            public const string WordValid      = "PAROLA VALIDA!";
            public const string WordInvalid    = "Parola non trovata";
            public const string TimesUp        = "TEMPO SCADUTO!";
            public const string NewRecord      = "★ NUOVO RECORD ★";
        }

        // ----------------------------------------------------------
        // Helper: colore tipo creatura
        // ----------------------------------------------------------
        public static Color CreatureTypeColor(string creatureType) => creatureType switch
        {
            "MentalDragon"    => Colors.TypeDragon,
            "AstralWolf"      => Colors.TypeWolf,
            "EtherealSerpent" => Colors.TypeSerpent,
            _                 => Colors.Gold,
        };

        /// <summary>Calcola il colore della barra energia (verde→giallo→rosso).</summary>
        public static Color EnergyBarColor(float normalized)
        {
            if (normalized > 0.5f)
                return Color.Lerp(Colors.EnergyMid, Colors.EnergyFull, (normalized - 0.5f) * 2f);
            return Color.Lerp(Colors.EnergyLow, Colors.EnergyMid, normalized * 2f);
        }
    }
}
