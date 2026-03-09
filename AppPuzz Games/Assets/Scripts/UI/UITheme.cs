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
            public static readonly Color BackgroundDeep  = HEX("#0A0F1E"); // quasi nero-blu (più caldo)
            public static readonly Color BackgroundPanel = HEX("#141E33"); // navy-ardesia
            public static readonly Color BackgroundCard  = HEX("#1B2F5E"); // card navy

            // ── Sky blue (home / schermate principali) ─────────────
            public static readonly Color SkyBlue        = HEX("#18B5E8"); // cielo luminoso
            public static readonly Color SkyBlueBright  = HEX("#4DD3F7"); // cielo chiaro
            public static readonly Color SkyBlueDark    = HEX("#0A7BA8"); // cielo scuro
            public static readonly Color TealDark       = HEX("#005F80"); // teal fondo

            // ── Tessere griglia (stile Word Domination) ────────────
            public static readonly Color TileNormal     = HEX("#FFFFFF"); // tile bianca
            public static readonly Color TileBorder     = HEX("#C88020"); // bordo ambra Ruzzle
            public static readonly Color TileBorderSel  = HEX("#FF8C00"); // bordo selezionato
            public static readonly Color TileSelected   = HEX("#FFD040"); // tile selezionata
            public static readonly Color TileText       = HEX("#12186A"); // lettera navy scura
            public static readonly Color TileShadow     = HEX("#A06000"); // ombra bordo

            // ── Tessere stile PIETRA INCISA ────────────────────────
            public static readonly Color StoneBase      = HEX("#B8A890"); // superficie pietra (arenaria)
            public static readonly Color StoneBorder    = HEX("#4E3C2C"); // cornice roccia scura
            public static readonly Color StoneShadow    = HEX("#2C1C0C"); // base sottostante
            public static readonly Color StoneSelected  = HEX("#D4AA50"); // pietra illuminata / rune attivate
            public static readonly Color StoneBorderSel = HEX("#C8960A"); // bordo dorato selezione
            public static readonly Color StoneText      = HEX("#1A1008"); // lettera incisa (scurissima)
            public static readonly Color StoneHighlight = new Color(0.85f, 0.76f, 0.62f, 0.55f); // bordo incisione
            public static readonly Color StonePointText = HEX("#E8D8B8"); // valore punti (crema pietra)

            // Pietra danneggiata – la superficie si scurisce/sgretola:
            public static readonly Color StoneDmg1     = HEX("#A89070"); // leggera usura
            public static readonly Color StoneDmg2     = HEX("#907858"); // usura moderata
            public static readonly Color StoneDmg3     = HEX("#785040"); // danno pesante
            public static readonly Color StoneDmg4     = HEX("#5A3428"); // danno grave
            public static readonly Color StoneDmg5     = HEX("#3C1E12"); // critico / pietra nera

            // Crepa: ombra scura + highlight frattura (più visibili)
            public static readonly Color CrackDark     = new Color(0.05f, 0.03f, 0.01f, 0.95f);
            public static readonly Color CrackEdge     = new Color(0.85f, 0.76f, 0.60f, 0.55f);

            // ── Oro / Pokémon accent ───────────────────────────────
            public static readonly Color Gold           = HEX("#FFD700");
            public static readonly Color GoldDark       = HEX("#B8860B");
            public static readonly Color GoldGlow       = HEX("#FFF44F");

            // ── Tipi creatura ──────────────────────────────────────
            public static readonly Color TypeDragon     = HEX("#7B2FFF"); // viola drago
            public static readonly Color TypeWolf       = HEX("#2B8EFF"); // blu lupo
            public static readonly Color TypeSerpent    = HEX("#1EC87C"); // verde serpente

            // ── Creature accent (dettagli procedurali) ───────────
            public static readonly Color DragonAccent   = HEX("#B06FFF"); // viola chiaro
            public static readonly Color WolfAccent     = HEX("#70B8FF"); // azzurro chiaro
            public static readonly Color SerpentAccent  = HEX("#60E8A8"); // verde chiaro

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

            // ── Danno tessera (6 livelli: 1=lieve → 5=critico) ────
            public static readonly Color Damage1        = new Color(0.30f, 0.10f, 0.00f, 0.15f);
            public static readonly Color Damage2        = new Color(0.35f, 0.12f, 0.00f, 0.30f);
            public static readonly Color Damage3        = new Color(0.45f, 0.10f, 0.00f, 0.45f);
            public static readonly Color Damage4        = new Color(0.55f, 0.08f, 0.00f, 0.60f);
            public static readonly Color Damage5        = new Color(0.65f, 0.04f, 0.00f, 0.75f);

            // Colori superficie interna per livello danno
            public static readonly Color TileDamage1   = HEX("#FFF4E0");
            public static readonly Color TileDamage2   = HEX("#FFE0B0");
            public static readonly Color TileDamage3   = HEX("#FFC070");
            public static readonly Color TileDamage4   = HEX("#FF9040");
            public static readonly Color TileDamage5   = HEX("#FF5018");

            // ── Tile 2.5D — 5 tier di colore Ruzzle-style ────────────────
            // Tier 1 (1pt) Blu calmo | Tier 2 (2pt) Verde fresco
            // Tier 3 (3pt) Ambra dorato | Tier 4 (4pt) Rosso caldo | Tier 5 (5pt) Viola ricco
            public static readonly Color Tile1Face  = HEX("#5B9BD5"); // blu calmo
            public static readonly Color Tile1Depth = HEX("#2D5A8C"); // blu profondo
            public static readonly Color Tile2Face  = HEX("#6BC76F"); // verde fresco
            public static readonly Color Tile2Depth = HEX("#357A38"); // verde scuro
            public static readonly Color Tile3Face  = HEX("#E8A838"); // ambra dorato
            public static readonly Color Tile3Depth = HEX("#9A6810"); // ambra profondo
            public static readonly Color Tile4Face  = HEX("#D85040"); // rosso caldo
            public static readonly Color Tile4Depth = HEX("#802820"); // rosso profondo
            public static readonly Color Tile5Face  = HEX("#9B6EC8"); // viola ricco
            public static readonly Color Tile5Depth = HEX("#553880"); // viola profondo

            // Selezione (oro brillante Ruzzle-style)
            public static readonly Color TileSelFace  = HEX("#FFD030"); // oro brillante
            public static readonly Color TileSelDepth = HEX("#AA7800"); // oro profondo

            // Freeze 2.5D
            public static readonly Color TileFrozenFace  = HEX("#80D0F0"); // azzurro ghiaccio
            public static readonly Color TileFrozenDepth = HEX("#3080A8"); // azzurro scuro

            // ── Ghiaccio (freeze) ──────────────────────────────────
            public static readonly Color FreezeOverlay = new Color(0.50f, 0.85f, 1.00f, 0.60f);
            public static readonly Color FreezeBorder  = HEX("#A0E8FF");

            // ── Fulmini per tipo creatura ──────────────────────────
            public static readonly Color LightningFire  = HEX("#FF6000"); // fuoco  → arancio
            public static readonly Color LightningWater = HEX("#00BFFF"); // acqua  → azzurro
            public static readonly Color LightningEarth = HEX("#80FF00"); // terra  → verde lime
            public static readonly Color LightningAir   = HEX("#FFFF80"); // aria   → giallo chiaro

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

        /// <summary>Restituisce il colore del fulmine per tipo creatura.</summary>
        public static Color LightningColor(string creatureType) => creatureType switch
        {
            "MentalDragon"    => Colors.LightningFire,
            "AstralWolf"      => Colors.LightningWater,
            "EtherealSerpent" => Colors.LightningEarth,
            _                 => Colors.LightningAir,
        };

        /// <summary>
        /// Restituisce il colore overlay danno per livello (1–5).
        /// Livello 0 = nessun overlay.
        /// </summary>
        public static Color DamageOverlayColor(int level) => level switch
        {
            1 => Colors.Damage1,
            2 => Colors.Damage2,
            3 => Colors.Damage3,
            4 => Colors.Damage4,
            5 => Colors.Damage5,
            _ => Color.clear,
        };

        /// <summary>
        /// Restituisce il colore della superficie stone in base al livello di danno (0=intatta, 5=critica).
        /// </summary>
        public static Color TileSurfaceColor(int level) => level switch
        {
            1 => Colors.StoneDmg1,
            2 => Colors.StoneDmg2,
            3 => Colors.StoneDmg3,
            4 => Colors.StoneDmg4,
            5 => Colors.StoneDmg5,
            _ => Colors.StoneBase,
        };

        /// <summary>Calcola il colore della barra energia (verde→giallo→rosso).</summary>
        public static Color EnergyBarColor(float normalized)
        {
            if (normalized > 0.5f)
                return Color.Lerp(Colors.EnergyMid, Colors.EnergyFull, (normalized - 0.5f) * 2f);
            return Color.Lerp(Colors.EnergyLow, Colors.EnergyMid, normalized * 2f);
        }

        // ----------------------------------------------------------
        // Tile 2.5D colors helper
        // ----------------------------------------------------------

        /// <summary>Coppia di colori per una tessera 2.5D (faccia + profondità).</summary>
        public struct TileColors
        {
            public Color Face;   // superficie principale
            public Color Depth;  // lato inferiore/destro (effetto 3D)
        }

        /// <summary>
        /// Restituisce i colori di una tessera in base al tier (1–5),
        /// allo stato selezionato e allo stato frozen.
        /// </summary>
        public static TileColors GetTileColors(int tier, bool selected, bool frozen)
        {
            if (frozen)
                return new TileColors { Face = Colors.TileFrozenFace, Depth = Colors.TileFrozenDepth };
            if (selected)
                return new TileColors { Face = Colors.TileSelFace, Depth = Colors.TileSelDepth };
            return tier switch
            {
                2 => new TileColors { Face = Colors.Tile2Face, Depth = Colors.Tile2Depth },
                3 => new TileColors { Face = Colors.Tile3Face, Depth = Colors.Tile3Depth },
                4 => new TileColors { Face = Colors.Tile4Face, Depth = Colors.Tile4Depth },
                5 => new TileColors { Face = Colors.Tile5Face, Depth = Colors.Tile5Depth },
                _ => new TileColors { Face = Colors.Tile1Face, Depth = Colors.Tile1Depth },
            };
        }
    }
}
