// ============================================================
// LetterScoring.cs — Valori per lettera e moltiplicatori parola.
// Supporta dizionario Italiano ed Inglese.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace AppPuzz.Gameplay
{
    /// <summary>
    /// Sistema statico di punteggio basato sul valore delle singole lettere
    /// e su moltiplicatori legati alla lunghezza della parola trovata.
    /// </summary>
    public static class LetterScoring
    {
        // ── Valori per lettera – Italiano ────────────────────────
        // 5 punti: Z X H J Q Y W K
        // 4 punti: B V
        // 3 punti: F P S M U G
        // 2 punti: R T N C D L
        // 1 punto : A E I O
        private static readonly Dictionary<char, int> _italianValues = new()
        {
            { 'z', 5 }, { 'x', 5 }, { 'h', 5 }, { 'j', 5 },
            { 'q', 5 }, { 'y', 5 }, { 'w', 5 }, { 'k', 5 },
            { 'b', 4 }, { 'v', 4 },
            { 'f', 3 }, { 'p', 3 }, { 's', 3 }, { 'm', 3 }, { 'u', 3 }, { 'g', 3 },
            { 'r', 2 }, { 't', 2 }, { 'n', 2 }, { 'c', 2 }, { 'd', 2 }, { 'l', 2 },
            { 'a', 1 }, { 'e', 1 }, { 'i', 1 }, { 'o', 1 },
        };

        // ── Valori per lettera – Inglese ─────────────────────────
        // 5 punti: Z X Q
        // 4 punti: B V
        // 3 punti: F P S M U G H J
        // 2 punti: R T N C D L W K Y
        // 1 punto : A E I O
        private static readonly Dictionary<char, int> _englishValues = new()
        {
            { 'z', 5 }, { 'x', 5 }, { 'q', 5 },
            { 'b', 4 }, { 'v', 4 },
            { 'f', 3 }, { 'p', 3 }, { 's', 3 }, { 'm', 3 }, { 'u', 3 }, { 'g', 3 },
            { 'h', 3 }, { 'j', 3 },
            { 'r', 2 }, { 't', 2 }, { 'n', 2 }, { 'c', 2 }, { 'd', 2 }, { 'l', 2 },
            { 'w', 2 }, { 'k', 2 }, { 'y', 2 },
            { 'a', 1 }, { 'e', 1 }, { 'i', 1 }, { 'o', 1 },
        };

        // ── Moltiplicatori per lunghezza parola ──────────────────
        // Indice = numero di lettere (0-based, 0-4 = ×1)
        private static readonly float[] _lengthMultipliers =
        {
            1.00f, // 0 lettere  (non usato)
            1.00f, // 1 lettera  (non usato)
            1.00f, // 2 lettere
            1.00f, // 3 lettere
            1.00f, // 4 lettere
            1.25f, // 5 lettere
            1.50f, // 6 lettere
            1.75f, // 7 lettere
            2.00f, // 8 lettere
            2.50f, // 9 lettere
            3.00f, // 10 lettere
            3.50f, // 11 lettere
            4.00f, // 12 lettere (cap)
        };

        // ─────────────────────────────────────────────────────────
        // API pubblica
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Restituisce il valore in punti di una singola lettera
        /// in base alla lingua corrente (italiano / inglese).
        /// </summary>
        public static int GetLetterValue(char letter, bool isItalian)
        {
            char lower = char.ToLower(letter);
            var dict = isItalian ? _italianValues : _englishValues;
            return dict.TryGetValue(lower, out int value) ? value : 1;
        }

        /// <summary>Somma i valori di tutte le lettere della parola.</summary>
        public static int CalculateLetterSum(string word, bool isItalian)
        {
            int total = 0;
            foreach (char c in word)
                total += GetLetterValue(c, isItalian);
            return total;
        }

        /// <summary>
        /// Restituisce il moltiplicatore in base alla lunghezza della parola.
        /// (×1 per ≤4 lettere, fino a ×4 per ≥12 lettere)
        /// </summary>
        public static float GetLengthMultiplier(int wordLength)
        {
            int idx = Mathf.Clamp(wordLength, 0, _lengthMultipliers.Length - 1);
            return _lengthMultipliers[idx];
        }

        /// <summary>
        /// Calcola il punteggio base della parola:
        /// somma(valori lettere) × moltiplicatore_lunghezza.
        /// Non include streak né bonus leggendario.
        /// </summary>
        public static float CalculateBaseScore(string word, bool isItalian)
        {
            float sum  = CalculateLetterSum(word, isItalian);
            float mult = GetLengthMultiplier(word.Length);
            return sum * mult;
        }
    }
}
