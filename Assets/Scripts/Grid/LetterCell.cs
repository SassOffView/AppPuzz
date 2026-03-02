// ============================================================
// LetterCell.cs
// Rappresenta una singola cella della griglia 5×5.
// Ogni cella conosce la propria lettera e la propria posizione.
// Si occuperà anche dell'highlight visivo quando selezionata.
// ============================================================
// STEP 1 : stub compilabile — logica selezione/highlight nello STEP 3.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;               // TextMeshPro — assicurati di averlo installato (Package Manager)

namespace AppPuzz.Grid
{
    /// <summary>
    /// Componente attaccato al prefab LetterCell.
    /// Contiene la lettera visualizzata, la posizione nella griglia,
    /// e lo stato di selezione corrente.
    /// </summary>
    public class LetterCell : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti UI (assegnati dall'Inspector o da GridManager)
        // ----------------------------------------------------------

        [Header("Riferimenti UI")]
        [Tooltip("Il componente TextMeshPro che mostra la lettera.")]
        public TextMeshProUGUI letterText;

        [Tooltip("Il Button Unity associato a questa cella.")]
        public Button cellButton;

        [Tooltip("L'immagine di sfondo della cella (per l'highlight).")]
        public Image backgroundImage;

        // ----------------------------------------------------------
        // Dati della cella
        // ----------------------------------------------------------

        [Header("Dati (assegnati da GridManager)")]
        [Tooltip("La lettera visualizzata in questa cella.")]
        public char Letter { get; private set; }

        [Tooltip("Riga nella griglia 5×5 (0–4).")]
        public int Row { get; private set; }

        [Tooltip("Colonna nella griglia 5×5 (0–4).")]
        public int Col { get; private set; }

        // ----------------------------------------------------------
        // Stato visivo
        // ----------------------------------------------------------

        /// <summary>True se la cella è attualmente selezionata dal giocatore.</summary>
        public bool IsSelected { get; private set; }

        // Colori di default e di selezione (verranno spostati in un config nello STEP 3)
        private Color _normalColor  = Color.white;
        private Color _selectedColor = new Color(0.4f, 0.8f, 1f); // azzurro chiaro

        // ----------------------------------------------------------
        // API pubblica — chiamata da GridManager
        // ----------------------------------------------------------

        /// <summary>
        /// Inizializza la cella con lettera e posizione.
        /// Chiamata da GridManager dopo aver istanziato il prefab.
        /// </summary>
        public void Initialize(char letter, int row, int col)
        {
            Letter = letter;
            Row    = row;
            Col    = col;

            // Aggiorna il testo UI
            if (letterText != null)
                letterText.text = letter.ToString();
            else
                Debug.LogWarning($"[LetterCell] letterText non assegnato su ({row},{col})!");

            // Stato iniziale: non selezionato
            SetSelected(false);
        }

        /// <summary>
        /// Imposta visivamente la cella come selezionata / deselezionata.
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (backgroundImage != null)
                backgroundImage.color = selected ? _selectedColor : _normalColor;
        }

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------

        private void Start()
        {
            // TODO (STEP 3): registrare il callback del click su cellButton
        }
    }
}
