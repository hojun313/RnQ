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

        // UI 관련 추가 부분
        [Header("UI Panels")]
        public GameObject[] carbonLevelPanels; // Carbon 레벨 UI 패널 배열
        public GameObject[] resourceLevelPanels; // Resource 레벨 UI 패널 배열

        /// <summary>
        /// 버튼 클릭 시 호출하여 특정 레벨 패널을 표시합니다.
        /// </summary>
        /// <param name="levelIndex">표시할 레벨 인덱스 (0부터 시작)</param>
        /// <param name="category">카테고리 이름 ("Carbon" 또는 "Resource")</param>
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
        /// 모든 UI 패널을 비활성화합니다.
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
