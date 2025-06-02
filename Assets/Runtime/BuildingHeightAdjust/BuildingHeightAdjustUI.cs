using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using PLATEAU.CityInfo;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace Landscape2.Maebashi.Runtime
{
    public class BuildingHeightAdjustUI : ISubComponent
    {
        private VisualElement uiRoot;
        private BuildingHeightAdjust targetBuilding;
        private UxmlHandler uxmlHandler;
        private Camera mainCamera;
        
        public BuildingHeightAdjustUI(UxmlHandler uxmlHandler)
        {
            uiRoot = new UIDocumentFactory().CreateWithUxmlName("BuildingHeightAdjustUI");
            uiRoot.style.display = DisplayStyle.None;
            this.uxmlHandler = uxmlHandler;
            RegisterUIEvents();
            mainCamera = Camera.main;
        }

        private void RegisterUIEvents()
        {
            var increaseBtn = uiRoot.Q<Button>("IncreaseButton");
            var decreaseBtn = uiRoot.Q<Button>("DecreaseButton");
            var heightInput = uiRoot.Q<TextField>("HeightInput");
            var heightLabel = uiRoot.Q<Label>("HeightLabel");

            increaseBtn.clicked += () => AdjustHeight(1f);
            decreaseBtn.clicked += () => AdjustHeight(-1f);
            heightInput.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (targetBuilding != null && float.TryParse(evt.newValue, out float h))
                {
                    targetBuilding.SetHeight(h);
                    UpdateUI();
                }
            });
        }

        private void UpdateUI()
        {
            if (targetBuilding == null) return;
            var height = targetBuilding.GetHeight();
            uiRoot.Q<TextField>("HeightInput").SetValueWithoutNotify(height.ToString("F2"));
            uiRoot.Q<Label>("HeightLabel").text = $"高さ: {height:F2}m";
        }

        private void AdjustHeight(float delta)
        {
            if (targetBuilding == null) return;
            float newHeight = targetBuilding.GetHeight() + delta;
            targetBuilding.SetHeight(newHeight);
            UpdateUI();
        }

        private void ShowUIAtScreenPosition()
        {
            if (targetBuilding == null) return;
            var targetObj = targetBuilding.Target;
            var meshRenderer = targetObj.GetComponent<MeshRenderer>();
            if (meshRenderer != null && uiRoot.panel != null && mainCamera != null)
            {
                var bounds = meshRenderer.bounds;
                var wp = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z); // 上端中央
                var panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(uiRoot.panel, wp, mainCamera);

                uiRoot.style.display = DisplayStyle.Flex;
                float xoffset = -80;
                float yoffset = -100f;
                uiRoot.style.left = panelPos.x - xoffset;
                uiRoot.style.top = panelPos.y - yoffset;
            }
            else
            {
                // fallback: マウス位置
                uiRoot.style.display = DisplayStyle.Flex;
                var panel = uiRoot.panel;
                if (panel != null)
                {
                    var mousePos = Input.mousePosition;
                    var pos = RuntimePanelUtils.ScreenToPanel(panel, mousePos);
                    uiRoot.style.left = pos.x;
                    uiRoot.style.top = pos.y;
                }
            }
        }

        public void Update(float deltaTime)
        {
            // EditBuildingサブメニューが非表示ならUIも閉じる
            if (!uxmlHandler.IsVisible(SubComponents.SubMenuUxmlType.EditBuilding))
            {
                if (uiRoot.style.display == DisplayStyle.Flex)
                    uiRoot.style.display = DisplayStyle.None;
                return;
            }

            // UIが表示されているときはカメラ移動に合わせて位置を調整
            if (uiRoot != null && uiRoot.style.display == DisplayStyle.Flex && targetBuilding != null)
            {
                ShowUIAtScreenPosition();
            }

            // クリック判定
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var cityObjectGroup = hit.collider.GetComponent<PLATEAUCityObjectGroup>();
                    if (cityObjectGroup != null)
                    {
                        if (cityObjectGroup.GetAllCityObjects().Any(obj => obj.CityObjectType == PLATEAU.CityGML.CityObjectType.COT_Building))
                        {
                            // BuildingTRSEditingComponentを付与
                            var trsEditing = BuildingTRSEditingComponent.TryGetOrCreate(cityObjectGroup.gameObject);
                            if (targetBuilding == null || !targetBuilding.IsCurrentTarget(cityObjectGroup.gameObject))
                            {
                                targetBuilding = new BuildingHeightAdjust(cityObjectGroup.gameObject, trsEditing);
                                ShowUIAtScreenPosition();
                                UpdateUI();
                            }
                        }
                    }
                }
            }
        }

        public void LateUpdate(float deltaTime)
        {
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void Start()
        {
        }
    }
}