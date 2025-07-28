using UnityEngine;
using Landscape2.Runtime;

namespace Landscape2.Maebashi.Runtime.UICommon.Components
{
    public class DesignCompDisplay : ISubComponent
    {
        private const string AREA_ASSETS_NAME = "AreaAssets";
        private const string ROAD_OBJECT_NAME = "20250523_Road";
        private const string BRIDGE_OBJECT_NAME = "54394065_brid_6697.gml";
        private const string DEM_50_OBJECT_NAME = "543940_dem_6697_50_op.gml";
        private const string DEM_55_OBJECT_NAME = "543940_dem_6697_55_op.gml";
        private const string IVY_OBJECT_NAME = "ivy";
        private const string BOXWOOD_OBJECT_NAME = "boxwood";
        private const string IKEGAKI_OBJECT_NAME = "ikegaki";
        
        private KeyCode toggleKey = KeyCode.I;
        private string prefabPath = "DesignComp/250703_maebashi_3D";
        
        private GameObject designCompInstance;
        private GameObject areaAssetsObject;
        private GameObject roadObject;
        private GameObject bridgeObject;
        private GameObject[] dem50Objects;
        private GameObject[] dem55Objects;
        private GameObject[] ivyObjects;
        private GameObject[] boxwoodObjects;
        private GameObject[] ikegakiObjects;
        private bool isActive = false;
        
        private System.Collections.Generic.Dictionary<string, GameObject[]> cachedObjects = new();
        
        public void Start()
        {
            // プレハブを事前にロード
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                designCompInstance = Object.Instantiate(prefab);
                designCompInstance.SetActive(false); // デフォルトはOFF
                Debug.Log("DesignComp loaded successfully");
            }
            else
            {
                Debug.LogError($"Failed to load DesignComp prefab at path: {prefabPath}");
            }
        }

        public void Update(float deltaTime)
        {
            // Ctrl + I キーでDesignCompの表示/非表示を切り替える
            if (Input.GetKeyDown(toggleKey) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                ToggleDesignComp();
            }
        }

        private void ToggleDesignComp()
        {
            if (designCompInstance == null) return;
            
            isActive = !isActive;
            designCompInstance.SetActive(isActive);
            
            // 各オブジェクトを検索
            if (areaAssetsObject == null)
            {
                areaAssetsObject = GameObject.Find(AREA_ASSETS_NAME);
            }
            
            if (roadObject == null)
            {
                roadObject = GameObject.Find(ROAD_OBJECT_NAME);
            }
            
            if (bridgeObject == null)
            {
                bridgeObject = GameObject.Find(BRIDGE_OBJECT_NAME);
            }
            
            if (dem50Objects == null || dem50Objects.Length == 0)
            {
                dem50Objects = FindGameObjectsByName(DEM_50_OBJECT_NAME);
            }
            
            if (dem55Objects == null || dem55Objects.Length == 0)
            {
                dem55Objects = FindGameObjectsByName(DEM_55_OBJECT_NAME);
            }
            
            if (ivyObjects == null || ivyObjects.Length == 0)
            {
                ivyObjects = FindGameObjectsByName(IVY_OBJECT_NAME);
            }
            
            if (boxwoodObjects == null || boxwoodObjects.Length == 0)
            {
                boxwoodObjects = FindGameObjectsByName(BOXWOOD_OBJECT_NAME);
            }
            
            if (ikegakiObjects == null || ikegakiObjects.Length == 0)
            {
                ikegakiObjects = FindGameObjectsByName(IKEGAKI_OBJECT_NAME);
            }
            
            // DesignCompがONの時は各オブジェクトをOFFにする
            if (areaAssetsObject != null)
            {
                areaAssetsObject.SetActive(!isActive);
            }
            
            if (roadObject != null)
            {
                roadObject.SetActive(!isActive);
            }
            
            if (bridgeObject != null)
            {
                bridgeObject.SetActive(!isActive);
            }
            
            if (dem50Objects != null)
            {
                foreach (var obj in dem50Objects)
                {
                    if (obj != null) obj.SetActive(!isActive);
                }
            }
            
            if (dem55Objects != null)
            {
                foreach (var obj in dem55Objects)
                {
                    if (obj != null) obj.SetActive(!isActive);
                }
            }
            
            if (ivyObjects != null)
            {
                foreach (var obj in ivyObjects)
                {
                    if (obj != null) obj.SetActive(!isActive);
                }
            }
            
            if (boxwoodObjects != null)
            {
                foreach (var obj in boxwoodObjects)
                {
                    if (obj != null) obj.SetActive(!isActive);
                }
            }
            
            if (ikegakiObjects != null)
            {
                foreach (var obj in ikegakiObjects)
                {
                    if (obj != null) obj.SetActive(!isActive);
                }
            }
        }
        
        private GameObject[] FindGameObjectsByName(string name)
        {
            if (cachedObjects.TryGetValue(name, out var cachedResult))
            {
                return cachedResult;
            }
            
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            var matchingObjects = new System.Collections.Generic.List<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name == name)
                {
                    matchingObjects.Add(obj);
                }
            }
            
            var result = matchingObjects.ToArray();
            cachedObjects[name] = result;
            return result;
        }

        public void OnEnable() { }
        public void LateUpdate(float deltaTime) { }
        public void OnDisable() 
        {
            if (designCompInstance != null)
            {
                Object.DestroyImmediate(designCompInstance);
                designCompInstance = null;
            }
        }
    }
}