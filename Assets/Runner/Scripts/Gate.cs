using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperCasual.Core;
using HyperCasual.Gameplay; // QuestionData ���ӽ����̽� �߰�

//using System.Diagnostics;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class representing a Spawnable object.
    /// If a GameObject tagged "Player" collides
    /// with this object, it will trigger a fail
    /// state with the GameManager.
    /// </summary>
    public class Gate : Spawnable
    {
        const string k_PlayerTag = "Player";

        public bool IsTrueGate; // �� ����Ʈ�� ��(True)���� ����(False)���� ǥ��


        [SerializeField]
        GateType m_GateType;
        [SerializeField]
        float m_Value;
        [SerializeField]
        RectTransform m_Text;
        [SerializeField]
        int m_LifePenalty = 1; // Player�� ����� ��� ��


        // ��ƼŬ ȿ�� ������
        [SerializeField]
        ParticleSystem m_CorrectEffectPrefab;
        [SerializeField]
        ParticleSystem m_IncorrectEffectPrefab;

        // ����� �ǵ���� ���� SoundID �ʵ�
        [SerializeField]
        SoundID m_CorrectSoundID;
        [SerializeField]
        SoundID m_IncorrectSoundID;

        // Renderer ������Ʈ ���� (���� �����)
        [SerializeField]
        Renderer m_Renderer; // Renderer ������Ʈ ����
        [SerializeField]
        Color m_CorrectColor = Color.green;
        [SerializeField]
        Color m_IncorrectColor = Color.red;
        [SerializeField]
        float m_ColorChangeDuration = 1.0f;



        bool m_Applied;
        Vector3 m_TextInitialScale;

        enum GateType
        {
            ChangeSpeed,
            ChangeSize,
            ReduceLife
        }

        /// <summary>
        /// Sets the local scale of this spawnable object
        /// and ensures the Text attached to this gate
        /// does not scale.
        /// </summary>
        /// <param name="scale">
        /// The scale to apply to this spawnable object.
        /// </param>
        public override void SetScale(Vector3 scale)
        {
            // Ensure the text does not get scaled
            if (m_Text != null)
            {
                float xFactor = Mathf.Min(scale.y / scale.x, 1.0f);
                float yFactor = Mathf.Min(scale.x / scale.y, 1.0f);
                m_Text.localScale = Vector3.Scale(m_TextInitialScale, new Vector3(xFactor, yFactor, 1.0f));

                m_Transform.localScale = scale;
            }
        }

        /// <summary>
        /// Reset the gate to its initial state. Called when a level
        /// is restarted by the GameManager.
        /// </summary>
        public override void ResetSpawnable()
        {
            m_Applied = false;
        }

        protected override void Awake()
        {
            base.Awake();

            if (m_Text != null)
            {
                m_TextInitialScale = m_Text.localScale;
            }
        }

//        public bool IsCorrectGate; // �� ���� �������� ����


   

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(k_PlayerTag) && !m_Applied)
            {
                m_Applied = true;

                if (m_GateType == GateType.ReduceLife)
                {
                    // ���� ������ ����� ����Ʈ�� �� ��
                    QuestionData currentQuestion = GameManager.Instance.CurrentQuestion;
                    bool isCorrect = (currentQuestion.IsCorrect == IsTrueGate);

                    // ����/���信 ���� �ð��� ȿ�� Ʈ����
                    if (isCorrect)
                    {
                        GameManager.Instance.HandleAnswer(isCorrect);
                        TriggerEffect(true);
                        StartCoroutine(ChangeColorCoroutine(m_CorrectColor));
                    }
                    else
                    {
                        GameManager.Instance.HandleAnswer(isCorrect);
                        TriggerEffect(false);
                        StartCoroutine(ChangeColorCoroutine(m_IncorrectColor));
                    }

                    
                }
                else
                {
                    // �ٸ� ������ ����Ʈ�� ���� ���� ����
                    ActivateGate();
                }
            }
        }

        void ActivateGate()
        {
            switch (m_GateType)
            {
                case GateType.ChangeSpeed:
                    PlayerController.Instance.AdjustSpeed(m_Value);
                break;

                case GateType.ChangeSize:
                    PlayerController.Instance.AdjustScale(m_Value);
                break;

                case GateType.ReduceLife:
                    Debug.Log($"Gate of type ReduceLife activated. Life penalty: {m_LifePenalty}");

                    PlayerController.Instance.ReduceLife(m_LifePenalty);
                break;
            }

            m_Applied = true;
        }


        /// <summary>
        /// ���� ���ο� ���� �ð��� ȿ���� ����� �ǵ���� Ʈ�����մϴ�.
        /// </summary>
        /// <param name="isCorrect">���� ����</param>
        void TriggerEffect(bool isCorrect)
        {
            // ��ƼŬ ȿ�� ����
            ParticleSystem effectPrefab = isCorrect ? m_CorrectEffectPrefab : m_IncorrectEffectPrefab;
            if (effectPrefab != null)
            {
                // ȿ���� ���� ����Ʈ ��ġ�� ����
                ParticleSystem effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                // �ʿ信 ���� ȿ���� �θ�� �����ϰų�, �ڵ����� �ı��ǵ��� ������ �� �ֽ��ϴ�.
                Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
            }

            // ����� ȿ�� ���
            // Ensure the AudioManager instance is not null and play the correct sound effect
            if (AudioManager.Instance != null)
            {
                SoundID soundID = isCorrect ? m_CorrectSoundID : m_IncorrectSoundID;
                if (soundID != SoundID.None)
                {
                    Debug.Log($"Playing sound: {soundID}");
                    AudioManager.Instance.PlayEffect(soundID);
                }
                else
                {
                    Debug.Log("SoundID is None, no sound will be played.");
                }
            }
            else
            {
                Debug.Log("AudioManager.Instance is null, cannot play sound.");
            }
        }

        /// <summary>
        /// ���� ��ȭ�� �ڷ�ƾ���� ó���մϴ�.
        /// </summary>
        /// <param name="targetColor">������ ����</param>
        /// <returns></returns>
        IEnumerator ChangeColorCoroutine(Color targetColor)
        {
            if (m_Renderer != null)
            {
                Color initialColor = m_Renderer.material.color;
                float elapsed = 0f;

                while (elapsed < m_ColorChangeDuration)
                {
                    m_Renderer.material.color = Color.Lerp(initialColor, targetColor, elapsed / m_ColorChangeDuration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                m_Renderer.material.color = targetColor;
            }
        }

    }
}