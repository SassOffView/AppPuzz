// ============================================================
// RoundedRectHelper.cs
// Genera a runtime sprite con angoli arrotondati da usare
// come immagine 9-sliced per bottoni, card, chip, popup.
// Nessun asset esterno richiesto — tutto in memoria.
// ============================================================

using UnityEngine;

namespace AppPuzz.UI
{
    public static class RoundedRectHelper
    {
        // Sprite condivise per evitare di ricrearle ad ogni frame
        private static Sprite _smallRadius;  // r=8
        private static Sprite _medRadius;    // r=16
        private static Sprite _largeRadius;  // r=24

        /// <summary>Applica uno sprite 9-sliced con angoli arrotondati all'Image.</summary>
        public static void Apply(UnityEngine.UI.Image img, float cornerRadius = 16f)
        {
            if (img == null) return;
            img.sprite      = GetSprite(cornerRadius);
            img.type        = UnityEngine.UI.Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
        }

        /// <summary>Restituisce (o crea) una sprite con raggio specificato.</summary>
        public static Sprite GetSprite(float cornerRadius = 16f)
        {
            if (cornerRadius <= 10f)
            {
                if (_smallRadius == null) _smallRadius = Build(64, 64, 8f);
                return _smallRadius;
            }
            if (cornerRadius <= 20f)
            {
                if (_medRadius == null) _medRadius = Build(64, 64, 16f);
                return _medRadius;
            }
            if (_largeRadius == null) _largeRadius = Build(96, 96, 24f);
            return _largeRadius;
        }

        // ----------------------------------------------------------
        // Internals
        // ----------------------------------------------------------

        private static Sprite Build(int w, int h, float r)
        {
            var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode   = TextureWrapMode.Clamp;

            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = GetRoundedAlpha(x, y, w, h, r);
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            // Border per 9-slicing: zona interna non deformabile = r px da ogni lato
            float border = r;
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100f,
                extrude: 0,
                meshType: SpriteMeshType.FullRect,
                border: new Vector4(border, border, border, border)
            );
            return sprite;
        }

        /// <summary>
        /// Ritorna 1 se il pixel (x,y) è dentro il rettangolo arrotondato,
        /// usa anti-aliasing di 1px sul bordo dell'angolo.
        /// </summary>
        private static float GetRoundedAlpha(int x, int y, int w, int h, float r)
        {
            // Pixel nei quadranti interni (lontani dagli angoli): sempre opaco
            float fx = x + 0.5f;
            float fy = y + 0.5f;

            bool nearRight  = fx > w - r;
            bool nearLeft   = fx < r;
            bool nearTop    = fy > h - r;
            bool nearBottom = fy < r;

            if (!((nearLeft || nearRight) && (nearTop || nearBottom)))
                return 1f; // non in un angolo

            // Coordinate relative al centro dell'angolo
            float cx = nearLeft ? r : w - r;
            float cy = nearBottom ? r : h - r;

            float dist = Mathf.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy));

            // Anti-alias di 1 pixel
            return Mathf.Clamp01(r - dist + 0.5f);
        }
    }
}
