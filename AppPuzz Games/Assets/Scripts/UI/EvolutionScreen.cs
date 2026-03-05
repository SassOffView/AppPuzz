// ============================================================
// EvolutionScreen.cs
// Schermata Evoluzione: mostra stato attuale della creatura,
// requisiti per evolvere, animazione evoluzione epica.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Utils;
using AppPuzz.Creatures;

namespace AppPuzz.UI
{
    /// <summary>
    /// Controller dello screen Evoluzione stile Pokémon.
    /// Mostra creatura corrente, livello, XP, e permette di evocare l'evoluzione.
    /// </summary>
    public class EvolutionScreen : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Inspector
        // ----------------------------------------------------------

        [Header("Info creatura")]
        public Image           creatureImage;
        public TextMeshProUGUI creatureNameText;
        public TextMeshProUGUI creatureLevelText;
        public TextMeshProUGUI creatureTypeText;
        public Image           typeBadge;

        [Header("Barra XP evoluzione")]
        public Slider          evolutionXPBar;
        public TextMeshProUGUI evolutionXPText;
        public TextMeshProUGUI evolutionRequirementText;

        [Header("Forme (sprite per livello)")]
        public GameObject[]    formPanels;       // panel[0]=Forma1, [1]=Forma2, [2]=Forma3
        public Image[]         formLockIcons;    // lucchetto su forme non sbloccate
        public TextMeshProUGUI[] formLevelLabels;

        [Header("Pulsante evoluzione")]
        public Button          evolveButton;
        public TextMeshProUGUI evolveButtonText;
        public GameObject      evolveButtonGlow;

        [Header("Animazione evoluzione")]
        public GameObject      evolutionFlashOverlay;   // overlay bianco
        public ParticleSystem  evolutionParticles;

        [Header("Stats (post-evoluzione)")]
        public TextMeshProUGUI statPowerText;
        public TextMeshProUGUI statSpeedText;
        public TextMeshProUGUI statEnduranceText;

        [Header("Pulsanti navigazione")]
        public Button backButton;

        // ----------------------------------------------------------
        // Configurazione soglie XP
        // ----------------------------------------------------------
        private static readonly float[] EVOLUTION_THRESHOLDS = { 0f, 50f, 120f };
        private static readonly string[] FORM_NAMES = { "Forma Base", "Prima Evoluzione", "Forma Finale" };

        private static readonly (int power, int speed, int endurance)[] STATS_BY_LEVEL =
        {
            (3, 3, 3),
            (6, 6, 6),
            (9, 9, 9)
        };

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Start() { /* click handled in Update() */ }
        private void Update()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) clicked = true;
            if (!clicked) return;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return;
            Vector2 pos = (Input.touchCount > 0) ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            var pointer = new UnityEngine.EventSystems.PointerEventData(es) { position = pos };
            var results = new List<UnityEngine.EventSystems.RaycastResult>();
            es.RaycastAll(pointer, results);
            foreach (var r in results)
            {
                var go = r.gameObject;
                if (IsUnder(go, evolveButton)) { OnEvolve(); return; }
                if (IsUnder(go, backButton))   { ScreenManager.Instance?.Back(); return; }
            }
        }
        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }


        private void OnEnable()
        {
            RefreshUI();
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------
        private void RefreshUI()
        {
            var profile = PlayerProfile.Instance;
            if (profile == null) return;

            float xp          = profile.CurrentXP;
            int   currentLevel = GetCurrentLevel(xp);

            // ---- Info creatura ----
            string creatureName = GetCreatureName(profile.SelectedCreature);
            if (creatureNameText  != null) creatureNameText.text  = creatureName;
            if (creatureLevelText != null) creatureLevelText.text = $"Livello {currentLevel + 1}";
            if (creatureTypeText  != null) creatureTypeText.text  = GetCreatureTypeName(profile.SelectedCreature);
            if (typeBadge         != null) typeBadge.color        = UITheme.CreatureTypeColor(profile.SelectedCreature.ToString());

            // ---- Barra XP ----
            float nextThreshold = (currentLevel < 2) ? EVOLUTION_THRESHOLDS[currentLevel + 1] : EVOLUTION_THRESHOLDS[2];
            float prevThreshold = EVOLUTION_THRESHOLDS[currentLevel];
            float progress      = (currentLevel < 2) ? (xp - prevThreshold) / (nextThreshold - prevThreshold) : 1f;

            if (evolutionXPBar  != null) evolutionXPBar.value = Mathf.Clamp01(progress);
            if (evolutionXPText != null) evolutionXPText.text = $"{xp:F0} XP";
            if (evolutionRequirementText != null)
            {
                if (currentLevel < 2)
                    evolutionRequirementText.text = $"Richiesti: {nextThreshold:F0} XP";
                else
                    evolutionRequirementText.text = "FORMA FINALE raggiunta!";
            }

            // ---- Forme ----
            for (int i = 0; i < 3; i++)
            {
                bool unlocked = i <= currentLevel;
                if (formPanels   != null && i < formPanels.Length   && formPanels[i]   != null)
                    formPanels[i].SetActive(true);
                if (formLockIcons!= null && i < formLockIcons.Length && formLockIcons[i]!= null)
                    formLockIcons[i].gameObject.SetActive(!unlocked);
                if (formLevelLabels != null && i < formLevelLabels.Length && formLevelLabels[i] != null)
                {
                    formLevelLabels[i].text  = FORM_NAMES[i];
                    formLevelLabels[i].color = unlocked ? UITheme.Colors.Gold : UITheme.Colors.TextSecondary;
                }
            }

            // ---- Stats ----
            var (pw, sp, en) = STATS_BY_LEVEL[currentLevel];
            if (statPowerText     != null) statPowerText.text     = $"Potere: {"■".PadRight(pw, '■').PadRight(10, '□')}";
            if (statSpeedText     != null) statSpeedText.text     = $"Velocità: {"■".PadRight(sp, '■').PadRight(10, '□')}";
            if (statEnduranceText != null) statEnduranceText.text = $"Resistenza: {"■".PadRight(en, '■').PadRight(10, '□')}";

            // ---- Pulsante evoluzione ----
            bool canEvolve = currentLevel < 2 && xp >= EVOLUTION_THRESHOLDS[currentLevel + 1];
            if (evolveButton != null) evolveButton.interactable = canEvolve;
            if (evolveButtonText != null)
                evolveButtonText.text = canEvolve ? UITheme.Strings.EvolveBtn : "XP insufficiente";
            if (evolveButtonGlow != null) evolveButtonGlow.SetActive(canEvolve);
        }

        private void OnEvolve()
        {
            var profile = PlayerProfile.Instance;
            if (profile == null) return;

            int currentLevel = GetCurrentLevel(profile.CurrentXP);
            if (currentLevel >= 2) return;

            // Riduci XP (speso per evolvere)
            profile.CurrentXP -= EVOLUTION_THRESHOLDS[currentLevel + 1] - EVOLUTION_THRESHOLDS[currentLevel];

            StartCoroutine(PlayEvolutionAnimation());
        }

        private IEnumerator PlayEvolutionAnimation()
        {
            // Flash bianco
            if (evolutionFlashOverlay != null)
            {
                evolutionFlashOverlay.SetActive(true);
                var cg = evolutionFlashOverlay.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    float t = 0f;
                    while (t < 0.4f) { cg.alpha = Mathf.Lerp(0f, 1f, t / 0.4f); t += Time.deltaTime; yield return null; }
                    cg.alpha = 1f;
                    yield return new WaitForSeconds(0.5f);
                    t = 0f;
                    while (t < 0.4f) { cg.alpha = Mathf.Lerp(1f, 0f, t / 0.4f); t += Time.deltaTime; yield return null; }
                    cg.alpha = 0f;
                }
                evolutionFlashOverlay.SetActive(false);
            }

            // Particelle
            evolutionParticles?.Play();

            // Aggiorna CreatureController se presente
            CreatureController.Instance?.OnEnergyChanged(PlayerProfile.Instance?.CurrentXP ?? 0f);

            // Aggiorna UI
            RefreshUI();

            Debug.Log("[EvolutionScreen] Evoluzione completata!");
        }

        // ---- Helpers ----
        private static int GetCurrentLevel(float xp)
        {
            if (xp >= EVOLUTION_THRESHOLDS[2]) return 2;
            if (xp >= EVOLUTION_THRESHOLDS[1]) return 1;
            return 0;
        }

        private static string GetCreatureName(CreatureType t) => t switch
        {
            CreatureType.MentalDragon    => "Mental Dragon",
            CreatureType.AstralWolf      => "Astral Wolf",
            CreatureType.EtherealSerpent => "Ethereal Serpent",
            _                            => "Creatura"
        };

        private static string GetCreatureTypeName(CreatureType t) => t switch
        {
            CreatureType.MentalDragon    => "TIPO: DRAGO MENTALE",
            CreatureType.AstralWolf      => "TIPO: LUPO ASTRALE",
            CreatureType.EtherealSerpent => "TIPO: SERPENTE ETEREO",
            _                            => "TIPO: SCONOSCIUTO"
        };
    }
}
