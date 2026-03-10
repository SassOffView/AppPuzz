// ============================================================
// GridSolver.cs
// Trova tutte le parole valide nella griglia corrente tramite DFS.
// Usato a fine partita per mostrare le parole che si potevano trovare.
// ============================================================
using System.Collections.Generic;
using AppPuzz.Grid;

namespace AppPuzz.Gameplay
{
    public static class GridSolver
    {
        private const int MAX_WORD_LENGTH = 9;  // supporta parole fino a 9 lettere

        /// <summary>
        /// Restituisce tutte le parole valide trovabili nella griglia corrente,
        /// ordinate dalla più lunga alla più corta.
        /// </summary>
        public static List<string> FindAllWords(GridManager grid, WordValidator validator)
        {
            if (grid == null || validator == null) return new List<string>();

            var found = new HashSet<string>();
            int size = grid.CurrentSize;
            bool[,] visited = new bool[size, size];

            foreach (LetterCell start in grid.GetAllCells())
            {
                visited[start.Row, start.Col] = true;
                DFS(grid, validator, start, visited, start.Letter.ToString(), found);
                visited[start.Row, start.Col] = false;
            }

            var result = new List<string>(found);
            result.Sort((a, b) => b.Length == a.Length
                ? string.Compare(a, b, System.StringComparison.Ordinal)
                : b.Length.CompareTo(a.Length));
            return result;
        }

        private static void DFS(GridManager grid, WordValidator validator,
            LetterCell current, bool[,] visited, string word, HashSet<string> found)
        {
            if (word.Length >= 2 && validator.Validate(word.ToLower()) != ValidationResult.Invalid)
                found.Add(word.ToLower());

            if (word.Length >= MAX_WORD_LENGTH) return;

            for (int r = 0; r < grid.CurrentSize; r++)
            {
                for (int c = 0; c < grid.CurrentSize; c++)
                {
                    if (visited[r, c]) continue;
                    LetterCell next = grid.GetCell(r, c);
                    if (next == null) continue;
                    if (!grid.AreAdjacent(current, next)) continue;

                    visited[r, c] = true;
                    DFS(grid, validator, next, visited, word + next.Letter, found);
                    visited[r, c] = false;
                }
            }
        }
    }
}
