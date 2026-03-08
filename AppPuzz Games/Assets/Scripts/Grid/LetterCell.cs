// ============================================================
// LetterCell.cs — Tessera griglia stile Word Domination:
// bordo amber/gold, interno bianco, lettera navy-scura in bold.
// Animazione di scala al momento della selezione.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.UI;

namespace AppPuzz.Grid
{
    public class LetterCell : MonoBehaviour
    {
        // ----------------------------------------------------------
        // Riferimenti UI (assegnati dall'Inspector / GridManager)
        // ----------------------------------------------------------
        [Header("Riferimenti UI")]
        public TextMeshProUGUI letterText;
        public Button          cellButton;
        public Image           backgroundImage; // usata come BORDO gold

        // ----------------------------------------------------------
        // Dati cella
        // ----------------------------------------------------------
        public char Letter { get; private set; }
        public int  Row    { get; private set; }
        public int  Col    { get; private set; }
        public bool IsSelected { get; private set; }

        public System.Action<LetterCell> OnCellClicked;

        // ----------------------------------------------------------
        // Visivo interno
        // ----------------------------------------------------------
        private Image     _innerImage;   // tile bianca interna
        private Coroutine _scaleAnim;

        // ----------------------------------------------------------
        // Awake — costruisce la gerarchia visiva della tessera
        // ----------------------------------------------------------
        private void Awake()
        {
            SetupTileVisuals();
        }

        private void SetupTileVisuals()
        {
            // ── Bordo (backgroundImage = Image sul root GO) ────────
            if (backgroundImage != null)
            {
                backgroundImage.color = UITheme.Colors.TileBorder;
            }
            else
            {
                // fallback: prendi la prima Image sul root
                backgroundImage = GetComponent<Image>();
                if (backgroundImage != null)
                    backgroundImage.color = UITheme.Colors.TileBorder;
            }

            // ── Ombra interna (pannello leggermente offset sotto) ──
            // Crea già presente? evita duplicati
            if (transform.Find("TileShadow") == null)
            {
                var shadowGo  = new GameObject("TileShadow");
                shadowGo.transform.SetParent(transform, false);
                shadowGo.transform.SetSiblingIndex(0);
                var sRt = shadowGo.AddComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(2, -4);
                sRt.offsetMax = new Vector2(-2, -2);
                var sImg = shadowGo.AddComponent<Image>();
                sImg.color = UITheme.Colors.TileShadow;
            }

            // ── Superficie bianca interna ──────────────────────────
            if (transform.Find("TileInner") == null)
            {
                var innerGo = new GameObject("TileInner");
                innerGo.transform.SetParent(transform, false);
                var iRt = innerGo.AddComponent<RectTransform>();
                iRt.anchorMin = Vector2.zero;
                iRt.anchorMax = Vector2.one;
                iRt.offsetMin = new Vector2(4, 4);
                iRt.offsetMax = new Vector2(-4, -6); // -6 bottom = lascia ombra
                _innerImage = innerGo.AddComponent<Image>();
                _innerImage.color = UITheme.Colors.TileNormal;
            }
            else
            {
                _innerImage = transform.Find("TileInner").GetComponent<Image>();
            }

            // ── Lettera: scura, grassetto, grande ─────────────────
            if (letterText != null)
            {
                letterText.transform.SetAsLastSibling();
                letterText.color      = UITheme.Colors.TileText;
                letterText.fontStyle  = FontStyles.Bold;
                letterText.fontSize   = 72;
                letterText.alignment  = TextAlignmentOptions.Center;
            }
        }

        // ----------------------------------------------------------
        // Inizializzazione da GridManager
        // ----------------------------------------------------------
        public void Initialize(char letter, int row, int col)
        {
            Letter = letter;
            Row    = row;
            Col    = col;

            if (letterText != null)
                letterText.text = letter.ToString().ToUpper();
            else
                Debug.LogWarning($"[LetterCell] letterText non assegnato su ({row},{col})!");

            SetSelected(false);
        }

        // ----------------------------------------------------------
        // Selezione visiva con animazione scala
        // ----------------------------------------------------------
        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (selected)
            {
                if (backgroundImage != null)
                    backgroundImage.color = UITheme.Colors.TileBorderSel;
                if (_innerImage != null)
                    _innerImage.color = UITheme.Colors.TileSelected;
                if (letterText != null)
                    letterText.color = UITheme.Colors.BackgroundPanel; // scuro su gold
                AnimateScale(UITheme.Anim.TileSelectScale, 0.08f);
            }
            else
            {
                if (backgroundImage != null)
                    backgroundImage.color = UITheme.Colors.TileBorder;
                if (_innerImage != null)
                    _innerImage.color = UITheme.Colors.TileNormal;
                if (letterText != null)
                    letterText.color = UITheme.Colors.TileText;
                AnimateScale(1f, 0.10f);
            }
        }

        private void AnimateScale(float target, float duration)
        {
            if (_scaleAnim != null) StopCoroutine(_scaleAnim);
            _scaleAnim = StartCoroutine(ScaleCoroutine(target, duration));
        }

        private IEnumerator ScaleCoroutine(float target, float duration)
        {
            Vector3 start = transform.localScale;
            Vector3 end   = Vector3.one * target;
            float   t     = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            transform.localScale = end;
        }

        // ----------------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------------
        private void Start()
        {
            if (cellButton != null)
                cellButton.onClick.AddListener(() => OnCellClicked?.Invoke(this));
        }
    }
}
