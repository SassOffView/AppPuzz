// ============================================================
// CreatureController.cs
// Controlla la visualizzazione della creatura: quale variante
// è attiva, a quale livello di evoluzione si trova, e le reazioni
// ai cambi di energia (glow, animazioni).
// ============================================================
// STEP 1 : stub compilabile — reazioni visive nello STEP 6.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AppPuzz.Gameplay; // GameState

namespace AppPuzz.Creatures
{
    /// <summary>Le tre varianti iniziali di creatura selezionabili dal giocatore.</summary>
    public enum CreatureType
    {
        MentalDragon,   // Drago Mentale
        AstralWolf,     // Lupo Astrale
        EtherealSerpent // Serpente Etereo
    }

    /// <summary>
    /// Gestisce la creatura associata al giocatore:
    /// - selezione variante a inizio partita
    /// - evoluzione visiva al raggiungimento delle soglie energia
    /// - reazioni (glow/animazione) quando l'energia sale
    /// </summary>
    public class CreatureController : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static CreatureController Instance { get; private set; }

        // ----------------------------------------------------------
        // Configurazione (dall'Inspector)
        // ----------------------------------------------------------

        [Header("Configurazione Evoluzione")]
        [Tooltip("L'asset EvolutionConfig creato via ScriptableObject.")]
        public EvolutionConfig evolutionConfig;

        [Header("UI Creatura")]
        [Tooltip("L'Image UI che mostra lo sprite della creatura.")]
        public Image creatureImage;

        [Header("Variante iniziale")]
        [Tooltip("Tipo di creatura selezionata per questa partita.")]
        public CreatureType selectedCreature = CreatureType.MentalDragon;

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------

        private int _currentLevel = 1;
        private Coroutine _animCoroutine;

        /// <summary>Livello di evoluzione corrente (1, 2 o 3).</summary>
        public int CurrentLevel => _currentLevel;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            UpdateVisuals(1);
            Debug.Log($"[CreatureController] Creatura: {selectedCreature}, Livello: {_currentLevel}");
        }

        // ----------------------------------------------------------
        // API pubblica — chiamata da EnergyManager quando l'energia cambia
        // ----------------------------------------------------------

        /// <summary>
        /// Controlla se l'energia corrente fa scattare una nuova soglia di evoluzione.
        /// </summary>
        /// <param name="currentEnergy">Energia corrente del giocatore.</param>
        public void OnEnergyChanged(float currentEnergy)
        {
            if (evolutionConfig == null) return;

            int newLevel = evolutionConfig.GetLevel(currentEnergy);
            if (newLevel != _currentLevel)
            {
                _currentLevel = newLevel;
                UpdateVisuals(newLevel);
                Debug.Log($"[CreatureController] Evoluzione al livello {newLevel}!");
                TriggerAnimation(PlayEvolutionAnimation());
            }
            else
            {
                TriggerAnimation(PlayEnergyPulse());
            }
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private void UpdateVisuals(int level)
        {
            if (evolutionConfig == null || creatureImage == null) return;

            // Seleziona lo sprite corrispondente al livello
            Sprite targetSprite;
            if (level == 2)
                targetSprite = evolutionConfig.spriteLevel2;
            else if (level == 3)
                targetSprite = evolutionConfig.spriteLevel3;
            else
                targetSprite = evolutionConfig.spriteLevel1;

            if (targetSprite != null)
                creatureImage.sprite = targetSprite;

            // Tint in base alla variante: più luminoso a ogni livello
            Color baseColor = GetCreatureColor();
            float intensity = 0.4f + (level - 1) * 0.3f; // 0.4 / 0.7 / 1.0
            creatureImage.color = Color.Lerp(Color.white, baseColor, intensity);
        }

        // ----------------------------------------------------------
        // Animazioni
        // ----------------------------------------------------------

        private void TriggerAnimation(IEnumerator anim)
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(anim);
        }

        /// <summary>Pulsazione grande all'evoluzione: scala su → flash bianco → torna.</summary>
        private IEnumerator PlayEvolutionAnimation()
        {
            Transform t = creatureImage.transform;
            Vector3 original = t.localScale;
            Color originalColor = creatureImage.color;

            // Scala su in 0.15 s
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(original, original * 1.4f, elapsed / 0.15f);
                yield return null;
            }

            // Flash bianco
            creatureImage.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            creatureImage.color = originalColor;

            // Scala giù in 0.2 s
            elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(original * 1.4f, original, elapsed / 0.2f);
                yield return null;
            }
            t.localScale = original;
            _animCoroutine = null;
        }

        /// <summary>Pulsazione leggera ad ogni parola valida.</summary>
        private IEnumerator PlayEnergyPulse()
        {
            Transform t = creatureImage.transform;
            Vector3 original = t.localScale;

            // Scala su in 0.08 s
            float elapsed = 0f;
            while (elapsed < 0.08f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(original, original * 1.1f, elapsed / 0.08f);
                yield return null;
            }

            // Scala giù in 0.12 s
            elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(original * 1.1f, original, elapsed / 0.12f);
                yield return null;
            }
            t.localScale = original;
            _animCoroutine = null;
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private Color GetCreatureColor()
        {
            if (evolutionConfig == null) return Color.white;
            return selectedCreature switch
            {
                CreatureType.AstralWolf      => evolutionConfig.colorAstralWolf,
                CreatureType.EtherealSerpent => evolutionConfig.colorEtherealSerpent,
                _                            => evolutionConfig.colorMentalDragon,
            };
        }
    }
}
