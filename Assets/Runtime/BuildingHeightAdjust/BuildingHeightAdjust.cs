using Landscape2.Runtime;
using PLATEAU.CityInfo;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime
{
    public class BuildingHeightAdjust
    {
        public float MinHeight = 1.0f;
        public float MaxHeight = 1000.0f;
        public float BaseHeight = 1.0f;
        private const float ParentTransformOffset = 0.5f;
        private const float DefaultGroundLevel = 101.5f;

        private float originalY;
        private float originalScaleY;

        private GameObject targetBuilding;
        private BuildingTRSEditingComponent trsEditing;
        public GameObject Target => targetBuilding;
        
        public bool IsCurrentTarget(GameObject target)
        {
            return targetBuilding == target;
        }

        public BuildingHeightAdjust(GameObject target, BuildingTRSEditingComponent trsEditingComponent)
        {
            targetBuilding = target;
            trsEditing = trsEditingComponent;
            if (targetBuilding == null)
            {
                Debug.LogError("Target building is null.");
                return;
            }
            if (targetBuilding.TryGetComponent(out PLATEAUCityObjectGroup cityObject))
            {
                foreach (var cityObj in cityObject.GetAllCityObjects())
                {
                    if (cityObj.AttributesMap.TryGetValue("bldg:measuredheight", out var height))
                    {
                        BaseHeight = (float)height.DoubleValue;
                    }
                }
            }
            originalY = targetBuilding.transform.position.y;
            originalScaleY = targetBuilding.transform.localScale.y;
        }
        
        public void SetHeight(float height)
        {
            height = Mathf.Clamp(height, MinHeight, MaxHeight);
            float scaleY = height / BaseHeight;
            Vector3 scale = targetBuilding.transform.localScale;
            scale.y = scaleY;
            trsEditing.SetScale(scale);

            // メッシュの底辺を地面（Layer 31: Ground）に合わせる
            if (trsEditing.EditingObject == null)
            {
                Debug.LogWarning("EditingObject is null. Skipping mesh adjustment.");
                return;
            }
            
            // Retrieve the MeshFilter component from EditingObject.
            // Assumes that EditingObject is expected to have a MeshFilter component.
            var meshFilter = trsEditing.EditingObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                var renderer = meshFilter.GetComponent<Renderer>();
                var bounds = renderer.bounds;
                Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                float worldMinY = bottomCenter.y;
                float groundY = DefaultGroundLevel;
                
                // Layer 11 (Ground) だけにRaycast
                int layerMask = 1 << 11;
                var origin = bottomCenter + Vector3.up * 10000f;
                var ray = new Ray(origin, Vector3.down);
                // Rayの可視化
                Debug.DrawRay(origin, Vector3.down * 20000f, Color.red, 2f);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, float.PositiveInfinity, layerMask))
                {
                    groundY = hitInfo.point.y;
                }
                float offsetY = groundY - worldMinY;
                offsetY += ParentTransformOffset; // 親のTransformの位置を考慮して少し上げる
                Vector3 pos = targetBuilding.transform.position;
                pos.y += offsetY;
                trsEditing.SetPosition(pos);
            }
        }

        public float GetHeight()
        {
            return BaseHeight * targetBuilding.transform.localScale.y;
        }
    }
}