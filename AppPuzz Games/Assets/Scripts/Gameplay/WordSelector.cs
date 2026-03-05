// ============================================================
// WordSelector.cs
// Gestisce l'input stile Ruzzle: il giocatore trascina il dito
// (o il mouse in editor) sulle celle adiacenti per formare parole.
// ============================================================
// STEP 7 revisione: Input.GetMouseButton in Update() per massima
// compatibilità — niente più dipendenza dall'overlay EventSystem.
// ============================================================

using System.Collections.Generic;
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
            Debug.Log("[WordSelector] Awake OK — componente attivo su: " + gameObject.name);
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
            // Ignora input se non siamo nella schermata di gioco
            if (AppPuzz.UI.ScreenManager.Instance != null &&
                AppPuzz.UI.ScreenManager.Instance.Current != AppPuzz.UI.ScreenID.Gameplay &&
                AppPuzz.UI.ScreenManager.Instance.Current != AppPuzz.UI.ScreenID.Training &&
                AppPuzz.UI.ScreenManager.Instance.Current != AppPuzz.UI.ScreenID.Arena)
                return;

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
            Debug.Log("[WordSelector] BeginSelection @ " + screenPos);
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
            if (GridManager.Instance == null)
            {
                Debug.LogWarning("[WordSelector] GridManager.Instance è NULL!");
                return null;
            }

            int count = 0;
            foreach (LetterCell cell in GridManager.Instance.GetAllCells())
            {
                count++;
                RectTransform rt = cell.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Prima cella: logga posizione e dimensione per debug
                if (count == 1)
                    Debug.Log($"[WordSelector] Cella[0] rect={rt.rect}  anchoredPos={rt.anchoredPosition}  screenPos={screenPos}");

                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null))
                {
                    Debug.Log($"[WordSelector] Cella trovata ({cell.Row},{cell.Col}) '{cell.Letter}'");
                    return cell;
                }
            }

            Debug.Log($"[WordSelector] Nessuna cella trovata. Celle controllate={count}, screenPos={screenPos}");
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
