// ============================================================
// WordValidator.cs
// Valida se una parola è presente nel dizionario standard (IT/EN)
// o nel dizionario fantasy condiviso (→ bonus "leggendario").
// ============================================================
// STEP 1 : stub compilabile — caricamento JSON e logica nello STEP 4.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace AppPuzz.Gameplay
{
    /// <summary>Risultato della validazione di una parola.</summary>
    public enum ValidationResult
    {
        Invalid,    // parola non trovata
        Valid,      // trovata nel dizionario standard
        Legendary   // trovata nel dizionario fantasy → bonus extra
    }

    /// <summary>
    /// Classe non-MonoBehaviour: non ha bisogno di stare nella scena,
    /// viene creata e usata da GameManager.
    /// Carica i dizionari JSON e valida le parole inserite dal giocatore.
    /// </summary>
    public class WordValidator
    {
        // ----------------------------------------------------------
        // Dizionari (HashSet per ricerca O(1))
        // ----------------------------------------------------------

        /// <summary>Dizionario standard della lingua corrente.</summary>
        private HashSet<string> _standardWords = new HashSet<string>();

        /// <summary>Dizionario fantasy condiviso IT+EN.</summary>
        private HashSet<string> _fantasyWords  = new HashSet<string>();

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Valida la parola fornita.
        /// Ordine di controllo: 1) dizionario standard, 2) dizionario fantasy.
        /// </summary>
        /// <param name="word">Parola da controllare (in minuscolo).</param>
        /// <returns><see cref="ValidationResult"/> corrispondente.</returns>
        public ValidationResult Validate(string word)
        {
            // TODO (STEP 4): implementare la ricerca nei HashSet caricati da JSON
            // Per ora sempre Invalid — il progetto compila comunque.
            Debug.Log($"[WordValidator] Validate('{word}') — da implementare nello STEP 4.");
            return ValidationResult.Invalid;
        }

        /// <summary>
        /// Carica i dizionari JSON dalla cartella Resources.
        /// Da chiamare all'avvio del gioco (es. da GameManager.Start).
        /// </summary>
        /// <param name="standardFileName">Es. "italian" o "english".</param>
        public void LoadDictionaries(string standardFileName)
        {
            // TODO (STEP 4): leggere Assets/Dictionaries via Resources.Load<TextAsset>
            // e popolare _standardWords e _fantasyWords
            Debug.Log($"[WordValidator] LoadDictionaries('{standardFileName}') — da implementare nello STEP 4.");
        }
    }
}
