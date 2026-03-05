// ============================================================
// OnboardingManager.cs
// Gestisce il flusso di primo avvio:
//   Step 0 - Benvenuto + scelta lingua
//   Step 1 - Selezione creatura
//   Step 2 - Tutorial come si gioca
//   Step 3 - Pronto!
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Utils;
using AppPuzz.Localization;

namespace AppPuzz.UI
{
    /// <summary>
    /// Controller dell'onboarding. Attach a un pannello "OnboardingPanel".
    /// Ogni step è un child GameObject da attivare/disattivare.
    /// </summary>
    public class OnboardingManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti (Inspector)
        // ----------------------------------------------------------

        [Header("Step panels (GameObject figli)")]
        public GameObject stepWelcome;          // Step 0: Benvenuto
        public GameObject stepCreature;         // Step 1: Scelta creatura → usa CreatureSelectionScreen
        public GameObject stepTutorial;         // Step 2: Come si gioca
        public GameObject stepReady;            // Step 3: Pronto!

        [Header("Step Welcome")]
        public Button italianLangBtn;
        public Button englishLangBtn;
        public TextMeshProUGUI welcomeTitle;
        public TextMeshProUGUI welcomeSubtitle;

        [Header("Step Tutorial")]
        public TextMeshProUGUI tutorialText;
        public Button          tutorialNextBtn;

        [Header("Step Ready")]
        public TextMeshProUGUI readyTitle;
        public Button          startAdventureBtn;

        [Header("Indicatori step")]
        public Image[] stepDots;   // piccoli pallini progresso

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------
        private int _currentStep = 0;
        private const int TOTAL_STEPS = 4;

        private static readonly string[] TUTORIAL_TEXTS = {
            "Trascina il dito sulle lettere adiacenti per formare parole.",
            "Più la parola è lunga, più energia guadagni!",
            "Parole consecutive aumentano il tuo moltiplicatore STREAK.",
            "Trova parole leggendarie per triplicare la tua energia!",
        };

        private int _tutorialPage = 0;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void OnEnable()
        {
            Debug.Log($"[OnboardingManager] OnEnable — " +
                      $"italianLangBtn={italianLangBtn?.name ?? "NULL"}, " +
                      $"englishLangBtn={englishLangBtn?.name ?? "NULL"}");
            _currentStep   = 0;
            _tutorialPage  = 0;
            ShowStep(_currentStep);
            WireButtons();
        }

        // ----------------------------------------------------------
        // Diagnostica click (da rimuovere dopo il debug)
        // ----------------------------------------------------------
        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var es = EventSystem.current;
            if (es == null)
            {
                Debug.LogError("[OnboardingManager] EventSystem.current è NULL! " +
                               "Nessun input UI possibile.");
                return;
            }

            var pointer = new PointerEventData(es) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            es.RaycastAll(pointer, results);

            if (results.Count == 0)
            {
                Debug.LogWarning($"[OnboardingManager] Click {Input.mousePosition} — " +
                                 $"NESSUN elemento UI colpito! " +
                                 $"Verifica GraphicRaycaster e Canvas.");
            }
            else
            {
                var sb = new System.Text.StringBuilder(
                    $"[OnboardingManager] Click {Input.mousePosition} — elementi colpiti:\n");
                foreach (var r in results)
                {
                    sb.AppendLine($"  • {r.gameObject.name}  " +
                                  $"(path: {GetPath(r.gameObject)})  " +
                                  $"depth={r.depth}");
                }
                Debug.Log(sb.ToString());
            }
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var t = go.transform.parent;
            while (t != null) { path = t.name + "/" + path; t = t.parent; }
            return path;
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------
        private void WireButtons()
        {
            Debug.Log($"[OnboardingManager] WireButtons — " +
                      $"italianLangBtn={italianLangBtn?.name ?? "NULL"}, " +
                      $"englishLangBtn={englishLangBtn?.name ?? "NULL"}");

            italianLangBtn?.onClick.RemoveAllListeners();
            englishLangBtn?.onClick.RemoveAllListeners();
            tutorialNextBtn?.onClick.RemoveAllListeners();
            startAdventureBtn?.onClick.RemoveAllListeners();

            italianLangBtn?.onClick.AddListener(() =>
            {
                Debug.Log("[OnboardingManager] Click: ITALIANO");
                ChooseLanguage(Language.Italian);
            });
            englishLangBtn?.onClick.AddListener(() =>
            {
                Debug.Log("[OnboardingManager] Click: ENGLISH");
                ChooseLanguage(Language.English);
            });

            // Testo senza emoji (il font di default non le supporta)
            var itLabel = italianLangBtn?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            var enLabel = englishLangBtn?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (itLabel != null) itLabel.text = "ITALIANO";
            if (enLabel != null) enLabel.text = "ENGLISH";
            tutorialNextBtn?.onClick.AddListener(OnTutorialNext);
            startAdventureBtn?.onClick.AddListener(FinishOnboarding);
        }

        private void ShowStep(int step)
        {
            SetActive(stepWelcome,  step == 0);
            SetActive(stepCreature, step == 1);
            SetActive(stepTutorial, step == 2);
            SetActive(stepReady,    step == 3);

            UpdateStepDots(step);

            // Popola contenuti specifici
            if (step == 0)
            {
                if (welcomeTitle   != null) welcomeTitle.text   = "Benvenuto in\nWORD LEGEND!";
                if (welcomeSubtitle!= null) welcomeSubtitle.text = "Scegli la tua lingua";
            }
            else if (step == 2)
            {
                _tutorialPage = 0;
                UpdateTutorialText();
            }
            else if (step == 3)
            {
                if (readyTitle != null) readyTitle.text = "Sei pronto,\nLegendario!";
            }
        }

        private void UpdateTutorialText()
        {
            if (tutorialText != null)
                tutorialText.text = TUTORIAL_TEXTS[_tutorialPage];
            if (tutorialNextBtn != null)
            {
                var label = tutorialNextBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = (_tutorialPage < TUTORIAL_TEXTS.Length - 1) ? "AVANTI →" : "CAPITO!";
            }
        }

        private void UpdateStepDots(int active)
        {
            if (stepDots == null) return;
            for (int i = 0; i < stepDots.Length; i++)
            {
                if (stepDots[i] == null) continue;
                stepDots[i].color = (i == active)
                    ? UITheme.Colors.Gold
                    : UITheme.Colors.TextSecondary;
            }
        }

        // ---- Handler pulsanti ----
        private void ChooseLanguage(Language lang)
        {
            LanguageManager.Instance?.SetLanguage(lang);
            NextStep();
        }

        private void OnTutorialNext()
        {
            _tutorialPage++;
            if (_tutorialPage < TUTORIAL_TEXTS.Length)
                UpdateTutorialText();
            else
                NextStep();
        }

        private void NextStep()
        {
            _currentStep++;
            if (_currentStep < TOTAL_STEPS)
                ShowStep(_currentStep);
        }

        private void FinishOnboarding()
        {
            if (PlayerProfile.Instance != null)
                PlayerProfile.Instance.OnboardingComplete = true;

            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
