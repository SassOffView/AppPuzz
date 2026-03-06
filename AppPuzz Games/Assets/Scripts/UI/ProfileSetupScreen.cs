// ============================================================
// ProfileSetupScreen.cs
// Step onboarding: nickname + scelta avatar (10 disponibili).
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Utils;

namespace AppPuzz.UI
{
    /// <summary>
    /// Gestisce la scelta del nickname e dell'avatar durante l'onboarding.
    /// Usa RaycastAll() in Update() per i click (stesso pattern degli altri screen).
    /// </summary>
    public class ProfileSetupScreen : MonoBehaviour
    {
        // ------ Dati statici avatar ------
        public static readonly string[] AVATAR_NAMES = {
            "Fiamma","Ghiaccio","Fulmine","Ombra","Natura",
            "Oceano","Luce","Terra","Cosmo","Arcano"
        };
        public static readonly Color[] AVATAR_COLORS = {
            new Color(1.00f, 0.35f, 0.10f),  // Fiamma   - arancio-rosso
            new Color(0.40f, 0.80f, 1.00f),  // Ghiaccio - azzurro
            new Color(1.00f, 0.90f, 0.10f),  // Fulmine  - giallo
            new Color(0.45f, 0.15f, 0.70f),  // Ombra    - viola scuro
            new Color(0.20f, 0.80f, 0.30f),  // Natura   - verde
            new Color(0.10f, 0.40f, 0.90f),  // Oceano   - blu
            new Color(1.00f, 0.95f, 0.70f),  // Luce     - oro chiaro
            new Color(0.60f, 0.40f, 0.20f),  // Terra    - marrone
            new Color(0.25f, 0.10f, 0.60f),  // Cosmo    - viola notte
            new Color(0.90f, 0.20f, 0.80f),  // Arcano   - magenta
        };

        // ------ Inspector ------
        [Header("Nickname")]
        public TMP_InputField nicknameInput;

        [Header("Avatar (10 bottoni)")]
        public Button[]          avatarButtons;
        public Image[]           avatarHighlights; // bordo evidenziato
        public TextMeshProUGUI[] avatarLabels;

        [Header("Conferma")]
        public Button          confirmButton;

        // ------ Stato ------
        private int _selectedAvatar = 0;

        // ------ Lifecycle ------
        private void OnEnable()
        {
            _selectedAvatar = PlayerProfile.Instance != null
                ? PlayerProfile.Instance.SelectedAvatarId : 0;
            HighlightAvatar(_selectedAvatar);
            if (nicknameInput != null)
            {
                nicknameInput.text = PlayerProfile.Instance?.PlayerName == "Evocatore"
                    ? "" : (PlayerProfile.Instance?.PlayerName ?? "");
            }
        }

        private void Update()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                clicked = true;
            if (!clicked) return;

            var es = EventSystem.current;
            if (es == null) return;
            Vector2 pos = (Input.touchCount > 0)
                ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            var ptr = new PointerEventData(es) { position = pos };
            var hits = new List<RaycastResult>();
            es.RaycastAll(ptr, hits);

            foreach (var r in hits)
            {
                var go = r.gameObject;
                // Avatar buttons
                for (int i = 0; i < (avatarButtons?.Length ?? 0); i++)
                {
                    if (avatarButtons[i] != null &&
                        (go == avatarButtons[i].gameObject || go.transform.IsChildOf(avatarButtons[i].transform)))
                    {
                        SelectAvatar(i);
                        return;
                    }
                }
                // Confirm
                if (confirmButton != null &&
                    (go == confirmButton.gameObject || go.transform.IsChildOf(confirmButton.transform)))
                {
                    ConfirmProfile();
                    return;
                }
            }
        }

        // ------ Logica ------
        private void SelectAvatar(int index)
        {
            _selectedAvatar = index;
            HighlightAvatar(index);
        }

        private void HighlightAvatar(int index)
        {
            if (avatarHighlights == null) return;
            for (int i = 0; i < avatarHighlights.Length; i++)
            {
                if (avatarHighlights[i] == null) continue;
                avatarHighlights[i].enabled = (i == index);
            }
        }

        private void ConfirmProfile()
        {
            if (PlayerProfile.Instance != null)
            {
                string name = nicknameInput != null && !string.IsNullOrWhiteSpace(nicknameInput.text)
                    ? nicknameInput.text.Trim() : "Evocatore";
                PlayerProfile.Instance.PlayerName = name;
                PlayerProfile.Instance.SelectedAvatarId = _selectedAvatar;
            }
            // Notifica OnboardingManager per avanzare allo step successivo
            GetComponentInParent<OnboardingManager>()?.ProfileConfirmed();
        }
    }
}
