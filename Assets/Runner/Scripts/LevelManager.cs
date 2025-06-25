using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class used to hold a reference to the current
    /// level and provide access to other classes.
    /// </summary>
    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance => s_Instance;
        static LevelManager s_Instance;

        public LevelDefinition LevelDefinition
        {
            get => m_LevelDefinition;
            set
            {
                m_LevelDefinition = value;

                if (m_LevelDefinition != null && PlayerController.Instance != null)
                {
                    PlayerController.Instance.SetMaxXPosition(m_LevelDefinition.LevelWidth);
                }
            }
        }
        LevelDefinition m_LevelDefinition;

        List<Spawnable> m_ActiveSpawnables = new List<Spawnable>();

        // UI ���� �߰� �κ�
        [Header("UI Panels")]
        public GameObject[] carbonLevelPanels; // Carbon ���� UI �г� �迭
        public GameObject[] resourceLevelPanels; // Resource ���� UI �г� �迭

        /// <summary>
        /// ��ư Ŭ�� �� ȣ���Ͽ� Ư�� ���� �г��� ǥ���մϴ�.
        /// </summary>
        /// <param name="levelIndex">ǥ���� ���� �ε��� (0���� ����)</param>
        /// <param name="category">ī�װ� �̸� ("Carbon" �Ǵ� "Resource")</param>
        public void ShowLevelPanel(int levelIndex, string category)
        {
            DisableAllPanels();

            if (category == "Carbon" && levelIndex >= 0 && levelIndex < carbonLevelPanels.Length)
            {
                carbonLevelPanels[levelIndex].SetActive(true);
            }
            else if (category == "Resource" && levelIndex >= 0 && levelIndex < resourceLevelPanels.Length)
            {
                resourceLevelPanels[levelIndex].SetActive(true);
            }
        }

        /// <summary>
        /// ��� UI �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        void DisableAllPanels()
        {
            foreach (GameObject panel in carbonLevelPanels)
            {
                panel.SetActive(false);
            }

            foreach (GameObject panel in resourceLevelPanels)
            {
                panel.SetActive(false);
            }
        }

        public void AddSpawnable(Spawnable spawnable)
        {
            m_ActiveSpawnables.Add(spawnable);
        }

        public void ResetSpawnables()
        {
            for (int i = 0, c = m_ActiveSpawnables.Count; i < c; i++)
            {
                m_ActiveSpawnables[i].ResetSpawnable();
            }
        }

        void Awake()
        {
            SetupInstance();
        }

        void OnEnable()
        {
            SetupInstance();
        }

        void SetupInstance()
        {
            if (s_Instance != null && s_Instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
                return;
            }

            s_Instance = this;
        }
    }
}
