using UnityEngine;
using Landscape2.Runtime;

namespace Landscape2.Maebashi.Runtime.UICommon.Components
{
    public class DesignCompDisplay : MonoBehaviour
    {
        [SerializeField, Tooltip("別のシーン用に名前で検索")]
        private string[] disableObjectNames;
        
        [SerializeField]
        private GameObject[] disableGameObjects;
        
        private KeyCode toggleKey = KeyCode.I;
        private readonly string PrefabName = "DesignComp";
        
        private GameObject designCompObject;
        private bool isActive = false;
        
        private void Start()
        {
            var parent = GameObject.Find(PrefabName);
            designCompObject = parent.transform.Find(PrefabName).gameObject;
            if (designCompObject != null)
            {
                designCompObject.SetActive(false); // デフォルトはOFF
            }
            else
            {
                Debug.LogError($"Failed to DesignComp GameObject: {PrefabName}");
            }
        }

        private void Update()
        {
            // Ctrl + I キーでDesignCompの表示/非表示を切り替える
            if (Input.GetKeyDown(toggleKey) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                ToggleDesignComp();
            }
        }

        private void ToggleDesignComp()
        {
            if (designCompObject == null) return;
            
            isActive = !isActive;
            designCompObject.SetActive(isActive);
            
            // disableGameObjects配列に直接設定されたオブジェクトを処理
            if (disableGameObjects != null)
            {
                foreach (var obj in disableGameObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(!isActive);
                    }
                }
            }
            
            // disableObjectNames配列に設定されたオブジェクト名で検索して処理
            if (disableObjectNames != null)
            {
                foreach (var objectName in disableObjectNames)
                {
                    if (string.IsNullOrEmpty(objectName)) continue;
                    
                    // オブジェクトを検索
                    var objects = FindGameObjectsByName(objectName);
                    
                    // オブジェクトの表示/非表示を切り替え
                    foreach (var obj in objects)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(!isActive);
                        }
                    }
                }
            }
        }
        
        private GameObject[] FindGameObjectsByName(string name)
        {
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            var matchingObjects = new System.Collections.Generic.List<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name == name)
                {
                    matchingObjects.Add(obj);
                }
            }
            
            return matchingObjects.ToArray();
        }
    }
}