// ============================================================
// CreatureSelectionScreen.cs
// Schermata di selezione creatura stile Pokémon:
// - 3 carte con nome, tipo, descrizione, stats
// - Selezione con highlight animato
// - Bottone conferma
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Utils;
using AppPuzz.Creatures;

namespace AppPuzz.UI
{
    /// <summary>Dati descrittivi di una creatura selezionabile.</summary>
    [System.Serializable]
    public class CreatureCardData
    {
        public string name;
        public string typeName;
        public string description;
        public string specialty;
        public Color  typeColor;
        [Range(1, 10)] public int powerStat;
        [Range(1, 10)] public int speedStat;
        [Range(1, 10)] public int enduranceStat;
    }

    /// <summary>
    /// Controller dello screen "Scegli la tua creatura".
    /// Attach al pannello CreatureSelectionPanel.
    /// </summary>
    public class CreatureSelectionScreen : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti Inspector
        // ----------------------------------------------------------

        [Header("Cards (3 creature)")]
        public GameObject[] creatureCards;       // 3 card GameObjects

        [Header("Elementi dentro ogni card (stessa struttura)")]
        public TextMeshProUGUI[] nameTexts;
        public TextMeshProUGUI[] typeTexts;
        public TextMeshProUGUI[] descTexts;
        public TextMeshProUGUI[] specialtyTexts;
        public Image[]           cardBorders;    // bordo colorato per tipo
        public Image[]           creatureImages;
        public Slider[]          powerBars;
        public Slider[]          speedBars;
        public Slider[]          enduranceBars;
        public Button[]          selectButtons;

        [Header("Conferma")]
        public Button          confirmButton;
        public TextMeshProUGUI confirmText;
        public TextMeshProUGUI selectedCreatureLabel;

        [Header("Dati creature (opzionale: configurare da Inspector)")]
        public CreatureCardData[] creatures;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------
        private int _selectedIndex = 0;

        // Dati di default se non configurati dall'Inspector
        private static readonly CreatureCardData[] DEFAULT_DATA =
        {
            new CreatureCardData
            {
                name = "Mental Dragon",
                typeName = "DRAGO MENTALE",
                description = "Un drago nato dall'energia pura della mente. Potenzia le parole più rare.",
                specialty = "Bonus LEGGENDARIO ×4",
                typeColor = new Color(0.435f, 0.208f, 0.988f),
                powerStat = 9, speedStat = 6, enduranceStat = 7
            },
            new CreatureCardData
            {
                name = "Astral Wolf",
                typeName = "LUPO ASTRALE",
                description = "Il lupo delle stelle. Veloce e instancabile, potenzia gli streak.",
                specialty = "Streak Bonus ×3",
                typeColor = new Color(0.310f, 0.580f, 1f),
                powerStat = 7, speedStat = 10, enduranceStat = 6
            },
            new CreatureCardData
            {
                name = "Ethereal Serpent",
                typeName = "SERPENTE ETEREO",
                description = "Il serpente dell'eternità. Parole lunghe diventano devastanti.",
                specialty = "Bonus lunghezza ×2.5",
                typeColor = new Color(0.180f, 0.800f, 0.443f),
                powerStat = 8, speedStat = 5, enduranceStat = 10
            }
        };

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Start()
        {
            if (creatures == null || creatures.Length == 0)
                creatures = DEFAULT_DATA;

            PopulateCards();
            WireButtons();
            SelectCard(PlayerProfile.Instance != null ? (int)PlayerProfile.Instance.SelectedCreature : 0);
        }

        private void OnEnable()
        {
            if (creatures != null && creatures.Length > 0)
                SelectCard(_selectedIndex);
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------
        private void PopulateCards()
        {
            for (int i = 0; i < 3; i++)
            {
                if (i >= creatures.Length) continue;
                var d = creatures[i];

                SetText(nameTexts,      i, d.name);
                SetText(typeTexts,      i, d.typeName);
                SetText(descTexts,      i, d.description);
                SetText(specialtyTexts, i, $"* {d.specialty}");
                SetSlider(powerBars,      i, d.powerStat / 10f);
                SetSlider(speedBars,      i, d.speedStat / 10f);
                SetSlider(enduranceBars,  i, d.enduranceStat / 10f);
                if (cardBorders != null && i < cardBorders.Length && cardBorders[i] != null)
                    cardBorders[i].color = d.typeColor;
            }
        }

        private void WireButtons()
        {
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                if (selectButtons != null && i < selectButtons.Length && selectButtons[i] != null)
                    selectButtons[i].onClick.AddListener(() => SelectCard(idx));
                // Clic sulla card intera
                if (creatureCards != null && i < creatureCards.Length && creatureCards[i] != null)
                {
                    var btn = creatureCards[i].GetComponent<Button>();
                    if (btn == null) btn = creatureCards[i].AddComponent<Button>();
                    btn.onClick.AddListener(() => SelectCard(idx));
                }
            }
            confirmButton?.onClick.AddListener(OnConfirm);
        }

        private void SelectCard(int index)
        {
            _selectedIndex = index;

            // Highlight selezionato
            for (int i = 0; i < 3; i++)
            {
                if (creatureCards != null && i < creatureCards.Length && creatureCards[i] != null)
                {
                    var cg = creatureCards[i].GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = (i == index) ? 1f : 0.6f;
                }
                if (cardBorders != null && i < cardBorders.Length && cardBorders[i] != null)
                {
                    var c = cardBorders[i].color;
                    cardBorders[i].color = (i == index)
                        ? c
                        : new Color(c.r, c.g, c.b, 0.3f);
                }
            }

            // Label e conferma
            if (selectedCreatureLabel != null && creatures != null && index < creatures.Length)
                selectedCreatureLabel.text = creatures[index].name.ToUpper();

            // Scale bounce sul card selezionato
            if (creatureCards != null && index < creatureCards.Length && creatureCards[index] != null)
                StartCoroutine(BounceCard(creatureCards[index].transform));
        }

        private void OnConfirm()
        {
            if (PlayerProfile.Instance != null)
                PlayerProfile.Instance.SelectedCreature = (CreatureType)_selectedIndex;

            // Se siamo nell'onboarding, andiamo al passo successivo
            // Se siamo dalla Home, torniamo alla Home
            if (ScreenManager.Instance != null)
            {
                bool fromOnboarding = ScreenManager.Instance.Current == ScreenID.Onboarding;
                ScreenManager.Instance.ShowScreen(fromOnboarding ? ScreenID.Onboarding : ScreenID.Home);
            }

            Debug.Log($"[CreatureSelection] Creatura scelta: {(CreatureType)_selectedIndex}");
        }

        private IEnumerator BounceCard(Transform t)
        {
            Vector3 original = t.localScale;
            Vector3 big      = original * 1.07f;
            float   half     = 0.08f;

            float elapsed = 0f;
            while (elapsed < half) { t.localScale = Vector3.Lerp(original, big, elapsed / half); elapsed += Time.deltaTime; yield return null; }
            elapsed = 0f;
            while (elapsed < half) { t.localScale = Vector3.Lerp(big, original, elapsed / half); elapsed += Time.deltaTime; yield return null; }
            t.localScale = original;
        }

        // ---- Helpers ----
        private static void SetText(TextMeshProUGUI[] arr, int i, string v)
        {
            if (arr != null && i < arr.Length && arr[i] != null) arr[i].text = v;
        }
        private static void SetSlider(Slider[] arr, int i, float v)
        {
            if (arr != null && i < arr.Length && arr[i] != null) arr[i].value = v;
        }
    }
}
