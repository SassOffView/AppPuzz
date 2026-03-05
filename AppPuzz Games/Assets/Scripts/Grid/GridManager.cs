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
        /// Genera (o rigenera) l'intera griglia 5×5.
        /// Distrugge le celle precedenti e ne crea di nuove con lettere casuali
        /// distribuite in base alla lingua corrente.
        /// </summary>
        public void GenerateGrid()
        {
            // 1. Distruggi le celle esistenti nel contenitore
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            // 2. Scegli la distribuzione di lettere in base alla lingua corrente
            var weights = (LanguageManager.Instance?.CurrentLanguage == Language.English)
                ? WeightedRandom.EnglishWeights
                : WeightedRandom.ItalianWeights;

            // 3. Istanzia ogni cella e inizializzala
            for (int r = 0; r < GRID_SIZE; r++)
            {
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    GameObject obj = Instantiate(letterCellPrefab, gridContainer);
                    LetterCell cell = obj.GetComponent<LetterCell>();

                    char letter = WeightedRandom.GetLetter(weights);
                    cell.Initialize(letter, r, c);

                    _cells[r, c] = cell;
                }
            }

            // Forza il GridLayoutGroup a ricalcolare le posizioni subito.
            // Senza questo le celle rimangono ad anchoredPos=(0,0) finche
            // il Canvas non esegue il suo prossimo layout pass.
            if (gridContainer is RectTransform gridRt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRt);

            Debug.Log("[GridManager] Griglia 5×5 generata.");
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
    }
}
