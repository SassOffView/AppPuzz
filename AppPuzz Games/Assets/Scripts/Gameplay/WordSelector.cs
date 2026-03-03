// ============================================================
// WordSelector.cs
// Gestisce l'input stile Ruzzle: il giocatore trascina il dito
// (o il mouse in editor) sulle celle adiacenti per formare parole.
// ============================================================
// STEP 2 : implementazione completa drag-to-select.
// ============================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Grid;

namespace AppPuzz.Gameplay
{
    /// <summary>
    /// Componente da attaccare a un pannello trasparente che copre tutta la griglia.
    /// Intercetta i gesti di drag e costruisce la parola in tempo reale.
    /// Al rilascio, invia la parola a GameManager.
    /// </summary>
    public class WordSelector : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // ----------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------
        public static WordSelector Instance { get; private set; }

        // ----------------------------------------------------------
        // Riferimenti UI (assegnati dall'Inspector)
        // ----------------------------------------------------------

        [Header("UI")]
        [Tooltip("Il testo che mostra la parola in formazione.")]
        public TextMeshProUGUI currentWordText;

        // ----------------------------------------------------------
        // Stato selezione
        // ----------------------------------------------------------

        /// <summary>Celle selezionate nell'ordine in cui sono state toccate.</summary>
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

        // ----------------------------------------------------------
        // IPointerDownHandler — inizio del gesto
        // ----------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            _isSelecting = true;
            _selectedCells.Clear();
            GridManager.Instance?.ResetAllCells();
            UpdateWordDisplay();

            TryAddCellAt(eventData.position);
        }

        // ----------------------------------------------------------
        // IDragHandler — dito/mouse si sposta sulle celle
        // ----------------------------------------------------------

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isSelecting) return;
            TryAddCellAt(eventData.position);
        }

        // ----------------------------------------------------------
        // IPointerUpHandler — rilascio: conferma la parola
        // ----------------------------------------------------------

        public void OnPointerUp(PointerEventData eventData)
        {
            _isSelecting = false;
            SubmitCurrentWord();
        }

        // ----------------------------------------------------------
        // Logica interna
        // ----------------------------------------------------------

        /// <summary>
        /// Trova la cella UI sotto la posizione schermo indicata e,
        /// se è adiacente all'ultima selezionata e non già inclusa, la aggiunge.
        /// </summary>
        private void TryAddCellAt(Vector2 screenPos)
        {
            LetterCell cell = GetCellAtScreenPos(screenPos);
            if (cell == null) return;
            if (_selectedCells.Contains(cell)) return;

            // La prima cella è sempre accettata; le successive devono essere adiacenti
            bool isFirst = _selectedCells.Count == 0;
            bool adjacent = !isFirst && GridManager.Instance != null &&
                            GridManager.Instance.AreAdjacent(_selectedCells[_selectedCells.Count - 1], cell);

            if (isFirst || adjacent)
            {
                _selectedCells.Add(cell);
                cell.SetSelected(true);
                UpdateWordDisplay();
            }
        }

        /// <summary>
        /// Esegue un raycast UI per trovare il LetterCell sotto la posizione schermo.
        /// </summary>
        private LetterCell GetCellAtScreenPos(Vector2 screenPos)
        {
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var result in results)
            {
                // Controlla il GameObject stesso e il genitore (il prefab può avere sotto-oggetti)
                LetterCell cell = result.gameObject.GetComponent<LetterCell>()
                               ?? result.gameObject.GetComponentInParent<LetterCell>();
                if (cell != null) return cell;
            }
            return null;
        }

        /// <summary>
        /// Aggiorna il testo della parola in formazione.
        /// </summary>
        private void UpdateWordDisplay()
        {
            if (currentWordText != null)
                currentWordText.text = BuildCurrentWord();
        }

        /// <summary>
        /// Concatena le lettere delle celle selezionate.
        /// </summary>
        private string BuildCurrentWord()
        {
            var sb = new StringBuilder(_selectedCells.Count);
            foreach (var cell in _selectedCells)
                sb.Append(cell.Letter);
            return sb.ToString();
        }

        /// <summary>
        /// Invia la parola corrente a GameManager (minimo 3 lettere).
        /// Resetta poi la selezione visiva.
        /// </summary>
        private void SubmitCurrentWord()
        {
            string word = BuildCurrentWord();

            GridManager.Instance?.ResetAllCells();
            _selectedCells.Clear();

            if (word.Length >= 3)
            {
                GameManager.Instance?.SubmitWord(word);
            }

            if (currentWordText != null)
                currentWordText.text = string.Empty;
        }
    }
}
