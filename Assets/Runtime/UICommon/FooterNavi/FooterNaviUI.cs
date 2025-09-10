using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using ScreenCapture = Landscape2.Runtime.ScreenCapture;

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

            // MouseEnter/Leave イベントを登録してUI上でのクリック通り抜けを防ぐ
            RegisterMouseEvents();

            RegisterEvents();
        }

        private void RegisterMouseEvents()
        {
            RegisterMouseEventsForElement(UiRoot);
        }

        private void RegisterMouseEventsForElement(VisualElement element)
        {
            // 要素自体にイベントを登録
            element.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            element.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            // UIStateManagerにグローバル状態を設定
            UIStateManager.IsMouseOverUI = true;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            // UIStateManagerのグローバル状態をクリア
            UIStateManager.IsMouseOverUI = false;
        }

        private void RegisterEvents()
        {
            UiRoot.Q<Button>("Button_Capture").RegisterCallback<ClickEvent>((evt) =>
            {
                ScreenCapture.Instance.OnClickCaptureButton();
            });
        }
    }
}