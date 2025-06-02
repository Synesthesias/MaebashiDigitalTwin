using PLATEAU.CityGML;
using UnityEngine;
using PLATEAU.CityInfo;
using System.Linq;

namespace Landscape2.Maebashi.Runtime
{
    public class SetMeasuredHeightFromMesh : MonoBehaviour
    {
        private string heightAttributeName = "bldg:measuredheight";

        void Start()
        {
            // シーン上の全PLATEAUCityObjectGroupを取得
            var allGroups = FindObjectsOfType<PLATEAUCityObjectGroup>();
            foreach (var group in allGroups)
            {
                // メッシュの高さを取得
                var meshFilter = group.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                var meshCollider = group.GetComponent<MeshCollider>();
                if (meshCollider != null && meshCollider.sharedMesh != null)
                {
                    var bounds = meshCollider.sharedMesh.bounds;
                    Vector3 min = meshCollider.transform.TransformPoint(bounds.min);
                    Vector3 max = meshCollider.transform.TransformPoint(bounds.max);
                    float worldHeight = Mathf.Abs(max.y - min.y);

                    if (worldHeight <= 0)
                    {
                        // 高さが0以下の場合はスキップ
                        Debug.LogWarning($"Height is 0 or less for {group.name}");
                        continue;
                    }

                    foreach (var cityObj in group.GetAllCityObjects())
                    {
                        if (cityObj.CityObjectType != PLATEAU.CityGML.CityObjectType.COT_Building)
                        {
                            // 建物でなければスキップ
                            continue;
                        }

                        if (cityObj.AttributesMap.TryGetValue(heightAttributeName, out var _))
                        {
                            // すでに高さ情報があればスキップ
                            continue;
                        }

                        // CityObjectに高さ属性をセット
                        cityObj.AttributesMap.AddAttribute(heightAttributeName, AttributeType.Double, (double)worldHeight);
                        Debug.Log($"Set {heightAttributeName} for {group.name}: {worldHeight}");
                    }
                }
            }
        }
    }
}