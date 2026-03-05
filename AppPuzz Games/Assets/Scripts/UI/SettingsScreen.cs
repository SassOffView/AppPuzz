// ============================================================
// SettingsScreen.cs
// Schermata Impostazioni minimale con pulsante Indietro.
// Click gestito via RaycastAll() come negli altri screen.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AppPuzz.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        public Button backButton;

        private void Update()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                clicked = true;
            if (!clicked) return;

            var es = EventSystem.current;
            if (es == null) return;

            Vector2 pos = (Input.touchCount > 0)
                ? Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            var pointer = new PointerEventData(es) { position = pos };
            var results = new List<RaycastResult>();
            es.RaycastAll(pointer, results);

            foreach (var r in results)
            {
                var go = r.gameObject;
                if (backButton != null &&
                    (go == backButton.gameObject || go.transform.IsChildOf(backButton.transform)))
                {
                    ScreenManager.Instance?.Back();
                    return;
                }
            }
        }
    }
}
