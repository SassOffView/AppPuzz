// ============================================================
// ResultsPanel.cs
// Pannello di fine partita: mostra energia, livello creatura e
// streak massimo. Il pulsante "Rigioca" avvia una nuova partita.
// ============================================================
// STEP 7 : implementazione completa.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Gameplay;

namespace AppPuzz.UI
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce il pannello risultati.
    /// Il GameObject deve essere inizialmente disattivato nella scena.
    /// Assegna dall'Inspector: energyText, levelText, streakText, playAgainButton.
    /// </summary>
    public class ResultsPanel : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static ResultsPanel Instance { get; private set; }

        // ----------------------------------------------------------
        // Riferimenti UI (assegnati dall'Inspector)
        // ----------------------------------------------------------

        [Header("Testi risultato")]
        [Tooltip("Mostra l'energia finale raggiunta.")]
        public TextMeshProUGUI energyText;

        [Tooltip("Mostra il livello di evoluzione della creatura.")]
        public TextMeshProUGUI levelText;

        [Tooltip("Mostra lo streak massimo di parole consecutive.")]
        public TextMeshProUGUI streakText;

        [Header("Pulsanti")]
        [Tooltip("Pulsante per avviare una nuova partita.")]
        public Button playAgainButton;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            gameObject.SetActive(false); // nascosto finché EndGame() non lo chiama
        }

        private void Start()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
        }

        // ----------------------------------------------------------
        // API pubblica — chiamata da GameManager.EndGame()
        // ----------------------------------------------------------

        /// <summary>
        /// Mostra il pannello con i dati della partita appena conclusa.
        /// </summary>
        /// <param name="energy">Energia finale accumulata.</param>
        /// <param name="level">Livello di evoluzione raggiunto (1–3).</param>
        /// <param name="maxStreak">Streak massimo di parole valide consecutive.</param>
        public void Show(float energy, int level, int maxStreak)
        {
            if (energyText != null) energyText.text = $"Energia: {energy:F0}";
            if (levelText  != null) levelText.text  = $"Livello creatura: {level}";
            if (streakText != null) streakText.text = $"Streak massimo: {maxStreak}";

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Nasconde il pannello (chiamato da GameManager.StartGame() al riavvio).
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
