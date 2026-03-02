// ============================================================
// GridManager.cs
// Gestisce la generazione e il layout della griglia 5×5 di LetterCell.
// Istanzia i prefab, assegna lettere ponderate, mantiene la matrice.
// ============================================================
// STEP 1 : stub compilabile — la griglia reale viene costruita nello STEP 2.
// ============================================================

using UnityEngine;
using AppPuzz.Utils;        // WeightedRandom

namespace AppPuzz.Grid
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce la griglia 5×5.
    /// Deve essere attaccato a un GameObject nella scena Gameplay.
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
            // TODO (STEP 2): chiamare GenerateGrid() quando la scena è pronta
            Debug.Log("[GridManager] Pronto. GenerateGrid() verrà implementata nello STEP 2.");
        }

        // ----------------------------------------------------------
        // API pubblica
        // ----------------------------------------------------------

        /// <summary>
        /// Genera (o rigenera) l'intera griglia 5×5.
        /// Distrugge le celle precedenti e ne crea di nuove con lettere casuali.
        /// </summary>
        public void GenerateGrid()
        {
            // TODO (STEP 2): implementare
            // 1. Distruggere le celle esistenti in gridContainer
            // 2. Per ogni (row, col): istanziare letterCellPrefab
            // 3. Chiamare cell.Initialize(lettera, row, col)
            // 4. Salvare in _cells[row, col]
            Debug.Log("[GridManager] GenerateGrid() — da implementare nello STEP 2.");
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
    }
}
