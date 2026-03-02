// ============================================================
// WeightedRandom.cs
// Utility statica per estrarre lettere con distribuzione ponderata.
// Non dipende da Unity (nessun MonoBehaviour) → testabile anche fuori editor.
// ============================================================
// STEP 1 : stub compilabile — la logica reale arriva nello STEP 2.
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
        // Distribuzioni per lingua (verranno popolate nello STEP 2)
        // Chiave = lettera, Valore = peso relativo (es. A=14, Z=1)
        // ----------------------------------------------------------

        /// <summary>Distribuzione lettere per la lingua italiana.</summary>
        public static readonly Dictionary<char, float> ItalianWeights = new Dictionary<char, float>
        {
            // TODO (STEP 2): popolare con le frequenze reali dell'italiano
            { 'A', 1f }
        };

        /// <summary>Distribuzione lettere per la lingua inglese.</summary>
        public static readonly Dictionary<char, float> EnglishWeights = new Dictionary<char, float>
        {
            // TODO (STEP 2): popolare con le frequenze reali dell'inglese
            { 'A', 1f }
        };

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Estrae una lettera casuale dal dizionario dei pesi fornito.
        /// </summary>
        /// <param name="weights">Dizionario char→peso. I pesi non devono necessariamente
        /// sommare a 1: il metodo li normalizza internamente.</param>
        /// <returns>La lettera estratta.</returns>
        public static char GetLetter(Dictionary<char, float> weights)
        {
            // TODO (STEP 2): implementare l'algoritmo weighted random
            // Per ora restituisce sempre 'A' per far compilare il progetto.
            return 'A';
        }
    }
}
