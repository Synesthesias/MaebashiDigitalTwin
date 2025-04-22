using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Synesthesias.Common
{
    public class PrefabLoader : MonoBehaviour
    {
        [SerializeField]
        private string resourcesPath; // Resources配下のロード対象となるディレクトリパス

        [SerializeField]
        private Transform parentTransform;

        private List<GameObject> loadedPrefabs = new List<GameObject>();

        private void Start()
        {
            LoadAllPrefabs();
        }

        public void LoadAllPrefabs()
        {
            if (string.IsNullOrEmpty(resourcesPath))
            {
                Debug.LogError("Resources path is not set!");
                return;
            }
            
            if (parentTransform == null)
            {
                Debug.LogError("ParentTransform is not set!");
                return;
            }

            // 指定されたパスから全てのプレハブをロード
            GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcesPath);

            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning($"No prefabs found in Resources/{resourcesPath}");
                return;
            }

            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null) continue;
                
                GameObject instance = Instantiate(prefab, parentTransform);
                loadedPrefabs.Add(instance);
            }

            Debug.Log($"Loaded {prefabs.Length} prefabs from Resources/{resourcesPath}");
        }

        public void UnloadAllPrefabs()
        {
            foreach (GameObject obj in loadedPrefabs)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            loadedPrefabs.Clear();
        }
    }
} 