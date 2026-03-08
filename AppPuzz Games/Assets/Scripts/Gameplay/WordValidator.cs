// ============================================================
// WordValidator.cs
// Valida se una parola è presente nel dizionario standard (IT/EN)
// o nel dizionario fantasy condiviso (→ bonus "leggendario").
// ============================================================
// STEP 1 : stub compilabile — caricamento JSON e logica nello STEP 4.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppPuzz.Gameplay
{
    [Serializable]
    internal class WordList
    {
        public string[] words;
    }
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

        /// <summary>Indice per lunghezza — accesso rapido per GridBuilder.</summary>
        private readonly Dictionary<int, List<string>> _byLength = new Dictionary<int, List<string>>();

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Valida la parola fornita.
        /// Ordine: 1) fantasy (Legendary), 2) standard (Valid), 3) Invalid.
        /// </summary>
        /// <param name="word">Parola da controllare (già in minuscolo).</param>
        public ValidationResult Validate(string word)
        {
            if (_fantasyWords.Contains(word))  return ValidationResult.Legendary;
            if (_standardWords.Contains(word)) return ValidationResult.Valid;
            return ValidationResult.Invalid;
        }

        /// <summary>
        /// Tutte le parole standard di esattamente <paramref name="len"/> lettere.
        /// Usato da GridBuilder per l'embedding di parole lunghe.
        /// </summary>
        public IReadOnlyList<string> GetWordsOfLength(int len) =>
            _byLength.TryGetValue(len, out var list) ? list : System.Array.Empty<string>();

        /// <summary>True se i dizionari sono stati caricati.</summary>
        public bool IsLoaded => _standardWords.Count > 0;

        /// <summary>
        /// Carica i dizionari JSON da Assets/Resources/Dictionaries/.
        /// Da chiamare all'avvio tramite GameManager.StartGame().
        /// </summary>
        /// <param name="standardFileName">Nome file senza estensione: "italian" o "english".</param>
        public void LoadDictionaries(string standardFileName)
        {
            _standardWords.Clear();
            _fantasyWords.Clear();
            _byLength.Clear();

            LoadInto($"Dictionaries/{standardFileName}", _standardWords);
            LoadInto("Dictionaries/fantasy_shared", _fantasyWords);

            // Costruisce indice per lunghezza (solo parole standard)
            foreach (string w in _standardWords)
            {
                int len = w.Length;
                if (!_byLength.ContainsKey(len)) _byLength[len] = new List<string>();
                _byLength[len].Add(w);
            }
            Debug.Log($"[WordValidator] Indice costruito per {_byLength.Count} lunghezze diverse.");
        }

        // ----------------------------------------------------------
        // Privato
        // ----------------------------------------------------------

        private static void LoadInto(string resourcePath, HashSet<string> target)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[WordValidator] File non trovato: Resources/{resourcePath}.json");
                return;
            }

            WordList list = JsonUtility.FromJson<WordList>(asset.text);
            if (list?.words == null) return;

            foreach (string w in list.words)
                if (!string.IsNullOrEmpty(w))
                    target.Add(w.ToLower());

            Debug.Log($"[WordValidator] Caricato '{resourcePath}': {target.Count} parole.");
        }
    }
}
