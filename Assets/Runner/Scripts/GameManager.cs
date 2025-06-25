using System;
using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using UnityEngine;
using HyperCasual.Gameplay; // QuestionData ���ӽ����̽� �߰�


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class used to store game state information, 
    /// load levels, and save/load statistics as applicable.
    /// The GameManager class manages all game-related 
    /// state changes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Returns the GameManager.
        /// </summary>
        public static GameManager Instance => s_Instance;
        static GameManager s_Instance;

        [SerializeField]
        AbstractGameEvent m_WinEvent;

        [SerializeField]
        AbstractGameEvent m_LoseEvent;

        [SerializeField]
        private GameObject m_GameOverScreen; // ���� ���� ȭ�鿡 ���� ����

        [SerializeField]
        private AbstractGameEvent m_ContinueEvent;
        [SerializeField]
        private AbstractGameEvent m_BackEvent;

        // �̺�Ʈ ������ �ν��Ͻ�
        IGameEventListener m_ContinueListener;
        IGameEventListener m_BackListener;

        void OnEnable()
        {
            // �� �̺�Ʈ�� ���� ������ ���� �� ���
            m_ContinueListener = new GameEventListener(OnContinue);
            m_BackListener = new GameEventListener(OnBack);

            m_ContinueEvent.AddListener(m_ContinueListener);
            m_BackEvent.AddListener(m_BackListener);
        }

        void OnDisable()
        {
            // ������ ����
            m_ContinueEvent.RemoveListener(m_ContinueListener);
            m_BackEvent.RemoveListener(m_BackListener);
        }

        void OnContinue()
        {
            // ���� ����� ���� ����
            Debug.Log("OnContinue called: Restarting the game.");
            ResetLevel();
            StartGame();

            // ���� ���� ȭ�� �����
            m_GameOverScreen.SetActive(false);
        }

        void OnBack()
        {
            // ���� �޴��� ���ư��� ���� ����
            Debug.Log("OnBack called: Returning to main menu.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public void ApplySkybox(Material skyboxMaterial)
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
                DynamicGI.UpdateEnvironment(); // 글로벌 조명 업데이트
                Debug.Log($"Skybox applied: {skyboxMaterial.name}");
            }
            else
            {
                Debug.LogWarning("Skybox material is null. Skybox not changed.");
            }
        }

        LevelDefinition m_CurrentLevel;

        [SerializeField]
        Hud m_Hud; // HUD ����

        List<QuestionData> m_Questions; // ���� ���� ���� ����Ʈ
        int m_CurrentQuestionIndex = 0; // ���� ���� �ε���

        public QuestionData CurrentQuestion
        {
            get
            {
                if (m_CurrentQuestionIndex < m_Questions.Count)
                {
                    return m_Questions[m_CurrentQuestionIndex];
                }
                return null;
            }
        }



        /// <summary>
        /// Returns true if the game is currently active.
        /// Returns false if the game is paused, has not yet begun,
        /// or has ended.
        /// </summary>
        public bool IsPlaying => m_IsPlaying;
        bool m_IsPlaying;
        GameObject m_CurrentLevelGO;
        GameObject m_CurrentTerrainGO;
        GameObject m_LevelMarkersGO;

        public int lives = 3; //��� ����
        public event System.Action OnLivesChanged; // ü�� ���� �̺�Ʈ

        List<Spawnable> m_ActiveSpawnables = new List<Spawnable>();

#if UNITY_EDITOR
        bool m_LevelEditorMode;
#endif

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;

#if UNITY_EDITOR
            // If LevelManager already exists, user is in the LevelEditorWindow
            if (LevelManager.Instance != null)
            {
                StartGame();
                m_LevelEditorMode = true;
            }
#endif
        }

        /// <summary>
        /// This method calls all methods necessary to load and
        /// instantiate a level from a level definition.
        /// </summary>
        public void LoadLevel(LevelDefinition levelDefinition)
        {
            m_CurrentLevel = levelDefinition;
            LoadLevel(m_CurrentLevel, ref m_CurrentLevelGO);
            CreateTerrain(m_CurrentLevel, ref m_CurrentTerrainGO);
            PlaceLevelMarkers(m_CurrentLevel, ref m_LevelMarkersGO);
            ApplySkybox(levelDefinition.SkyboxMaterial);
            // ���� ���� ���� ����Ʈ ����
            m_Questions = levelDefinition.Questions;
            m_CurrentQuestionIndex = 0;


            StartGame();
        }

        /// <summary>
        /// This method calls all methods necessary to restart a level,
        /// including resetting the player to their starting position
        /// </summary>
        public void ResetLevel()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.ResetPlayer();
            }

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.ResetCamera();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ResetSpawnables();
            }
        }

        /// <summary>
        /// This method loads and instantiates the level defined in levelDefinition,
        /// storing a reference to its parent GameObject in levelGameObject
        /// </summary>
        /// <param name="levelDefinition">
        /// A LevelDefinition ScriptableObject that holds all information needed to 
        /// load and instantiate a level.
        /// </param>
        /// <param name="levelGameObject">
        /// A new GameObject to be created, acting as the parent for the level to be loaded
        /// </param>
        public static void LoadLevel(LevelDefinition levelDefinition, ref GameObject levelGameObject)
        {
            if (levelDefinition == null)
            {
                Debug.LogError("Invalid Level!");
                return;
            }

            if (levelGameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(levelGameObject);
                }
                else
                {
                    DestroyImmediate(levelGameObject);
                }
            }

            levelGameObject = new GameObject("LevelManager");
            LevelManager levelManager = levelGameObject.AddComponent<LevelManager>();
            levelManager.LevelDefinition = levelDefinition;

            Transform levelParent = levelGameObject.transform;
        
            InstantiatePrefab(levelDefinition.HousePrefab, levelParent, "HousePrefab");
            InstantiatePrefab(levelDefinition.CarPrefab, levelParent, "CarPrefab");
            InstantiatePrefab(levelDefinition.LandPrefab, levelParent, "LandPrefab");


            for (int i = 0; i < levelDefinition.Spawnables.Length; i++)
            {
                LevelDefinition.SpawnableObject spawnableObject = levelDefinition.Spawnables[i];

                if (spawnableObject.SpawnablePrefab == null)
                {
                    continue;
                }

                Vector3 position = spawnableObject.Position;
                Vector3 eulerAngles = spawnableObject.EulerAngles;
                Vector3 scale = spawnableObject.Scale;

                GameObject go = null;
                
                if (Application.isPlaying)
                {
                    go = GameObject.Instantiate(spawnableObject.SpawnablePrefab, position, Quaternion.Euler(eulerAngles));
                }
                else
                {
#if UNITY_EDITOR
                    go = (GameObject)PrefabUtility.InstantiatePrefab(spawnableObject.SpawnablePrefab);
                    go.transform.position = position;
                    go.transform.eulerAngles = eulerAngles;
#endif
                }

                if (go == null)
                {
                    return;
                }

                // Set Base Color
                Spawnable spawnable = go.GetComponent<Spawnable>();
                if (spawnable != null)
                {
                    spawnable.SetBaseColor(spawnableObject.BaseColor);
                    spawnable.SetScale(scale);
                    levelManager.AddSpawnable(spawnable);
                }

                if (go != null)
                {
                    go.transform.SetParent(levelParent);
                }
            }
        }

private static void InstantiatePrefab(GameObject prefab, Transform parent, string prefabName)
{
    if (prefab != null)
    {
        GameObject instance = GameObject.Instantiate(prefab);
        instance.name = prefabName;
        instance.transform.SetParent(parent);
    }
}


        public void UnloadCurrentLevel()
        {
            if (m_CurrentLevelGO != null)
            {
                GameObject.Destroy(m_CurrentLevelGO);
            }

            if (m_LevelMarkersGO != null)
            {
                GameObject.Destroy(m_LevelMarkersGO);
            }

            if (m_CurrentTerrainGO != null)
            {
                GameObject.Destroy(m_CurrentTerrainGO);
            }

            m_CurrentLevel = null;
        }

        void StartGame()
        {
            ResetLevel();
            m_IsPlaying = true;
            lives = 3;

            // 질문 인덱스 초기화
            m_CurrentQuestionIndex = 0;
            
            if (m_Questions == null || m_Questions.Count == 0)
            {
                Debug.LogError("Questions list is null or empty!");
            }

            OnLivesChanged?.Invoke(); // �ʱ� ü�� ���� �ݿ�
            ShowQuestion(); // ù ��° ���� ǥ��
        }

        void ShowQuestion()
        {
            if (m_CurrentQuestionIndex < m_Questions.Count)
            {
                m_Hud.DisplayQuestion(CurrentQuestion);
            }
            else
            {
                Win();
            }
        }

        public void HandleAnswer(bool isCorrect)
        {
            if (isCorrect)
            {
                m_CurrentQuestionIndex++;
                ShowQuestion();
            }
            else
            {
                lives--;
                OnLivesChanged?.Invoke();

                if (lives <= 0)
                {
                    Lose();
                    return;
                }
                else
                {
                    m_CurrentQuestionIndex++;
                    ShowQuestion(); // ���� ���� ǥ��
                }
            }
        }

        public void ReduceLife(int amount)
        {
            Debug.Log($"ReduceLife called with amount: {amount}");
            lives -= amount;
            Debug.Log($"Lives after reduction: {lives}");
            OnLivesChanged?.Invoke();

            if (lives <= 0)
            {
                Lose();
            }
        }

        /// <summary>
        /// ���� Ŭ����: �̺�Ʈ ������ ����
        /// </summary>
        private class GameEventListener : IGameEventListener
        {
            private readonly Action callback;

            public GameEventListener(Action callback)
            {
                this.callback = callback;
            }

            public void OnEventRaised()
            {
                callback?.Invoke();
            }
        }



        /// <summary>
        /// Creates and instantiates the StartPrefab and EndPrefab defined inside
        /// the levelDefinition.
        /// </summary>
        /// <param name="levelDefinition">
        /// A LevelDefinition ScriptableObject that defines the start and end prefabs.
        /// </param>
        /// <param name="levelMarkersGameObject">
        /// A new GameObject that is created to be the parent of the start and end prefabs.
        /// </param>
        public static void PlaceLevelMarkers(LevelDefinition levelDefinition, ref GameObject levelMarkersGameObject)
        {
            if (levelMarkersGameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(levelMarkersGameObject);
                }
                else
                {
                    DestroyImmediate(levelMarkersGameObject);
                }
            }

            levelMarkersGameObject = new GameObject("Level Markers");

            GameObject start = levelDefinition.StartPrefab;
            GameObject end = levelDefinition.EndPrefab;

            if (start != null)
            {
                GameObject go = GameObject.Instantiate(start, new Vector3(start.transform.position.x, start.transform.position.y, 0.0f), Quaternion.identity);
                go.transform.SetParent(levelMarkersGameObject.transform);
            }

            if (end != null)
            {
                GameObject go = GameObject.Instantiate(end, new Vector3(end.transform.position.x, end.transform.position.y, levelDefinition.LevelLength), Quaternion.identity);
                go.transform.SetParent(levelMarkersGameObject.transform);
            }
        }

        /// <summary>
        /// Creates and instantiates a Terrain GameObject, built
        /// to the specifications saved in levelDefinition.
        /// </summary>
        /// <param name="levelDefinition">
        /// A LevelDefinition ScriptableObject that defines the terrain size.
        /// </param>
        /// <param name="terrainGameObject">
        /// A new GameObject that is created to hold the terrain.
        /// </param>
        public static void CreateTerrain(LevelDefinition levelDefinition, ref GameObject terrainGameObject)
        {
            TerrainGenerator.TerrainDimensions terrainDimensions = new TerrainGenerator.TerrainDimensions()
            {
                Width = levelDefinition.LevelWidth,
                Length = levelDefinition.LevelLength,
                StartBuffer = levelDefinition.LevelLengthBufferStart,
                EndBuffer = levelDefinition.LevelLengthBufferEnd,
                Thickness = levelDefinition.LevelThickness
            };
            TerrainGenerator.CreateTerrain(terrainDimensions, levelDefinition.TerrainMaterial, ref terrainGameObject);
        }

        public void Win()
        {
            m_WinEvent.Raise();

#if UNITY_EDITOR
            if (m_LevelEditorMode)
            {
                ResetLevel();
            }
#endif
        }

        public void Lose()
        {
            Debug.Log("Lose() �޼��尡 ȣ��Ǿ����ϴ�.");

            m_IsPlaying = false; // ���� ���¸� ����� ���� (�ʿ信 ���� �߰�)

            // ���� ���� ȭ�� ǥ��
            m_GameOverScreen.SetActive(true);


#if UNITY_EDITOR
    if (m_LevelEditorMode)
    {
        ResetLevel();
    }
#endif
        }
    }
}