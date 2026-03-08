// ============================================================
// CreatureSpriteLoader.cs
// Carica a runtime gli sprite delle creature da Resources/Creatures/.
// Se lo sprite non esiste mostra il placeholder colorato.
//
// Come aggiungere le tue immagini:
//   1. Esporta ogni creatura come PNG (512×512 o 1024×1024, RGBA)
//   2. Metti i file in:  Assets/Resources/Creatures/
//   3. Rinomina così:
//        dragon.png   → Mental Dragon
//        wolf.png     → Astral Wolf
//        serpent.png  → Ethereal Serpent
//   4. In Unity Inspector imposta:
//        Texture Type  = Sprite (2D and UI)
//        Filter Mode   = Bilinear
//        Compression   = High Quality
//   Il sistema li caricherà in automatico.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace AppPuzz.Creatures
{
    public static class CreatureSpriteLoader
    {
        // Nomi file attesi in Resources/Creatures/
        private static readonly string[] ResourceNames =
        {
            "dragon",   // Mental Dragon   (indice 0)
            "wolf",     // Astral Wolf      (indice 1)
            "serpent",  // Ethereal Serpent (indice 2)
        };

        // Cache per evitare di ricaricare ogni frame
        private static readonly Sprite[] _cache = new Sprite[3];
        private static readonly bool[]   _tried  = new bool[3];

        /// <summary>
        /// Applica lo sprite della creatura all'Image indicata.
        /// Se lo sprite non è disponibile, mantiene il colore placeholder.
        /// </summary>
        public static void Apply(Image img, int creatureIndex)
        {
            if (img == null || creatureIndex < 0 || creatureIndex >= ResourceNames.Length)
                return;

            var sprite = Load(creatureIndex);
            if (sprite != null)
            {
                img.sprite          = sprite;
                img.color           = Color.white;         // rimuove tinta placeholder
                img.preserveAspect  = true;
                img.type            = Image.Type.Simple;
                img.raycastTarget   = false;
            }
            // Se sprite == null rimane il colore placeholder assegnato da UIAutoSetup
        }

        /// <summary>
        /// Carica (con cache) lo sprite dal percorso Resources/Creatures/<name>.
        /// Ritorna null se il file non esiste.
        /// </summary>
        public static Sprite Load(int creatureIndex)
        {
            if (creatureIndex < 0 || creatureIndex >= ResourceNames.Length) return null;

            if (!_tried[creatureIndex])
            {
                _tried[creatureIndex]  = true;
                string path = "Creatures/" + ResourceNames[creatureIndex];
                _cache[creatureIndex]  = Resources.Load<Sprite>(path);
                if (_cache[creatureIndex] == null)
                    Debug.Log($"[CreatureSpriteLoader] Sprite non trovata: Resources/{path}.png — " +
                              $"verrà usato il placeholder colorato. " +
                              $"Aggiungi il file per mostrare l'immagine vera.");
                else
                    Debug.Log($"[CreatureSpriteLoader] Sprite caricata: {path}");
            }
            return _cache[creatureIndex];
        }

        /// <summary>Invalida la cache (utile se si caricano sprite a caldo).</summary>
        public static void ClearCache()
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                _cache[i] = null;
                _tried[i] = false;
            }
        }
    }
}
