using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField]
        private CinemachineVirtualCamera virtualCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;
        
        [SerializeField]
        private ThirdPersonController thirdPersonController;
        public ThirdPersonController ThirdPersonController => thirdPersonController;
        
        [Header("Camera Rotation Control")]
        [SerializeField]
        private float rotateSpeed = 1f;
        
        private bool isRightClicking = false;
        private CinemachineFramingTransposer framingTransposer;
        
        private const float cameraHeightOffset = 0.5f; // カメラの高さオフセット（胸の高さ）

        public void Initialize()
        {
            // thirdPersonControllerが設定されていない場合は自動で取得
            if (thirdPersonController == null)
            {
                thirdPersonController = GetComponent<ThirdPersonController>();
            }
            
            if (virtualCamera != null)
            {
                framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
            else
            {
                Debug.LogWarning("VirtualCamera is not assigned. Please assign it in the Inspector.");
            }
            
            if (thirdPersonController != null)
            {
                var height = thirdPersonController.CharacterHeight;
                
                // カメラの高さをキャラクターの高さに合わせる
                if (framingTransposer != null)
                {
                    framingTransposer.m_TrackedObjectOffset.y = height - cameraHeightOffset;
                }
            }
        }

        private void Update()
        {
            if (thirdPersonController.CurrentViewMode == ThirdPersonController.ViewMode.Overhead)
            {
                return;
            }
            
            if (thirdPersonController != null)
            {
                thirdPersonController.Move();
                HandleCameraRotationInput();
            }
        }

        private void HandleCameraRotationInput()
        {
            if (Input.GetMouseButtonDown(1))
            {
                isRightClicking = true;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isRightClicking = false;
            }

            if (!isRightClicking)
            {
                return;
            }
            var deltaMouseXY = Mouse.current.delta.ReadValue();
            RotateCamera(rotateSpeed * deltaMouseXY);
        }

        private void RotateCamera(Vector2 rotationDelta)
        {
            var newAngles = virtualCamera.transform.eulerAngles;
            newAngles.x -= rotationDelta.y;
            newAngles.y += rotationDelta.x;
            newAngles.z = 0f;
            virtualCamera.transform.eulerAngles = newAngles;
        }
    }
}