// ============================================================
// GridBuilder.cs
// Algoritmo garantito per generare griglie 5×5 con word minima:
//   • ≥ 1 parola da 9 lettere
//   • ≥ 4 parole da 8 lettere
//   • ≥ 6 parole da 7 lettere
//
// Strategia:
//   1. Tenta di "piantare" una parola da 9 lettere su un percorso
//      snake casuale nella griglia.
//   2. Riempie le celle rimanenti con lettere frequenza-ponderata.
//   3. Verifica i requisiti tramite DFS leggero sulla char[,].
//   4. Se i requisiti non sono soddisfatti, riprova fino a MAX_ATTEMPTS.
//   5. Alla fine restituisce il migliore risultato ottenuto.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using AppPuzz.Gameplay;

namespace AppPuzz.Grid
{
    public static class GridBuilder
    {
        // ----------------------------------------------------------
        // Requisiti minimi garantiti
        // ----------------------------------------------------------
        public const int MIN_WORDS_9 = 1;
        public const int MIN_WORDS_8 = 4;
        public const int MIN_WORDS_7 = 6;

        private static int _size        = GridManager.GRID_SIZE; // default 5
        private const int MAX_ATTEMPTS = 50;
        private const int MAX_DEPTH    = 9;  // lunghezza massima ricercata

        // ----------------------------------------------------------
        // Entry point
        // ----------------------------------------------------------

        /// <summary>
        /// Genera un char[5,5] di lettere maiuscole che rispettano i requisiti minimi.
        /// <paramref name="validator"/> può essere null: in quel caso si usa puro random.
        /// </summary>
        public static char[,] Build(Dictionary<char, float> weights, WordValidator validator, int gridSize = 5)
        {
            _size = gridSize;
            bool canEmbed = validator != null && validator.IsLoaded;

            IReadOnlyList<string> pool9 = canEmbed
                ? validator.GetWordsOfLength(9)
                : System.Array.Empty<string>();

            char[,] best    = null;
            int     bestScore = -1;

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                char[,] grid;

                // Prima metà dei tentativi: embedding (se parole disponibili)
                if (canEmbed && pool9.Count > 0 && attempt < MAX_ATTEMPTS / 2)
                    grid = BuildWithEmbedded(weights, pool9);
                else
                    grid = BuildRandom(weights);

                var (c9, c8, c7) = CountByLength(grid, validator);
                int score = c9 * 1000 + c8 * 100 + c7;

                if (c9 >= MIN_WORDS_9 && c8 >= MIN_WORDS_8 && c7 >= MIN_WORDS_7)
                {
                    Debug.Log($"[GridBuilder] OK — {c9}×9L {c8}×8L {c7}×7L (tentativo {attempt + 1}/{MAX_ATTEMPTS})");
                    return grid;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best      = grid;
                }
            }

            // Decomponi punteggio per il log
            int b9 = bestScore / 1000;
            int b8 = (bestScore % 1000) / 100;
            int b7 = bestScore % 100;
            Debug.LogWarning($"[GridBuilder] Requisiti non raggiunti in {MAX_ATTEMPTS} tentativi. " +
                             $"Miglior risultato: {b9}×9L {b8}×8L {b7}×7L. " +
                             $"Verifica che i dizionari siano caricati in Resources/Dictionaries/.");
            return best ?? BuildRandom(weights);
        }

        // ----------------------------------------------------------
        // Costruzione griglia con parola da 9 lettere embedded
        // ----------------------------------------------------------

        private static char[,] BuildWithEmbedded(Dictionary<char, float> weights,
            IReadOnlyList<string> pool9)
        {
            // Prendi parola casuale dal pool
            string word = pool9[Random.Range(0, pool9.Count)];

            // Trova percorso snake di 9 celle adiacenti
            var path = FindRandomPath(word.Length);

            // Genera griglia base random
            char[,] grid = BuildRandom(weights);

            // Pianta le lettere della parola sul percorso
            for (int i = 0; i < word.Length && i < path.Count; i++)
                grid[path[i].r, path[i].c] = char.ToUpper(word[i]);

            return grid;
        }

        private static char[,] BuildRandom(Dictionary<char, float> weights)
        {
            var grid = new char[_size, _size];
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                    grid[r, c] = AppPuzz.Utils.WeightedRandom.GetLetter(weights);
            return grid;
        }

        // ----------------------------------------------------------
        // Percorso snake casuale di esattamente `length` celle
        // ----------------------------------------------------------

        private static List<(int r, int c)> FindRandomPath(int length)
        {
            // Prova ogni cella come punto di partenza in ordine casuale
            var starts = new List<(int r, int c)>(_size * _size);
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                    starts.Add((r, c));
            Shuffle(starts);

            foreach (var start in starts)
            {
                var path    = new List<(int r, int c)> { start };
                var visited = new bool[_size, _size];
                visited[start.r, start.c] = true;
                if (DFSPath(path, visited, length))
                    return path;
            }

            // Fallback: percorso a serpentina deterministica (row 0 →, row 1 ←, …)
            return SnakeFallback(length);
        }

        private static bool DFSPath(List<(int r, int c)> path, bool[,] visited, int target)
        {
            if (path.Count == target) return true;

            var (r, c) = path[path.Count - 1];
            var neighbors = ShuffledNeighbors(r, c);

            foreach (var (nr, nc) in neighbors)
            {
                if (visited[nr, nc]) continue;
                path.Add((nr, nc));
                visited[nr, nc] = true;
                if (DFSPath(path, visited, target)) return true;
                path.RemoveAt(path.Count - 1);
                visited[nr, nc] = false;
            }
            return false;
        }

        private static List<(int r, int c)> ShuffledNeighbors(int r, int c)
        {
            var list = new List<(int, int)>(8);
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nr < _size && nc >= 0 && nc < _size)
                        list.Add((nr, nc));
                }
            Shuffle(list);
            return list;
        }

        private static List<(int r, int c)> SnakeFallback(int length)
        {
            var path = new List<(int, int)>(length);
            for (int r = 0; r < _size && path.Count < length; r++)
            {
                if (r % 2 == 0)
                    for (int c = 0; c < _size && path.Count < length; c++)
                        path.Add((r, c));
                else
                    for (int c = _size - 1; c >= 0 && path.Count < length; c--)
                        path.Add((r, c));
            }
            return path;
        }

        // ----------------------------------------------------------
        // Verifica DFS su char[,] — conta parole valide per lunghezza
        // ----------------------------------------------------------

        private static (int cnt9, int cnt8, int cnt7) CountByLength(
            char[,] grid, WordValidator validator)
        {
            if (validator == null) return (0, 0, 0);

            var found   = new HashSet<string>();
            var visited = new bool[_size, _size];

            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                {
                    visited[r, c] = true;
                    string first = char.ToLower(grid[r, c]).ToString();
                    DFSCount(grid, validator, r, c, visited, first, found);
                    visited[r, c] = false;
                }

            int c9 = 0, c8 = 0, c7 = 0;
            foreach (string w in found)
            {
                if      (w.Length >= 9) c9++;
                else if (w.Length == 8) c8++;
                else if (w.Length == 7) c7++;
            }
            return (c9, c8, c7);
        }

        private static void DFSCount(char[,] grid, WordValidator validator,
            int r, int c, bool[,] visited, string word, HashSet<string> found)
        {
            // Controlla validità solo per lunghezze che interessano
            if (word.Length >= 7 &&
                validator.Validate(word) != ValidationResult.Invalid)
                found.Add(word);

            if (word.Length >= MAX_DEPTH) return;

            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr, nc = c + dc;
                    if (nr < 0 || nr >= _size || nc < 0 || nc >= _size) continue;
                    if (visited[nr, nc]) continue;

                    visited[nr, nc] = true;
                    DFSCount(grid, validator, nr, nc, visited,
                             word + char.ToLower(grid[nr, nc]), found);
                    visited[nr, nc] = false;
                }
        }

        // ----------------------------------------------------------
        // Utility
        // ----------------------------------------------------------

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
