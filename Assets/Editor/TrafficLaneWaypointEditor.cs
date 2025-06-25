using UnityEngine;
using UnityEditor;
using AWSIM.TrafficSimulation;

public class TrafficLaneWaypointEditor : EditorWindow
{
    private float yOffset = 0f;
    private bool isAdd = true;
    private float groundOffset = -0.1f; // 地面より少し下にオフセット

    [MenuItem("Tools/TrafficLane/全WaypointsのY座標を一括変更")]
    public static void ShowWindow()
    {
        GetWindow<TrafficLaneWaypointEditor>("Waypoints Y一括変更");
    }

    private void OnGUI()
    {
        GUILayout.Label("WaypointsのY座標を一括変更", EditorStyles.boldLabel);
        yOffset = EditorGUILayout.FloatField("Y軸オフセット値", yOffset);
        isAdd = EditorGUILayout.Toggle("加算 (オフで減算)", isAdd);

        if (GUILayout.Button("選択中オブジェクト配下の全TrafficLaneに適用"))
        {
            ApplyYOffsetToAllTrafficLanes();
        }

        GUILayout.Space(20);
        GUILayout.Label("Ground Raycast機能", EditorStyles.boldLabel);
        groundOffset = EditorGUILayout.FloatField("地面からのオフセット", groundOffset);
        
        if (GUILayout.Button("WaypointsをGroundレイヤーに合わせる"))
        {
            SnapWaypointsToGround();
        }
    }

    private void ApplyYOffsetToAllTrafficLanes()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("エラー", "オブジェクトを選択してください。", "OK");
            return;
        }
        var lanes = selected.GetComponentsInChildren<TrafficLane>(true);
        int count = 0;
        System.Text.StringBuilder log = new System.Text.StringBuilder();
        foreach (var lane in lanes)
        {
            Undo.RecordObject(lane, "Waypoints Y一括変更");
            var waypoints = lane.Waypoints;
            if (waypoints == null) continue;
            // 新しい配列を作成しY値を変更
            Vector3[] newWaypoints = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                var wp = waypoints[i];
                wp.y += isAdd ? yOffset : -yOffset;
                newWaypoints[i] = wp;
            }
            // SerializedObject経由でwaypointsを書き換え
            var so = new SerializedObject(lane);
            var prop = so.FindProperty("waypoints");
            prop.arraySize = newWaypoints.Length;
            for (int i = 0; i < prop.arraySize; i++)
            {
                var vecProp = prop.GetArrayElementAtIndex(i);
                vecProp.vector3Value = newWaypoints[i];
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(lane);
            count++;
            log.AppendLine(ObjectPath(lane.gameObject));
        }
        Debug.Log($"[TrafficLaneWaypointEditor] 変更したTrafficLane一覧:\n" + log.ToString());
        EditorUtility.DisplayDialog("完了", $"{count}個のTrafficLaneのWaypointsを変更しました。\n詳細はConsoleログを参照してください。", "OK");
    }

    // オブジェクトの階層パスを取得
    private string ObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform t = obj.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    private void SnapWaypointsToGround()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("エラー", "オブジェクトを選択してください。", "OK");
            return;
        }

        var lanes = selected.GetComponentsInChildren<TrafficLane>(true);
        int count = 0;
        int waypointCount = 0;
        System.Text.StringBuilder log = new System.Text.StringBuilder();
        LayerMask groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        foreach (var lane in lanes)
        {
            Undo.RecordObject(lane, "Waypoints Ground Snap");
            var waypoints = lane.Waypoints;
            if (waypoints == null) continue;

            Vector3[] newWaypoints = new Vector3[waypoints.Length];
            bool modified = false;

            for (int i = 0; i < waypoints.Length; i++)
            {
                var wp = waypoints[i];
                
                // 上から下にRaycastして地面を検出
                Ray ray = new Ray(new Vector3(wp.x, wp.y + 100f, wp.z), Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayerMask))
                {
                    // 地面の位置 + オフセット
                    wp.y = hit.point.y + groundOffset;
                    modified = true;
                    waypointCount++;
                }
                
                newWaypoints[i] = wp;
            }

            if (modified)
            {
                // SerializedObject経由でwaypointsを書き換え
                var so = new SerializedObject(lane);
                var prop = so.FindProperty("waypoints");
                prop.arraySize = newWaypoints.Length;
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var vecProp = prop.GetArrayElementAtIndex(i);
                    vecProp.vector3Value = newWaypoints[i];
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(lane);
                count++;
                log.AppendLine(ObjectPath(lane.gameObject));
            }
        }

        Debug.Log($"[TrafficLaneWaypointEditor] Groundに合わせて変更したTrafficLane一覧:\n" + log.ToString());
        EditorUtility.DisplayDialog("完了", $"{count}個のTrafficLane、{waypointCount}個のWaypointsをGroundに合わせました。\n詳細はConsoleログを参照してください。", "OK");
    }
} 