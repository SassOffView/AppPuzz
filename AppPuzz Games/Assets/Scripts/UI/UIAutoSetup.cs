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
            img.color = UITheme.Colors.BackgroundDeep;
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
            bg.AddComponent<Image>().color = UITheme.Colors.BackgroundPanel;

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

        // -------------------------------------------------------
        // HOME PANEL
        // -------------------------------------------------------
        private void BuildHomePanel(Canvas canvas)
        {
            var panel = FindOrCreatePanel(canvas, "HomePanel");
            if (panel.GetComponent<HomeScreen>() != null) return; // già configurato

            panel.SetActive(false);
            var hs = panel.AddComponent<HomeScreen>();

            // Titolo
            hs.titleText = MakeText(panel.transform, "TitleText", UITheme.Strings.AppTitle,
                72, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
            hs.titleText.fontStyle = FontStyles.Bold;

            // Livello giocatore
            hs.playerLevelText = MakeText(panel.transform, "PlayerLevelText", "LV. 1",
                40, UITheme.Colors.TextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.75f), new Vector2(0.4f, 0.82f), Vector2.zero, Vector2.zero);

            // Barra XP
            hs.xpBar = MakeSlider(panel.transform, "XPBar", UITheme.Colors.Gold,
                new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.755f), Vector2.zero, Vector2.zero);
            hs.xpBar.value = 0.3f;

            hs.xpText = MakeText(panel.transform, "XPText", "350 XP al prossimo livello",
                28, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.695f), new Vector2(0.95f, 0.72f), Vector2.zero, Vector2.zero);

            // Nome creatura
            hs.creatureNameText = MakeText(panel.transform, "CreatureNameText", "Mental Dragon",
                46, UITheme.Colors.TypeDragon, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.69f), Vector2.zero, Vector2.zero);
            hs.creatureNameText.fontStyle = FontStyles.Bold;

            // Stats
            hs.bestScoreText = MakeText(panel.transform, "BestScoreText", "Record: 0",
                36, UITheme.Colors.TextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.54f), new Vector2(0.5f, 0.60f), Vector2.zero, Vector2.zero);
            hs.totalMatchesText = MakeText(panel.transform, "TotalMatchesText", "Partite: 0",
                36, UITheme.Colors.TextSecondary, TextAlignmentOptions.Right,
                new Vector2(0.5f, 0.54f), new Vector2(0.95f, 0.60f), Vector2.zero, Vector2.zero);
            hs.arenaRankText = MakeText(panel.transform, "ArenaRankText", "BRONZO",
                36, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.49f), new Vector2(0.9f, 0.54f), Vector2.zero, Vector2.zero);

            // Bottoni (5 in colonna)
            float btnH = 0.072f;
            float startY = 0.41f;
            float gap = 0.082f;

            hs.playButton = MakeButton(panel.transform, "PlayButton", UITheme.Strings.PlayBtn,
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 48,
                new Vector2(0.1f, startY), new Vector2(0.9f, startY + btnH), Vector2.zero, Vector2.zero);
            hs.trainingButton = MakeButton(panel.transform, "TrainingButton", UITheme.Strings.TrainingBtn,
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.1f, startY - gap), new Vector2(0.9f, startY - gap + btnH), Vector2.zero, Vector2.zero);
            hs.arenaButton = MakeButton(panel.transform, "ArenaButton", UITheme.Strings.ArenaBtn,
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.1f, startY - gap * 2), new Vector2(0.9f, startY - gap * 2 + btnH), Vector2.zero, Vector2.zero);
            hs.evolutionButton = MakeButton(panel.transform, "EvolutionButton", UITheme.Strings.EvolutionBtn,
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.1f, startY - gap * 3), new Vector2(0.9f, startY - gap * 3 + btnH), Vector2.zero, Vector2.zero);
            hs.settingsButton = MakeButton(panel.transform, "SettingsButton", UITheme.Strings.SettingsBtn,
                new Color(0.2f, 0.2f, 0.35f), UITheme.Colors.TextSecondary, 32,
                new Vector2(0.25f, startY - gap * 4), new Vector2(0.75f, startY - gap * 4 + btnH), Vector2.zero, Vector2.zero);

            panel.SetActive(false);
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
                UITheme.Colors.BackgroundDeep,
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
                UITheme.Colors.BackgroundDeep,
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
            {
                var inputGo = new GameObject("NicknameInput");
                inputGo.transform.SetParent(om.stepCreature.transform, false);
                var inputRt = inputGo.AddComponent<RectTransform>();
                inputRt.anchorMin = new Vector2(0.08f, 0.72f);
                inputRt.anchorMax = new Vector2(0.92f, 0.79f);
                inputRt.offsetMin = inputRt.offsetMax = Vector2.zero;
                var bg = inputGo.AddComponent<Image>();
                bg.color = UITheme.Colors.BackgroundPanel;
                var field = inputGo.AddComponent<TMPro.TMP_InputField>();
                var txtGo = new GameObject("Text");
                txtGo.transform.SetParent(inputGo.transform, false);
                var trt2 = txtGo.AddComponent<RectTransform>();
                trt2.anchorMin = new Vector2(0.02f, 0.05f); trt2.anchorMax = new Vector2(0.98f, 0.95f);
                trt2.offsetMin = trt2.offsetMax = Vector2.zero;
                var tmp2 = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
                tmp2.fontSize = 40; tmp2.color = UITheme.Colors.TextPrimary;
                tmp2.alignment = TextAlignmentOptions.MidlineLeft;
                field.textComponent = tmp2;
                field.characterLimit = 16;
                pss.nicknameInput = field;
            }

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
                UITheme.Colors.BackgroundDeep,
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
                UITheme.Colors.BackgroundDeep,
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

            // Titolo
            MakeText(panel.transform, "Title", UITheme.Strings.SelectCreature,
                60, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.02f, 0.87f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero)
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
            panel.GetComponent<Image>().color = UITheme.Colors.BackgroundDeep;

            MakeText(panel.transform, "TrainingTitle", "ALLENAMENTO",
                64, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            tm.scoreText = MakeText(panel.transform, "ScoreText", "Energia: 0",
                46, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            tm.wordsFoundText = MakeText(panel.transform, "WordsFoundText", "Parole: 0",
                38, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.80f), Vector2.zero, Vector2.zero);

            tm.feedbackText = MakeText(panel.transform, "FeedbackText", "",
                42, UITheme.Colors.TextSuccess, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.74f), Vector2.zero, Vector2.zero);

            tm.energyBar = MakeSlider(panel.transform, "EnergyBar", UITheme.Colors.EnergyFull,
                new Vector2(0.05f, 0.64f), new Vector2(0.95f, 0.68f), Vector2.zero, Vector2.zero);

            tm.hintText = MakeText(panel.transform, "HintText", "",
                32, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.64f), Vector2.zero, Vector2.zero);

            // Grid area — GridManager spawna le celle qui
            {
                var gridArea = new GameObject("GridArea");
                gridArea.transform.SetParent(panel.transform, false);
                var rt = gridArea.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.19f);
                rt.anchorMax = new Vector2(0.98f, 0.60f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                // Aggiungi GridLayoutGroup per le celle
                var glg = gridArea.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                glg.childAlignment = TextAnchor.MiddleCenter;
                glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 5;
                glg.spacing = new Vector2(4, 4);
                tm.gridContainer = gridArea.transform;
            }

            tm.hintButton = MakeButton(panel.transform, "HintButton", "Suggerimento (3)",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.Gold, 36,
                new Vector2(0.05f, 0.10f), new Vector2(0.48f, 0.18f), Vector2.zero, Vector2.zero);

            tm.newGridButton = MakeButton(panel.transform, "NewGridButton", "Nuova Griglia",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 36,
                new Vector2(0.52f, 0.10f), new Vector2(0.95f, 0.18f), Vector2.zero, Vector2.zero);

            tm.exitButton = MakeButton(panel.transform, "ExitButton", "ESCI",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.09f), Vector2.zero, Vector2.zero);

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

            MakeText(panel.transform, "ArenaTitle", "ARENA",
                72, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            am.rankText = MakeText(panel.transform, "RankText", "Rango: Bronzo",
                36, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.83f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            // Timer
            am.timerText = MakeText(panel.transform, "TimerText", "01:30",
                72, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.74f), new Vector2(0.75f, 0.83f), Vector2.zero, Vector2.zero);
            am.timerText.fontStyle = FontStyles.Bold;

            am.timerBar = MakeSlider(panel.transform, "TimerBar", UITheme.Colors.EnergyFull,
                new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.745f), Vector2.zero, Vector2.zero);
            am.timerBarFill = am.timerBar.fillRect?.GetComponent<Image>();

            am.scoreText = MakeText(panel.transform, "ScoreText", "0",
                64, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.64f), new Vector2(0.9f, 0.72f), Vector2.zero, Vector2.zero);
            am.scoreText.fontStyle = FontStyles.Bold;

            am.streakText = MakeText(panel.transform, "StreakText", "Streak x0",
                38, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.59f), new Vector2(0.9f, 0.64f), Vector2.zero, Vector2.zero);

            am.objectiveText = MakeText(panel.transform, "ObjectiveText", "Obiettivo: 1500",
                32, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.59f), Vector2.zero, Vector2.zero);

            am.energyBar = MakeSlider(panel.transform, "EnergyBar", UITheme.Colors.Gold,
                new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.555f), Vector2.zero, Vector2.zero);

            // Pannello avversario
            am.opponentNameText = MakeText(panel.transform, "OpponentName", "Lupo Grigio",
                40, UITheme.Colors.TextDanger, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.47f), new Vector2(0.95f, 0.52f), Vector2.zero, Vector2.zero);
            am.opponentHealthBar = MakeSlider(panel.transform, "OpponentHP", UITheme.Colors.TextDanger,
                new Vector2(0.05f, 0.44f), new Vector2(0.95f, 0.47f), Vector2.zero, Vector2.zero);
            am.opponentHealthBar.value = 1f;

            // Grid area — GridManager spawna le celle qui
            {
                var gridArea = new GameObject("GridArea");
                gridArea.transform.SetParent(panel.transform, false);
                var rt = gridArea.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.19f);
                rt.anchorMax = new Vector2(0.98f, 0.43f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var glg = gridArea.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                glg.childAlignment = TextAnchor.MiddleCenter;
                glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 5;
                glg.spacing = new Vector2(4, 4);
                am.gridContainer = gridArea.transform;
            }

            // Results overlay
            var resultsOverlay = MakePanel(panel.transform, "ResultsOverlay",
                new Color(0, 0, 0, 0.85f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            am.resultsOverlay = resultsOverlay;

            am.resultTitle = MakeText(resultsOverlay.transform, "ResultTitle", "VITTORIA!",
                72, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.6f), new Vector2(0.95f, 0.8f), Vector2.zero, Vector2.zero);

            am.resultScoreText = MakeText(resultsOverlay.transform, "ResultScore", "Energia: 0",
                52, UITheme.Colors.TextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f), Vector2.zero, Vector2.zero);

            am.resultRankText = MakeText(resultsOverlay.transform, "ResultRank", "Rango: Bronzo",
                40, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.43f), new Vector2(0.9f, 0.5f), Vector2.zero, Vector2.zero);

            am.playAgainBtn = MakeButton(resultsOverlay.transform, "PlayAgainBtn", "GIOCA ANCORA",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 44,
                new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.38f), Vector2.zero, Vector2.zero);

            am.exitBtn = MakeButton(resultsOverlay.transform, "ExitBtn", "MENU",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 38,
                new Vector2(0.2f, 0.17f), new Vector2(0.8f, 0.26f), Vector2.zero, Vector2.zero);

            resultsOverlay.SetActive(false);

            // Bottone "CONCLUDI BATTAGLIA" (visibile durante la partita)
            am.quitButton = MakeButton(panel.transform, "QuitButton", "CONCLUDI BATTAGLIA",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 32,
                new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.18f), Vector2.zero, Vector2.zero);

            // Popup conferma uscita
            var quitPopup = MakePanel(panel.transform, "QuitPopup",
                new Color(0f, 0f, 0f, 0.92f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            MakeText(quitPopup.transform, "QuitTitle", "VUOI USCIRE?",
                64, UITheme.Colors.TextDanger, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.78f), Vector2.zero, Vector2.zero);
            MakeText(quitPopup.transform, "QuitWarning",
                "Perderai tutti i punti guadagnati in questa battaglia!",
                38, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.60f), Vector2.zero, Vector2.zero);
            am.confirmQuitBtn = MakeButton(quitPopup.transform, "ConfirmQuit", "SI, ESCI",
                UITheme.Colors.ButtonDanger, UITheme.Colors.TextPrimary, 44,
                new Vector2(0.1f, 0.30f), new Vector2(0.9f, 0.43f), Vector2.zero, Vector2.zero);
            am.cancelQuitBtn = MakeButton(quitPopup.transform, "CancelQuit", "NO, CONTINUA",
                UITheme.Colors.Gold, UITheme.Colors.BackgroundDeep, 38,
                new Vector2(0.15f, 0.16f), new Vector2(0.85f, 0.27f), Vector2.zero, Vector2.zero);
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

            MakeText(panel.transform, "EvolutionTitle", "EVOLUZIONE",
                70, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero)
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
            panel.GetComponent<Image>().color = UITheme.Colors.BackgroundDeep;

            var ss = panel.AddComponent<SettingsScreen>();

            MakeText(panel.transform, "SettingsTitle", "IMPOSTAZIONI",
                64, UITheme.Colors.Gold, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero)
                .fontStyle = FontStyles.Bold;

            MakeText(panel.transform, "SettingsInfo", "Versione 1.0",
                38, UITheme.Colors.TextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.78f), Vector2.zero, Vector2.zero);

            ss.backButton = MakeButton(panel.transform, "BackButton", "INDIETRO",
                UITheme.Colors.ButtonSecondary, UITheme.Colors.TextPrimary, 40,
                new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.13f), Vector2.zero, Vector2.zero);

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
