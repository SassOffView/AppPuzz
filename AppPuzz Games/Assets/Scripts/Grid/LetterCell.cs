// ============================================================
// LetterCell.cs — Tessera griglia con:
//   • Valore punti (piccolo testo in basso a destra)
//   • Sistema di danno visivo a 6 livelli
//   • Stato ghiaccio (freeze) con overlay blu
//   • Flash fulmine animato per parola corretta
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AppPuzz.UI;
using AppPuzz.Localization;
using AppPuzz.Gameplay;

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
        public Image           backgroundImage; // bordo gold

        // ----------------------------------------------------------
        // Dati cella
        // ----------------------------------------------------------
        public char Letter     { get; private set; }
        public int  Row        { get; private set; }
        public int  Col        { get; private set; }
        public bool IsSelected { get; private set; }

        // ── Sistema danno ─────────────────────────────────────────
        /// <summary>Colpi accumulati (1 per parola corretta, 2 per errata). Max 6 → rottura.</summary>
        public int HitPoints { get; private set; }

        /// <summary>Colpi errati consecutivi. Al terzo → ghiaccio.</summary>
        public int ConsecutiveWrongHits { get; private set; }

        /// <summary>True se la lettera è ghiacciata e non selezionabile.</summary>
        public bool IsFrozen { get; private set; }

        /// <summary>Parole corrette da trovare prima dello sblocco ghiaccio.</summary>
        public int FrozenTurnsRemaining { get; private set; }

        /// <summary>True se la lettera è consumata (≥ 6 colpi) e deve essere sostituita.</summary>
        public bool IsLetterBroken => HitPoints >= 6;

        public System.Action<LetterCell> OnCellClicked;

        // ----------------------------------------------------------
        // Visivo interno
        // ----------------------------------------------------------
        private Image     _innerImage;
        private Image     _damageOverlay;  // sovrapposto sopra TileInner
        private Image     _freezeOverlay;  // sovrapposto sopra tutto (ghiaccio)
        private TextMeshProUGUI _pointValueText; // punti lettera (piccolo, angolo basso-dx)
        private Coroutine _scaleAnim;
        private Coroutine _flashAnim;

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
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }
            if (backgroundImage != null)
                backgroundImage.color = UITheme.Colors.TileBorder;

            // ── Ombra interna ─────────────────────────────────────
            if (transform.Find("TileShadow") == null)
            {
                var shadowGo = new GameObject("TileShadow");
                shadowGo.transform.SetParent(transform, false);
                shadowGo.transform.SetSiblingIndex(0);
                var sRt  = shadowGo.AddComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(2, -4);
                sRt.offsetMax = new Vector2(-2, -2);
                shadowGo.AddComponent<Image>().color = UITheme.Colors.TileShadow;
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
                iRt.offsetMax = new Vector2(-4, -6);
                _innerImage = innerGo.AddComponent<Image>();
                _innerImage.color = UITheme.Colors.TileNormal;
            }
            else
            {
                _innerImage = transform.Find("TileInner").GetComponent<Image>();
            }

            // ── Overlay danno ──────────────────────────────────────
            if (transform.Find("DamageOverlay") == null)
            {
                var dmgGo = new GameObject("DamageOverlay");
                dmgGo.transform.SetParent(transform, false);
                var dRt = dmgGo.AddComponent<RectTransform>();
                dRt.anchorMin = Vector2.zero;
                dRt.anchorMax = Vector2.one;
                dRt.offsetMin = new Vector2(4, 4);
                dRt.offsetMax = new Vector2(-4, -6);
                _damageOverlay = dmgGo.AddComponent<Image>();
                _damageOverlay.color = Color.clear;
                _damageOverlay.raycastTarget = false;
            }
            else
            {
                _damageOverlay = transform.Find("DamageOverlay").GetComponent<Image>();
            }

            // ── Overlay ghiaccio ───────────────────────────────────
            if (transform.Find("FreezeOverlay") == null)
            {
                var frzGo = new GameObject("FreezeOverlay");
                frzGo.transform.SetParent(transform, false);
                var fRt = frzGo.AddComponent<RectTransform>();
                fRt.anchorMin = Vector2.zero;
                fRt.anchorMax = Vector2.one;
                fRt.offsetMin = Vector2.zero;
                fRt.offsetMax = Vector2.zero;
                _freezeOverlay = frzGo.AddComponent<Image>();
                _freezeOverlay.color = Color.clear;
                _freezeOverlay.raycastTarget = false;
            }
            else
            {
                _freezeOverlay = transform.Find("FreezeOverlay").GetComponent<Image>();
            }

            // ── Lettera ────────────────────────────────────────────
            if (letterText != null)
            {
                letterText.transform.SetAsLastSibling();
                letterText.color     = UITheme.Colors.TileText;
                letterText.fontStyle = FontStyles.Bold;
                letterText.fontSize  = 72;
                letterText.alignment = TextAlignmentOptions.Center;
            }

            // ── Valore punti (piccolo, angolo basso-destra) ────────
            if (transform.Find("PointText") == null)
            {
                var ptGo = new GameObject("PointText");
                ptGo.transform.SetParent(transform, false);
                var pRt = ptGo.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.55f, 0f);
                pRt.anchorMax = Vector2.one;
                pRt.offsetMin = new Vector2(0, 2);
                pRt.offsetMax = new Vector2(-4, -2);
                _pointValueText = ptGo.AddComponent<TextMeshProUGUI>();
                _pointValueText.color     = new Color(0.08f, 0.08f, 0.40f, 0.75f); // navy semi-trasparente
                _pointValueText.fontStyle = FontStyles.Bold;
                _pointValueText.fontSize  = 20;
                _pointValueText.alignment = TextAlignmentOptions.BottomRight;
                _pointValueText.raycastTarget = false;
            }
            else
            {
                _pointValueText = transform.Find("PointText").GetComponent<TextMeshProUGUI>();
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

            // Reset stato danno/ghiaccio
            HitPoints            = 0;
            ConsecutiveWrongHits = 0;
            IsFrozen             = false;
            FrozenTurnsRemaining = 0;

            if (letterText != null)
                letterText.text = letter.ToString().ToUpper();
            else
                Debug.LogWarning($"[LetterCell] letterText non assegnato su ({row},{col})!");

            RefreshPointValue();
            UpdateDamageVisual();
            UpdateFreezeVisual();
            SetSelected(false);
        }

        // ----------------------------------------------------------
        // Valore punti
        // ----------------------------------------------------------
        /// <summary>Aggiorna il testo del valore punti in base alla lingua corrente.</summary>
        public void RefreshPointValue()
        {
            if (_pointValueText == null) return;
            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;
            int val = LetterScoring.GetLetterValue(Letter, isItalian);
            _pointValueText.text = val.ToString();
        }

        // ----------------------------------------------------------
        // Selezione visiva con animazione scala
        // ----------------------------------------------------------
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);

            if (selected)
            {
                if (backgroundImage != null)
                    backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.TileBorderSel;
                if (_innerImage != null)
                    _innerImage.color = UITheme.Colors.TileSelected;
                if (letterText != null)
                    letterText.color = UITheme.Colors.BackgroundPanel;
                AnimateScale(UITheme.Anim.TileSelectScale, 0.08f);
            }
            else
            {
                if (backgroundImage != null)
                    backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.TileBorder;
                if (_innerImage != null)
                    _innerImage.color = UITheme.TileSurfaceColor(dmgLevel);
                if (letterText != null)
                    letterText.color = UITheme.Colors.TileText;
                AnimateScale(1f, 0.10f);
            }
        }

        // ----------------------------------------------------------
        // Sistema danno
        // ----------------------------------------------------------

        /// <summary>
        /// Applica un colpo da parola CORRETTA (+1 hit point).
        /// Resetta il contatore di errori consecutivi.
        /// Restituisce true se la lettera è ora rotta (≥6 hit).
        /// </summary>
        public bool ApplyCorrectHit()
        {
            ConsecutiveWrongHits = 0;
            HitPoints = Mathf.Min(HitPoints + 1, 6);
            UpdateDamageVisual();
            return IsLetterBroken;
        }

        /// <summary>
        /// Applica un colpo da parola ERRATA (+2 hit point equivalenti).
        /// Incrementa il contatore di errori consecutivi.
        /// Restituisce true se la lettera è ora ghiacciata (3 errori consecutivi).
        /// </summary>
        public bool ApplyWrongHit()
        {
            HitPoints = Mathf.Min(HitPoints + 2, 6);
            ConsecutiveWrongHits++;
            UpdateDamageVisual();

            if (ConsecutiveWrongHits >= 3 && !IsFrozen)
            {
                FreezeCell();
                return true; // ghiacciata
            }
            return false;
        }

        /// <summary>
        /// Chiamato dopo ogni parola corretta trovata (globalmente).
        /// Decrementa il counter di freeze. Sblocca se arriva a 0.
        /// </summary>
        public void DecrementFrozenTurn()
        {
            if (!IsFrozen) return;
            FrozenTurnsRemaining--;
            if (FrozenTurnsRemaining <= 0)
                UnfreezeCell();
        }

        private void FreezeCell()
        {
            IsFrozen             = true;
            FrozenTurnsRemaining = 2;
            ConsecutiveWrongHits = 0;
            UpdateFreezeVisual();
        }

        private void UnfreezeCell()
        {
            IsFrozen             = false;
            FrozenTurnsRemaining = 0;
            UpdateFreezeVisual();
        }

        // ----------------------------------------------------------
        // Flash fulmine (coroutine pubblica)
        // ----------------------------------------------------------

        /// <summary>
        /// Colpo di fulmine visivo: flash rapido con il colore dell'elemento della creatura,
        /// poi ritorna allo stato di danno corrente.
        /// </summary>
        public IEnumerator FlashLightning(Color lightningColor, float holdDuration = 0.09f)
        {
            if (_flashAnim != null) StopCoroutine(_flashAnim);

            // Flash: colore fulmine
            if (_innerImage      != null) _innerImage.color      = lightningColor;
            if (backgroundImage  != null) backgroundImage.color  = lightningColor;
            if (letterText       != null) letterText.color        = Color.white;
            transform.localScale = Vector3.one * 1.18f;

            yield return new WaitForSeconds(holdDuration);

            // Ritorna allo stato normale/danno
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);
            if (_innerImage      != null) _innerImage.color      = UITheme.TileSurfaceColor(dmgLevel);
            if (backgroundImage  != null) backgroundImage.color  = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.TileBorder;
            if (letterText       != null) letterText.color        = UITheme.Colors.TileText;
            transform.localScale = Vector3.one;
        }

        /// <summary>Flash rapido rosso per parola errata.</summary>
        public IEnumerator FlashWrong(float holdDuration = 0.12f)
        {
            if (backgroundImage != null) backgroundImage.color = UITheme.Colors.TextDanger;
            if (_innerImage     != null) _innerImage.color     = new Color(1f, 0.3f, 0.3f, 1f);
            if (letterText      != null) letterText.color      = Color.white;

            yield return new WaitForSeconds(holdDuration);

            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);
            if (_innerImage      != null) _innerImage.color      = UITheme.TileSurfaceColor(dmgLevel);
            if (backgroundImage  != null) backgroundImage.color  = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.TileBorder;
            if (letterText       != null) letterText.color        = UITheme.Colors.TileText;
        }

        // ----------------------------------------------------------
        // Visuale danno e ghiaccio
        // ----------------------------------------------------------
        private void UpdateDamageVisual()
        {
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);
            if (_damageOverlay != null)
                _damageOverlay.color = UITheme.DamageOverlayColor(dmgLevel);
            if (_innerImage != null && !IsSelected)
                _innerImage.color = UITheme.TileSurfaceColor(dmgLevel);
        }

        private void UpdateFreezeVisual()
        {
            if (_freezeOverlay != null)
                _freezeOverlay.color = IsFrozen ? UITheme.Colors.FreezeOverlay : Color.clear;
            if (backgroundImage != null && !IsSelected)
                backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.TileBorder;
        }

        // ----------------------------------------------------------
        // Animazione scala
        // ----------------------------------------------------------
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
