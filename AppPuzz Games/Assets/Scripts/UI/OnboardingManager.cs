// ============================================================
// OnboardingManager.cs
// Gestisce il flusso di primo avvio:
//   Step 0 - Benvenuto + scelta lingua
//   Step 1 - Selezione creatura
//   Step 2 - Tutorial come si gioca
//   Step 3 - Pronto!
// ============================================================

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
    /// Il rilevamento dei click usa RaycastAll() in Update() anziché
    /// Button.onClick, perché lo StandaloneInputModule della scena non
    /// consegna gli eventi ai Button in questo setup.
    /// </summary>
    public class OnboardingManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti (Inspector)
        // ----------------------------------------------------------

        [Header("Step panels (GameObject figli)")]
        public GameObject stepWelcome;
        public GameObject stepCreature;
        public GameObject stepTutorial;
        public GameObject stepReady;

        [Header("Step Welcome")]
        public Button italianLangBtn;
        public Button englishLangBtn;
        public TextMeshProUGUI welcomeTitle;
        public TextMeshProUGUI welcomeSubtitle;

        [Header("Step Profile")]
        public ProfileSetupScreen profileSetupScreen;
        public Button creatureChooseBtn; // legacy - non usato

        [Header("Step Tutorial")]
        public TextMeshProUGUI tutorialText;
        public Button          tutorialNextBtn;

        [Header("Step Ready")]
        public TextMeshProUGUI readyTitle;
        public Button          startAdventureBtn;

        [Header("Indicatori step")]
        public Image[] stepDots;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------
        private int _currentStep = 0;
        private const int TOTAL_STEPS = 4;

        private static readonly string[] TUTORIAL_TEXTS = {
            "Trascina il dito sulle lettere adiacenti per formare parole.",
            "Piu la parola e lunga, piu energia guadagni!",
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
            _currentStep  = 0;
            _tutorialPage = 0;
            ShowStep(_currentStep);
            SetupLabels();
        }

        /// <summary>
        /// Rileva click/tap via EventSystem.RaycastAll() e gestisce tutti i
        /// bottoni dell'onboarding. Necessario perche Button.onClick non viene
        /// consegnato dallo StandaloneInputModule presente nella scena.
        /// </summary>
        private void Update()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                clicked = true;
            if (!clicked) return;

            var es = EventSystem.current;
            if (es == null) return;

            Vector2 pos = (Input.touchCount > 0)
                ? Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            var pointer = new PointerEventData(es) { position = pos };
            var results = new List<RaycastResult>();
            es.RaycastAll(pointer, results);

            foreach (var r in results)
            {
                var go = r.gameObject;
                if (IsUnder(go, italianLangBtn))
                {
                    Debug.Log("[OnboardingManager] Click: ITALIANO");
                    ChooseLanguage(Language.Italian);
                    return;
                }
                if (IsUnder(go, englishLangBtn))
                {
                    Debug.Log("[OnboardingManager] Click: ENGLISH");
                    ChooseLanguage(Language.English);
                    return;
                }
                // creatureChooseBtn rimosso — il profilo si conferma da ProfileSetupScreen
                if (IsUnder(go, tutorialNextBtn))
                {
                    OnTutorialNext();
                    return;
                }
                if (IsUnder(go, startAdventureBtn))
                {
                    FinishOnboarding();
                    return;
                }
            }
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void SetupLabels()
        {
            var itLabel = italianLangBtn?.GetComponentInChildren<TextMeshProUGUI>();
            var enLabel = englishLangBtn?.GetComponentInChildren<TextMeshProUGUI>();
            if (itLabel != null) itLabel.text = "ITALIANO";
            if (enLabel != null) enLabel.text = "ENGLISH";
        }

        private void ShowStep(int step)
        {
            SetActive(stepWelcome,  step == 0);
            SetActive(stepCreature, step == 1);
            SetActive(stepTutorial, step == 2);
            SetActive(stepReady,    step == 3);

            UpdateStepDots(step);

            if (step == 0)
            {
                if (welcomeTitle    != null) welcomeTitle.text    = "Benvenuto in\nWORD LEGEND!";
                if (welcomeSubtitle != null) welcomeSubtitle.text = "Scegli la tua lingua";
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
                    label.text = (_tutorialPage < TUTORIAL_TEXTS.Length - 1) ? "AVANTI ->" : "CAPITO!";
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

        /// <summary>Chiamato da ProfileSetupScreen.ConfirmProfile() quando il giocatore conferma il profilo.</summary>
        public void ProfileConfirmed()
        {
            NextStep();
        }

        private void FinishOnboarding()
        {
            if (PlayerProfile.Instance != null)
                PlayerProfile.Instance.OnboardingComplete = true;

            ScreenManager.Instance?.ShowScreen(ScreenID.Home);
        }

        // ---- Utilita ----
        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
