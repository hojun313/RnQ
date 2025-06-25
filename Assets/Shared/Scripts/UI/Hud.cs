using System;
using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using HyperCasual.Runner;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyperCasual.Gameplay; // QuestionData ���ӽ����̽� �߰�

namespace HyperCasual.Gameplay
{
    /// <summary>
    /// This View contains head-up-display functionalities
    /// </summary>
    public class Hud : View
    {
        [SerializeField]
        TextMeshProUGUI m_GoldText;
        [SerializeField]
        Slider m_XpSlider;
        [SerializeField]
        HyperCasualButton m_PauseButton;
        [SerializeField]
        AbstractGameEvent m_PauseEvent;

        [SerializeField]
        TextMeshProUGUI m_HealthText;// �÷��̾� ü�� ǥ��
        [SerializeField]
        GameObject m_QuestionPanel;  // ���� ǥ�� �г�
        [SerializeField]
        TextMeshProUGUI m_QuestionText; // ���� �ؽ�Ʈ

        // �ǵ�� UI ��� �߰�
        [SerializeField]
        GameObject m_FeedbackPanel; // �ǵ�� �г�
        [SerializeField]
        TextMeshProUGUI m_FeedbackText; // �ǵ�� �ؽ�Ʈ

        Animator m_FeedbackAnimator;


        /// <summary>
        /// The slider that displays the XP value 
        /// </summary>
        public Slider XpSlider => m_XpSlider;

        int m_GoldValue;

        /// <summary>
        /// The amount of gold to display on the hud.
        /// The setter method also sets the hud text.
        /// </summary>
        public int GoldValue
        {
            get => m_GoldValue;
            set
            {
                if (m_GoldValue != value)
                {
                    m_GoldValue = value;
                    m_GoldText.text = GoldValue.ToString();
                }
            }
        }

        float m_XpValue;

        /// <summary>
        /// The amount of XP to display on the hud.
        /// The setter method also sets the hud slider value.
        /// </summary>
        public float XpValue
        {
            get => m_XpValue;
            set
            {
                if (!Mathf.Approximately(m_XpValue, value))
                {
                    m_XpValue = value;
                    m_XpSlider.value = m_XpValue;
                }
            }
        }

        void OnEnable()
        {
            m_PauseButton.AddListener(OnPauseButtonClick);

            // GameManager�� ü�� ������Ʈ �̺�Ʈ ����
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += UpdateHealthDisplay; // �̺�Ʈ ����
                UpdateHealthDisplay(); // �ʱ� HUD ������Ʈ
            }

            // Animator ������Ʈ ��������
            if (m_FeedbackPanel != null)
            {
                m_FeedbackAnimator = m_FeedbackPanel.GetComponent<Animator>();
            }
        }

        void OnDisable()
        {
            m_PauseButton.RemoveListener(OnPauseButtonClick);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= UpdateHealthDisplay; // �̺�Ʈ ���� ����
            }
        }

        void OnPauseButtonClick()
        {
            m_PauseEvent.Raise();
        }

        /// <summary>
        /// ü���� ������Ʈ�Ͽ� HUD�� ǥ���մϴ�.
        /// </summary>
        public void UpdateHealthDisplay()
        {
            if (GameManager.Instance != null)
            {
                m_HealthText.text = $"Lives:  {GameManager.Instance.lives}";
            }
        }

        /// <summary>
        /// ������ HUD�� ǥ���մϴ�.
        /// </summary>
        /// <param name="question">ǥ���� ���� ������</param>
        public void DisplayQuestion(QuestionData question)
        {
            m_QuestionPanel.SetActive(true);
            m_QuestionText.text = question.QuestionText;
        }

        /// <summary>
        /// ������ ����ϴ�.
        /// </summary>
        public void HideQuestion()
        {
            m_QuestionPanel.SetActive(false);


        }

        /// <summary>
        /// ����/���� �ǵ���� ǥ���մϴ�.
        /// </summary>
        /// <param name="isCorrect">���� ����</param>
        public void ShowFeedback(bool isCorrect)
        {
            if (m_FeedbackPanel != null && m_FeedbackText != null && m_FeedbackAnimator != null)
            {
                m_FeedbackText.text = isCorrect ? "����!" : "����!";
                m_FeedbackPanel.SetActive(true);
                m_FeedbackAnimator.SetTrigger("Show");

                // �ǵ���� ��� �� �ִϸ��̼��� ���� �����
                StartCoroutine(HideFeedbackAfterDelay(2.0f));
            }
        }

        IEnumerator HideFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (m_FeedbackAnimator != null)
            {
                m_FeedbackAnimator.SetTrigger("Hide");
            }
        }
    }
}
