// using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace HyperCasual.Runner
{
    /// <summary>
    /// A class used to manage camera movement in a Runner game.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance => s_Instance;
        static CameraManager s_Instance;

        [SerializeField]
        CameraAnglePreset m_CameraAnglePreset = CameraAnglePreset.Behind;

        [SerializeField]
        Vector3 m_Offset;

        [SerializeField]
        Vector3 m_LookAtOffset;

        [SerializeField]
        bool m_LockCameraPosition;

        [SerializeField]
        bool m_SmoothCameraFollow;

        [SerializeField]
        float m_SmoothCameraFollowStrength = 10.0f;

        enum CameraAnglePreset
        {
            Behind,
            Overhead,
            Side,
            FirstPerson,
            Custom,
        }

        Vector3[] m_PresetOffsets = new Vector3[]
        {
            new Vector3(0.0f, 5.0f, -9.0f), // Behind
            new Vector3(0.0f, 9.0f, -5.0f), // Overhead
            new Vector3(5.0f, 5.0f, -8.0f), // Side
            new Vector3(0.0f, 1.0f, 0.0f),  // FirstPerson
            Vector3.zero                    // Custom
        };

        Vector3[] m_PresetLookAtOffsets = new Vector3[]
        {
            new Vector3(0.0f, 2.0f, 6.0f),  // Behind
            new Vector3(0.0f, 0.0f, 4.0f),  // Overhead
            new Vector3(-0.5f, 1.0f, 2.0f), // Side
            new Vector3(0.0f, 1.0f, 2.0f),  // FirstPerson
            Vector3.zero                    // Custom
        };

        bool[] m_PresetLockCameraPosition = new bool[]
        {
            false, // Behind
            false, // Overhead
            true,  // Side
            false, // FirstPerson
            false  // Custom
        };

        Transform m_Transform;

        static readonly Vector3 k_CenteredScale = new Vector3(0.0f, 1.0f, 1.0f);


        private bool IsInPrefabMode()
        {
            #if UNITY_EDITOR
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null;
            #else
            return false;
            #endif
        }

        void Awake()
        {
            SetupInstance();
        }

        void OnEnable()
        {
            if (IsInPrefabMode())
            {
                Debug.LogWarning("CameraManager is running in Prefab Mode. Skipping execution.");
                return;
            }

            SetupInstance();
        }

        void SetupInstance()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            m_Transform = transform;
        }

        /// <summary>
        /// Reset the camera to its starting position relative to the player.
        /// </summary>
        public void ResetCamera()
        {
            SetCameraPositionAndOrientation(false);
        }

        Vector3 GetCameraOffset()
        {
            return m_PresetOffsets[(int)m_CameraAnglePreset] + m_Offset;
        }

        Vector3 GetCameraLookAtOffset()
        {
            return m_PresetLookAtOffsets[(int)m_CameraAnglePreset] + m_LookAtOffset;
        }

        bool GetCameraLockStatus()
        {
            if (m_LockCameraPosition)
            {
                return true;
            }

            return m_PresetLockCameraPosition[(int)m_CameraAnglePreset];
        }

        Vector3 GetPlayerPosition()
        {
            Vector3 playerPosition = Vector3.up;
            if (PlayerController.Instance != null)
            {
                playerPosition = PlayerController.Instance.GetPlayerTop();
            }

            if (GetCameraLockStatus())
            {
                playerPosition = Vector3.Scale(playerPosition, k_CenteredScale);
            }

            return playerPosition;
        }

        void LateUpdate()
        {
            if (IsInPrefabMode() || m_Transform == null)
            {
                return;
            }

            SetCameraPositionAndOrientation(m_SmoothCameraFollow);
        }

        void SetCameraPositionAndOrientation(bool smoothCameraFollow)
        {
            Vector3 playerPosition = GetPlayerPosition();

            Vector3 offset = playerPosition + GetCameraOffset();
            Vector3 lookAtOffset = playerPosition + GetCameraLookAtOffset();

            if (smoothCameraFollow)
            {
                float lerpAmount = Time.deltaTime * m_SmoothCameraFollowStrength;

                m_Transform.position = Vector3.Lerp(m_Transform.position, offset, lerpAmount);
                m_Transform.LookAt(Vector3.Lerp(m_Transform.position + m_Transform.forward, lookAtOffset, lerpAmount));

                m_Transform.position = new Vector3(m_Transform.position.x, m_Transform.position.y, offset.z);
            }
            else
            {
                m_Transform.position = offset;
                m_Transform.LookAt(lookAtOffset);
            }
        }

#if UNITY_EDITOR
        private bool IsInPrefabMode11111111()
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null;
        }
#endif
    }
}
