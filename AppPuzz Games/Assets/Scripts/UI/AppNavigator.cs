// ============================================================
// AppNavigator.cs
// Punto centrale di navigazione dell'app.
// Decide quale screen mostrare all'avvio (onboarding vs home)
// e fornisce metodi di navigazione a tutti i sistemi.
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using AppPuzz.Utils;

namespace AppPuzz.UI
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce la navigazione globale.
    /// All'avvio decide se mostrare Onboarding o Home.
    /// </summary>
    public class AppNavigator : MonoBehaviour
    {
        public static AppNavigator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Se PlayerProfile non esiste, creane uno
            if (PlayerProfile.Instance == null)
            {
                new GameObject("PlayerProfile").AddComponent<PlayerProfile>();
            }

            // Routing iniziale
            bool onboardingDone = PlayerProfile.Instance?.OnboardingComplete ?? false;
            if (!onboardingDone)
                ShowOnboarding();
            else
                ShowHome();
        }

        // ----------------------------------------------------------
        // Navigazione pubblica
        // ----------------------------------------------------------

        public void ShowHome()
        {
            if (ScreenManager.Instance != null)
                ScreenManager.Instance.ShowScreen(ScreenID.Home);
            else
                SceneManager.LoadScene(0);
        }

        public void ShowOnboarding()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Onboarding);
        }

        public void ShowCreatureSelection()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.CreatureSelection);
        }

        public void ShowGameplay()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Gameplay);
        }

        public void ShowTraining()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Training);
        }

        public void ShowArena()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Arena);
        }

        public void ShowEvolution()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Evolution);
        }

        public void ShowSettings()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Settings);
        }

        public void ShowLegend()
        {
            ScreenManager.Instance?.ShowScreen(ScreenID.Legend);
        }

        /// <summary>Ricarica la scena corrente (hard reset).</summary>
        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
