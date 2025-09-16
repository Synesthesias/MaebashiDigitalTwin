using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Common
{
    /// <summary>
    /// BoxCollider範囲外のGameObjectを非表示にするコンポーネント
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class VisibilityBoxController : MonoBehaviour
    {
        // 固定の待機時間
        private const float DELAY_AFTER_SCENE_LOAD = 0.5f;
        
        private BoxCollider boxCollider;
        
        [Header("設定")]
        [Tooltip("true: 範囲外を非表示, false: 範囲内を非表示")]
        public bool hideOutside = true;
        
        private void Start()
        {
            boxCollider = GetComponent<BoxCollider>();
            
            // シーンのロードを待って自動実行
            SceneLoadUtility.RegisterSceneLoadCallback(
                SceneLoadUtility.MaebashiScenes.ALL,
                () => Invoke(nameof(Execute), DELAY_AFTER_SCENE_LOAD)
            );
        }
        
        /// <summary>
        /// BoxCollider範囲に基づいてGameObjectの表示/非表示を制御
        /// </summary>
        public void Execute()
        {
            if (boxCollider == null)
            {
                Debug.LogError("BoxColliderが見つかりません");
                return;
            }
            
            // BoxColliderのワールド座標でのBoundsを計算
            Bounds worldBounds = new Bounds(
                transform.TransformPoint(boxCollider.center),
                Vector3.Scale(boxCollider.size, transform.lossyScale)
            );
            
            // MeshRendererを持つオブジェクトのみを取得して処理（パフォーマンス最適化）
            MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();

            int processedCount = 0;
            int hiddenCount = 0;

            foreach (var meshRenderer in allRenderers)
            {
                // 自分自身はスキップ
                if (meshRenderer.gameObject == gameObject) continue;

                // MeshRendererのBoundsと交差判定（一部でも重なっていればtrue）
                bool isInside = worldBounds.Intersects(meshRenderer.bounds);
                bool shouldBeActive = hideOutside ? isInside : !isInside;

                if (meshRenderer.gameObject.activeSelf != shouldBeActive)
                {
                    meshRenderer.gameObject.SetActive(shouldBeActive);
                    if (!shouldBeActive) hiddenCount++;
                }

                processedCount++;
            }
            
            Debug.Log($"VisibilityBoxController: {processedCount}個のオブジェクトを処理, {hiddenCount}個を非表示にしました");
        }
        
        /// <summary>
        /// BoxColliderの範囲をGizmosで表示
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
            }
            
            if (boxCollider != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.color = new Color(0, 1, 0, 1f);
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}