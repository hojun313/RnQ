using System.Collections;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// This class provides functionality to modify quality settings in runtime.
    /// </summary>
    public class QualityManager : MonoBehaviour
    {
        private static QualityManager _instance;

        public static QualityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("QualityManager is not initialized. Make sure it exists in the scene.");
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); // Prevent duplicate instances
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist through scenes
        }

        int m_QualityLevel;

        /// <summary>
        /// The current value of quality level.
        /// </summary>
        public int QualityLevel
        {
            get => m_QualityLevel;
            set
            {
                m_QualityLevel = value;
                if (QualitySettings.GetQualityLevel() != m_QualityLevel)
                {
                    QualitySettings.SetQualityLevel(m_QualityLevel, true);
                }
            }
        }

        void OnEnable()
        {
            StartCoroutine(InitializeQualityLevel());
        }

        private IEnumerator InitializeQualityLevel()
        {
            while (SaveManager.Instance == null)
            {
                Debug.LogWarning("Waiting for SaveManager initialization...");
                yield return null; // Wait for the next frame
            }

            if (SaveManager.Instance.IsQualityLevelSaved)
            {
                QualityLevel = SaveManager.Instance.QualityLevel;
                Debug.Log($"Quality Level loaded: {QualityLevel}");
            }
            else
            {
                QualityLevel = 2; // Default quality level
                Debug.Log("Quality Level not saved. Using default: 2");
            }
        }

        void OnDisable()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.QualityLevel = QualityLevel;
                Debug.Log($"Quality Level saved: {QualityLevel}");
            }
            else
            {
                Debug.LogWarning("SaveManager is null. Quality Level could not be saved.");
            }
        }
    }
}
