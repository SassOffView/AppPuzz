// ============================================================
// CreatureIdleAnimator.cs
// Animazione idle: pulsazione lenta di scala + alpha sull'aura
// della creatura nella Home. Stile Pokémon glow.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AppPuzz.UI;

namespace AppPuzz.Creatures
{
    [RequireComponent(typeof(Image))]
    public class CreatureIdleAnimator : MonoBehaviour
    {
        private Image _image;
        private Vector3 _baseScale;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            StartCoroutine(PulseLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            // Reset to base state
            if (_image != null)
            {
                var c = _image.color;
                c.a = 0.18f;
                _image.color = c;
            }
            transform.localScale = _baseScale;
        }

        private IEnumerator PulseLoop()
        {
            float duration = UITheme.Anim.PulseDuration;
            float minAlpha = 0.10f;
            float maxAlpha = 0.26f;
            float minScale = 0.95f;
            float maxScale = 1.05f;

            while (true)
            {
                // Expand phase
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / (duration * 0.5f);
                    float s = Mathf.SmoothStep(0f, 1f, t);
                    float alpha = Mathf.Lerp(minAlpha, maxAlpha, s);
                    float scale = Mathf.Lerp(minScale, maxScale, s);

                    if (_image != null)
                    {
                        var c = _image.color;
                        c.a = alpha;
                        _image.color = c;
                    }
                    transform.localScale = _baseScale * scale;
                    yield return null;
                }

                // Contract phase
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / (duration * 0.5f);
                    float s = Mathf.SmoothStep(0f, 1f, t);
                    float alpha = Mathf.Lerp(maxAlpha, minAlpha, s);
                    float scale = Mathf.Lerp(maxScale, minScale, s);

                    if (_image != null)
                    {
                        var c = _image.color;
                        c.a = alpha;
                        _image.color = c;
                    }
                    transform.localScale = _baseScale * scale;
                    yield return null;
                }
            }
        }
    }
}
