// ============================================================
// LetterCell.cs — Tessera griglia STILE PIETRA INCISA
//
// Aspetto visivo:
//   • Lastra di pietra (arenaria calda) con cornice rocciosa scura
//   • Lettera incisa in profondità: testo scuro + highlight chiaro
//   • Valore punti in basso a destra (numeri incisi, crema pietra)
//
// Sistema danno (6 livelli):
//   • Livello 0 → pietra intatta (grigio-sabbia)
//   • Livelli 1–5 → pietra sempre più scura + crepe progressive
//   • Livello 6 → lettera si rompe e viene sostituita
//   Crepe simulate con Image strips ruotate (no sprite richiesto):
//   ciascuna = linea scura (crepa) + linea chiara offset (bordo frattura)
//
// Meccanica:
//   • ApplyCorrectHit()  → +1 colpo, reset errori consecutivi
//   • ApplyWrongHit()    → +2 colpi, se 3 errati consecutivi → ghiaccio
//   • DecrementFrozenTurn() → sbloccato dopo 2 parole corrette globali
//   • FlashLightning(color) → fulmine colorato per parola corretta
//   • FlashWrong()          → flash rosso per parola errata
// ============================================================

using System.Collections;
using System.Collections.Generic;
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
        // Riferimenti UI (possono essere assegnati dall'Inspector,
        // ma vengono creati/inizializzati anche via codice)
        // ----------------------------------------------------------
        [Header("Riferimenti UI")]
        public TextMeshProUGUI letterText;
        public Button          cellButton;
        public Image           backgroundImage;

        // ----------------------------------------------------------
        // Dati cella
        // ----------------------------------------------------------
        public char Letter     { get; private set; }
        public int  Row        { get; private set; }
        public int  Col        { get; private set; }
        public bool IsSelected { get; private set; }

        // ── Sistema danno ─────────────────────────────────────────
        public int  HitPoints            { get; private set; }
        public int  ConsecutiveWrongHits { get; private set; }
        public bool IsFrozen             { get; private set; }
        public int  FrozenTurnsRemaining { get; private set; }
        public bool IsLetterBroken       => HitPoints >= 6;

        public System.Action<LetterCell> OnCellClicked;

        // ----------------------------------------------------------
        // Nodi visivi interni
        // ----------------------------------------------------------
        private Image     _innerImage;         // superficie pietra
        private Image     _freezeOverlay;      // overlay ghiaccio
        private TextMeshProUGUI _letterHighlight; // testo highlight per effetto incisione
        private TextMeshProUGUI _pointValueText;  // valore punti lettera

        // Crack system – due Image per ciascuna crepa (ombra + bordo)
        private Image[] _crackDark;
        private Image[] _crackEdge;

        private Coroutine _scaleAnim;

        // ----------------------------------------------------------
        // Definizione crepe (normalizzate rispetto al tile 0–1)
        // CenterX/Y = punto centrale della crepa
        // HalfW/H   = semi-larghezza e semi-altezza prima della rotazione
        // Rotation  = angolo gradi (Z-axis)
        // ShowAt    = livello danno minimo per apparire (1–5)
        // ----------------------------------------------------------
        private readonly struct CrackDef
        {
            public readonly float CX, CY;        // centro normalizzato
            public readonly float HalfW, HalfH;  // metà dimensioni
            public readonly float Rot;            // rotazione gradi
            public readonly int   ShowAt;         // livello danno minimo

            public CrackDef(float cx, float cy, float hw, float hh, float rot, int showAt)
            { CX=cx; CY=cy; HalfW=hw; HalfH=hh; Rot=rot; ShowAt=showAt; }
        }

        // 8 crepe disposte per coprire la tessera progressivamente
        private static readonly CrackDef[] _cracks = {
            // ── Livello 1: prima crepa diagonale (alto-sinistra → centro-destra)
            new(0.38f, 0.67f, 0.018f, 0.22f, -26f, 1),

            // ── Livello 2: seconda crepa incrociata
            new(0.63f, 0.62f, 0.016f, 0.19f,  22f, 2),

            // ── Livello 3: rami laterali
            new(0.30f, 0.40f, 0.013f, 0.15f, -54f, 3),
            new(0.70f, 0.36f, 0.013f, 0.16f,  42f, 3),

            // ── Livello 4: fratture profonde
            new(0.50f, 0.50f, 0.012f, 0.12f,  88f, 4),
            new(0.20f, 0.53f, 0.011f, 0.11f, -70f, 4),

            // ── Livello 5: sgretolamento critico
            new(0.78f, 0.50f, 0.012f, 0.13f,  63f, 5),
            new(0.50f, 0.25f, 0.013f, 0.14f, -33f, 5),
        };

        // ----------------------------------------------------------
        // Awake — costruisce la gerarchia visiva
        // ----------------------------------------------------------
        private void Awake()
        {
            BuildTileHierarchy();
            BuildCrackImages();
        }

        private void Start()
        {
            if (cellButton != null)
                cellButton.onClick.AddListener(() => OnCellClicked?.Invoke(this));
        }

        // ----------------------------------------------------------
        // Costruzione gerarchia: pietra incisa
        // ----------------------------------------------------------
        private void BuildTileHierarchy()
        {
            // ── Bordo / cornice rocciosa ───────────────────────────
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = UITheme.Colors.StoneBorder;
                backgroundImage.raycastTarget = true;
            }

            // ── Ombra sottostante (profondità) ─────────────────────
            EnsureImage("TileShadow", Vector2.zero, Vector2.one,
                        new Vector2(2, -4), new Vector2(-2, -2),
                        UITheme.Colors.StoneShadow, raycast: false, siblingIndex: 0);

            // ── Superficie pietra ──────────────────────────────────
            _innerImage = EnsureImage("TileInner", Vector2.zero, Vector2.one,
                                      new Vector2(4, 4), new Vector2(-4, -6),
                                      UITheme.Colors.StoneBase, raycast: false);

            // ── Highlight incisione lettera (dietro il testo) ──────
            //    Stesso testo ma leggermente più chiaro e offset (+1,-1)
            //    per simulare il bordo luminoso del solco inciso
            _letterHighlight = EnsureTMP("LetterHighlight",
                offset: new Vector2(1.5f, -1.5f),
                color: UITheme.Colors.StoneHighlight,
                fontSize: 72,
                bold: true,
                siblingAtEnd: false);

            // ── Testo lettera principale (inciso scuro) ────────────
            if (letterText != null)
            {
                letterText.transform.SetAsLastSibling();
                letterText.color     = UITheme.Colors.StoneText;
                letterText.fontStyle = FontStyles.Bold;
                letterText.fontSize  = 72;
                letterText.alignment = TextAlignmentOptions.Center;
            }

            // ── Overlay ghiaccio ───────────────────────────────────
            _freezeOverlay = EnsureImage("FreezeOverlay", Vector2.zero, Vector2.one,
                                          Vector2.zero, Vector2.zero,
                                          Color.clear, raycast: false);
            _freezeOverlay.transform.SetAsLastSibling();

            // ── Valore punti (angolo basso-destra, inciso) ─────────
            _pointValueText = EnsureTMP("PointText",
                offset: Vector2.zero,
                color: UITheme.Colors.StonePointText,
                fontSize: 20,
                bold: true,
                siblingAtEnd: true);
            if (_pointValueText != null)
            {
                var rt = _pointValueText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.52f, 0f);
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(0, 3);
                rt.offsetMax = new Vector2(-5, -2);
                _pointValueText.alignment    = TextAlignmentOptions.BottomRight;
                _pointValueText.raycastTarget = false;
            }
        }

        // ── Helper: crea/riusa un nodo Image ──────────────────────
        private Image EnsureImage(string nodeName,
                                   Vector2 anchorMin, Vector2 anchorMax,
                                   Vector2 offsetMin, Vector2 offsetMax,
                                   Color color, bool raycast = false,
                                   int siblingIndex = -1)
        {
            Transform existing = transform.Find(nodeName);
            Image img;
            if (existing != null)
            {
                img = existing.GetComponent<Image>();
            }
            else
            {
                var go = new GameObject(nodeName);
                go.transform.SetParent(transform, false);
                var rt       = go.AddComponent<RectTransform>();
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.offsetMin = offsetMin;
                rt.offsetMax = offsetMax;
                img = go.AddComponent<Image>();
                if (siblingIndex >= 0)
                    go.transform.SetSiblingIndex(siblingIndex);
            }
            img.color          = color;
            img.raycastTarget  = raycast;
            return img;
        }

        // ── Helper: crea/riusa un nodo TextMeshProUGUI ────────────
        private TextMeshProUGUI EnsureTMP(string nodeName,
                                           Vector2 offset,
                                           Color color,
                                           float fontSize,
                                           bool bold,
                                           bool siblingAtEnd)
        {
            Transform existing = transform.Find(nodeName);
            TextMeshProUGUI tmp;
            if (existing != null)
            {
                tmp = existing.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                var go = new GameObject(nodeName);
                go.transform.SetParent(transform, false);
                var rt       = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = offset;
                rt.offsetMax = offset;
                tmp = go.AddComponent<TextMeshProUGUI>();
            }
            tmp.color          = color;
            tmp.fontStyle      = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.fontSize       = fontSize;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;
            if (siblingAtEnd) tmp.transform.SetAsLastSibling();
            return tmp;
        }

        // ----------------------------------------------------------
        // Costruzione crepe (pre-create tutte, poi nascoste)
        // Ogni crepa = due Image: una scura (gap) + una chiara (bordo)
        // ----------------------------------------------------------
        private void BuildCrackImages()
        {
            int n = _cracks.Length;
            _crackDark = new Image[n];
            _crackEdge = new Image[n];

            for (int i = 0; i < n; i++)
            {
                var def = _cracks[i];

                // ── Crepa scura (solco) ────────────────────────────
                _crackDark[i] = BuildCrackStrip($"CrackD{i}", def, 0f, UITheme.Colors.CrackDark);

                // ── Bordo chiaro (bordo frattura, leggero offset) ──
                // offset perpendicolare alla crepa: dx = sin(rot), dy = -cos(rot)
                float θ   = def.Rot * Mathf.Deg2Rad;
                float ofx = Mathf.Sin(θ) * 0.020f;
                float ofy = -Mathf.Cos(θ) * 0.020f;
                var   lightDef = new CrackDef(def.CX + ofx, def.CY + ofy,
                                              def.HalfW * 0.5f, def.HalfH,
                                              def.Rot, def.ShowAt);
                _crackEdge[i] = BuildCrackStrip($"CrackE{i}", lightDef, 0f, UITheme.Colors.CrackEdge);
            }
        }

        private Image BuildCrackStrip(string name, CrackDef def, float startAlpha, Color baseColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var rt = go.AddComponent<RectTransform>();
            // Anchors = proporzione normalizzata della tessera → tile-size independent
            rt.anchorMin = new Vector2(def.CX - def.HalfW, def.CY - def.HalfH);
            rt.anchorMax = new Vector2(def.CX + def.HalfW, def.CY + def.HalfH);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.localRotation = Quaternion.Euler(0f, 0f, def.Rot);

            var img = go.AddComponent<Image>();
            var c   = baseColor;
            c.a     = startAlpha;
            img.color         = c;
            img.raycastTarget = false;

            // Le crepe stanno sopra la superficie ma sotto il testo lettera
            // Verranno ordinate dopo la costruzione completa
            return img;
        }

        // ----------------------------------------------------------
        // Initialize — chiamato da GridManager
        // ----------------------------------------------------------
        public void Initialize(char letter, int row, int col)
        {
            Letter = letter;
            Row    = row;
            Col    = col;

            HitPoints            = 0;
            ConsecutiveWrongHits = 0;
            IsFrozen             = false;
            FrozenTurnsRemaining = 0;

            // Imposta testo lettera
            string upper = letter.ToString().ToUpper();
            if (letterText       != null) letterText.text       = upper;
            if (_letterHighlight != null) _letterHighlight.text = upper;

            RefreshPointValue();
            UpdateDamageVisual(animate: false);
            UpdateFreezeVisual();
            SetSelected(false);
            OrderChildrenZ();
        }

        // ── Ordina i figli: ombre → superficie → crepe → lettere → overlay ──
        private void OrderChildrenZ()
        {
            SetSiblingOrder("TileShadow", 0);
            SetSiblingOrder("TileInner",  1);
            // crepe: 2...(2+n*2-1)
            for (int i = 0; i < _cracks.Length; i++)
            {
                if (_crackDark != null && _crackDark[i] != null)
                    _crackDark[i].transform.SetSiblingIndex(2 + i * 2);
                if (_crackEdge != null && _crackEdge[i] != null)
                    _crackEdge[i].transform.SetSiblingIndex(2 + i * 2 + 1);
            }
            int afterCracks = 2 + _cracks.Length * 2;
            SetSiblingOrder("LetterHighlight", afterCracks);
            if (letterText != null) letterText.transform.SetSiblingIndex(afterCracks + 1);
            SetSiblingOrder("FreezeOverlay",   afterCracks + 2);
            SetSiblingOrder("PointText",        afterCracks + 3);
        }

        private void SetSiblingOrder(string childName, int index)
        {
            var t = transform.Find(childName);
            if (t != null) t.SetSiblingIndex(index);
        }

        // ----------------------------------------------------------
        // Valore punti
        // ----------------------------------------------------------
        public void RefreshPointValue()
        {
            if (_pointValueText == null) return;
            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;
            int val = LetterScoring.GetLetterValue(Letter, isItalian);
            _pointValueText.text = val.ToString();
        }

        // ----------------------------------------------------------
        // Selezione visiva
        // ----------------------------------------------------------
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);

            if (selected)
            {
                if (backgroundImage  != null) backgroundImage.color = UITheme.Colors.StoneBorderSel;
                if (_innerImage      != null) _innerImage.color     = UITheme.Colors.StoneSelected;
                if (letterText       != null) letterText.color      = UITheme.Colors.StoneText;
                if (_letterHighlight != null) _letterHighlight.color = new Color(1f, 0.9f, 0.5f, 0.7f);
                AnimateScale(UITheme.Anim.TileSelectScale, 0.08f);
            }
            else
            {
                if (backgroundImage  != null)
                    backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.StoneBorder;
                if (_innerImage      != null)
                    _innerImage.color = UITheme.TileSurfaceColor(dmgLevel);
                if (letterText       != null) letterText.color      = UITheme.Colors.StoneText;
                if (_letterHighlight != null) _letterHighlight.color = UITheme.Colors.StoneHighlight;
                AnimateScale(1f, 0.10f);
            }
        }

        // ----------------------------------------------------------
        // Sistema danno
        // ----------------------------------------------------------

        /// <summary>
        /// Colpo da parola corretta (+1 hit). Resetta gli errori consecutivi.
        /// Restituisce true se la lettera è rotta (≥6 hit).
        /// </summary>
        public bool ApplyCorrectHit()
        {
            ConsecutiveWrongHits = 0;
            HitPoints = Mathf.Min(HitPoints + 1, 6);
            UpdateDamageVisual(animate: true);
            return IsLetterBroken;
        }

        /// <summary>
        /// Colpo da parola errata (+2 hit equivalenti).
        /// Se 3 errori consecutivi → ghiaccio 2 turni.
        /// Restituisce true se la lettera è stata ghiacciata.
        /// </summary>
        public bool ApplyWrongHit()
        {
            HitPoints = Mathf.Min(HitPoints + 2, 6);
            ConsecutiveWrongHits++;
            UpdateDamageVisual(animate: true);

            if (ConsecutiveWrongHits >= 3 && !IsFrozen)
            {
                FreezeCell();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Decrementa il contatore di freeze (chiamato su ogni parola corretta globale).
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
        // Visuale danno: superficie si scurisce + crepe appaiono
        // ----------------------------------------------------------
        private void UpdateDamageVisual(bool animate)
        {
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);

            // Superficie pietra
            Color targetSurface = UITheme.TileSurfaceColor(dmgLevel);
            if (_innerImage != null && !IsSelected)
            {
                if (animate)
                    StartCoroutine(AnimateColor(_innerImage, targetSurface, 0.20f));
                else
                    _innerImage.color = targetSurface;
            }

            // Mostra/nasconde le crepe
            for (int i = 0; i < _cracks.Length; i++)
            {
                bool show       = dmgLevel >= _cracks[i].ShowAt;
                float targetA   = show ? 1f : 0f;
                float edgeA     = show ? 1f : 0f;

                if (animate && show && (_crackDark[i].color.a < 0.1f))
                {
                    // Crepa appena rivelata → fade-in rapido
                    StartCoroutine(FadeCrack(_crackDark[i], UITheme.Colors.CrackDark,  0.15f));
                    StartCoroutine(FadeCrack(_crackEdge[i], UITheme.Colors.CrackEdge,  0.15f));
                }
                else
                {
                    SetCrackAlpha(_crackDark[i], UITheme.Colors.CrackDark, targetA);
                    SetCrackAlpha(_crackEdge[i], UITheme.Colors.CrackEdge, edgeA);
                }
            }
        }

        private void SetCrackAlpha(Image img, Color baseColor, float alpha)
        {
            if (img == null) return;
            var c = baseColor;
            c.a   = alpha;
            img.color = c;
        }

        private IEnumerator FadeCrack(Image img, Color baseColor, float duration)
        {
            if (img == null) yield break;
            float t   = 0f;
            float end = baseColor.a;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                var c = baseColor;
                c.a   = Mathf.Lerp(0f, end, Mathf.SmoothStep(0f, 1f, t));
                img.color = c;
                yield return null;
            }
            var final = baseColor;
            img.color = final;
        }

        private IEnumerator AnimateColor(Image img, Color target, float duration)
        {
            if (img == null) yield break;
            Color start = img.color;
            float t     = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                img.color = Color.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            img.color = target;
        }

        // ----------------------------------------------------------
        // Visuale ghiaccio
        // ----------------------------------------------------------
        private void UpdateFreezeVisual()
        {
            if (_freezeOverlay != null)
                _freezeOverlay.color = IsFrozen ? UITheme.Colors.FreezeOverlay : Color.clear;
            if (backgroundImage != null && !IsSelected)
                backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.StoneBorder;
        }

        // ----------------------------------------------------------
        // Flash fulmine (parola corretta)
        // ----------------------------------------------------------
        public IEnumerator FlashLightning(Color lightningColor, float holdDuration = 0.09f)
        {
            // Impatto: flash luminoso sulla tessera
            if (_innerImage      != null) _innerImage.color      = lightningColor;
            if (backgroundImage  != null) backgroundImage.color  = lightningColor;
            if (letterText       != null) letterText.color        = Color.white;
            if (_letterHighlight != null) _letterHighlight.color  = Color.clear;
            transform.localScale = Vector3.one * 1.18f;

            yield return new WaitForSeconds(holdDuration);

            // Ritorno allo stato danno corrente
            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);
            if (_innerImage      != null) _innerImage.color      = UITheme.TileSurfaceColor(dmgLevel);
            if (backgroundImage  != null) backgroundImage.color  = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.StoneBorder;
            if (letterText       != null) letterText.color        = UITheme.Colors.StoneText;
            if (_letterHighlight != null) _letterHighlight.color  = UITheme.Colors.StoneHighlight;
            transform.localScale = Vector3.one;
        }

        // ----------------------------------------------------------
        // Flash errore (parola errata)
        // ----------------------------------------------------------
        public IEnumerator FlashWrong(float holdDuration = 0.13f)
        {
            if (backgroundImage != null) backgroundImage.color = UITheme.Colors.TextDanger;
            if (_innerImage     != null) _innerImage.color     = new Color(0.65f, 0.15f, 0.10f, 1f);
            if (letterText      != null) letterText.color      = Color.white;

            yield return new WaitForSeconds(holdDuration);

            int dmgLevel = Mathf.Clamp(HitPoints, 0, 5);
            if (_innerImage      != null) _innerImage.color     = UITheme.TileSurfaceColor(dmgLevel);
            if (backgroundImage  != null) backgroundImage.color = IsFrozen ? UITheme.Colors.FreezeBorder : UITheme.Colors.StoneBorder;
            if (letterText       != null) letterText.color      = UITheme.Colors.StoneText;
        }

        // ----------------------------------------------------------
        // Animazione scala
        // ----------------------------------------------------------
        private void AnimateScale(float target, float duration)
        {
            if (_scaleAnim != null) StopCoroutine(_scaleAnim);
            _scaleAnim = StartCoroutine(ScaleTo(target, duration));
        }

        private IEnumerator ScaleTo(float target, float duration)
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
    }
}
