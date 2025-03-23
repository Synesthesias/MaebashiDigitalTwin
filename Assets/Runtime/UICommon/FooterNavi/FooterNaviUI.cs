using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    public class FooterNaviUI
    {
        public VisualElement UiRoot { get; }
        private UxmlHandler uxmlHandler;
        public UnityEvent<float> OnTimeChanged = new();
        
        public FooterNaviUI(UxmlHandler uxmlHandler, GlobalNaviUI globalNaviUI, LandscapeCamera landscapeCamera)
        {
            this.uxmlHandler = uxmlHandler;
            
            // UI生成
            UiRoot = new UIDocumentFactory().CreateWithUxmlName("FooterNavi");
            GameObject.Find("FooterNavi").GetComponent<UIDocument>().sortingOrder = 1;

            var walkViewUI = new WalkViewUI(UiRoot, uxmlHandler, globalNaviUI, landscapeCamera);

            // 時間スライダーのUI要素を追加
            var timeSliderUI = new TimeSliderUI(UiRoot);
            timeSliderUI.OnTimeChanged.AddListener(OnTimeChanged.Invoke);
        }
    }
}