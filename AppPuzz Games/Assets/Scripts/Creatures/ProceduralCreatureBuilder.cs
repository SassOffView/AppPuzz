// ============================================================
// ProceduralCreatureBuilder.cs
// Genera creature composte da forme geometriche (Image layers)
// quando non sono disponibili sprite PNG.
//
// Ogni creatura ha 10-12 layer sovrapposti:
//   aura → corpo → dettagli → occhi → accessori livello
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AppPuzz.Creatures
{
    public static class ProceduralCreatureBuilder
    {
        /// <summary>
        /// Costruisce la creatura procedurale sotto il parent.
        /// Rimuove eventuali figli precedenti con tag "PCB_".
        /// </summary>
        public static void Build(Transform parent, CreatureType type, int level)
        {
            // Pulisci creature precedenti
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.name.StartsWith("PCB_"))
                    Object.DestroyImmediate(child.gameObject);
            }

            switch (type)
            {
                case CreatureType.MentalDragon:    BuildDragon(parent, level);  break;
                case CreatureType.AstralWolf:      BuildWolf(parent, level);    break;
                case CreatureType.EtherealSerpent:  BuildSerpent(parent, level); break;
            }
        }

        // ==============================================================
        // MENTAL DRAGON — Drago Mentale (viola)
        // ==============================================================
        private static void BuildDragon(Transform parent, int level)
        {
            Color primary = UI.UITheme.Colors.TypeDragon;   // #7B2FFF
            Color accent  = UI.UITheme.Colors.DragonAccent;  // #B06FFF
            Color dark    = HEX("#553880");

            // 1. Aura glow
            MakeShape(parent, "PCB_Aura", 0.05f, 0.05f, 0.95f, 0.95f,
                      new Color(primary.r, primary.g, primary.b, 0.12f));

            // 2. Coda (dietro il corpo)
            MakeShape(parent, "PCB_Tail", 0.58f, 0.08f, 0.82f, 0.28f, dark);

            // 3. Ali — sinistra e destra
            MakeShape(parent, "PCB_WingL", 0.02f, 0.35f, 0.22f, 0.70f,
                      new Color(dark.r, dark.g, dark.b, 0.70f));
            MakeShape(parent, "PCB_WingR", 0.78f, 0.35f, 0.98f, 0.70f,
                      new Color(dark.r, dark.g, dark.b, 0.70f));

            // 4. Corpo principale
            MakeShape(parent, "PCB_Body", 0.22f, 0.12f, 0.78f, 0.68f, primary);

            // 5. Ventre chiaro
            MakeShape(parent, "PCB_Belly", 0.30f, 0.12f, 0.70f, 0.42f, accent);

            // 6. Testa
            MakeShape(parent, "PCB_Head", 0.28f, 0.55f, 0.72f, 0.88f, primary);

            // 7. Corna
            MakeShape(parent, "PCB_HornL", 0.24f, 0.80f, 0.38f, 0.96f, HEX("#9B6EC8"));
            MakeShape(parent, "PCB_HornR", 0.62f, 0.80f, 0.76f, 0.96f, HEX("#9B6EC8"));

            // 8. Occhi
            MakeShape(parent, "PCB_EyeL", 0.34f, 0.66f, 0.44f, 0.78f, HEX("#FFD700"));
            MakeShape(parent, "PCB_EyeR", 0.56f, 0.66f, 0.66f, 0.78f, HEX("#FFD700"));

            // 9. Pupille
            MakeShape(parent, "PCB_PupilL", 0.37f, 0.68f, 0.42f, 0.76f, Color.black);
            MakeShape(parent, "PCB_PupilR", 0.58f, 0.68f, 0.63f, 0.76f, Color.black);

            // 10. Bocca
            MakeShape(parent, "PCB_Mouth", 0.40f, 0.56f, 0.60f, 0.60f,
                      new Color(0.2f, 0.05f, 0.3f, 0.80f));

            // 11. Livello 2+: fiamma
            if (level >= 2)
            {
                MakeShape(parent, "PCB_Flame1", 0.60f, 0.52f, 0.74f, 0.62f, HEX("#FF6000"));
                MakeShape(parent, "PCB_Flame2", 0.64f, 0.48f, 0.72f, 0.56f, HEX("#FFAA00"));
            }

            // 12. Livello 3: corona
            if (level >= 3)
            {
                MakeText(parent, "PCB_Crown", "♛", 0.32f, 0.88f, 0.68f, 1.00f,
                         HEX("#FFD700"), 36);
            }
        }

        // ==============================================================
        // ASTRAL WOLF — Lupo Astrale (blu)
        // ==============================================================
        private static void BuildWolf(Transform parent, int level)
        {
            Color primary = UI.UITheme.Colors.TypeWolf;    // #2B8EFF
            Color accent  = UI.UITheme.Colors.WolfAccent;   // #70B8FF
            Color dark    = HEX("#1A6ABB");

            // 1. Aura
            MakeShape(parent, "PCB_Aura", 0.05f, 0.05f, 0.95f, 0.95f,
                      new Color(primary.r, primary.g, primary.b, 0.12f));

            // 2. Coda
            MakeShape(parent, "PCB_Tail", 0.62f, 0.06f, 0.90f, 0.30f, dark);

            // 3. Corpo (ovale verticale)
            MakeShape(parent, "PCB_Body", 0.20f, 0.10f, 0.75f, 0.62f, primary);

            // 4. Pancia chiara
            MakeShape(parent, "PCB_Belly", 0.30f, 0.10f, 0.65f, 0.38f, accent);

            // 5. Testa
            MakeShape(parent, "PCB_Head", 0.25f, 0.52f, 0.70f, 0.85f, primary);

            // 6. Orecchie
            MakeShape(parent, "PCB_EarL", 0.20f, 0.78f, 0.35f, 0.98f, dark);
            MakeShape(parent, "PCB_EarR", 0.60f, 0.78f, 0.75f, 0.98f, dark);

            // 7. Muso
            MakeShape(parent, "PCB_Snout", 0.35f, 0.52f, 0.60f, 0.65f, accent);

            // 8. Naso
            MakeShape(parent, "PCB_Nose", 0.43f, 0.60f, 0.52f, 0.66f, Color.black);

            // 9. Occhi
            MakeShape(parent, "PCB_EyeL", 0.30f, 0.66f, 0.40f, 0.77f, HEX("#FFD700"));
            MakeShape(parent, "PCB_EyeR", 0.55f, 0.66f, 0.65f, 0.77f, HEX("#FFD700"));

            // 10. Pupille
            MakeShape(parent, "PCB_PupilL", 0.33f, 0.68f, 0.38f, 0.75f, Color.black);
            MakeShape(parent, "PCB_PupilR", 0.57f, 0.68f, 0.62f, 0.75f, Color.black);

            // 11. Zampe anteriori
            MakeShape(parent, "PCB_PawL", 0.22f, 0.02f, 0.34f, 0.16f, dark);
            MakeShape(parent, "PCB_PawR", 0.58f, 0.02f, 0.70f, 0.16f, dark);

            // 12. Livello 2+: stelline
            if (level >= 2)
            {
                MakeText(parent, "PCB_Star1", "*", 0.08f, 0.72f, 0.18f, 0.82f,
                         new Color(1f, 1f, 1f, 0.45f), 20);
                MakeText(parent, "PCB_Star2", "*", 0.78f, 0.82f, 0.88f, 0.92f,
                         new Color(1f, 1f, 1f, 0.35f), 16);
                MakeText(parent, "PCB_Star3", "*", 0.82f, 0.45f, 0.92f, 0.55f,
                         new Color(1f, 1f, 1f, 0.30f), 14);
            }

            // 13. Livello 3: aureola
            if (level >= 3)
            {
                MakeText(parent, "PCB_Halo", "O", 0.30f, 0.90f, 0.65f, 1.02f,
                         HEX("#FFD700"), 32);
            }
        }

        // ==============================================================
        // ETHEREAL SERPENT — Serpente Etereo (verde)
        // ==============================================================
        private static void BuildSerpent(Transform parent, int level)
        {
            Color primary = UI.UITheme.Colors.TypeSerpent;    // #1EC87C
            Color accent  = UI.UITheme.Colors.SerpentAccent;   // #60E8A8
            Color dark    = HEX("#157A50");

            // 1. Aura
            MakeShape(parent, "PCB_Aura", 0.05f, 0.05f, 0.95f, 0.95f,
                      new Color(primary.r, primary.g, primary.b, 0.12f));

            // 2. Spirale coda (sinistra)
            MakeShape(parent, "PCB_TailCoil", 0.05f, 0.18f, 0.28f, 0.42f, dark);

            // 3. Corpo principale (lungo orizzontale)
            MakeShape(parent, "PCB_Body", 0.12f, 0.28f, 0.82f, 0.58f, primary);

            // 4. Pattern scuri lungo il corpo
            MakeShape(parent, "PCB_Pat1", 0.20f, 0.32f, 0.30f, 0.54f, dark);
            MakeShape(parent, "PCB_Pat2", 0.38f, 0.32f, 0.48f, 0.54f, dark);
            MakeShape(parent, "PCB_Pat3", 0.56f, 0.32f, 0.66f, 0.54f, dark);

            // 5. Ventre chiaro
            MakeShape(parent, "PCB_Belly", 0.18f, 0.26f, 0.76f, 0.38f, accent);

            // 6. Testa (destra, più grande)
            MakeShape(parent, "PCB_Head", 0.62f, 0.48f, 0.92f, 0.82f, primary);

            // 7. Squame decorative
            MakeShape(parent, "PCB_Scale1", 0.70f, 0.74f, 0.78f, 0.82f, accent);
            MakeShape(parent, "PCB_Scale2", 0.80f, 0.70f, 0.88f, 0.78f, accent);

            // 8. Occhi (rossi — serpente)
            MakeShape(parent, "PCB_EyeL", 0.68f, 0.62f, 0.76f, 0.72f, HEX("#FF4040"));
            MakeShape(parent, "PCB_EyeR", 0.80f, 0.62f, 0.88f, 0.72f, HEX("#FF4040"));

            // 9. Pupille a fessura (rettangoli verticali)
            MakeShape(parent, "PCB_PupilL", 0.71f, 0.63f, 0.74f, 0.71f, Color.black);
            MakeShape(parent, "PCB_PupilR", 0.83f, 0.63f, 0.86f, 0.71f, Color.black);

            // 10. Lingua
            MakeShape(parent, "PCB_Tongue", 0.90f, 0.56f, 0.98f, 0.60f, HEX("#FF2020"));

            // 11. Livello 2+: gocce veleno
            if (level >= 2)
            {
                MakeShape(parent, "PCB_Venom1", 0.92f, 0.50f, 0.97f, 0.56f,
                           HEX("#80FF00"));
                MakeShape(parent, "PCB_Venom2", 0.88f, 0.44f, 0.93f, 0.50f,
                           HEX("#80FF00"));
            }

            // 12. Livello 3: corona serpente
            if (level >= 3)
            {
                MakeText(parent, "PCB_Crown", "♔", 0.66f, 0.78f, 0.90f, 0.94f,
                         HEX("#FFD700"), 28);
            }
        }

        // ==============================================================
        // Helper: crea un Image rettangolare posizionato con anchor
        // ==============================================================
        private static Image MakeShape(Transform parent, string name,
            float xMin, float yMin, float xMax, float yMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color         = color;
            img.raycastTarget = false;
            return img;
        }

        // ==============================================================
        // Helper: crea un TMP per icone/simboli (stelle, corone, etc.)
        // ==============================================================
        private static void MakeText(Transform parent, string name, string text,
            float xMin, float yMin, float xMax, float yMax,
            Color color, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.color         = color;
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = false;
            tmp.overflowMode  = TMPro.TextOverflowModes.Overflow;
        }

        private static Color HEX(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
