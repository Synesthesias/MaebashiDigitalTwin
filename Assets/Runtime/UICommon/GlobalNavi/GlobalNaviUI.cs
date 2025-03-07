using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    public class GlobalNaviUI
    {
        public VisualElement UiRoot { get; }
        private UxmlHandler uxmlHandler;
        private VisualElement cameraListUI;
        private VisualElement projectListUI;
        private VisualElement projectSettingUI;
        private LandscapeCamera landscapeCamera;
        
        public UnityEvent<SubComponents.SubMenuUxmlType> onChangeView = new();
        
        public GlobalNaviUI(UxmlHandler uxmlHandler, LandscapeCamera landscapeCamera)
        {
            this.uxmlHandler = uxmlHandler;
            this.landscapeCamera = landscapeCamera;
            
            // UI生成
            UiRoot = new UIDocumentFactory().CreateWithUxmlName("GlobalNavi");
            GameObject.Find("GlobalNavi").GetComponent<UIDocument>().sortingOrder = 1;
            
            // カメラリスト
            cameraListUI = UiRoot.Q<VisualElement>("GlobalNavi_SubMenuContainer");
            
            // プロジェクト管理リスト
            projectListUI = UiRoot.Q<VisualElement>("Project_List");
            
            // プロジェクトのセッティング
            projectSettingUI = UiRoot.Q<VisualElement>("GlobalNavi__SettingPanel");
            
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            // イベント登録
            
            // メインタブ
            UiRoot.Q<RadioButton>("MenuMain").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    return;
                }
                HideAll();
                uxmlHandler.HideAll();
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.Menu);
            });
            
            // 建物編集タブ
            UiRoot.Q<RadioButton>("MenuEdit").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    return;
                }
                HideAll();
                uxmlHandler.Show(SubComponents.SubMenuUxmlType.EditBuilding);
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.EditBuilding);
            });
            
            // アセット配置タブ
            UiRoot.Q<RadioButton>("MenuAsset").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    return;
                }
                HideAll();
                uxmlHandler.Show(SubComponents.SubMenuUxmlType.Asset);
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.Asset);
            });

            // カメラタブ
            UiRoot.Q<RadioButton>("MenuCamera").RegisterCallback<ClickEvent>((evt) =>
            {
                cameraListUI.Show();
            });

            // カメラリスト
            UiRoot.Q<Label>("GlobalNavi-CameraList").RegisterCallback<ClickEvent>((evt) =>
            {
                HideAll();
                if (landscapeCamera.cameraState == LandscapeCameraState.PointOfView)
                {
                    uxmlHandler.Show(SubComponents.SubMenuUxmlType.CameraList);
                }
                else
                {
                    uxmlHandler.Show(SubComponents.SubMenuUxmlType.WalkMode);
                }
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.CameraList);
            });

            // カメラ編集
            UiRoot.Q<Label>("GlobalNavi-CameraEdit").RegisterCallback<ClickEvent>((evt) =>
            {
                HideAll();
                if (landscapeCamera.cameraState == LandscapeCameraState.PointOfView)
                {
                    uxmlHandler.Show(SubComponents.SubMenuUxmlType.CameraEdit);
                }
                else
                {
                    uxmlHandler.Show(SubComponents.SubMenuUxmlType.WalkMode);
                }
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.CameraEdit);
            });
            
            // ダッシュボード
            UiRoot.Q<RadioButton>("MenuDashboard").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    return;
                }
                HideAll();
                uxmlHandler.Show(SubComponents.SubMenuUxmlType.DashBoard);
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.DashBoard);
            });
            
            // GISタブ
            UiRoot.Q<RadioButton>("MenuGis").RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    return;
                }
                HideAll();
                uxmlHandler.Show(SubComponents.SubMenuUxmlType.Gis);
                onChangeView.Invoke(SubComponents.SubMenuUxmlType.Gis);
            });

            // プロジェクトリスト
            UiRoot.Q<Button>("Btn_SelectProject").clicked += () =>
            {
                HideAll();
                projectListUI.ToggleShow();
            };
            
            // 設定
            UiRoot.Q<Button>("Button_Setting").clicked += () => 
            {
                HideAll();
                projectSettingUI.ToggleShow();
            };
        }

        private void HideAll()
        {
            uxmlHandler.HideAll();
            
            // 表示されているリスト等を消す
            cameraListUI.Hide();
            projectListUI.Hide();
            projectSettingUI.Hide();
        }
    }
}