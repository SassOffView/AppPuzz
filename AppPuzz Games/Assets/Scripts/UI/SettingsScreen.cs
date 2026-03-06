// ============================================================
// SettingsScreen.cs — Impostazioni: lingua + cambio nickname
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AppPuzz.Utils;
using AppPuzz.Localization;

namespace AppPuzz.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [Header("Bottoni")]
        public Button backButton;
        public Button langItalianBtn;
        public Button langEnglishBtn;
        public Button saveNicknameBtn;

        [Header("Nickname")]
        public TMP_InputField nicknameInput;

        [Header("Testi di stato")]
        public TextMeshProUGUI langStatusText;

        private void OnEnable()
        {
            // Forza inizializzazione TMP_InputField (necessaria se creata su GO inattivo)
            if (nicknameInput != null)
            {
                nicknameInput.interactable = true;
                nicknameInput.enabled = true;
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            // Mostra lingua corrente
            if (langStatusText != null)
            {
                bool isIT = LanguageManager.Instance == null ||
                            LanguageManager.Instance.CurrentLanguage == Language.Italian;
                langStatusText.text = isIT ? "Lingua: ITALIANO" : "Language: ENGLISH";
            }
            // Mostra nickname corrente
            if (nicknameInput != null)
                nicknameInput.text = PlayerProfile.Instance?.PlayerName ?? "";
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

                // Attiva campo nickname
                if (nicknameInput != null &&
                    (go == nicknameInput.gameObject || go.transform.IsChildOf(nicknameInput.transform)))
                {
                    nicknameInput.Select();
                    nicknameInput.ActivateInputField();
                    return;
                }

                if (IsUnder(go, langItalianBtn))  { SetLanguage(Language.Italian);  return; }
                if (IsUnder(go, langEnglishBtn))  { SetLanguage(Language.English);  return; }
                if (IsUnder(go, saveNicknameBtn)) { SaveNickname();                 return; }
                if (IsUnder(go, backButton))      { ScreenManager.Instance?.Back(); return; }
            }
        }

        private void SetLanguage(Language lang)
        {
            if (LanguageManager.Instance != null)
                LanguageManager.Instance.SetLanguage(lang);
            RefreshUI();
        }

        private void SaveNickname()
        {
            if (nicknameInput != null && !string.IsNullOrWhiteSpace(nicknameInput.text))
            {
                PlayerProfile.Instance.PlayerName = nicknameInput.text.Trim();
                // Feedback visivo
                if (langStatusText != null)
                {
                    string prev = langStatusText.text;
                    langStatusText.text = "Nickname salvato!";
                    StartCoroutine(ResetStatus(prev, 1.5f));
                }
            }
        }

        private System.Collections.IEnumerator ResetStatus(string prev, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            if (langStatusText != null) langStatusText.text = prev;
        }

        private static bool IsUnder(GameObject go, Component owner)
        {
            if (owner == null || go == null) return false;
            return go == owner.gameObject || go.transform.IsChildOf(owner.transform);
        }
    }
}
