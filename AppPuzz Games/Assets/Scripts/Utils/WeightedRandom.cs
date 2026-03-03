// ============================================================
// WeightedRandom.cs
// Utility statica per estrarre lettere con distribuzione ponderata.
// Non dipende da Unity (nessun MonoBehaviour) → testabile anche fuori editor.
// ============================================================
// STEP 2 : frequenze reali IT/EN implementate + algoritmo weighted random.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace AppPuzz.Utils
{
    /// <summary>
    /// Fornisce metodi per estrarre elementi casuali da una distribuzione ponderata.
    /// Viene usato da GridManager per generare le lettere della griglia.
    /// </summary>
    public static class WeightedRandom
    {
        // ----------------------------------------------------------
        // Distribuzioni per lingua
        // Chiave = lettera, Valore = peso relativo (frequenza percentuale)
        // ----------------------------------------------------------

        /// <summary>
        /// Distribuzione lettere per la lingua italiana.
        /// Basata sulle frequenze reali dell'italiano scritto.
        /// Nota: J, K, W, X, Y non sono usate in italiano nativo.
        /// </summary>
        public static readonly Dictionary<char, float> ItalianWeights = new Dictionary<char, float>
        {
            { 'A', 11.7f },
            { 'B',  1.0f },
            { 'C',  4.5f },
            { 'D',  3.7f },
            { 'E', 11.8f },
            { 'F',  1.1f },
            { 'G',  1.6f },
            { 'H',  1.5f },
            { 'I', 10.1f },
            { 'L',  6.5f },
            { 'M',  2.5f },
            { 'N',  6.9f },
            { 'O',  9.8f },
            { 'P',  3.0f },
            { 'Q',  0.5f },
            { 'R',  6.4f },
            { 'S',  5.0f },
            { 'T',  5.6f },
            { 'U',  3.0f },
            { 'V',  2.1f },
            { 'Z',  0.5f },
        };

        /// <summary>
        /// Distribuzione lettere per la lingua inglese.
        /// Basata sulle frequenze reali dell'inglese scritto.
        /// </summary>
        public static readonly Dictionary<char, float> EnglishWeights = new Dictionary<char, float>
        {
            { 'A',  8.2f },
            { 'B',  1.5f },
            { 'C',  2.8f },
            { 'D',  4.3f },
            { 'E', 13.0f },
            { 'F',  2.2f },
            { 'G',  2.0f },
            { 'H',  6.1f },
            { 'I',  7.0f },
            { 'J',  0.2f },
            { 'K',  0.8f },
            { 'L',  4.0f },
            { 'M',  2.4f },
            { 'N',  6.7f },
            { 'O',  7.5f },
            { 'P',  1.9f },
            { 'Q',  0.1f },
            { 'R',  6.0f },
            { 'S',  6.3f },
            { 'T',  9.1f },
            { 'U',  2.8f },
            { 'V',  1.0f },
            { 'W',  2.4f },
            { 'X',  0.2f },
            { 'Y',  2.0f },
            { 'Z',  0.1f },
        };

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Estrae una lettera casuale dal dizionario dei pesi fornito.
        /// Usa l'algoritmo cumulativo: somma i pesi e sceglie in base a un valore random.
        /// </summary>
        /// <param name="weights">Dizionario char→peso. I pesi non devono necessariamente
        /// sommare a 1: il metodo li normalizza internamente.</param>
        /// <returns>La lettera estratta secondo la distribuzione ponderata.</returns>
        public static char GetLetter(Dictionary<char, float> weights)
        {
            // Calcola il totale dei pesi
            float total = 0f;
            foreach (var kvp in weights)
                total += kvp.Value;

            // Estrai un valore casuale nell'intervallo [0, total)
            float roll = Random.Range(0f, total);

            // Trova la lettera corrispondente tramite peso cumulativo
            float cumulative = 0f;
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (roll < cumulative)
                    return kvp.Key;
            }

            // Fallback di sicurezza (non dovrebbe mai accadere)
            return 'A';
        }
    }
}
