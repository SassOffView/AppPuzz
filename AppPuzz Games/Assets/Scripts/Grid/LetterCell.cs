// ============================================================
// LetterCell.cs — Tessera griglia STILE 2.5D RUZZLE
//
// Aspetto visivo:
//   • Root Image = colore profondità (visibile a destra e in basso)
//   • TileFace (child Image) = superficie principale, offset 5px
//     in alto e a sinistra → espone la profondità sul lato basso/dx
//   • 5 tier di colore per valore lettera:
//       Tier 1 (1pt) Blu  | Tier 2 (2pt) Verde
//       Tier 3 (3pt) Ambra | Tier 4 (4pt) Rosso | Tier 5 (5pt) Viola
//   • Lettera bianca bold, valore punti in basso a destra (traslucido)
//
// Animazioni:
//   • BounceCoroutine()  — spring bounce su selezione (1→1.22→0.9→1)
//   • ShakeWrong()       — shake orizzontale su parola errata
//   • FlashLightning()   — flash colorato su parola corretta
//   • FlashWrong()       — flash rosso su parola errata
//
// Sistema danno (6 livelli):
//   • Livello 0 → tessera intatta
//   • Livelli 1–5 → superficie si scurisce + crepe progressive
//   • Livello 6 → lettera si rompe e viene sostituita
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
        // Riferimenti UI — privati (mai serializzati, sempre creati da codice)
        // ----------------------------------------------------------
        // NOTA: questi NON devono essere public/serializzati.
        // Unity sovrascrive i valori serializzati PRIMA di Awake(), rendendo
        // le reference stantie e invisibili. Li creiamo da zero ogni volta.
        private TextMeshProUGUI letterText;
        private Button          cellButton;
        private Image           backgroundImage;

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
        private Image           _tileFace;         // superficie principale (2.5D)
        private Image           _freezeOverlay;    // overlay ghiaccio
        private TextMeshProUGUI _letterHighlight;  // drop shadow lettera
        private TextMeshProUGUI _pointValueText;   // valore punti

        // Tier colore: 1=blu, 2=verde, 3=ambra, 4=rosso, 5=viola
        private int _tier = 1;

        // Crack system
        private Image[] _crackDark;
        private Image[] _crackEdge;

        // Animazioni
        private Coroutine _bounceAnim;
        private Coroutine _shakeAnim;

        // Profondità 3D (pixel esposti sul lato basso e destro)
        private const float DepthPx = 7f;

        // ----------------------------------------------------------
        // Definizione crepe (normalizzate rispetto al tile 0–1)
        // ----------------------------------------------------------
        private readonly struct CrackDef
        {
            public readonly float CX, CY;
            public readonly float HalfW, HalfH;
            public readonly float Rot;
            public readonly int   ShowAt;

            public CrackDef(float cx, float cy, float hw, float hh, float rot, int showAt)
            { CX=cx; CY=cy; HalfW=hw; HalfH=hh; Rot=rot; ShowAt=showAt; }
        }

        private static readonly CrackDef[] _cracks = {
            new(0.38f, 0.67f, 0.026f, 0.24f, -26f, 1),
            new(0.63f, 0.62f, 0.024f, 0.21f,  22f, 2),
            new(0.30f, 0.40f, 0.020f, 0.17f, -54f, 3),
            new(0.70f, 0.36f, 0.020f, 0.18f,  42f, 3),
            new(0.50f, 0.50f, 0.018f, 0.14f,  88f, 4),
            new(0.20f, 0.53f, 0.017f, 0.13f, -70f, 4),
            new(0.78f, 0.50f, 0.018f, 0.15f,  63f, 5),
            new(0.50f, 0.25f, 0.020f, 0.16f, -33f, 5),
        };

        // ----------------------------------------------------------
        // Awake — costruisce la gerarchia visiva
        // ----------------------------------------------------------
        private void Awake()
        {
            // Rimuovi nodi legacy da eventuali prefab vecchi (stile pietra)
            DestroyChildByName("TileShadow");
            DestroyChildByName("TileInner");
            BuildTileHierarchy();
            BuildCrackImages();
        }

        private void Start()
        {
            if (cellButton != null)
                cellButton.onClick.AddListener(() => OnCellClicked?.Invoke(this));
        }

        // ----------------------------------------------------------
        // Costruzione gerarchia 2.5D
        // ----------------------------------------------------------
        private void BuildTileHierarchy()
        {
            // ── ROOT = colore profondità (visibile sul lato basso e destro) ──
            // backgroundImage è privato → non serializzato → sempre ricreato
            backgroundImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            backgroundImage.color         = UITheme.Colors.Tile1Depth;
            backgroundImage.raycastTarget = true;

            // Recupera Button se già presente sul GO
            cellButton = GetComponent<Button>();

            // ── TileFace: superficie principale, offset per effetto 3D ────────
            _tileFace = EnsureImage("TileFace", Vector2.zero, Vector2.one,
                                    new Vector2(0f, DepthPx), new Vector2(-DepthPx, 0f),
                                    UITheme.Colors.Tile1Face, raycast: false);

            // ── Top-edge highlight: striscia luminosa sul bordo superiore ────
            {
                DestroyChildByName("TopEdgeHighlight");
                var goTe = new GameObject("TopEdgeHighlight");
                goTe.transform.SetParent(_tileFace.transform, false);
                var rtTe       = goTe.AddComponent<RectTransform>();
                rtTe.anchorMin = new Vector2(0f, 1f);
                rtTe.anchorMax = new Vector2(1f, 1f);
                rtTe.pivot     = new Vector2(0.5f, 1f);
                rtTe.offsetMin = new Vector2(2f, -5f);
                rtTe.offsetMax = new Vector2(-2f, -1f);
                var imgTe = goTe.AddComponent<Image>();
                imgTe.color         = new Color(1f, 1f, 1f, 0.40f);
                imgTe.raycastTarget = false;
            }

            // ── Left-edge highlight: striscia verticale sul bordo sinistro ────
            {
                DestroyChildByName("LeftEdgeHighlight");
                var goLe = new GameObject("LeftEdgeHighlight");
                goLe.transform.SetParent(_tileFace.transform, false);
                var rtLe       = goLe.AddComponent<RectTransform>();
                rtLe.anchorMin = new Vector2(0f, 0f);
                rtLe.anchorMax = new Vector2(0f, 1f);
                rtLe.pivot     = new Vector2(0f, 0.5f);
                rtLe.offsetMin = new Vector2(1f, 2f);
                rtLe.offsetMax = new Vector2(4f, -2f);
                var imgLe = goLe.AddComponent<Image>();
                imgLe.color         = new Color(1f, 1f, 1f, 0.15f);
                imgLe.raycastTarget = false;
            }

            // ── Bottom-edge shadow: ombra interna sul bordo inferiore ────
            {
                DestroyChildByName("BottomEdgeShadow");
                var goBot = new GameObject("BottomEdgeShadow");
                goBot.transform.SetParent(_tileFace.transform, false);
                var rtBot       = goBot.AddComponent<RectTransform>();
                rtBot.anchorMin = new Vector2(0f, 0f);
                rtBot.anchorMax = new Vector2(1f, 0f);
                rtBot.pivot     = new Vector2(0.5f, 0f);
                rtBot.offsetMin = new Vector2(2f, 1f);
                rtBot.offsetMax = new Vector2(-2f, 4f);
                var imgBot = goBot.AddComponent<Image>();
                imgBot.color         = new Color(0f, 0f, 0f, 0.20f);
                imgBot.raycastTarget = false;
            }

            // ── Drop shadow lettera — SEMPRE distrutto e ricreato da zero ────
            // (evita qualsiasi stato stantio dal prefab)
            DestroyChildByName("LetterHighlight");
            {
                var go = new GameObject("LetterHighlight");
                go.transform.SetParent(transform, false);
                var rt       = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
                // shadow: offset 2px basso/destra rispetto alla lettera principale
                rt.offsetMin = new Vector2(2f, -2f);
                rt.offsetMax = new Vector2(2f, -2f);
                _letterHighlight                   = go.AddComponent<TextMeshProUGUI>();
                _letterHighlight.color             = new Color(0f, 0f, 0f, 0.50f);
                _letterHighlight.fontStyle         = FontStyles.Bold;
                _letterHighlight.fontSize          = 72f;
                _letterHighlight.enableAutoSizing  = false;
                _letterHighlight.alignment         = TextAlignmentOptions.Center;
                _letterHighlight.overflowMode      = TMPro.TextOverflowModes.Overflow;
                _letterHighlight.raycastTarget     = false;
                _letterHighlight.text              = "";
            }

            // ── Testo lettera principale — SEMPRE distrutto e ricreato da zero ──
            // fontSize fisso: auto-sizing può fallire silenziosamente se il rect
            // non è ancora stato calcolato dalla canvas al momento di Awake.
            DestroyChildByName("LetterText");
            {
                var go = new GameObject("LetterText");
                go.transform.SetParent(transform, false);
                var rt       = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                letterText                   = go.AddComponent<TextMeshProUGUI>();
                letterText.color             = Color.white;
                letterText.fontStyle         = FontStyles.Bold;
                letterText.fontSize          = 72f;
                letterText.enableAutoSizing  = false;
                letterText.alignment         = TextAlignmentOptions.Center;
                letterText.overflowMode      = TMPro.TextOverflowModes.Overflow;
                letterText.raycastTarget     = false;
                letterText.text              = "";
            }

            // ── Overlay ghiaccio ─────────────────────────────────────────────
            _freezeOverlay = EnsureImage("FreezeOverlay", Vector2.zero, Vector2.one,
                                          Vector2.zero, Vector2.zero,
                                          Color.clear, raycast: false);

            // ── Valore punti (angolo basso-destra, semi-traslucido) ──────────
            _pointValueText = EnsureTMP("PointText",
                offset: Vector2.zero,
                color: new Color(1f, 1f, 1f, 0.75f),
                fontSize: 20,
                bold: false,
                siblingAtEnd: true);
            if (_pointValueText != null)
            {
                var rt = _pointValueText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(0f, DepthPx + 2f);
                rt.offsetMax = new Vector2(-DepthPx - 2f, -2f);
                _pointValueText.alignment     = TextAlignmentOptions.BottomRight;
                _pointValueText.raycastTarget = false;
            }
        }

        // ── Elimina figlio per nome (pulizia nodi legacy) ─────────────────
        private void DestroyChildByName(string name)
        {
            var t = transform.Find(name);
            if (t != null) DestroyImmediate(t.gameObject);
        }

        // ── Cerca o crea TMP per nome (ignora riferimenti Inspector) ────────
        private TextMeshProUGUI FindOrCreateTMP(string nodeName)
        {
            var existing = transform.Find(nodeName);
            if (existing != null)
            {
                var t = existing.GetComponent<TextMeshProUGUI>();
                if (t != null) return t;
                return existing.gameObject.AddComponent<TextMeshProUGUI>();
            }
            var go = new GameObject(nodeName);
            go.transform.SetParent(transform, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<TextMeshProUGUI>();
        }

        // ── Crea/riusa Image, aggiorna sempre RectTransform ─────────────────
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
                img = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
                var rt       = img.GetComponent<RectTransform>();
                rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
                rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            }
            else
            {
                var go = new GameObject(nodeName);
                go.transform.SetParent(transform, false);
                var rt       = go.AddComponent<RectTransform>();
                rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
                rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
                img = go.AddComponent<Image>();
                if (siblingIndex >= 0)
                    go.transform.SetSiblingIndex(siblingIndex);
            }
            img.color         = color;
            img.raycastTarget = raycast;
            return img;
        }

        // ── Crea/riusa TMP ───────────────────────────────────────────────────
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
                tmp = existing.GetComponent<TextMeshProUGUI>() ??
                      existing.gameObject.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                var go = new GameObject(nodeName);
                go.transform.SetParent(transform, false);
                tmp = go.AddComponent<TextMeshProUGUI>();
            }
            var rt       = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offset;
            rt.offsetMax = offset;
            tmp.color            = color;
            tmp.fontStyle        = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin      = 12f;
            tmp.fontSizeMax      = fontSize;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.overflowMode     = TMPro.TextOverflowModes.Overflow;
            tmp.raycastTarget    = false;
            if (siblingAtEnd) tmp.transform.SetAsLastSibling();
            return tmp;
        }

        // ----------------------------------------------------------
        // Costruzione crepe
        // ----------------------------------------------------------
        private void BuildCrackImages()
        {
            int n = _cracks.Length;
            _crackDark = new Image[n];
            _crackEdge = new Image[n];

            for (int i = 0; i < n; i++)
            {
                var def = _cracks[i];
                _crackDark[i] = BuildCrackStrip($"CrackD{i}", def, UITheme.Colors.CrackDark);

                float θ   = def.Rot * Mathf.Deg2Rad;
                float ofx = Mathf.Sin(θ) * 0.020f;
                float ofy = -Mathf.Cos(θ) * 0.020f;
                var   lightDef = new CrackDef(def.CX + ofx, def.CY + ofy,
                                              def.HalfW * 0.5f, def.HalfH,
                                              def.Rot, def.ShowAt);
                _crackEdge[i] = BuildCrackStrip($"CrackE{i}", lightDef, UITheme.Colors.CrackEdge);
            }
        }

        private Image BuildCrackStrip(string name, CrackDef def, Color baseColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin     = new Vector2(def.CX - def.HalfW, def.CY - def.HalfH);
            rt.anchorMax     = new Vector2(def.CX + def.HalfW, def.CY + def.HalfH);
            rt.offsetMin     = rt.offsetMax = Vector2.zero;
            rt.localRotation = Quaternion.Euler(0f, 0f, def.Rot);
            var img = go.AddComponent<Image>();
            var c   = baseColor;
            c.a     = 0f;
            img.color         = c;
            img.raycastTarget = false;
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

            string upper = letter.ToString().ToUpper();
            if (letterText       != null) letterText.text       = upper;
            if (_letterHighlight != null) _letterHighlight.text = upper;

            ApplyTierColors();
            RefreshPointValue();
            UpdateDamageVisual(animate: false);
            UpdateFreezeVisual();
            SetSelected(false);
            OrderChildrenZ();
        }

        // ── Applica i colori in base al valore/tier della lettera ────────────
        private void ApplyTierColors()
        {
            bool isItalian = LanguageManager.Instance == null ||
                             LanguageManager.Instance.CurrentLanguage == Language.Italian;
            _tier = Mathf.Clamp(LetterScoring.GetLetterValue(Letter, isItalian), 1, 5);
            var colors = UITheme.GetTileColors(_tier, IsSelected, IsFrozen);
            if (backgroundImage != null) backgroundImage.color = colors.Depth;
            if (_tileFace       != null) _tileFace.color       = colors.Face;
        }

        // ── Faccia con danno applicato (scurisce progressivamente) ───────────
        private Color GetDamagedFaceColor()
        {
            var colors = UITheme.GetTileColors(_tier, IsSelected, IsFrozen);
            float darken = HitPoints * 0.10f; // 0% intatta → 60% critica
            return Color.Lerp(colors.Face, new Color(0.04f, 0.02f, 0.06f, 1f), darken);
        }

        // ── Ordina Z: TileFace → crepe → lettere → overlay → punti ──────────
        private void OrderChildrenZ()
        {
            var order = new List<Transform>();
            void Add(string n) { var t = transform.Find(n); if (t != null) order.Add(t); }

            Add("TileFace");
            for (int i = 0; i < _cracks.Length; i++)
            {
                if (_crackDark?[i] != null) order.Add(_crackDark[i].transform);
                if (_crackEdge?[i] != null) order.Add(_crackEdge[i].transform);
            }
            Add("LetterHighlight");
            if (letterText != null) order.Add(letterText.transform);
            Add("FreezeOverlay");
            Add("PointText");

            foreach (var t in order)
                t.SetAsLastSibling();
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
        // Selezione visiva + bounce animation
        // ----------------------------------------------------------
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            var colors = UITheme.GetTileColors(_tier, selected, IsFrozen);
            if (backgroundImage != null) backgroundImage.color = colors.Depth;
            if (_tileFace       != null) _tileFace.color       = selected ? colors.Face : GetDamagedFaceColor();

            if (selected)
            {
                if (_bounceAnim != null) StopCoroutine(_bounceAnim);
                _bounceAnim = StartCoroutine(BounceCoroutine());
            }
            else
            {
                if (_bounceAnim != null) StopCoroutine(_bounceAnim);
                _bounceAnim = StartCoroutine(ScaleTo(1f, 0.12f));
            }
        }

        // ----------------------------------------------------------
        // Sistema danno
        // ----------------------------------------------------------
        public bool ApplyCorrectHit()
        {
            ConsecutiveWrongHits = 0;
            HitPoints = Mathf.Min(HitPoints + 1, 6);
            UpdateDamageVisual(animate: true);
            return IsLetterBroken;
        }

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
        // Visuale danno: faccia si scurisce + crepe compaiono
        // ----------------------------------------------------------
        private void UpdateDamageVisual(bool animate)
        {
            Color targetFace = GetDamagedFaceColor();
            if (_tileFace != null && !IsSelected)
            {
                if (animate)
                    StartCoroutine(AnimateColor(_tileFace, targetFace, 0.20f));
                else
                    _tileFace.color = targetFace;
            }

            for (int i = 0; i < _cracks.Length; i++)
            {
                bool show = HitPoints >= _cracks[i].ShowAt;
                if (animate && show && _crackDark[i].color.a < 0.1f)
                {
                    StartCoroutine(FadeCrack(_crackDark[i], UITheme.Colors.CrackDark, 0.15f));
                    StartCoroutine(FadeCrack(_crackEdge[i], UITheme.Colors.CrackEdge, 0.15f));
                }
                else
                {
                    SetCrackAlpha(_crackDark[i], UITheme.Colors.CrackDark, show ? 1f : 0f);
                    SetCrackAlpha(_crackEdge[i], UITheme.Colors.CrackEdge, show ? 1f : 0f);
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
            float t = 0f, end = baseColor.a;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                var c = baseColor;
                c.a   = Mathf.Lerp(0f, end, Mathf.SmoothStep(0f, 1f, t));
                img.color = c;
                yield return null;
            }
            img.color = baseColor;
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
            var colors = UITheme.GetTileColors(_tier, IsSelected, IsFrozen);
            if (backgroundImage != null) backgroundImage.color = colors.Depth;
            if (_tileFace       != null) _tileFace.color       = IsFrozen ? colors.Face : GetDamagedFaceColor();
        }

        // ----------------------------------------------------------
        // Flash fulmine (parola corretta)
        // ----------------------------------------------------------
        public IEnumerator FlashLightning(Color lightningColor, float holdDuration = 0.09f)
        {
            if (_tileFace       != null) _tileFace.color       = lightningColor;
            if (backgroundImage != null) backgroundImage.color = lightningColor;
            if (letterText      != null) letterText.color      = Color.black;
            transform.localScale = Vector3.one * 1.18f;

            yield return new WaitForSeconds(holdDuration);

            if (_tileFace       != null) _tileFace.color       = GetDamagedFaceColor();
            if (backgroundImage != null) backgroundImage.color = UITheme.GetTileColors(_tier, IsSelected, IsFrozen).Depth;
            if (letterText      != null) letterText.color      = Color.white;
            transform.localScale = Vector3.one;
        }

        // ----------------------------------------------------------
        // Flash errore (parola errata)
        // ----------------------------------------------------------
        public IEnumerator FlashWrong(float holdDuration = 0.13f)
        {
            if (_tileFace       != null) _tileFace.color       = UITheme.Colors.TextDanger;
            if (backgroundImage != null) backgroundImage.color = new Color(0.55f, 0.05f, 0.05f, 1f);
            if (letterText      != null) letterText.color      = Color.white;

            yield return new WaitForSeconds(holdDuration);

            if (_tileFace       != null) _tileFace.color       = GetDamagedFaceColor();
            if (backgroundImage != null) backgroundImage.color = UITheme.GetTileColors(_tier, IsSelected, IsFrozen).Depth;
            if (letterText      != null) letterText.color      = Color.white;
        }

        // ----------------------------------------------------------
        // Shake (parola errata) — chiamato da GameManager
        // ----------------------------------------------------------
        public void ShakeWrong()
        {
            if (_shakeAnim != null) StopCoroutine(_shakeAnim);
            _shakeAnim = StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            Vector3 orig    = transform.localPosition;
            float[] offsets = { -7f, 7f, -5f, 5f, -3f, 3f, 0f };
            foreach (float dx in offsets)
            {
                transform.localPosition = orig + new Vector3(dx, 0f, 0f);
                yield return new WaitForSeconds(0.04f);
            }
            transform.localPosition = orig;
            _shakeAnim = null;
        }

        // ----------------------------------------------------------
        // Bounce selezione (spring animation)
        // ----------------------------------------------------------
        private IEnumerator BounceCoroutine()
        {
            // Keyframes: spring snappy (più elastico e veloce)
            float[] times  = { 0f,    0.06f, 0.13f, 0.19f, 0.24f, 0.28f };
            float[] scales = { 1.0f,  1.25f, 0.88f, 1.10f, 0.96f, 1.00f };

            float elapsed = 0f;
            float total   = times[times.Length - 1];

            while (elapsed < total)
            {
                elapsed += Time.deltaTime;
                int seg = 0;
                for (int i = 0; i < times.Length - 2; i++)
                    if (elapsed > times[i + 1]) seg = i + 1;
                seg = Mathf.Min(seg, times.Length - 2);

                float t0 = times[seg], t1 = times[seg + 1];
                float s0 = scales[seg], s1 = scales[seg + 1];
                float frac = (t1 > t0) ? Mathf.Clamp01((elapsed - t0) / (t1 - t0)) : 1f;
                transform.localScale = Vector3.one * Mathf.Lerp(s0, s1, Mathf.SmoothStep(0f, 1f, frac));
                yield return null;
            }
            transform.localScale = Vector3.one;
            _bounceAnim = null;
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
