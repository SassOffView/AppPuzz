// ============================================================
// WordSelector.cs — input Ruzzle: drag su celle adiacenti.
// • Celle ghiacciate (IsFrozen) non sono selezionabili.
// • Le celle selezionate vengono passate ai manager con la parola.
// ============================================================
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;

namespace AppPuzz.Gameplay
{
    public class WordSelector : MonoBehaviour
    {
        public static WordSelector Instance { get; private set; }

        // Evento: notifica la parola in formazione a tutti i manager
        public static event System.Action<string> OnCurrentWordChanged;

        [Header("UI — può essere null, i manager si aggiornano via evento")]
        public TextMeshProUGUI currentWordText;

        private readonly List<LetterCell> _selectedCells = new List<LetterCell>();
        private bool _isSelecting = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            var img = GetComponent<Image>();
            if (img != null) { var c = img.color; c.a = 0f; img.color = c; }
        }

        private void Update()
        {
            var sm = AppPuzz.UI.ScreenManager.Instance;
            if (sm != null &&
                sm.Current != AppPuzz.UI.ScreenID.Gameplay &&
                sm.Current != AppPuzz.UI.ScreenID.Training &&
                sm.Current != AppPuzz.UI.ScreenID.Arena)
                return;

            if (Input.GetMouseButtonDown(0))      BeginSelection(Input.mousePosition);
            else if (Input.GetMouseButton(0) && _isSelecting) ContinueSelection(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0) && _isSelecting) EndSelection();

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                switch (t.phase)
                {
                    case TouchPhase.Began:      BeginSelection(t.position);                      break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary: if (_isSelecting) ContinueSelection(t.position); break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:   if (_isSelecting) EndSelection();                break;
                }
            }
        }

        private void BeginSelection(Vector2 pos)
        {
            _isSelecting = true;
            _selectedCells.Clear();
            GridManager.Instance?.ResetAllCells();
            BroadcastWord("");
            TryAddCellAt(pos);
        }

        private void ContinueSelection(Vector2 pos) => TryAddCellAt(pos);

        private void EndSelection()
        {
            _isSelecting = false;
            SubmitCurrentWord();
        }

        private void TryAddCellAt(Vector2 screenPos)
        {
            LetterCell cell = GetCellAtScreenPos(screenPos);
            if (cell == null) return;

            // Le celle ghiacciate non possono essere selezionate
            if (cell.IsFrozen) return;

            // Backtrack: se la cella è il penultimo elemento, rimuovi l'ultimo
            if (_selectedCells.Count >= 2 && _selectedCells[_selectedCells.Count - 2] == cell)
            {
                LetterCell last = _selectedCells[_selectedCells.Count - 1];
                last.SetSelected(false);
                _selectedCells.RemoveAt(_selectedCells.Count - 1);
                BroadcastWord(BuildCurrentWord());
                return;
            }

            if (_selectedCells.Contains(cell)) return;

            bool isFirst  = _selectedCells.Count == 0;
            bool adjacent = !isFirst && GridManager.Instance != null &&
                            GridManager.Instance.AreAdjacent(_selectedCells[_selectedCells.Count - 1], cell);

            if (isFirst || adjacent)
            {
                _selectedCells.Add(cell);
                cell.SetSelected(true);
                BroadcastWord(BuildCurrentWord());
            }
        }

        private LetterCell GetCellAtScreenPos(Vector2 screenPos)
        {
            if (GridManager.Instance == null) return null;
            foreach (LetterCell cell in GridManager.Instance.GetAllCells())
            {
                RectTransform rt = cell.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null))
                    return cell;
            }
            return null;
        }

        private void BroadcastWord(string word)
        {
            if (currentWordText != null) currentWordText.text = word;
            OnCurrentWordChanged?.Invoke(word);
        }

        private string BuildCurrentWord()
        {
            var sb = new StringBuilder(_selectedCells.Count);
            foreach (var c in _selectedCells) sb.Append(c.Letter);
            return sb.ToString();
        }

        private void SubmitCurrentWord()
        {
            string word = BuildCurrentWord();

            // Cattura le celle PRIMA di pulire la selezione
            var submittedCells = new List<LetterCell>(_selectedCells);

            GridManager.Instance?.ResetAllCells();
            _selectedCells.Clear();
            BroadcastWord("");

            if (word.Length >= 2)
            {
                var screen = AppPuzz.UI.ScreenManager.Instance?.Current;
                if (screen == AppPuzz.UI.ScreenID.Training)
                    TrainingManager.Instance?.SubmitWord(word, submittedCells);
                else if (screen == AppPuzz.UI.ScreenID.Arena)
                    ArenaManager.Instance?.SubmitWord(word, submittedCells);
                else
                    GameManager.Instance?.SubmitWord(word, submittedCells);
            }
        }
    }
}
