// ============================================================
// ProfileSetupScreen.cs — onboarding: nickname + avatar
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Utils;

namespace AppPuzz.UI
{
    public class ProfileSetupScreen : MonoBehaviour
    {
        public static readonly string[] AVATAR_NAMES = {
            "Fiamma","Ghiaccio","Fulmine","Ombra","Natura",
            "Oceano","Luce","Terra","Cosmo","Arcano"
        };
        public static readonly Color[] AVATAR_COLORS = {
            new Color(1.00f,0.35f,0.10f), new Color(0.40f,0.80f,1.00f),
            new Color(1.00f,0.90f,0.10f), new Color(0.45f,0.15f,0.70f),
            new Color(0.20f,0.80f,0.30f), new Color(0.10f,0.40f,0.90f),
            new Color(1.00f,0.95f,0.70f), new Color(0.60f,0.40f,0.20f),
            new Color(0.25f,0.10f,0.60f), new Color(0.90f,0.20f,0.80f),
        };

        [Header("Nickname")]
        public TMP_InputField nicknameInput;

        [Header("Avatar")]
        public Button[]          avatarButtons;
        public Image[]           avatarHighlights;
        public TextMeshProUGUI[] avatarLabels;

        [Header("Conferma")]
        public Button confirmButton;

        private int _selectedAvatar = 0;

        private void OnEnable()
        {
            _selectedAvatar = PlayerProfile.Instance?.SelectedAvatarId ?? 0;
            HighlightAvatar(_selectedAvatar);
            if (nicknameInput != null)
                nicknameInput.text = PlayerProfile.Instance?.PlayerName == "Evocatore"
                    ? "" : (PlayerProfile.Instance?.PlayerName ?? "");
        }

        private void Update()
        {
            bool clicked = Input.GetMouseButtonDown(0);
            if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                clicked = true;
            if (!clicked) return;

            var es = EventSystem.current;
            if (es == null) return;
            Vector2 pos = Input.touchCount > 0
                ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            var ptr = new PointerEventData(es) { position = pos };
            var hits = new List<RaycastResult>();
            es.RaycastAll(ptr, hits);

            foreach (var r in hits)
            {
                var go = r.gameObject;

                // Clic sul campo nickname → attiva la tastiera
                if (nicknameInput != null &&
                    (go == nicknameInput.gameObject || go.transform.IsChildOf(nicknameInput.transform)))
                {
                    nicknameInput.Select();
                    nicknameInput.ActivateInputField();
                    return;
                }

                // Avatar buttons
                for (int i = 0; i < (avatarButtons?.Length ?? 0); i++)
                {
                    if (avatarButtons[i] != null &&
                        (go == avatarButtons[i].gameObject || go.transform.IsChildOf(avatarButtons[i].transform)))
                    { SelectAvatar(i); return; }
                }

                // Confirm
                if (confirmButton != null &&
                    (go == confirmButton.gameObject || go.transform.IsChildOf(confirmButton.transform)))
                { ConfirmProfile(); return; }
            }
        }

        private void SelectAvatar(int i) { _selectedAvatar = i; HighlightAvatar(i); }

        private void HighlightAvatar(int index)
        {
            if (avatarHighlights == null) return;
            for (int i = 0; i < avatarHighlights.Length; i++)
                if (avatarHighlights[i] != null)
                    avatarHighlights[i].enabled = (i == index);
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
            GetComponentInParent<OnboardingManager>()?.ProfileConfirmed();
        }
    }
}
