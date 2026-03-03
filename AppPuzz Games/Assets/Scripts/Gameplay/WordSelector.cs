// ============================================================
// WordSelector.cs
// Gestisce l'input stile Ruzzle: il giocatore trascina il dito
// (o il mouse in editor) sulle celle adiacenti per formare parole.
// ============================================================
// STEP 7 revisione: Input.GetMouseButton in Update() per massima
// compatibilità — niente più dipendenza dall'overlay EventSystem.
// ============================================================

using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.Grid;

namespace AppPuzz.Gameplay
{
    /// <summary>
    /// Legge l'input mouse/touch ogni frame con Input.GetMouseButton.
    /// Può stare su qualsiasi GameObject attivo della scena (overlay,
    /// GridManager, GameManager, ecc.) — non dipende da IPointerHandler.
    /// </summary>
    public class WordSelector : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static WordSelector Instance { get; private set; }

        // ----------------------------------------------------------
        // Riferimenti UI
        // ----------------------------------------------------------

        [Header("UI")]
        [Tooltip("Testo che mostra la parola in formazione.")]
        public TextMeshProUGUI currentWordText;

        // ----------------------------------------------------------
        // Stato selezione
        // ----------------------------------------------------------

        private readonly List<LetterCell> _selectedCells = new List<LetterCell>();
        private bool _isSelecting = false;

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
            // Se questo script è su un oggetto con Image (es. WordSelectorOverlay),
            // lo rende automaticamente trasparente così non copre la griglia.
            var img = GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }

        private void Update()
        {
            // ---- MOUSE (editor / PC) ----
            if (Input.GetMouseButtonDown(0))
            {
                BeginSelection(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && _isSelecting)
            {
                ContinueSelection(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && _isSelecting)
            {
                EndSelection();
            }

            // ---- TOUCH (mobile) ----
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        BeginSelection(touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_isSelecting) ContinueSelection(touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_isSelecting) EndSelection();
                        break;
                }
            }
        }

        // ----------------------------------------------------------
        // Fasi della selezione
        // ----------------------------------------------------------

        private void BeginSelection(Vector2 screenPos)
        {
            _isSelecting = true;
            _selectedCells.Clear();
            GridManager.Instance?.ResetAllCells();
            UpdateWordDisplay();
            TryAddCellAt(screenPos);
        }

        private void ContinueSelection(Vector2 screenPos)
        {
            TryAddCellAt(screenPos);
        }

        private void EndSelection()
        {
            _isSelecting = false;
            SubmitCurrentWord();
        }

        // ----------------------------------------------------------
        // Logica interna
        // ----------------------------------------------------------

        private void TryAddCellAt(Vector2 screenPos)
        {
            LetterCell cell = GetCellAtScreenPos(screenPos);
            if (cell == null) return;
            if (_selectedCells.Contains(cell)) return;

            bool isFirst  = _selectedCells.Count == 0;
            bool adjacent = !isFirst && GridManager.Instance != null &&
                            GridManager.Instance.AreAdjacent(
                                _selectedCells[_selectedCells.Count - 1], cell);

            if (isFirst || adjacent)
            {
                _selectedCells.Add(cell);
                cell.SetSelected(true);
                UpdateWordDisplay();
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

        private void UpdateWordDisplay()
        {
            if (currentWordText != null)
                currentWordText.text = BuildCurrentWord();
        }

        private string BuildCurrentWord()
        {
            var sb = new StringBuilder(_selectedCells.Count);
            foreach (var cell in _selectedCells)
                sb.Append(cell.Letter);
            return sb.ToString();
        }

        private void SubmitCurrentWord()
        {
            string word = BuildCurrentWord();
            GridManager.Instance?.ResetAllCells();
            _selectedCells.Clear();

            if (word.Length >= 3)
                GameManager.Instance?.SubmitWord(word);

            if (currentWordText != null)
                currentWordText.text = string.Empty;
        }
    }
}
