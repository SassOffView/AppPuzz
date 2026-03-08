// ============================================================
// CreatureSelectionScreen.cs
// Schermata di selezione creatura stile Pokemon:
// - 3 carte con nome, tipo, descrizione, stats
// - Selezione con highlight animato
// - Bottone conferma
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    /// I click sono gestiti via RaycastAll() in Update() (stessa
    /// soluzione adottata in OnboardingManager).
    /// </summary>
    public class CreatureSelectionScreen : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti Inspector
        // ----------------------------------------------------------

        [Header("Cards (3 creature)")]
        public GameObject[] creatureCards;

        [Header("Elementi dentro ogni card")]
        public TextMeshProUGUI[] nameTexts;
        public TextMeshProUGUI[] typeTexts;
        public TextMeshProUGUI[] descTexts;
        public TextMeshProUGUI[] specialtyTexts;
        public Image[]           cardBorders;
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

        private static readonly CreatureCardData[] DEFAULT_DATA =
        {
            new CreatureCardData
            {
                name = "Mental Dragon",
                typeName = "DRAGO MENTALE",
                description = "Un drago nato dall'energia pura della mente. Potenzia le parole piu rare.",
                specialty = "Bonus LEGGENDARIO x4",
                typeColor = new Color(0.435f, 0.208f, 0.988f),
                powerStat = 9, speedStat = 6, enduranceStat = 7
            },
            new CreatureCardData
            {
                name = "Astral Wolf",
                typeName = "LUPO ASTRALE",
                description = "Il lupo delle stelle. Veloce e instancabile, potenzia gli streak.",
                specialty = "Streak Bonus x3",
                typeColor = new Color(0.310f, 0.580f, 1f),
                powerStat = 7, speedStat = 10, enduranceStat = 6
            },
            new CreatureCardData
            {
                name = "Ethereal Serpent",
                typeName = "SERPENTE ETEREO",
                description = "Il serpente dell'eternita. Parole lunghe diventano devastanti.",
                specialty = "Bonus lunghezza x2.5",
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
            SelectCard(PlayerProfile.Instance != null ? (int)PlayerProfile.Instance.SelectedCreature : 0);
        }

        private void OnEnable()
        {
            if (creatures != null && creatures.Length > 0)
                SelectCard(_selectedIndex);
        }

        /// <summary>
        /// Gestisce click/tap sulle card e sui bottoni via RaycastAll(),
        /// bypassando Button.onClick che non viene consegnato in questa scena.
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

                // Bottone SCEGLI o click sull'intera card
                for (int i = 0; i < 3; i++)
                {
                    if (IsUnder(go, selectButtons, i) || IsUnder(go, creatureCards, i))
                    {
                        SelectCard(i);
                        return;
                    }
                }

                // Bottone conferma
                if (confirmButton != null &&
                    (go == confirmButton.gameObject || go.transform.IsChildOf(confirmButton.transform)))
                {
                    OnConfirm();
                    return;
                }
            }
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
                // Carica sprite creatura da Resources/Creatures/ (se presente)
                if (creatureImages != null && i < creatureImages.Length && creatureImages[i] != null)
                    CreatureSpriteLoader.Apply(creatureImages[i], i);
            }
        }

        private void SelectCard(int index)
        {
            _selectedIndex = index;

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

            if (selectedCreatureLabel != null && creatures != null && index < creatures.Length)
                selectedCreatureLabel.text = creatures[index].name.ToUpper();

            if (creatureCards != null && index < creatureCards.Length && creatureCards[index] != null)
                StartCoroutine(BounceCard(creatureCards[index].transform));
        }

        private void OnConfirm()
        {
            if (PlayerProfile.Instance != null)
                PlayerProfile.Instance.SelectedCreature = (CreatureType)_selectedIndex;

            Debug.Log($"[CreatureSelection] Creatura scelta: {(CreatureType)_selectedIndex}");

            if (ScreenManager.Instance != null)
                ScreenManager.Instance.ShowScreen(ScreenID.Home);
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
        private static bool IsUnder(GameObject go, Component[] arr, int i)
        {
            if (arr == null || i >= arr.Length || arr[i] == null || go == null) return false;
            return go == arr[i].gameObject || go.transform.IsChildOf(arr[i].transform);
        }
        private static bool IsUnder(GameObject go, GameObject[] arr, int i)
        {
            if (arr == null || i >= arr.Length || arr[i] == null || go == null) return false;
            return go == arr[i] || go.transform.IsChildOf(arr[i].transform);
        }

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
