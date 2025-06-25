using System;
using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using HyperCasual.Runner;
using UnityEngine;


namespace HyperCasual.Gameplay
{
    /// <summary>
    /// This View contains level selection screen functionalities
    /// </summary>
    public class LevelSelectionScreen : View
    {
        [SerializeField]
        public HyperCasualButton m_ResourceRecyclingButton; // �ڿ� ��ȯ ��ư
        [SerializeField]
        public HyperCasualButton m_CarbonNeutralityButton; // ź�� �߸� ��ư
        [SerializeField]
        HyperCasualButton m_QuickPlayButton;
        [SerializeField]
        HyperCasualButton m_BackButton;
        [Space]
        [SerializeField]
        LevelSelectButton m_LevelButtonPrefab;
        [SerializeField]
        RectTransform m_LevelButtonsRoot;
        [SerializeField]
        AbstractGameEvent m_NextLevelEvent;
        [SerializeField]
        AbstractGameEvent m_BackEvent;
        [SerializeField]
        private GameObject m_HudGameObject; // HUD ������Ʈ�� ���� ����

#if UNITY_EDITOR
        [SerializeField]
        bool m_UnlockAllLevels;
#endif

        readonly List<LevelSelectButton> m_Buttons = new();

        void Start()
        {
            var levels = SequenceManager.Instance.Levels;
            for (int i = 0; i < levels.Length; i++)
            {
                m_Buttons.Add(Instantiate(m_LevelButtonPrefab, m_LevelButtonsRoot));
            }

            ResetButtonData();
        }

        void OnEnable()
        {
            m_ResourceRecyclingButton.AddListener(OnResourceRecyclingSelected);
            m_CarbonNeutralityButton.AddListener(OnCarbonNeutralitySelected);
            m_BackButton.AddListener(OnBackButtonClicked);

            // �ʱ� ��ư ������ ����////
            ResetButtonData();
        }

        void OnDisable()
        {
            m_ResourceRecyclingButton.RemoveListener(OnResourceRecyclingSelected);
            m_CarbonNeutralityButton.RemoveListener(OnCarbonNeutralitySelected);
            m_BackButton.RemoveListener(OnBackButtonClicked);
        }

    void ResetButtonData(string theme = "")
    {
        if (SequenceManager.Instance == null)
        {
            Debug.LogError("SequenceManager.Instance is null");
        return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance is null");
            return;
        }

        var levels = SequenceManager.Instance.Levels;
        var levelProgress = SaveManager.Instance.LevelProgress;

        if (m_Buttons == null)
        {
           Debug.LogError("m_Buttons is null");
           return;
        }

        for (int i = 0; i < m_Buttons.Count; i++)
        {
            var button = m_Buttons[i];
            if (button == null)
            {
                Debug.LogWarning($"Button at index {i} is null");
                continue;
            }

            if (i < levels.Length)
            {
                var unlocked = i <= levelProgress;
    #if UNITY_EDITOR
                unlocked = unlocked || m_UnlockAllLevels;
    #endif
                button.SetData(i, unlocked, OnClick, theme);
                button.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }

        void OnClick(int startingIndex)
        {
            if (startingIndex < 0)
                throw new Exception("Button is not initialized");

            SequenceManager.Instance.SetStartingLevel(startingIndex);
            m_NextLevelEvent.Raise();

            // ���� ���� ȭ�� �����
            this.gameObject.SetActive(false);

            // ���� HUD ǥ��
            m_HudGameObject.SetActive(true);
        }
        
        void OnQuickPlayButtonClicked()
        {
            OnClick(SaveManager.Instance.LevelProgress);
        }
        
        void OnBackButtonClicked()
        {
            m_BackEvent.Raise();
        }
        void OnResourceRecyclingSelected()
        {
            SequenceManager.Instance.SetLevelsByTheme("ResourceRecycling");
            ResetButtonData("ResourceRecycling");
        }
        void OnCarbonNeutralitySelected()
        {
            SequenceManager.Instance.SetLevelsByTheme("CarbonNeutrality");
            ResetButtonData("CarbonNeutrality");
        }
    }
}
