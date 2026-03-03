// ============================================================
// LanguageManager.cs
// Singleton che gestisce la lingua corrente del gioco (IT / EN).
// Gli altri sistemi lo interrogano per sapere quale dizionario caricare.
// ============================================================
// STEP 1 : stub compilabile — la logica dizionari arriva nello STEP 4.
// ============================================================

using UnityEngine;

namespace AppPuzz.Localization
{
    /// <summary>Lingue supportate dal gioco.</summary>
    public enum Language
    {
        Italian,
        English
    }

    /// <summary>
    /// Singleton MonoBehaviour. Un solo oggetto nella scena gestisce
    /// la lingua e notifica i sistemi dipendenti quando cambia.
    /// </summary>
    public class LanguageManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static LanguageManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Stato
        // ----------------------------------------------------------

        /// <summary>Lingua attiva al momento. Default: italiano.</summary>
        [Header("Impostazioni Lingua")]
        [Tooltip("Lingua di partenza quando si avvia il gioco.")]
        public Language startLanguage = Language.Italian;

        /// <summary>Lingua corrente accessibile da tutti gli script.</summary>
        public Language CurrentLanguage { get; private set; }

        /// <summary>
        /// Evento statico: viene invocato ogni volta che la lingua cambia.
        /// Chiunque dipenda dalla lingua (es. GameManager) si sottoscrive qui.
        /// </summary>
        public static event System.Action<Language> OnLanguageChanged;

        /// <summary>
        /// Evento lanciato ogni volta che la lingua cambia.
        /// Sottoscrivi per ricaricare dizionari o aggiornare testi.
        /// </summary>
        public static event System.Action<Language> OnLanguageChanged;

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Awake()
        {
            // Pattern Singleton: se esiste già un'istanza, distruggi questo duplicato
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);     // assicura che sia un root object
            DontDestroyOnLoad(gameObject); // sopravvive al cambio scena

            CurrentLanguage = startLanguage;
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Cambia la lingua attiva a runtime (es. da un menu impostazioni).
        /// </summary>
        public void SetLanguage(Language newLanguage)
        {
            CurrentLanguage = newLanguage;
            Debug.Log($"[LanguageManager] Lingua cambiata a: {newLanguage}");
            OnLanguageChanged?.Invoke(newLanguage);
        }

        /// <summary>
        /// Restituisce il nome del file JSON della lingua corrente.
        /// Es. "italian" oppure "english".
        /// </summary>
        public string GetDictionaryFileName()
        {
            return CurrentLanguage == Language.Italian ? "italian" : "english";
        }
    }
}
