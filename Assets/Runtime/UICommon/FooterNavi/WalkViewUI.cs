using Landscape2.Runtime;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    public class WalkViewUI
    {
        private VisualElement uiRoot;
        private VisualElement walkUI;
        private UxmlHandler uxmlHandler;
        private LandscapeCamera landscapeCamera;
        private GlobalNaviUI globalNaviUI;
        
        private const float FooterHeight = 100;
        
        public WalkViewUI(VisualElement uiRoot, UxmlHandler uxmlHandler, GlobalNaviUI globalNaviUI, LandscapeCamera landscapeCamera)
        {
            this.uxmlHandler = uxmlHandler;
            this.uiRoot = uiRoot;
            this.walkUI = uxmlHandler.GetUxml(SubComponents.SubMenuUxmlType.WalkMode);
            this.landscapeCamera = landscapeCamera;
            this.globalNaviUI = globalNaviUI;
            
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            uiRoot.Q<Toggle>("Toggle_WalkMode").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                uxmlHandler.HideAll();
                if (evt.newValue)
                {
                    uxmlHandler.Show(SubComponents.SubMenuUxmlType.WalkMode);
                    AdjustWalkUI();
                }
            });
            
            // カメラ変更通知
            landscapeCamera.OnSetCameraCalled += () =>
            {
                if (landscapeCamera.cameraState == LandscapeCameraState.SelectWalkPoint)
                {
                    // 歩行位置選択時は非表示
                    uiRoot.Hide();
                }
                else
                {
                    uiRoot.Show();
                }
            };
            
            // 画面の変更通知
            globalNaviUI.onChangeView.AddListener(UpdateWakeUI);
        }
        
        private void SetSelectWalkPointUI()
        {
            landscapeCamera.SetCameraState(LandscapeCameraState.SelectWalkPoint);
        }
        
        private void UpdateWakeUI(SubComponents.SubMenuUxmlType uxmlType)
        {
            if (uxmlType != SubComponents.SubMenuUxmlType.WalkMode &&
                uxmlType != SubComponents.SubMenuUxmlType.CameraEdit &&
                uxmlType != SubComponents.SubMenuUxmlType.CameraList)
            {
                return;
            }

            var register = walkUI.Q<TemplateContainer>("Panel_WalkViewRegister");
            var editor = walkUI.Q<TemplateContainer>("Panel_WalkViewEditor");
            var registerTitle = walkUI.Q<VisualElement>("Title_CameraRegist");
            var title = walkUI.Q<VisualElement>("Title_WalkController");
            var walkControl = walkUI.Q<TemplateContainer>("Panel_WalkController");     
            
            register.style.display = DisplayStyle.None;
            editor.style.display = DisplayStyle.None;
            registerTitle.style.display = DisplayStyle.None;
            title.style.display = DisplayStyle.None;
            
            if (landscapeCamera.cameraState == LandscapeCameraState.SelectWalkPoint)
            {
                walkControl.style.display = DisplayStyle.None;
                return;
            }
            
            walkControl.style.display = DisplayStyle.Flex;
            
            if (uxmlType == SubComponents.SubMenuUxmlType.CameraEdit)
            {
                registerTitle.style.display = DisplayStyle.Flex;
                register.style.display = DisplayStyle.Flex;
                title.style.display = DisplayStyle.Flex;
            }
            else if (uxmlType == SubComponents.SubMenuUxmlType.CameraList)
            {
                title.style.display = DisplayStyle.Flex;
            }
        }

        private void AdjustWalkUI()
        {
            // FooterUIと被らないように調整
            var angleUI = walkUI.Q<VisualElement>("AngleController");
            angleUI.style.bottom = FooterHeight;
        }
    }
}