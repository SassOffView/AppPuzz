// ============================================================
// EvolutionConfig.cs
// ScriptableObject di configurazione per soglie di evoluzione
// e riferimenti agli effetti visivi della creatura.
// ============================================================
// STEP 1 : stub compilabile — FX e animazioni nello STEP 6.
// ============================================================

using UnityEngine;

namespace AppPuzz.Creatures
{
    /// <summary>
    /// ScriptableObject: crea un asset in Unity con
    /// tasto destro → Create → AppPuzz → EvolutionConfig.
    /// Permette di modificare le soglie di evoluzione senza toccare il codice.
    /// </summary>
    [CreateAssetMenu(fileName = "EvolutionConfig", menuName = "AppPuzz/EvolutionConfig")]
    public class EvolutionConfig : ScriptableObject
    {
        // ----------------------------------------------------------
        // Soglie di evoluzione (MVP: 3 livelli)
        // ----------------------------------------------------------

        [Header("Soglie Energia per Livello")]
        [Tooltip("Energia minima per raggiungere il livello 1 (stato iniziale).")]
        public float thresholdLevel1 = 0f;

        [Tooltip("Energia minima per raggiungere il livello 2.")]
        public float thresholdLevel2 = 50f;

        [Tooltip("Energia minima per raggiungere il livello 3.")]
        public float thresholdLevel3 = 120f;

        // ----------------------------------------------------------
        // Effetti visivi (placeholder — verranno collegati nello STEP 6)
        // ----------------------------------------------------------

        [Header("Sprite per livello (opzionale STEP 6)")]
        public Sprite spriteLevel1;
        public Sprite spriteLevel2;
        public Sprite spriteLevel3;

        [Header("Colori per variante creatura (STEP 6)")]
        [Tooltip("Colore tint per il Drago Mentale.")]
        public Color colorMentalDragon    = new Color(0.7f, 0.4f, 1.0f); // viola

        [Tooltip("Colore tint per il Lupo Astrale.")]
        public Color colorAstralWolf      = new Color(0.4f, 0.8f, 1.0f); // ciano

        [Tooltip("Colore tint per il Serpente Etereo.")]
        public Color colorEtherealSerpent = new Color(0.4f, 1.0f, 0.5f); // verde

        // ----------------------------------------------------------
        // Metodo di utilità
        // ----------------------------------------------------------

        /// <summary>
        /// Restituisce il livello di evoluzione (1, 2 o 3) dato il valore energia corrente.
        /// </summary>
        public int GetLevel(float energy)
        {
            if (energy >= thresholdLevel3) return 3;
            if (energy >= thresholdLevel2) return 2;
            return 1;
        }
    }
}
