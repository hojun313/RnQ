using System;
using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using HyperCasual.Runner;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyperCasual.Gameplay; // QuestionData 네임스페이스 추가

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
        TextMeshProUGUI m_HealthText;// 플레이어 체력 표시
        [SerializeField]
        GameObject m_QuestionPanel;  // 문제 표시 패널
        [SerializeField]
        TextMeshProUGUI m_QuestionText; // 문제 텍스트

        // 피드백 UI 요소 추가
        [SerializeField]
        GameObject m_FeedbackPanel; // 피드백 패널
        [SerializeField]
        TextMeshProUGUI m_FeedbackText; // 피드백 텍스트

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

            // GameManager의 체력 업데이트 이벤트 연결
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += UpdateHealthDisplay; // 이벤트 구독
                UpdateHealthDisplay(); // 초기 HUD 업데이트
            }

            // Animator 컴포넌트 가져오기
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
                GameManager.Instance.OnLivesChanged -= UpdateHealthDisplay; // 이벤트 구독 해제
            }
        }

        void OnPauseButtonClick()
        {
            m_PauseEvent.Raise();
        }

        /// <summary>
        /// 체력을 업데이트하여 HUD에 표시합니다.
        /// </summary>
        public void UpdateHealthDisplay()
        {
            if (GameManager.Instance != null)
            {
                m_HealthText.text = $"Lives:  {GameManager.Instance.lives}";
            }
        }

        /// <summary>
        /// 문제를 HUD에 표시합니다.
        /// </summary>
        /// <param name="question">표시할 문제 데이터</param>
        public void DisplayQuestion(QuestionData question)
        {
            m_QuestionPanel.SetActive(true);
            m_QuestionText.text = question.QuestionText;
        }

        /// <summary>
        /// 문제를 숨깁니다.
        /// </summary>
        public void HideQuestion()
        {
            m_QuestionPanel.SetActive(false);


        }

        /// <summary>
        /// 정답/오답 피드백을 표시합니다.
        /// </summary>
        /// <param name="isCorrect">정답 여부</param>
        public void ShowFeedback(bool isCorrect)
        {
            if (m_FeedbackPanel != null && m_FeedbackText != null && m_FeedbackAnimator != null)
            {
                m_FeedbackText.text = isCorrect ? "정답!" : "오답!";
                m_FeedbackPanel.SetActive(true);
                m_FeedbackAnimator.SetTrigger("Show");

                // 피드백을 잠시 후 애니메이션을 통해 숨기기
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
