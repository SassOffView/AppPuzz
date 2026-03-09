// ============================================================
// UIAutoSetup.cs
// Costruisce e stila TUTTA la UI automaticamente a runtime.
// Aggiungilo a un GameObject "UIAutoSetup" nella scena.
// Non devi configurare nulla dall'Inspector: fa tutto da solo.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Utils;
using AppPuzz.Creatures;
using AppPuzz.Localization;
using AppPuzz.Gameplay;

namespace AppPuzz.UI
{
    public class UIAutoSetup : MonoBehaviour
    {
        // -------------------------------------------------------
        // Awake: costruisce tutto prima che gli altri script
        // facciano i loro Start/OnEnable
        // -------------------------------------------------------
        private void Awake()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("[UIAutoSetup] Canvas non trovato!"); return; }

            EnsureManagers();
            EnsureEventSystem();
            EnsureCanvasInput(canvas);

            BuildHomePanel(canvas);
            BuildOnboardingPanel(canvas);
            BuildCreatureSelectionPanel(canvas);
            BuildTrainingPanel(canvas);
            BuildArenaPanel(canvas);
            BuildEvolutionPanel(canvas);
            BuildSettingsPanel(canvas);
            BuildTransitionOverlay(canvas);
            WireScreenManager(canvas);
            DisableStrayBlockers(canvas);

            Debug.Log("[UIAutoSetup] UI costruita automaticamente.");
        }

        // -------------------------------------------------------
        // Manager globali (PlayerProfile, AppNavigator, ScreenManager)
        // -------------------------------------------------------
        private void EnsureManagers()
        {
            if (PlayerProfile.Instance == null)
            {
                var go = new GameObject("PlayerProfile");
                go.AddComponent<PlayerProfile>();
            }
            if (FindFirstObjectByType<ScreenManager>() == null)
            {
                var go = new GameObject("ScreenManager");
                go.AddComponent<ScreenManager>();
            }
            if (AppNavigator.Instance == null)
            {
                var go = new GameObject("AppNavigator");
                go.AddComponent<AppNavigator>();
            }
            if (AppPuzz.Audio.AudioManager.Instance == null)
            {
                var go = new GameObject("AudioManager");
                go.AddComponent<AppPuzz.Audio.AudioManager>();
            }
        }

        // -------------------------------------------------------
        // EventSystem
        // -------------------------------------------------------
        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
                Debug.LogWarning("[UIAutoSetup] EventSystem mancante — creato automaticamente. " +
                                 "Senza EventSystem i click UI non funzionano!");
            }
        }

        // -------------------------------------------------------
        // GraphicRaycaster + Canvas render mode
        // -------------------------------------------------------
        private static void EnsureCanvasInput(Canvas canvas)
        {
            // GraphicRaycaster obbligatorio per i click UI
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.LogWarning("[UIAutoSetup] GraphicRaycaster mancante sul Canvas — aggiunto. " +
                                 "Senza di esso nessun click UI viene processato!");
            }

            // Forza Screen Space Overlay: non richiede Camera ed è il più affidabile
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning($"[UIAutoSetup] Canvas renderMode={canvas.renderMode}. " +
                                 $"Impostato su ScreenSpaceOverlay per garantire il funzionamento dei click.");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            Debug.Log($"[UIAutoSetup] Canvas OK — renderMode={canvas.renderMode}, " +
                      $"GraphicRaycaster presente={canvas.GetComponent<GraphicRaycaster>() != null}");
        }

        // -------------------------------------------------------
        // Pannelli "trasparenti" non gestiti che bloccano i click
        // -------------------------------------------------------
        private static void DisableStrayBlockers(Canvas canvas)
        {
            var managed = new HashSet<string>
            {
                "HomePanel", "OnboardingPanel", "CreatureSelectionPanel",
                "TrainingPanel", "ArenaPanel", "EvolutionPanel",
                "GameplayPanel", "TransitionOverlay"
            };

            var log = new System.Text.StringBuilder("[UIAutoSetup] Gerarchia Canvas:\n");
            foreach (Transform child in canvas.transform)
            {
                log.AppendLine($"  • {child.name}  active={child.gameObject.activeSelf}");

                if (managed.Contains(child.name)) continue;
                if (!child.gameObject.activeSelf) continue;

                // Cerca Image fullscreen con raycastTarget=true → può bloccare i click
                var img = child.GetComponent<Image>();
                if (img == null || !img.raycastTarget) continue;

                var rt = child.GetComponent<RectTransform>();
                if (rt == null) continue;

                bool isFullScreen =
                    rt.anchorMin == Vector2.zero  &&
                    rt.anchorMax == Vector2.one   &&
                    rt.offsetMin == Vector2.zero  &&
                    rt.offsetMax == Vector2.zero;

                if (isFullScreen)
                {
                    img.raycastTarget = false;
                    Debug.LogWarning($"[UIAutoSetup] Pannello NON GESTITO '{child.name}' copriva " +
                                     $"l'intera schermata con raycastTarget=true. " +
                                     $"Disabilitato. Verifica se puoi eliminarlo dalla scena.");
                }
            }
            Debug.Log(log.ToString());
        }

        // -------------------------------------------------------
        // Helpers base
        // -------------------------------------------------------
        private static GameObject FindOrCreatePanel(Canvas canvas, string name)
        {
            var existing = canvas.transform.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = UITheme.Colors.BackgroundPanel;
            return go;
        }

        private static GameObject MakePanel(Transform parent, string name, Color bgColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, string text,
            float fontSize, Color color, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            Color bgColor, Color textColor, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();

            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor      = bgColor;
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
            colors.pressedColor     = Color.Lerp(bgColor, Color.black, 0.2f);
            btn.colors = colors;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.color = textColor; tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            btn.onClick.AddListener(AppPuzz.Audio.AudioManager.PlayClick);
            return btn;
        }

        private static Slider MakeSlider(Transform parent, string name, Color fillColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = UITheme.Colors.EnergyBarBg;

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
            faRt.offsetMin = new Vector2(5, 0); faRt.offsetMax = new Vector2(-5, 0);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 0.5f;

            return slider;
        }

        /// <summary>Crea un TMP_InputField con struttura corretta (viewport + placeholder).</summary>
        private static TMPro.TMP_InputField MakeTMPInputField(Transform parent, string name,
            float fontSize, Color textColor, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = anchorMin; rootRt.anchorMax = anchorMax;
            rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = bgColor;
            var field = root.AddComponent<TMPro.TMP_InputField>();

            // Text Area (viewport con clipping)
            var areaGo = new GameObject("Text Area");
            areaGo.transform.SetParent(root.transform, false);
            var areaRt = areaGo.AddComponent<RectTransform>();
            areaRt.anchorMin = Vector2.zero; areaRt.anchorMax = Vector2.one;
            areaRt.offsetMin = new Vector2(8, 4); areaRt.offsetMax = new Vector2(-8, -4);
            areaGo.AddComponent<RectMask2D>();

            // Placeholder
            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(areaGo.transform, false);
            var phRt = phGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            phRt.offsetMin = phRt.offsetMax = Vector2.zero;
            var phTxt = phGo.AddComponent<TMPro.TextMeshProUGUI>();
            phTxt.text = "Inserisci nome...";
            phTxt.fontSize = fontSize;
            phTxt.color = new Color(textColor.r, textColor.g, textColor.b, 0.4f);
            phTxt.alignment = TextAlignmentOptions.MidlineLeft;
            phTxt.fontStyle = FontStyles.Italic;

            // Input Text
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(areaGo.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            var inputTxt = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
            inputTxt.fontSize = fontSize;
            inputTxt.color = textColor;
            inputTxt.alignment = TextAlignmentOptions.MidlineLeft;

            field.textViewport   = areaRt;
            field.textComponent  = inputTxt;
            field.placeholder    = phTxt;
            field.characterLimit = 16;
            field.interactable   = true;
            field.contentType    = TMPro.TMP_InputField.ContentType.Standard;

            return field;
        }

        /// <summary>
        /// Crea un'area scorrevole (ScrollRect + ScrollBar) per la lista parole trovabili.
        /// Restituisce il TextMeshProUGUI interno da popolare con il testo.
        /// Font 4× rispetto al precedente valore di 26 → 104.
        /// </summary>
        private static TextMeshProUGUI MakeScrollableWordsList(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            // Pannello sfondo
            var bgGo = new GameObject("ScrollArea");
            bgGo.transform.SetParent(parent, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = anchorMin; bgRt.anchorMax = anchorMax;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bgGo.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.18f, 1f);

            // ScrollRect
            var sr = bgGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical   = true;

            // Viewport
            var vpGo = new GameObject("Viewport");
            vpGo.transform.SetParent(bgGo.transform, false);
            var vpRt = vpGo.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(4, 4); vpRt.offsetMax = new Vector2(-4, -4);
            vpGo.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            vpGo.AddComponent<Mask>().showMaskGraphic = false;
            sr.viewport = vpRt;

            // Content (si espande verso il basso man mano che il testo cresce)
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(vpGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.offsetMin = contentRt.offsetMax = Vector2.zero;
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = contentRt;

            // TextMeshProUGUI
            var txtGo = new GameObject("WordsText");
            txtGo.transform.SetParent(contentGo.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(8, 8); txtRt.offsetMax = new Vector2(-8, -8);
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize  = 104;   // 4× l'originale 26
            txt.color     = UITheme.Colors.TextPrimary;
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.textWrappingMode = TMPro.TextWrappingModes.Normal;

            // Content size fitter reagisce al testo
            var txtCsf = txtGo.AddComponent<ContentSizeFitter>();
            txtCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return txt;
        }

        // -------------------------------------------------------
        // HOME PANEL
        // -------------------------------------------------------
        private void BuildHomePanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "HomePanel");
            if (panel.GetComponent<HomeScreen>() != null) return;

            panel.SetActive(false);
            // Sfondo sky blue (come WD loading screen)
            panel.GetComponent<Image>().color = UITheme.Colors.SkyBlue;
            var hs = panel.AddComponent<HomeScreen>();

            // ══════════════════════════════════════════════════════
            // TOP BAR RISORSE  (y 0.925 – 1.00)
            // ══════════════════════════════════════════════════════
            var resBar = MakePanel(panel.transform, "ResourceBar",
                UITheme.Colors.SkyBlueDark,
                new Vector2(0, 0.928f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);

            // ⚡ Energia
            MakeResourceChip(resBar.transform, "EnergyChip",
                "⚡", "60/60", UITheme.Colors.ResourceBg, UITheme.Colors.EnergyColor,
                new Vector2(0.01f, 0.08f), new Vector2(0.32f, 0.92f));

            // 💎 Gemme
            MakeResourceChip(resBar.transform, "GemChip",
                "◆", "110", UITheme.Colors.ResourceBg, UITheme.Colors.GemColor,
                new Vector2(0.34f, 0.08f), new Vector2(0.64f, 0.92f));

            // 🪙 Monete
            MakeResourceChip(resBar.transform, "CoinChip",
                "●", "5.960", UITheme.Colors.ResourceBg, UITheme.Colors.CoinColor,
                new Vector2(0.66f, 0.08f), new Vector2(0.99f, 0.92f));

            // ══════════════════════════════════════════════════════
            // PLAYER INFO STRIP  (y 0.860 – 0.928)
            // ══════════════════════════════════════════════════════
            var playerStrip = MakePanel(panel.transform, "PlayerStrip",
                UITheme.Colors.SkyBlueDark,
                new Vector2(0, 0.860f), new Vector2(1, 0.928f), Vector2.zero, Vector2.zero);

            // Titolo/nome app centrato
            hs.titleText = MakeText(playerStrip.transform, "TitleText", UITheme.Strings.AppTitle,
                42, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.2f, 0), new Vector2(0.8f, 1), Vector2.zero, Vector2.zero);
            hs.titleText.fontStyle = FontStyles.Bold;

            // Badge livello (sinistra)
            var lvBadge = MakePanel(playerStrip.transform, "LvBadge",
                UITheme.Colors.NavBarBg,
                new Vector2(0.01f, 0.08f), new Vector2(0.18f, 0.92f), Vector2.zero, Vector2.zero);
            hs.playerLevelText = MakeText(lvBadge.transform, "LvText", "⭐ LV.1",
                28, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Badge rango (destra)
            var rankBadge = MakePanel(playerStrip.transform, "RankBadge",
                UITheme.Colors.NavBarBg,
                new Vector2(0.82f, 0.08f), new Vector2(0.99f, 0.92f), Vector2.zero, Vector2.zero);
            hs.arenaRankText = MakeText(rankBadge.transform, "RankText", "🏆 BRONZO",
                24, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // ══════════════════════════════════════════════════════
            // CREATURA  (y 0.300 – 0.860)  ← area principale
            // ══════════════════════════════════════════════════════
            // Sky blue sfumato (pannello + layer scuro in basso)
            var creatureZone = MakePanel(panel.transform, "CreatureZone",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.300f), new Vector2(1, 0.860f), Vector2.zero, Vector2.zero);

            // Gradiente simulato: pannello semi-trasparente in basso
            var gradFade = MakePanel(creatureZone.transform, "GradFade",
                new Color(0.04f, 0.16f, 0.36f, 0.70f),
                new Vector2(0, 0), new Vector2(1, 0.30f), Vector2.zero, Vector2.zero);

            // Aura glow (cerchio grande semitrasparente dietro creatura)
            var aura = new GameObject("CreatureAura");
            aura.transform.SetParent(creatureZone.transform, false);
            var auraRt = aura.AddComponent<RectTransform>();
            auraRt.anchorMin = new Vector2(0.10f, 0.12f);
            auraRt.anchorMax = new Vector2(0.90f, 0.92f);
            auraRt.offsetMin = auraRt.offsetMax = Vector2.zero;
            var auraImg = aura.AddComponent<Image>();
            auraImg.color = new Color(1f, 0.85f, 0.0f, 0.18f); // glow gold
            // Aggiungi componente di animazione idle
            var idleAnim = aura.AddComponent<CreatureIdleAnimator>();

            // Cerchio creatura (icona colorata per tipo)
            var circle = new GameObject("CreatureCircle");
            circle.transform.SetParent(creatureZone.transform, false);
            var cRt = circle.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.20f, 0.18f);
            cRt.anchorMax = new Vector2(0.80f, 0.86f);
            cRt.offsetMin = cRt.offsetMax = Vector2.zero;
            hs.creatureImage = circle.AddComponent<Image>();
            hs.creatureImage.color = UITheme.Colors.TypeDragon;

            // Lettera grande al centro del cerchio (rappresenta la creatura)
            MakeText(circle.transform, "CreatureIcon", "★",
                110, new Color(1, 1, 1, 0.35f), TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            // Type badge sopra
            var typePill = MakePanel(creatureZone.transform, "TypePill",
                UITheme.Colors.TypeDragon,
                new Vector2(0.28f, 0.87f), new Vector2(0.72f, 0.97f), Vector2.zero, Vector2.zero);
            hs.creatureTypeBadge = typePill.GetComponent<Image>();
            hs.creatureTypeText = MakeText(typePill.transform, "TypeTxt", "✦ DRAGO MENTALE ✦",
                26, Color.white, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            hs.creatureTypeText.fontStyle = FontStyles.Bold;

            // Nome creatura in basso nella zona
            hs.creatureNameText = MakeText(creatureZone.transform, "CreatureName", "Mental Dragon",
                52, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.01f), new Vector2(0.95f, 0.16f), Vector2.zero, Vector2.zero);
            hs.creatureNameText.fontStyle = FontStyles.Bold;

            // Statistiche (in overlay sulla zona)
            hs.bestScoreText = MakeText(creatureZone.transform, "BestScore", "🏆 Record: 0",
                24, UITheme.Colors.TextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.02f, 0.89f), new Vector2(0.45f, 0.97f), Vector2.zero, Vector2.zero);
            hs.totalMatchesText = MakeText(creatureZone.transform, "TotalMatches", "Partite: 0",
                24, UITheme.Colors.TextSecondary, TextAlignmentOptions.Right,
                new Vector2(0.55f, 0.89f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);

            // XP bar sotto il nome
            hs.xpBar = MakeSlider(creatureZone.transform, "XPBar", UITheme.Colors.Gold,
                new Vector2(0.08f, 0.13f), new Vector2(0.92f, 0.18f), Vector2.zero, Vector2.zero);
            hs.xpBar.value = 0.3f;
            hs.xpText = MakeText(creatureZone.transform, "XPText", "350 XP al prossimo livello",
                22, new Color(1,1,1,0.7f), TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.13f), Vector2.zero, Vector2.zero);

            // ══════════════════════════════════════════════════════
            // BOTTONI PRINCIPALI 3D  (y 0.195 – 0.298)
            // ══════════════════════════════════════════════════════
            var btnZone = MakePanel(panel.transform, "MainBtnZone",
                UITheme.Colors.SkyBlueDark,
                new Vector2(0, 0.195f), new Vector2(1, 0.298f), Vector2.zero, Vector2.zero);

            hs.playButton = MakeButton3D(btnZone.transform, "PlayButton", "▶  GIOCA",
                UITheme.Colors.BtnGreen, UITheme.Colors.BtnGreenDark, Color.white, 54,
                new Vector2(0.03f, 0.08f), new Vector2(0.48f, 0.92f));

            hs.arenaButton = MakeButton3D(btnZone.transform, "ArenaButton", "⚡ ARENA",
                UITheme.Colors.BtnPurple, UITheme.Colors.BtnPurpleDark, Color.white, 48,
                new Vector2(0.52f, 0.08f), new Vector2(0.97f, 0.92f));

            // ══════════════════════════════════════════════════════
            // BOTTONI SECONDARI  (y 0.105 – 0.194)
            // ══════════════════════════════════════════════════════
            var secZone = MakePanel(panel.transform, "SecBtnZone",
                new Color(0.04f, 0.14f, 0.28f, 1f),
                new Vector2(0, 0.105f), new Vector2(1, 0.194f), Vector2.zero, Vector2.zero);

            hs.trainingButton = MakeButton3D(secZone.transform, "TrainingButton", "★ ALLENAMENTO",
                UITheme.Colors.BtnOrange, UITheme.Colors.BtnOrangeDark, Color.white, 40,
                new Vector2(0.02f, 0.08f), new Vector2(0.48f, 0.92f));

            hs.evolutionButton = MakeButton3D(secZone.transform, "EvolutionButton", "✦ EVOLUZIONE",
                UITheme.Colors.TypeDragon, UITheme.Colors.BackgroundPanel, Color.white, 40,
                new Vector2(0.52f, 0.08f), new Vector2(0.98f, 0.92f));

            // ══════════════════════════════════════════════════════
            // BOTTOM NAV BAR  (y 0.00 – 0.105)
            // ══════════════════════════════════════════════════════
            MakeNavBar(panel.transform);

            // Settings button (dentro nav bar area — piccolo)
            hs.settingsButton = MakeButton(panel.transform, "SettingsButton", "⚙",
                new Color(0,0,0,0), UITheme.Colors.TextSecondary, 36,
                new Vector2(0.76f, 0.01f), new Vector2(0.98f, UITheme.Layout.NavBarHeight - 0.005f),
                Vector2.zero, Vector2.zero);

            panel.SetActive(false);
        }

        /// <summary>Crea un bottone modalità con icona grande + etichetta sotto.</summary>
        private static Button MakeModeButton(Transform parent, string name,
            string icon, string label, Color bgColor, Color textColor)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rootImg = root.AddComponent<Image>();
            rootImg.color = bgColor;
            var btn = root.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor  = bgColor;
            colors.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
            btn.colors = colors;

            var vert = root.AddComponent<VerticalLayoutGroup>();
            vert.childAlignment = TextAnchor.MiddleCenter;
            vert.childForceExpandWidth  = true;
            vert.childForceExpandHeight = false;
            vert.spacing = 2f;
            vert.padding = new RectOffset(2, 2, 4, 4);

            // Icona
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            var iconTxt = iconGo.AddComponent<TextMeshProUGUI>();
            iconTxt.text = icon;
            iconTxt.fontSize = 52;
            iconTxt.color = textColor;
            iconTxt.alignment = TextAlignmentOptions.Center;
            iconTxt.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredHeight = 70f;

            // Label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            var labelTxt = labelGo.AddComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.fontSize = 22;
            labelTxt.color = textColor;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.raycastTarget = false;
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.preferredHeight = 32f;

            return btn;
        }

        // -------------------------------------------------------
        // NUOVI HELPER VISIVI (stile Word Domination)
        // -------------------------------------------------------

        /// <summary>
        /// Bottone 3D: layer ombra scuro offset + layer principale sopra.
        /// Dà l'effetto "pressed" dei giochi mobile WD.
        /// </summary>
        private static Button MakeButton3D(Transform parent, string name, string label,
            Color topColor, Color shadowColor, Color textColor, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            // Contenitore (non ha Image, serve solo come pivot)
            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            var wRt = wrapper.AddComponent<RectTransform>();
            wRt.anchorMin = anchorMin; wRt.anchorMax = anchorMax;
            wRt.offsetMin = wRt.offsetMax = Vector2.zero;

            // Shadow layer (leggermente più in basso)
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(wrapper.transform, false);
            var sRt = shadow.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero; sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(0, -6); sRt.offsetMax = new Vector2(0, -2);
            var shImg = shadow.AddComponent<Image>();
            shImg.color = shadowColor;
            RoundedRectHelper.Apply(shImg, UITheme.Layout.CornerRadius);

            // Top layer (bottone vero)
            var top = new GameObject("Top");
            top.transform.SetParent(wrapper.transform, false);
            var tRt = top.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(0, 2); tRt.offsetMax = new Vector2(0, 4);
            var topImg = top.AddComponent<Image>();
            topImg.color = topColor;
            RoundedRectHelper.Apply(topImg, UITheme.Layout.CornerRadius);
            var btn = top.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor      = topColor;
            cb.highlightedColor = Color.Lerp(topColor, Color.white, 0.15f);
            cb.pressedColor     = shadowColor;
            btn.colors = cb;

            // Testo
            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(top.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = fontSize;
            tmp.color     = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            btn.onClick.AddListener(AppPuzz.Audio.AudioManager.PlayClick);
            return btn;
        }

        /// <summary>
        /// Card con bordo colorato (stile WD inventory card).
        /// border ≈ 4px attorno a inner panel.
        /// </summary>
        private static GameObject MakeBorderedCard(Transform parent, string name,
            Color borderColor, Color innerColor,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var outer = new GameObject(name);
            outer.transform.SetParent(parent, false);
            var oRt = outer.AddComponent<RectTransform>();
            oRt.anchorMin = anchorMin; oRt.anchorMax = anchorMax;
            oRt.offsetMin = oRt.offsetMax = Vector2.zero;
            var outerImg = outer.AddComponent<Image>();
            outerImg.color = borderColor;
            RoundedRectHelper.Apply(outerImg, UITheme.Layout.CornerRadius);

            var inner = new GameObject("Inner");
            inner.transform.SetParent(outer.transform, false);
            var iRt = inner.AddComponent<RectTransform>();
            iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
            iRt.offsetMin = new Vector2(4, 4); iRt.offsetMax = new Vector2(-4, -4);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = innerColor;
            RoundedRectHelper.Apply(innerImg, UITheme.Layout.CornerRadius - 2f);

            return inner; // restituisce l'inner su cui mettere i figli
        }

        /// <summary>
        /// Chip risorsa (energia/gemme/monete) per la top bar.
        /// Crea un pannello pill-shaped con icona testo + valore.
        /// </summary>
        private static TextMeshProUGUI MakeResourceChip(Transform parent, string name,
            string icon, string value, Color chipColor, Color iconColor,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var chip = new GameObject(name);
            chip.transform.SetParent(parent, false);
            var rt = chip.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var chipImg = chip.AddComponent<Image>();
            chipImg.color = chipColor;
            RoundedRectHelper.Apply(chipImg, 8f);

            // Icona
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(chip.transform, false);
            var iRt = iconGo.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0, 0); iRt.anchorMax = new Vector2(0.35f, 1);
            iRt.offsetMin = iRt.offsetMax = Vector2.zero;
            var iconTxt = iconGo.AddComponent<TextMeshProUGUI>();
            iconTxt.text = icon; iconTxt.fontSize = 36;
            iconTxt.color = iconColor; iconTxt.alignment = TextAlignmentOptions.Center;
            iconTxt.fontStyle = FontStyles.Bold;

            // Valore
            var valGo = new GameObject("Value");
            valGo.transform.SetParent(chip.transform, false);
            var vRt = valGo.AddComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.35f, 0); vRt.anchorMax = Vector2.one;
            vRt.offsetMin = vRt.offsetMax = Vector2.zero;
            var valTxt = valGo.AddComponent<TextMeshProUGUI>();
            valTxt.text = value; valTxt.fontSize = 34;
            valTxt.color = Color.white; valTxt.alignment = TextAlignmentOptions.Center;
            valTxt.fontStyle = FontStyles.Bold;

            return valTxt;
        }

        /// <summary>
        /// Bottom navigation bar fissa con tab.
        /// </summary>
        private static void MakeNavBar(Transform parent)
        {
            var bar = new GameObject("NavBar");
            bar.transform.SetParent(parent, false);
            var rt = bar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, UITheme.Layout.NavBarHeight);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            bar.AddComponent<Image>().color = UITheme.Colors.NavBarBg;

            // Separatore top
            var sep = new GameObject("Separator");
            sep.transform.SetParent(bar.transform, false);
            var sRt = sep.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0, 0.92f); sRt.anchorMax = Vector2.one;
            sRt.offsetMin = sRt.offsetMax = Vector2.zero;
            sep.AddComponent<Image>().color = UITheme.Colors.SkyBlueDark;

            // Tab icon row
            var hlg = new GameObject("TabRow");
            hlg.transform.SetParent(bar.transform, false);
            var hlgRt = hlg.AddComponent<RectTransform>();
            hlgRt.anchorMin = new Vector2(0, 0); hlgRt.anchorMax = new Vector2(1, 0.92f);
            hlgRt.offsetMin = hlgRt.offsetMax = Vector2.zero;
            var layout = hlg.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 0;

            string[] icons   = { "★", "▶", "☆", "⚙" };
            string[] labels  = { "HOME", "GIOCA", "ARENA", "OPT" };
            Color[]  colors  = {
                UITheme.Colors.NavBarActive,
                UITheme.Colors.BtnGreen,
                UITheme.Colors.BtnPurple,
                UITheme.Colors.TextSecondary
            };

            for (int i = 0; i < icons.Length; i++)
            {
                var tab = new GameObject($"Tab{i}");
                tab.transform.SetParent(hlg.transform, false);
                tab.AddComponent<Image>().color = new Color(0, 0, 0, 0);

                var col = new GameObject("Col");
                col.transform.SetParent(tab.transform, false);
                var colRt = col.AddComponent<RectTransform>();
                colRt.anchorMin = Vector2.zero; colRt.anchorMax = Vector2.one;
                colRt.offsetMin = colRt.offsetMax = Vector2.zero;
                var vl = col.AddComponent<VerticalLayoutGroup>();
                vl.childAlignment = TextAnchor.MiddleCenter;
                vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

                var ico = new GameObject("I");
                ico.transform.SetParent(col.transform, false);
                var icoTxt = ico.AddComponent<TextMeshProUGUI>();
                icoTxt.text = icons[i]; icoTxt.fontSize = 38;
                icoTxt.color = colors[i]; icoTxt.alignment = TextAlignmentOptions.Center;
                var icoLe = ico.AddComponent<LayoutElement>(); icoLe.preferredHeight = 46;

                var lbl = new GameObject("L");
                lbl.transform.SetParent(col.transform, false);
                var lblTxt = lbl.AddComponent<TextMeshProUGUI>();
                lblTxt.text = labels[i]; lblTxt.fontSize = 20;
                lblTxt.color = colors[i]; lblTxt.alignment = TextAlignmentOptions.Center;
                var lblLe = lbl.AddComponent<LayoutElement>(); lblLe.preferredHeight = 24;
            }
        }

        // -------------------------------------------------------
        // ONBOARDING PANEL
        // -------------------------------------------------------
        private void BuildOnboardingPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "OnboardingPanel");
            if (panel.GetComponent<OnboardingManager>() != null) return;

            // Disattiva prima di aggiungere il componente per evitare che OnEnable
            // scatti prima che i riferimenti ai bottoni siano stati assegnati.
            panel.SetActive(false);
            var om = panel.AddComponent<OnboardingManager>();

            // --- Step Welcome ---
            om.stepWelcome = MakePanel(panel.transform, "StepWelcome",
                UITheme.Colors.BackgroundPanel,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            om.welcomeTitle = MakeText(om.stepWelcome.transform, "WelcomeTitle",
                "Benvenuto in\nWORD LEGEND!", 76, UITheme.Colors.Gold,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);
            om.welcomeTitle.fontStyle = FontStyles.Bold;

            om.welcomeSubtitle = MakeText(om.stepWelcome.transform, "WelcomeSubtitle",
                "Scegli la tua lingua", 44, UITheme.Colors.TextSecondary,
                TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.55f), Vector2.zero, Vector2.zero);

            om.italianLangBtn = MakeButton(om.stepWelcome.transform, "ItalianLangBtn", "ITALIANO",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 46,
                new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.44f), Vector2.zero, Vector2.zero);

            om.englishLangBtn = MakeButton(om.stepWelcome.transform, "EnglishLangBtn", "ENGLISH",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 46,
                new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.32f), Vector2.zero, Vector2.zero);

            // --- Step Profile: nickname + avatar ---
            om.stepCreature = MakePanel(panel.transform, "StepProfile",
                UITheme.Colors.BackgroundPanel,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var pss = om.stepCreature.AddComponent<ProfileSetupScreen>();
            om.profileSetupScreen = pss;

            MakeText(om.stepCreature.transform, "ProfileTitle",
                "CREA IL TUO PROFILO", 64, UITheme.Colors.Gold,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            // Nickname input
            MakeText(om.stepCreature.transform, "NickLabel",
                "NOME EVOCATORE", 36, UITheme.Colors.TextSecondary,
                TextAlignmentOptions.Left,
                new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.86f), Vector2.zero, Vector2.zero);
            pss.nicknameInput = MakeTMPInputField(om.stepCreature.transform, "NicknameInput",
                36, UITheme.Colors.TextPrimary, UITheme.Colors.BackgroundPanel,
                new Vector2(0.08f, 0.71f), new Vector2(0.92f, 0.79f));

            // 10 Avatar buttons (5x2 grid)
            MakeText(om.stepCreature.transform, "AvatarLabel",
                "SCEGLI IL TUO AVATAR", 36, UITheme.Colors.TextSecondary,
                TextAlignmentOptions.Left,
                new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.71f), Vector2.zero, Vector2.zero);
            {
                var avBtns      = new Button[10];
                var avHighlights = new Image[10];
                var avLabels    = new TMPro.TextMeshProUGUI[10];
                float cellW = 0.17f, cellH = 0.11f, gapX = 0.02f, gapY = 0.015f;
                float startX = 0.04f, row0Y = 0.52f, row1Y = row0Y - cellH - gapY;
                for (int i = 0; i < 10; i++)
                {
                    int col = i % 5, row = i / 5;
                    float ax = startX + col * (cellW + gapX);
                    float ay = (row == 0) ? row0Y : row1Y;
                    var avGo = new GameObject($"Avatar{i}");
                    avGo.transform.SetParent(om.stepCreature.transform, false);
                    var avRt = avGo.AddComponent<RectTransform>();
                    avRt.anchorMin = new Vector2(ax, ay);
                    avRt.anchorMax = new Vector2(ax + cellW, ay + cellH);
                    avRt.offsetMin = avRt.offsetMax = Vector2.zero;
                    var avImg = avGo.AddComponent<Image>();
                    avImg.color = ProfileSetupScreen.AVATAR_COLORS[i];
                    var avBtn = avGo.AddComponent<Button>();
                    avBtns[i] = avBtn;
                    // Highlight border (white outline)
                    var hlGo = new GameObject("Highlight");
                    hlGo.transform.SetParent(avGo.transform, false);
                    var hlRt = hlGo.AddComponent<RectTransform>();
                    hlRt.anchorMin = Vector2.zero; hlRt.anchorMax = Vector2.one;
                    hlRt.offsetMin = new Vector2(-4,-4); hlRt.offsetMax = new Vector2(4,4);
                    var hlImg = hlGo.AddComponent<Image>();
                    hlImg.color = Color.white;
                    hlImg.enabled = (i == 0);
                    avHighlights[i] = hlImg;
                    // Label
                    var lbl = MakeText(avGo.transform, "AvatarLabel", ProfileSetupScreen.AVATAR_NAMES[i],
                        20, UITheme.Colors.BackgroundDeep, TextAlignmentOptions.Center,
                        new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.45f), Vector2.zero, Vector2.zero);
                    lbl.fontStyle = FontStyles.Bold;
                    avLabels[i] = lbl;
                }
                pss.avatarButtons   = avBtns;
                pss.avatarHighlights = avHighlights;
                pss.avatarLabels    = avLabels;
            }

            // Confirm button
            pss.confirmButton = MakeButton(om.stepCreature.transform, "ConfirmProfileBtn",
                "CONFERMA", UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 48,
                new Vector2(0.15f, 0.27f), new Vector2(0.85f, 0.37f), Vector2.zero, Vector2.zero);
            om.stepCreature.SetActive(false);

            // --- Step Tutorial ---
            om.stepTutorial = MakePanel(panel.transform, "StepTutorial",
                UITheme.Colors.BackgroundPanel,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            om.tutorialText = MakeText(om.stepTutorial.transform, "TutorialText",
                "Trascina le lettere per formare parole!", 48, UITheme.Colors.TextPrimary,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.7f), Vector2.zero, Vector2.zero);

            om.tutorialNextBtn = MakeButton(om.stepTutorial.transform, "TutorialNextBtn", "AVANTI",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 44,
                new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.35f), Vector2.zero, Vector2.zero);
            om.stepTutorial.SetActive(false);

            // --- Step Ready ---
            om.stepReady = MakePanel(panel.transform, "StepReady",
                UITheme.Colors.BackgroundPanel,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            om.readyTitle = MakeText(om.stepReady.transform, "ReadyTitle",
                "Sei pronto,\nLeggendario!", 70, UITheme.Colors.Gold,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.8f), Vector2.zero, Vector2.zero);

            om.startAdventureBtn = MakeButton(om.stepReady.transform, "StartAdventureBtn",
                "INIZIA L'AVVENTURA!", UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 46,
                new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.42f), Vector2.zero, Vector2.zero);
            om.stepReady.SetActive(false);

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // CREATURE SELECTION PANEL
        // -------------------------------------------------------
        private void BuildCreatureSelectionPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "CreatureSelectionPanel");
            if (panel.GetComponent<CreatureSelectionScreen>() != null) return;

            panel.SetActive(false);
            var css = panel.AddComponent<CreatureSelectionScreen>();
            panel.GetComponent<Image>().color = UITheme.Colors.BackgroundPanel;

            // Top bar strip
            var selTopBar = MakePanel(panel.transform, "TopBar",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.88f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);
            MakeText(selTopBar.transform, "Title", UITheme.Strings.SelectCreature,
                60, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            // 3 Carte affiancate
            var cards    = new GameObject[3];
            var names    = new TextMeshProUGUI[3];
            var types    = new TextMeshProUGUI[3];
            var descs    = new TextMeshProUGUI[3];
            var specs    = new TextMeshProUGUI[3];
            var borders  = new Image[3];
            var powers   = new Slider[3];
            var speeds   = new Slider[3];
            var endurs   = new Slider[3];
            var selBtns  = new Button[3];

            Color[] cardColors = { UITheme.Colors.TypeDragon, UITheme.Colors.TypeWolf, UITheme.Colors.TypeSerpent };
            string[] cardNames = { "Mental Dragon", "Astral Wolf", "Ethereal Serpent" };
            string[] cardTypes = { "DRAGO MENTALE", "LUPO ASTRALE", "SERPENTE ETEREO" };
            string[] cardDescs = {
                "Potenzia\nle parole\nleggendarie",
                "Accelera\nlo streak\nbonuses",
                "Potenzia\nle parole\nlunghe"
            };

            float cardW = 0.28f;
            float[] cardX = { 0.04f, 0.36f, 0.68f };

            for (int i = 0; i < 3; i++)
            {
                float x0 = cardX[i], x1 = cardX[i] + cardW;
                float y0 = 0.10f,   y1 = 0.85f;

                // Card container
                var card = MakePanel(panel.transform, $"Card{i}",
                    UITheme.Colors.BackgroundCard,
                    new Vector2(x0, y0), new Vector2(x1, y1), Vector2.zero, Vector2.zero);
                cards[i] = card;

                // Bordo colorato
                var border = new GameObject("Border");
                border.transform.SetParent(card.transform, false);
                var brt = border.AddComponent<RectTransform>();
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
                brt.offsetMin = new Vector2(-3, -3); brt.offsetMax = new Vector2(3, 3);
                var bImg = border.AddComponent<Image>();
                bImg.color = new Color(cardColors[i].r, cardColors[i].g, cardColors[i].b, 0.5f);
                borders[i] = bImg;

                // Testi dentro la card (coordinate relative alla card)
                names[i] = MakeText(card.transform, "NameText", cardNames[i], 30,
                    cardColors[i], TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.93f), Vector2.zero, Vector2.zero);
                names[i].fontStyle = FontStyles.Bold;

                types[i] = MakeText(card.transform, "TypeText", cardTypes[i], 20,
                    UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);

                descs[i] = MakeText(card.transform, "DescText", cardDescs[i], 24,
                    UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.74f), Vector2.zero, Vector2.zero);

                // Label stat
                MakeText(card.transform, "LabelPow", "POT", 18, UITheme.Colors.TextSecondary,
                    TextAlignmentOptions.Left,
                    new Vector2(0.05f, 0.44f), new Vector2(0.35f, 0.50f), Vector2.zero, Vector2.zero);
                powers[i] = MakeSlider(card.transform, "PowerBar", cardColors[i],
                    new Vector2(0.35f, 0.44f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero);
                powers[i].value = 0.8f;

                MakeText(card.transform, "LabelSpd", "VEL", 18, UITheme.Colors.TextSecondary,
                    TextAlignmentOptions.Left,
                    new Vector2(0.05f, 0.36f), new Vector2(0.35f, 0.42f), Vector2.zero, Vector2.zero);
                speeds[i] = MakeSlider(card.transform, "SpeedBar", cardColors[i],
                    new Vector2(0.35f, 0.36f), new Vector2(0.95f, 0.42f), Vector2.zero, Vector2.zero);
                speeds[i].value = 0.6f;

                MakeText(card.transform, "LabelEnd", "RES", 18, UITheme.Colors.TextSecondary,
                    TextAlignmentOptions.Left,
                    new Vector2(0.05f, 0.28f), new Vector2(0.35f, 0.34f), Vector2.zero, Vector2.zero);
                endurs[i] = MakeSlider(card.transform, "EnduranceBar", cardColors[i],
                    new Vector2(0.35f, 0.28f), new Vector2(0.95f, 0.34f), Vector2.zero, Vector2.zero);
                endurs[i].value = 0.7f;

                specs[i] = MakeText(card.transform, "SpecText", "* Bonus speciale", 20,
                    UITheme.Colors.Gold, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.27f), Vector2.zero, Vector2.zero);

                // No per-card select button — use the main confirm button
                selBtns[i] = null;
            }

            // Bottone conferma in basso
            var confirmBtn = MakeButton(panel.transform, "ConfirmButton", "CONFERMA SELEZIONE",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 42,
                new Vector2(0.1f, 0.01f), new Vector2(0.9f, 0.09f), Vector2.zero, Vector2.zero);

            var confirmLbl = MakeText(panel.transform, "SelectedLabel", "",
                28, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.0f), new Vector2(0.9f, 0.0f), Vector2.zero, Vector2.zero);

            // Assegna arrays al componente
            css.creatureCards    = cards;
            css.nameTexts        = names;
            css.typeTexts        = types;
            css.descTexts        = descs;
            css.specialtyTexts   = specs;
            css.cardBorders      = borders;
            css.powerBars        = powers;
            css.speedBars        = speeds;
            css.enduranceBars    = endurs;
            css.selectButtons    = selBtns;
            css.confirmButton    = confirmBtn;
            css.selectedCreatureLabel = confirmLbl;

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // TRAINING PANEL
        // -------------------------------------------------------
        private void BuildTrainingPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "TrainingPanel");
            if (panel.GetComponent<TrainingManager>() != null) return;

            panel.SetActive(false);
            var tm = panel.AddComponent<TrainingManager>();
            panel.GetComponent<Image>().color = UITheme.Colors.SkyBlueDark;

            // Top bar strip (compatta)
            MakePanel(panel.transform, "TopBar",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.94f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);

            // Titolo
            MakeText(panel.transform, "TrainingTitle", "ALLENAMENTO",
                56, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.94f), new Vector2(0.95f, 1.00f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            // Timer (testo + barra)
            tm.timerText = MakeText(panel.transform, "TimerText", "02:00",
                72, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.15f, 0.885f), new Vector2(0.85f, 0.940f), Vector2.zero, Vector2.zero);
            tm.timerText.fontStyle = FontStyles.Bold;
            tm.timerBar = MakeSlider(panel.transform, "TimerBar", UITheme.Colors.EnergyFull,
                new Vector2(0.04f, 0.870f), new Vector2(0.96f, 0.886f), Vector2.zero, Vector2.zero);

            // Punteggio e parole (riga compatta)
            tm.scoreText = MakeText(panel.transform, "ScoreText", "Energia: 0",
                34, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.03f, 0.838f), new Vector2(0.52f, 0.870f), Vector2.zero, Vector2.zero);
            tm.wordsFoundText = MakeText(panel.transform, "WordsFoundText", "Parole: 0",
                34, UITheme.Colors.TextSecondary, TextAlignmentOptions.Right,
                new Vector2(0.52f, 0.838f), new Vector2(0.97f, 0.870f), Vector2.zero, Vector2.zero);

            // Barra energia
            tm.energyBar = MakeSlider(panel.transform, "EnergyBar", UITheme.Colors.Gold,
                new Vector2(0.04f, 0.818f), new Vector2(0.96f, 0.838f), Vector2.zero, Vector2.zero);

            // Parola corrente (sopra la griglia)
            tm.currentWordText = MakeText(panel.transform, "CurrentWordText", "",
                56, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.775f), new Vector2(0.95f, 0.818f), Vector2.zero, Vector2.zero);
            tm.currentWordText.fontStyle = FontStyles.Bold;

            // Feedback + Hint text (riga unica compatta)
            tm.feedbackText = MakeText(panel.transform, "FeedbackText", "",
                30, UITheme.Colors.TextSuccess, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.750f), new Vector2(0.72f, 0.775f), Vector2.zero, Vector2.zero);
            tm.hintText = MakeText(panel.transform, "HintText", "",
                24, UITheme.Colors.TextSecondary, TextAlignmentOptions.Right,
                new Vector2(0.72f, 0.750f), new Vector2(0.97f, 0.775f), Vector2.zero, Vector2.zero);

            // Cornice griglia estesa — quasi a bordo schermo
            {
                var frameBg = new GameObject("GridFrame");
                frameBg.transform.SetParent(panel.transform, false);
                var frt = frameBg.AddComponent<RectTransform>();
                frt.anchorMin = new Vector2(0.00f, 0.060f);
                frt.anchorMax = new Vector2(1.00f, 0.748f);
                frt.offsetMin = frt.offsetMax = Vector2.zero;
                var fImg = frameBg.AddComponent<Image>();
                fImg.color = UITheme.Colors.TileBorder;
            }

            // Grid area estesa
            {
                var gridArea = new GameObject("GridArea");
                gridArea.transform.SetParent(panel.transform, false);
                var rt = gridArea.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.01f, 0.065f);
                rt.anchorMax = new Vector2(0.99f, 0.742f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var glg = gridArea.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                glg.childAlignment  = TextAnchor.MiddleCenter;
                glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 5;
                glg.spacing         = new Vector2(8, 8);
                glg.padding         = new RectOffset(6, 6, 6, 6);
                tm.gridContainer    = gridArea.transform;
            }

            // Pulsanti compatti in basso
            tm.hintButton = MakeButton(panel.transform, "HintButton", "Suggerimento",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.Gold, 26,
                new Vector2(0.03f, 0.032f), new Vector2(0.46f, 0.060f), Vector2.zero, Vector2.zero);
            tm.newGridButton = MakeButton(panel.transform, "NewGridButton", "Nuova Griglia",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 26,
                new Vector2(0.54f, 0.032f), new Vector2(0.97f, 0.060f), Vector2.zero, Vector2.zero);
            tm.exitButton = MakeButton(panel.transform, "ExitButton", "ESCI",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 28,
                new Vector2(0.30f, 0.004f), new Vector2(0.70f, 0.032f), Vector2.zero, Vector2.zero);

            // Overlay fine sessione con ScrollRect — stile popup WD viola
            {
                var ov = MakePanel(panel.transform, "EndSessionOverlay",
                    UITheme.Colors.PopupBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                // Bordo popup viola
                var ovBorder = MakePanel(ov.transform, "PopupBorder",
                    UITheme.Colors.PopupBorder,
                    new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
                var ovInner = MakePanel(ovBorder.transform, "PopupInner",
                    UITheme.Colors.PopupInner,
                    new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero);
                tm.endSessionOverlay = ov;

                tm.endSessionTitle = MakeText(ovInner.transform, "EndTitle", "Sessione terminata!",
                    52, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.83f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);
                tm.endSessionTitle.fontStyle = FontStyles.Bold;

                // ScrollRect per lista parole
                tm.endWordsScrollText = MakeScrollableWordsList(ovInner.transform,
                    new Vector2(0.03f, 0.17f), new Vector2(0.97f, 0.82f));

                tm.endCloseButton = MakeButton(ovInner.transform, "EndCloseBtn", "CHIUDI",
                    UITheme.Colors.PopupCTA, UITheme.Colors.BackgroundDeep, 44,
                    new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.15f), Vector2.zero, Vector2.zero);
                ov.SetActive(false);
            }

            // Canvas per XP flottanti
            {
                var floatGo = new GameObject("FloatCanvas");
                floatGo.transform.SetParent(panel.transform, false);
                var fc = floatGo.AddComponent<Canvas>();
                fc.overrideSorting = true;
                fc.sortingOrder = 10;
                var fcRt = floatGo.GetComponent<RectTransform>();
                fcRt.anchorMin = Vector2.zero; fcRt.anchorMax = Vector2.one;
                fcRt.offsetMin = fcRt.offsetMax = Vector2.zero;
                tm.floatCanvas = fc;
            }

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // ARENA PANEL
        // -------------------------------------------------------
        private void BuildArenaPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "ArenaPanel");
            if (panel.GetComponent<ArenaManager>() != null) return;

            panel.SetActive(false);
            var am = panel.AddComponent<ArenaManager>();
            panel.GetComponent<Image>().color = UITheme.Colors.SkyBlueDark;

            // Top bar strip (compatta)
            MakePanel(panel.transform, "TopBar",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.94f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);

            MakeText(panel.transform, "ArenaTitle", "ARENA",
                56, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.94f), new Vector2(0.95f, 1.00f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            // Rank (sinistra) + Timer (destra) su stessa riga
            am.rankText = MakeText(panel.transform, "RankText", "Rango: Bronzo",
                26, UITheme.Colors.TextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.890f), new Vector2(0.50f, 0.940f), Vector2.zero, Vector2.zero);

            am.timerText = MakeText(panel.transform, "TimerText", "01:30",
                60, UITheme.Colors.TextPrimary, TextAlignmentOptions.Right,
                new Vector2(0.50f, 0.885f), new Vector2(0.96f, 0.940f), Vector2.zero, Vector2.zero);
            am.timerText.fontStyle = FontStyles.Bold;

            am.timerBar = MakeSlider(panel.transform, "TimerBar", UITheme.Colors.EnergyFull,
                new Vector2(0.04f, 0.870f), new Vector2(0.96f, 0.886f), Vector2.zero, Vector2.zero);
            am.timerBarFill = am.timerBar.fillRect?.GetComponent<Image>();

            // Score (sinistra) + Streak (destra)
            am.scoreText = MakeText(panel.transform, "ScoreText", "0",
                46, UITheme.Colors.Gold, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.832f), new Vector2(0.55f, 0.870f), Vector2.zero, Vector2.zero);
            am.scoreText.fontStyle = FontStyles.Bold;

            am.streakText = MakeText(panel.transform, "StreakText", "Streak x0",
                28, UITheme.Colors.TextSecondary, TextAlignmentOptions.Right,
                new Vector2(0.55f, 0.832f), new Vector2(0.96f, 0.870f), Vector2.zero, Vector2.zero);

            // Obiettivo + barra energia
            am.objectiveText = MakeText(panel.transform, "ObjectiveText", "Obiettivo: 1500",
                24, UITheme.Colors.TextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.808f), new Vector2(0.96f, 0.832f), Vector2.zero, Vector2.zero);

            am.energyBar = MakeSlider(panel.transform, "EnergyBar", UITheme.Colors.Gold,
                new Vector2(0.04f, 0.789f), new Vector2(0.96f, 0.808f), Vector2.zero, Vector2.zero);

            // Avversario
            am.opponentNameText = MakeText(panel.transform, "OpponentName", "Lupo Grigio",
                28, UITheme.Colors.TextDanger, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.761f), new Vector2(0.96f, 0.789f), Vector2.zero, Vector2.zero);
            am.opponentHealthBar = MakeSlider(panel.transform, "OpponentHP", UITheme.Colors.TextDanger,
                new Vector2(0.04f, 0.743f), new Vector2(0.96f, 0.761f), Vector2.zero, Vector2.zero);
            am.opponentHealthBar.value = 1f;

            // Parola corrente sopra la griglia
            am.currentWordText = MakeText(panel.transform, "CurrentWordText", "",
                54, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.700f), new Vector2(0.95f, 0.743f), Vector2.zero, Vector2.zero);
            am.currentWordText.fontStyle = FontStyles.Bold;

            // Cornice griglia estesa — quasi a bordo schermo
            {
                var gridFrame = new GameObject("GridFrame");
                gridFrame.transform.SetParent(panel.transform, false);
                var frt = gridFrame.AddComponent<RectTransform>();
                frt.anchorMin = new Vector2(0.00f, 0.060f);
                frt.anchorMax = new Vector2(1.00f, 0.698f);
                frt.offsetMin = frt.offsetMax = Vector2.zero;
                var fImg = gridFrame.AddComponent<Image>();
                fImg.color = UITheme.Colors.TileBorder;
            }

            // Grid area estesa
            {
                var gridArea = new GameObject("GridArea");
                gridArea.transform.SetParent(panel.transform, false);
                var rt = gridArea.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.01f, 0.065f);
                rt.anchorMax = new Vector2(0.99f, 0.692f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var glg = gridArea.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                glg.childAlignment  = TextAnchor.MiddleCenter;
                glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 5;
                glg.spacing         = new Vector2(8, 8);
                glg.padding         = new RectOffset(6, 6, 6, 6);
                am.gridContainer    = gridArea.transform;
            }

            // Float canvas for XP animations
            {
                var floatGo = new GameObject("FloatCanvas");
                floatGo.transform.SetParent(panel.transform, false);
                var fc = floatGo.AddComponent<Canvas>();
                fc.overrideSorting = true;
                fc.sortingOrder = 50;
                var fcRt = floatGo.GetComponent<RectTransform>();
                fcRt.anchorMin = Vector2.zero; fcRt.anchorMax = Vector2.one;
                fcRt.offsetMin = fcRt.offsetMax = Vector2.zero;
                am.floatCanvas = fc;
            }

            // Results overlay — stile popup WD viola
            var resultsOverlay = MakePanel(panel.transform, "ResultsOverlay",
                UITheme.Colors.PopupBg,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            am.resultsOverlay = resultsOverlay;
            var roInner = MakeBorderedCard(resultsOverlay.transform, "ResultsBorder",
                UITheme.Colors.PopupBorder, UITheme.Colors.PopupInner,
                new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f));

            am.resultTitle = MakeText(roInner.transform, "ResultTitle", "VITTORIA!",
                72, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.83f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);

            am.resultScoreText = MakeText(roInner.transform, "ResultScore", "Energia: 0",
                48, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.83f), Vector2.zero, Vector2.zero);

            am.resultRankText = MakeText(roInner.transform, "ResultRank", "Rango: Bronzo",
                34, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.74f), Vector2.zero, Vector2.zero);

            // Parole trovabili — ScrollRect
            am.resultWordsText = MakeScrollableWordsList(roInner.transform,
                new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.68f));

            am.playAgainBtn = MakeButton(roInner.transform, "PlayAgainBtn", "GIOCA ANCORA",
                UITheme.Colors.PopupCTA, UITheme.Colors.TextPrimary, 44,
                new Vector2(0.1f, 0.09f), new Vector2(0.9f, 0.17f), Vector2.zero, Vector2.zero);

            am.exitBtn = MakeButton(roInner.transform, "ExitBtn", "MENU",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 36,
                new Vector2(0.2f, 0.01f), new Vector2(0.8f, 0.08f), Vector2.zero, Vector2.zero);

            resultsOverlay.SetActive(false);

            // Bottone "CONCLUDI BATTAGLIA" (compatto in basso)
            am.quitButton = MakeButton(panel.transform, "QuitButton", "CONCLUDI",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 26,
                new Vector2(0.20f, 0.004f), new Vector2(0.80f, 0.060f), Vector2.zero, Vector2.zero);

            // Popup conferma uscita — stile WD viola
            var quitPopup = MakePanel(panel.transform, "QuitPopup",
                UITheme.Colors.PopupBg,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var qpInner = MakeBorderedCard(quitPopup.transform, "QuitBorder",
                UITheme.Colors.PopupBorder, UITheme.Colors.PopupInner,
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.75f));
            MakeText(qpInner.transform, "QuitTitle", "VUOI USCIRE?",
                64, UITheme.Colors.TextDanger, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.92f), Vector2.zero, Vector2.zero);
            MakeText(qpInner.transform, "QuitWarning",
                "Perderai tutti i punti guadagnati in questa battaglia!",
                34, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.65f), Vector2.zero, Vector2.zero);
            am.confirmQuitBtn = MakeButton(qpInner.transform, "ConfirmQuit", "SI, ESCI",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 44,
                new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.40f), Vector2.zero, Vector2.zero);
            am.cancelQuitBtn = MakeButton(qpInner.transform, "CancelQuit", "NO, CONTINUA",
                UITheme.Colors.PopupCTA, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.12f, 0.03f), new Vector2(0.88f, 0.17f), Vector2.zero, Vector2.zero);
            am.quitPopup = quitPopup;
            quitPopup.SetActive(false);

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // EVOLUTION PANEL
        // -------------------------------------------------------
        private void BuildEvolutionPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "EvolutionPanel");
            if (panel.GetComponent<EvolutionScreen>() != null) return;

            panel.SetActive(false);
            var es = panel.AddComponent<EvolutionScreen>();
            panel.GetComponent<Image>().color = UITheme.Colors.BackgroundPanel;

            // Top bar strip
            var evolTopBar = MakePanel(panel.transform, "TopBar",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.88f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);
            MakeText(evolTopBar.transform, "EvolutionTitle", "EVOLUZIONE",
                70, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            es.creatureNameText = MakeText(panel.transform, "CreatureNameText", "Mental Dragon",
                52, UITheme.Colors.TypeDragon, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.79f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            es.creatureLevelText = MakeText(panel.transform, "CreatureLevelText", "Livello 1",
                38, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.79f), Vector2.zero, Vector2.zero);

            es.creatureTypeText = MakeText(panel.transform, "CreatureTypeText", "TIPO: DRAGO MENTALE",
                30, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.74f), Vector2.zero, Vector2.zero);

            // XP Bar
            MakeText(panel.transform, "XPLabel", "ENERGIA EVOLUZIONE",
                28, UITheme.Colors.TextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.645f), new Vector2(0.95f, 0.68f), Vector2.zero, Vector2.zero);

            es.evolutionXPBar = MakeSlider(panel.transform, "EvolutionXPBar", UITheme.Colors.TypeDragon,
                new Vector2(0.05f, 0.61f), new Vector2(0.95f, 0.645f), Vector2.zero, Vector2.zero);

            es.evolutionXPText = MakeText(panel.transform, "EvolutionXPText", "0 XP",
                36, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.565f), new Vector2(0.9f, 0.61f), Vector2.zero, Vector2.zero);

            es.evolutionRequirementText = MakeText(panel.transform, "RequirementText",
                "Richiesti: 50 XP", 32, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.525f), new Vector2(0.9f, 0.565f), Vector2.zero, Vector2.zero);

            // Forme (3 mini-card)
            var forms = new GameObject[3];
            var formLocks = new Image[3];
            var formLabels = new TextMeshProUGUI[3];
            string[] fNames = { "Forma Base", "1a Evoluzione", "Forma Finale" };
            float[] fx = { 0.04f, 0.37f, 0.70f };
            for (int i = 0; i < 3; i++)
            {
                forms[i] = MakePanel(panel.transform, $"Form{i}",
                    UITheme.Colors.BackgroundCard,
                    new Vector2(fx[i], 0.32f), new Vector2(fx[i] + 0.26f, 0.50f), Vector2.zero, Vector2.zero);
                formLabels[i] = MakeText(forms[i].transform, "FormLabel", fNames[i],
                    22, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.35f), Vector2.zero, Vector2.zero);
                // Lock icon (panel scuro)
                var lockGo = MakePanel(forms[i].transform, "LockOverlay",
                    new Color(0, 0, 0, 0.6f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                MakeText(lockGo.transform, "LockIcon", "BLOCCATA", 20, UITheme.Colors.TextSecondary,
                    TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.7f), Vector2.zero, Vector2.zero);
                formLocks[i] = lockGo.GetComponent<Image>();
            }
            es.formPanels     = forms;
            es.formLockIcons  = formLocks;
            es.formLevelLabels = formLabels;

            // Stats
            es.statPowerText = MakeText(panel.transform, "StatPower", "",
                30, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.30f), Vector2.zero, Vector2.zero);
            es.statSpeedText = MakeText(panel.transform, "StatSpeed", "",
                30, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.24f), Vector2.zero, Vector2.zero);
            es.statEnduranceText = MakeText(panel.transform, "StatEndurance", "",
                30, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.18f), Vector2.zero, Vector2.zero);

            // Bottone evolvi
            es.evolveButton = MakeButton(panel.transform, "EvolveButton", UITheme.Strings.EvolveBtn,
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 50,
                new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.11f), Vector2.zero, Vector2.zero);

            // Flash overlay
            var flash = MakePanel(panel.transform, "EvolutionFlash",
                Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var flashCG = flash.AddComponent<CanvasGroup>();
            flashCG.alpha = 0f;
            flash.SetActive(false);
            es.evolutionFlashOverlay = flash;

            // Back button
            es.backButton = MakeButton(panel.transform, "BackButton", "INDIETRO",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextSecondary, 30,
                new Vector2(0.0f, 0.92f), new Vector2(0.3f, 0.99f), Vector2.zero, Vector2.zero);

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // TRANSITION OVERLAY
        // -------------------------------------------------------
        private void BuildTransitionOverlay(Canvas canvas)
        {
            var go = canvas.transform.Find("TransitionOverlay");
            if (go != null)
            {
                // Assicura che anche un overlay già esistente non blocchi i click
                var existingImg = go.GetComponent<Image>();
                if (existingImg != null) existingImg.raycastTarget = false;
                var existingCg = go.GetComponent<CanvasGroup>();
                if (existingCg != null) existingCg.blocksRaycasts = false;
                return;
            }

            var overlay = MakePanel(canvas.transform, "TransitionOverlay",
                Color.black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Disabilita ENTRAMBI i meccanismi di blocco raycast
            var img = overlay.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;

            var cg = overlay.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable    = false;

            overlay.SetActive(false);
        }

        // -------------------------------------------------------
        // SETTINGS PANEL
        // -------------------------------------------------------
        private void BuildSettingsPanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "SettingsPanel");
            if (panel.GetComponent<SettingsScreen>() != null) return;

            panel.SetActive(false);
            panel.GetComponent<Image>().color = UITheme.Colors.BackgroundPanel;

            var ss = panel.AddComponent<SettingsScreen>();

            // Top bar strip (sfondo sky blue)
            var settingsTopBar = MakePanel(panel.transform, "TopBar",
                UITheme.Colors.SkyBlue,
                new Vector2(0, 0.88f), new Vector2(1, 1.00f), Vector2.zero, Vector2.zero);
            MakeText(settingsTopBar.transform, "SettingsTitle", "IMPOSTAZIONI",
                80, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            MakeText(panel.transform, "SettingsInfo", "Versione 1.0",
                36, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.83f), new Vector2(0.9f, 0.88f), Vector2.zero, Vector2.zero);

            // --- Sezione lingua ---
            MakeText(panel.transform, "LangLabel", "LINGUA",
                56, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            ss.langItalianBtn = MakeButton(panel.transform, "LangItalianBtn", "ITALIANO",
                UITheme.Colors.ButtonPrimary, UITheme.Colors.BackgroundDeep, 56,
                new Vector2(0.05f, 0.63f), new Vector2(0.47f, 0.74f), Vector2.zero, Vector2.zero);

            ss.langEnglishBtn = MakeButton(panel.transform, "LangEnglishBtn", "ENGLISH",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 56,
                new Vector2(0.52f, 0.63f), new Vector2(0.93f, 0.74f), Vector2.zero, Vector2.zero);

            ss.langStatusText = MakeText(panel.transform, "LangStatusText", "Lingua attiva: Italiano",
                44, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.57f), new Vector2(0.95f, 0.63f), Vector2.zero, Vector2.zero);

            // --- Sezione nickname ---
            MakeText(panel.transform, "NickLabel", "NOME EVOCATORE",
                56, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.47f), new Vector2(0.95f, 0.56f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            ss.nicknameInput = MakeTMPInputField(panel.transform, "NicknameInput",
                56, UITheme.Colors.TextPrimary, new Color(0.09f, 0.13f, 0.24f),
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.47f));

            ss.saveNicknameBtn = MakeButton(panel.transform, "SaveNicknameBtn", "SALVA NOME",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 56,
                new Vector2(0.10f, 0.23f), new Vector2(0.90f, 0.34f), Vector2.zero, Vector2.zero);

            // --- Indietro ---
            ss.backButton = MakeButton(panel.transform, "BackButton", "INDIETRO",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 52,
                new Vector2(0.20f, 0.05f), new Vector2(0.80f, 0.15f), Vector2.zero, Vector2.zero);

            panel.SetActive(false);
        }

        // -------------------------------------------------------
        // WIRE SCREEN MANAGER
        // -------------------------------------------------------
        private void WireScreenManager(Canvas canvas)
        {
            var sm = FindFirstObjectByType<ScreenManager>();
            if (sm == null) return;

            sm.homePanel              = canvas.transform.Find("HomePanel")?.gameObject;
            sm.onboardingPanel        = canvas.transform.Find("OnboardingPanel")?.gameObject;
            sm.creatureSelectionPanel = canvas.transform.Find("CreatureSelectionPanel")?.gameObject;
            sm.trainingPanel          = canvas.transform.Find("TrainingPanel")?.gameObject;
            sm.arenaPanel             = canvas.transform.Find("ArenaPanel")?.gameObject;
            sm.evolutionPanel         = canvas.transform.Find("EvolutionPanel")?.gameObject;
            sm.settingsPanel          = canvas.transform.Find("SettingsPanel")?.gameObject;

            // GameplayPanel = il contenitore degli elementi di gioco esistenti
            sm.gameplayPanel = canvas.transform.Find("GameplayPanel")?.gameObject;

            // Salva il container griglia originale (GameplayPanel) per ripristinarlo
            var gm = FindFirstObjectByType<AppPuzz.Grid.GridManager>(FindObjectsInactive.Include);
            if (gm != null)
                sm.gameplayGridContainer = gm.gridContainer;

            // Wire training/arena gridContainer
            var trainingPanel = canvas.transform.Find("TrainingPanel");
            if (trainingPanel != null)
            {
                var tm = trainingPanel.GetComponent<AppPuzz.Gameplay.TrainingManager>();
                if (tm != null)
                {
                    var gridArea = trainingPanel.Find("GridArea");
                    if (gridArea != null) tm.gridContainer = gridArea;
                }
            }
            var arenaPanel = canvas.transform.Find("ArenaPanel");
            if (arenaPanel != null)
            {
                var am = arenaPanel.GetComponent<AppPuzz.Gameplay.ArenaManager>();
                if (am != null)
                {
                    var gridArea = arenaPanel.Find("GridArea");
                    if (gridArea != null) am.gridContainer = gridArea;
                }
            }

            // Wire GameManager references
            var gameManager = FindFirstObjectByType<AppPuzz.Gameplay.GameManager>(FindObjectsInactive.Include);
            if (gameManager != null)
            {
                if (gameManager.energyManager == null)
                    gameManager.energyManager = FindFirstObjectByType<AppPuzz.Gameplay.EnergyManager>(FindObjectsInactive.Include);
                if (gameManager.gridManager == null)
                    gameManager.gridManager = gm;

                // Crea FloatCanvas sul GameplayPanel per la modalità Gioca
                var gpPanel = sm.gameplayPanel;
                if (gpPanel != null && gameManager.floatCanvas == null)
                {
                    var fcGo = new GameObject("FloatCanvas");
                    fcGo.transform.SetParent(gpPanel.transform, false);
                    var fc = fcGo.AddComponent<Canvas>();
                    fc.overrideSorting = true;
                    fc.sortingOrder = 20;
                    var fcRt = fcGo.GetComponent<RectTransform>();
                    fcRt.anchorMin = Vector2.zero; fcRt.anchorMax = Vector2.one;
                    fcRt.offsetMin = fcRt.offsetMax = Vector2.zero;
                    gameManager.floatCanvas = fc;
                }

                // Crea currentWordText nel GameplayPanel se non presente
                if (gpPanel != null && gameManager.currentWordText == null)
                {
                    gameManager.currentWordText = MakeText(gpPanel.transform, "GameplayCurrentWord", "",
                        60, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                        new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.10f), Vector2.zero, Vector2.zero);
                    gameManager.currentWordText.fontStyle = FontStyles.Bold;
                }
            }

            var overlayGo = canvas.transform.Find("TransitionOverlay");
            if (overlayGo != null)
                sm.transitionOverlay = overlayGo.GetComponent<CanvasGroup>();

            // Transizioni temporaneamente disabilitate per garantire click immediati.
            // Riabilitare (true) dopo aver verificato che i click funzionino.
            sm.useTransitions    = false;
            sm.transitionDuration = 0.2f;
        }
    }
}
