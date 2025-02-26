using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime
{
    /// <summary>
    /// Main処理
    /// </summary>
    public class SubComponents : MonoBehaviour
    {
        private List<ISubComponent> subComponents = new();
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("GlobalNavi");
            var cameraManager = new CameraManager();
        }
        
        private void Start()
        {
            foreach (var c in subComponents)
            {
                c.Start();
            }
        }

        private void OnEnable()
        {
            foreach (var c in subComponents)
            {
                c.OnEnable();
            }
        }

        private void Update()
        {
            foreach (var c in subComponents)
            {
                c.Update(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            foreach (var c in subComponents)
            {
                c.LateUpdate(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (var c in subComponents)
            {
                c.OnDisable();
            }
        }
    }
}