using System;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Landscape2.Runtime;
using Landscape2.Runtime.UiCommon;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime
{
    public class SpeedControlUI
    {
        public UnityEvent<float> OnSpeedChanged = new();

        private VisualElement speedControlPanel;

        private float defaultWalkSpeed;
        private CameraMoveData cameraMoveSpeedData;
        
        public SpeedControlUI(LandscapeCamera landscapeCamera, StarterAssets.ThirdPersonController thirdPersonController, float minSpeed = 2f, float maxSpeed = 20f)
        {
            // Create SpeedControlPanel using UIDocumentFactory
            var speedControlRoot = new UIDocumentFactory().CreateWithUxmlName("SpeedControlPanel");
            GameObject.Find("SpeedControlPanel").GetComponent<UIDocument>().sortingOrder = 1;
            speedControlPanel = speedControlRoot.Q<VisualElement>("SpeedControlPanel");
            
            if (speedControlPanel == null)
            {
                throw new InvalidOperationException("SpeedControlPanel element not found in the UI hierarchy.");
            }

            // Find the speed slider
            var speedSlider = speedControlPanel.Q<Slider>("SpeedSlider");
            if (speedSlider == null)
            {
                throw new InvalidOperationException("Slider with name 'SpeedSlider' not found in the UI hierarchy.");
            }

            // Load CameraMoveSpeedData to get default walk speed
            cameraMoveSpeedData = Resources.Load<CameraMoveData>("CameraMoveSpeedData");
            defaultWalkSpeed = minSpeed + (maxSpeed - minSpeed) / 2;
            
            // Set slider range and default value
            speedSlider.lowValue = minSpeed;
            speedSlider.highValue = maxSpeed;
            speedSlider.value = defaultWalkSpeed; // Use defaultWalkSpeed from CameraMoveSpeedData

            // Register callback for speed changes
            speedSlider.RegisterValueChangedCallback(evt =>
            {
                OnSpeedChanged?.Invoke(evt.newValue);
                // Update both ThirdPersonController and CameraMoveSpeedData
                if (thirdPersonController != null)
                {
                    thirdPersonController.MoveSpeed = evt.newValue;
                }
                if (cameraMoveSpeedData != null)
                {
                    cameraMoveSpeedData.walkerMoveSpeed = evt.newValue;
                }
            });

            // Apply the slider's initial value to the ThirdPersonController and CameraMoveSpeedData
            if (thirdPersonController != null)
            {
                thirdPersonController.MoveSpeed = defaultWalkSpeed;
            }
            if (cameraMoveSpeedData != null)
            {
                cameraMoveSpeedData.walkerMoveSpeed = defaultWalkSpeed;
            }

            // Subscribe to camera state changes to show/hide speed control
            landscapeCamera.OnSetCameraCalled += () =>
            {
                var isWalkMode = landscapeCamera.cameraState == LandscapeCameraState.Walker;
                SetVisible(isWalkMode);
            };
            
        }

        public void SetVisible(bool visible)
        {
            if (speedControlPanel != null)
            {
                speedControlPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void SetSpeed(float speed)
        {
            var speedSlider = speedControlPanel?.Q<Slider>("SpeedSlider");
            if (speedSlider != null)
            {
                speedSlider.value = speed;
            }
        }

        public float GetSpeed()
        {
            var speedSlider = speedControlPanel?.Q<Slider>("SpeedSlider");
            return speedSlider?.value ?? defaultWalkSpeed;
        }
        
        public float GetDefaultWalkSpeed()
        {
            return defaultWalkSpeed;
        }
    }
}