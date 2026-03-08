// ============================================================
// GridManager.cs
// Gestisce la generazione e il layout della griglia 5×5 di LetterCell.
// Istanzia i prefab, assegna lettere ponderate, mantiene la matrice.
// ============================================================
// STEP 2 : GenerateGrid() e ResetAllCells() implementati.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using AppPuzz.Utils;
using AppPuzz.Localization;

namespace AppPuzz.Grid
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce la griglia 5×5.
    /// Deve essere attaccato a un GameObject nella scena Gameplay.
    /// Il gridContainer deve avere un GridLayoutGroup configurato 5×5.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Costanti
        // ----------------------------------------------------------
        public const int GRID_SIZE = 5; // 5 righe × 5 colonne

        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static GridManager Instance { get; private set; }

        // ----------------------------------------------------------
        // Configurazione (assegnata dall'Inspector)
        // ----------------------------------------------------------

        [Header("Prefab & Contenitore")]
        [Tooltip("Il prefab LetterCell da istanziare per ogni cella.")]
        public GameObject letterCellPrefab;

        [Tooltip("Il Transform del pannello UI che contiene le celle (GridLayoutGroup).")]
        public Transform gridContainer;

        // ----------------------------------------------------------
        // Stato interno
        // ----------------------------------------------------------

        /// <summary>Matrice 5×5 delle celle correnti.</summary>
        private LetterCell[,] _cells = new LetterCell[GRID_SIZE, GRID_SIZE];

        /// <summary>Validator passato dall'esterno (GameManager / TrainingManager / ArenaManager).</summary>
        private AppPuzz.Gameplay.WordValidator _validator;

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
            // La griglia viene generata da GameManager.StartGame()
            // Qui non la generiamo automaticamente per evitare doppie chiamate.
            Debug.Log("[GridManager] Pronto. In attesa di GenerateGrid() da GameManager.");
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Imposta il WordValidator da usare per garantire i requisiti minimi.
        /// Deve essere chiamato prima di GenerateGrid().
        /// </summary>
        public void SetValidator(AppPuzz.Gameplay.WordValidator validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Genera (o rigenera) l'intera griglia 5×5.
        /// Usa GridBuilder per garantire almeno:
        ///   1 parola da 9 lettere, 4 da 8, 6 da 7.
        /// Richiede che SetValidator() sia stato chiamato con dizionari caricati.
        /// </summary>
        public void GenerateGrid()
        {
            // 1. Distruggi le celle esistenti nel contenitore
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            // 2. Distribuzione lettere per lingua
            var weights = (LanguageManager.Instance?.CurrentLanguage == Language.English)
                ? WeightedRandom.EnglishWeights
                : WeightedRandom.ItalianWeights;

            // 3. GridBuilder genera char[,] con requisiti garantiti
            char[,] letters = GridBuilder.Build(weights, _validator);

            // 4. Istanzia celle con le lettere calcolate
            for (int r = 0; r < GRID_SIZE; r++)
            {
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    GameObject obj  = Instantiate(letterCellPrefab, gridContainer);
                    LetterCell cell = obj.GetComponent<LetterCell>();
                    cell.Initialize(letters[r, c], r, c);
                    _cells[r, c] = cell;
                }
            }

            // 5. Forza ricalcolo layout
            if (gridContainer is RectTransform gridRt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRt);

            Debug.Log("[GridManager] Griglia 5×5 generata con requisiti minimi garantiti.");
        }

        /// <summary>
        /// Restituisce la cella alla posizione (row, col), o null se fuori bounds.
        /// </summary>
        public LetterCell GetCell(int row, int col)
        {
            if (row < 0 || row >= GRID_SIZE || col < 0 || col >= GRID_SIZE)
                return null;
            return _cells[row, col];
        }

        /// <summary>
        /// Verifica se due celle sono adiacenti (le 8 direzioni incluse le diagonali).
        /// </summary>
        public bool AreAdjacent(LetterCell a, LetterCell b)
        {
            int dr = Mathf.Abs(a.Row - b.Row);
            int dc = Mathf.Abs(a.Col - b.Col);
            return dr <= 1 && dc <= 1 && !(dr == 0 && dc == 0);
        }

        /// <summary>
        /// Restituisce tutte le 25 celle della griglia (usato da WordSelector).
        /// </summary>
        public System.Collections.Generic.IEnumerable<LetterCell> GetAllCells()
        {
            for (int r = 0; r < GRID_SIZE; r++)
                for (int c = 0; c < GRID_SIZE; c++)
                    if (_cells[r, c] != null)
                        yield return _cells[r, c];
        }

        /// <summary>
        /// Deseleziona visivamente tutte le celle della griglia.
        /// Chiamato da WordSelector dopo ogni parola (valida o no).
        /// </summary>
        public void ResetAllCells()
        {
            foreach (var cell in _cells)
                cell?.SetSelected(false);
        }

        /// <summary>
        /// Sostituisce la lettera in una cella rotta con una nuova casuale.
        /// Resetta il contatore di colpi e lo stato ghiaccio.
        /// </summary>
        public void ReplaceCell(int row, int col)
        {
            if (row < 0 || row >= GRID_SIZE || col < 0 || col >= GRID_SIZE) return;
            LetterCell cell = _cells[row, col];
            if (cell == null) return;

            var weights = (AppPuzz.Localization.LanguageManager.Instance?.CurrentLanguage
                           == AppPuzz.Localization.Language.English)
                ? WeightedRandom.EnglishWeights
                : WeightedRandom.ItalianWeights;

            char newLetter = WeightedRandom.GetLetter(weights);
            cell.Initialize(newLetter, row, col);

            // Animazione breve: scala da 0 → 1 per segnalare la sostituzione
            StartCoroutine(ReplacePop(cell));
            Debug.Log($"[GridManager] Cella ({row},{col}) sostituita con '{newLetter}'.");
        }

        private System.Collections.IEnumerator ReplacePop(LetterCell cell)
        {
            if (cell == null) yield break;
            cell.transform.localScale = Vector3.zero;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;
                cell.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            cell.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Decrementa il counter di freeze su tutte le celle ghiacciate.
        /// Va chiamato dopo ogni parola corretta trovata (globalmente).
        /// </summary>
        public void DecrementAllFrozenCells()
        {
            foreach (var cell in _cells)
                cell?.DecrementFrozenTurn();
        }
    }
}
