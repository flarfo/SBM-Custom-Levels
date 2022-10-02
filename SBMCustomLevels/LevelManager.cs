using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SceneSystem = SBM.Shared.SceneSystem;
using Systems = SBM.Shared.Systems;

namespace SBM_CustomLevels
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;

        public string PreviousSceneName { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log($"{GetType().Name} already exists, destroying object!");
                Destroy(this);
            }
        }

        public void LoadLevelScene() //add input
        {
            //create a base scene in assetbundle
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("scene-bundle");

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
            
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("base level", loadSceneParameters);

            asyncOperation.completed += delegate (AsyncOperation o)
            {
                SceneSystem.SetActiveScene("base level");

                AssetBundle loadedBundle2 = LevelLoader_Mod.GetAssetBundleFromResources("sbm-bundle");

                RenderSettings.skybox = loadedBundle2.LoadAsset<Material>("Skybox_World4.mat");
                RenderSettings.skybox.shader = Shader.Find("Skybox/Horizon With Sun Skybox");

                loadedBundle2.Unload(false);

                Scene sceneByName = SceneManager.GetSceneByName("base level");

                foreach (GameObject gameObject in SceneManager.GetSceneByName("Systems").GetRootGameObjects())
                {
                    if (!Systems.GameObjectIsSystem(gameObject))
                    {
                        SceneManager.MoveGameObjectToScene(gameObject, sceneByName);
                    }
                }
            };

            loadedBundle.Unload(false);
        }



        public void LoadLevel(bool isEditor, bool newLevel, string path)
        {
            LoadLevelScene();

            if (isEditor)
            {
                if (newLevel)
                {
                    EditorManager.LoadNewEditorLevel();
                }
                else
                {
                    EditorManager.LoadEditorLevel(path);
                }

                EditorManager.instance.InitializeEditor();
            }
            else
            {
                LoadJSONLevel(path);
            }

            Instantiate(Resources.Load("prefabs/level/LevelPrefab_Story") as GameObject); //must happen AFTER carrot is loaded in, otherwise some stuff is goofed^

            if (isEditor)
            {
                //add camera controller, (camera loaed in LevelPrefab_Story), moved from InitializeEditor as camera didnt exist previously...)
                Camera.main.gameObject.AddComponent<CameraController>();

                EditorManager.instance.editorCamera = Camera.main;
            }
        }

        public void LoadJSONLevel(string path)
        {
            Debug.Log(path);

            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }


            if (!File.Exists(path))
            {
                return;
            }

            string[] jsonLines = File.ReadAllLines(path);

            JsonObject jsonObject;

            //ERROR: missing playerspawns!

            Vector3 spawnPos_1 = (JsonUtility.FromJson(jsonLines[0], typeof(FloatObject)) as FloatObject).GetPosition();
            Vector3 spawnPos_2 = (JsonUtility.FromJson(jsonLines[1], typeof(FloatObject)) as FloatObject).GetPosition();

            for (int i = 2; i < jsonLines.Length; i++)
            {
                jsonObject = JsonUtility.FromJson(jsonLines[i], typeof(JsonObject)) as JsonObject;

                GameObject loadedObject = GameObject.Instantiate(Resources.Load(jsonObject.objectName) as GameObject, jsonObject.GetPosition(), Quaternion.Euler(jsonObject.GetRotation()));

                loadedObject.transform.localScale = jsonObject.GetScale();
            }

            //set positions (first 2 lines of json file?)
            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;
        }

        public void BeginLoadLevel(bool isEditor, bool newLevel, string path)
        {
            PreviousSceneName = SceneManager.GetActiveScene().name;

            if (PreviousSceneName == "Systems")
            {
                PreviousSceneName = "";
            }

            SceneSystem.Unload(PreviousSceneName).completed += delegate(AsyncOperation o)
            {
                LoadLevel(isEditor, newLevel, path);
            };
        }
    }
}
