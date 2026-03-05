// ============================================================
// ScreenManager.cs
// Gestisce la navigazione tra schermate tramite panel-switching
// all'interno della stessa scena (no LoadScene per ogni screen).
// Supporta storico per il tasto "Indietro".
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AppPuzz.UI
{
    /// <summary>Nome simbolico degli screen dell'app.</summary>
    public enum ScreenID
    {
        Home,
        Onboarding,
        CreatureSelection,
        Gameplay,
        Training,
        Arena,
        Evolution,
        Settings,
        Results
    }

    /// <summary>
    /// Singleton che gestisce l'attivazione dei pannelli UI.
    /// Ogni screen è un GameObject figlio del Canvas root.
    /// Aggiungere questo componente a un GameObject "ScreenManager"
    /// nella scena principale e collegare i pannelli dall'Inspector.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static ScreenManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Riferimenti Inspector
        // ----------------------------------------------------------
        [Header("Pannelli (assegnare dall'Inspector)")]
        public GameObject homePanel;
        public GameObject onboardingPanel;
        public GameObject creatureSelectionPanel;
        public GameObject gameplayPanel;
        public GameObject trainingPanel;
        public GameObject arenaPanel;
        public GameObject evolutionPanel;
        public GameObject settingsPanel;
        public GameObject resultsPanel;

        [Header("Overlay transizione")]
        [Tooltip("CanvasGroup nero per fade tra schermate (opzionale).")]
        public CanvasGroup transitionOverlay;

        [Header("Impostazioni")]
        public bool useTransitions = true;
        public float transitionDuration = 0.2f;

        // ----------------------------------------------------------
        // Stato interno
        // ----------------------------------------------------------
        private ScreenID          _current;
        private Stack<ScreenID>   _history = new Stack<ScreenID>();
        private bool              _isTransitioning;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            HideAll();
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>Mostra lo screen indicato, nasconde gli altri.</summary>
        public void ShowScreen(ScreenID id, bool addToHistory = true)
        {
            if (_isTransitioning) return;

            if (addToHistory && _current != id)
                _history.Push(_current);

            _current = id;

            if (useTransitions && transitionOverlay != null)
                StartCoroutine(TransitionTo(id));
            else
                ActivateScreen(id);
        }

        /// <summary>Torna allo screen precedente (stack history).</summary>
        public void Back()
        {
            if (_history.Count == 0) return;
            ScreenID prev = _history.Pop();
            ShowScreen(prev, addToHistory: false);
        }

        public ScreenID Current => _current;

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void HideAll()
        {
            SetActive(homePanel,             false);
            SetActive(onboardingPanel,       false);
            SetActive(creatureSelectionPanel,false);
            SetActive(gameplayPanel,         false);
            SetActive(trainingPanel,         false);
            SetActive(arenaPanel,            false);
            SetActive(evolutionPanel,        false);
            SetActive(settingsPanel,         false);
            SetActive(resultsPanel,          false);
        }

        private void ActivateScreen(ScreenID id)
        {
            HideAll();
            GameObject panel = GetPanel(id);
            if (panel != null)
                panel.SetActive(true);
            else
                Debug.LogWarning($"[ScreenManager] Nessun pannello assegnato per {id}");

            // Quando si mostra il gameplay, avvia la partita
            if (id == ScreenID.Gameplay)
            {
                var gm = AppPuzz.Gameplay.GameManager.Instance;
                if (gm != null && gm.CurrentState != AppPuzz.Gameplay.GameState.Playing)
                    gm.StartGame();
            }
        }

        private IEnumerator TransitionTo(ScreenID id)
        {
            _isTransitioning = true;

            // Fade to black
            if (transitionOverlay != null)
            {
                transitionOverlay.gameObject.SetActive(true);
                float t = 0f;
                while (t < transitionDuration)
                {
                    t += Time.deltaTime;
                    transitionOverlay.alpha = Mathf.Lerp(0f, 1f, t / transitionDuration);
                    yield return null;
                }
                transitionOverlay.alpha = 1f;
            }

            ActivateScreen(id);

            // Fade from black
            if (transitionOverlay != null)
            {
                float t = 0f;
                while (t < transitionDuration)
                {
                    t += Time.deltaTime;
                    transitionOverlay.alpha = Mathf.Lerp(1f, 0f, t / transitionDuration);
                    yield return null;
                }
                transitionOverlay.alpha = 0f;
                transitionOverlay.gameObject.SetActive(false);
            }

            _isTransitioning = false;
        }

        private GameObject GetPanel(ScreenID id) => id switch
        {
            ScreenID.Home             => homePanel,
            ScreenID.Onboarding       => onboardingPanel,
            ScreenID.CreatureSelection=> creatureSelectionPanel,
            ScreenID.Gameplay         => gameplayPanel,
            ScreenID.Training         => trainingPanel,
            ScreenID.Arena            => arenaPanel,
            ScreenID.Evolution        => evolutionPanel,
            ScreenID.Settings         => settingsPanel,
            ScreenID.Results          => resultsPanel,
            _                         => null,
        };

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
