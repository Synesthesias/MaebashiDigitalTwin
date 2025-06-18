using UnityEngine;
using UnityEngine.UI;

namespace Landscape2.Maebashi.Runtime.UICommon.Components
{
    public class BuildFPSDisplay : MonoBehaviour
    {
        [Header("Settings")]
        public bool isShow = true;
        
        [SerializeField]
        private Canvas canvas;
        
        [SerializeField]
        private Text fpsText;

        private float deltaTime = 0.0f;

        private void Start()
        {
            if (isShow)
            {
                return;
            }
            canvas.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (fpsText == null) return;

            // FPS計算
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            // 色分け
            Color fpsColor = Color.white;
            // if (fps < 30) fpsColor = Color.red;
            // else if (fps < 60) fpsColor = Color.yellow;
            // else fpsColor = Color.green;

            fpsText.color = fpsColor;
            fpsText.text = $"FPS: {fps:F0}";
        }
    }
}