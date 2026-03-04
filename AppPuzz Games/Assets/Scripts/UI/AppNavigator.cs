using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppPuzz.UI
{
    /// <summary>
    /// Singleton MonoBehaviour che gestisce la navigazione tra schermate.
    /// Aggiungere un GameObject con questo componente nella scena principale.
    /// </summary>
    public class AppNavigator : MonoBehaviour
    {
        public static AppNavigator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Torna alla schermata Home ricaricando la scena iniziale.</summary>
        public void ShowHome()
        {
            SceneManager.LoadScene(0);
        }
    }
}
