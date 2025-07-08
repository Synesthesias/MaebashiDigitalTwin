using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;
        [SerializeField] private ThirdPersonController thirdPersonController;
        public ThirdPersonController ThirdPersonController => thirdPersonController;
        
        [Header("Camera Settings")]
        [SerializeField] private float followOffset = 4f;
        [SerializeField] private float followHeight = 2f;
        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private float nearClipPlane = 0.3f;
        [SerializeField] private float farClipPlane = 1000f;
        [SerializeField] private int cameraPriority = 9;
        
        [Header("Camera Damping")]
        [SerializeField] private Vector3 damping = new Vector3(1f, 1f, 1f);
        
        [Header("Camera Rotation Control")]
        [SerializeField] private bool enableManualRotation = true;
        [SerializeField] private float rotateSpeed = 1f;
        
        private bool isRightClicking = false;
        
        private CinemachineTransposer transposer;
        private CinemachineInputProvider inputProvider;
        
        public void Initialize()
        {
            InitializeCameraController();
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
                MoveCameraTarget();
            }
        }

        private void InitializeCameraController()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            }
            
            if (thirdPersonController == null)
            {
                thirdPersonController = GetComponentInChildren<ThirdPersonController>();
            }
            
            if (virtualCamera != null)
            {
                SetupCameraProperties();
                SetupCameraComponents();
                SetupInputProvider();
            }
        }
        
        private void SetupCameraProperties()
        {
            virtualCamera.m_Lens.FieldOfView = fieldOfView;
            virtualCamera.m_Lens.NearClipPlane = nearClipPlane;
            virtualCamera.m_Lens.FarClipPlane = farClipPlane;
            virtualCamera.Priority = cameraPriority;
            virtualCamera.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
        }
        
        private void SetupCameraComponents()
        {
            // Add CinemachineTransposer if it doesn't exist
            if (virtualCamera.GetCinemachineComponent<CinemachineTransposer>() == null)
            {
                virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            }
            transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = new Vector3(0, followHeight, -followOffset);
                transposer.m_XDamping = damping.x;
                transposer.m_YDamping = damping.y;
                transposer.m_ZDamping = damping.z;
            }
            
            // Ensure no LookAt target to prevent rotation following
        }
        
        private void SetupInputProvider()
        {
            inputProvider = virtualCamera.gameObject.GetComponent<CinemachineInputProvider>();
            if (inputProvider == null)
            {
                inputProvider = virtualCamera.gameObject.AddComponent<CinemachineInputProvider>();
            }
            
            // var cameraMoveSpeedData = Resources.Load<Landscape2.Runtime.CameraMoveData>("CameraMoveSpeedData_Slow");
            // if (cameraMoveSpeedData != null)
            // {
            //     SetupInputActions(cameraMoveSpeedData.walkerCameraRotateSpeed);
            // }
        }
        
        private void SetupInputActions(float rotateSpeed)
        {
            var ia = new DefaultInputActions();
            
            float val = rotateSpeed;
            string overrideProcessor = $"ClampVector2Processor(minX={-val}, minY={-val}, maxX={val}, maxY={val})";
            
            ia.Player.Look.ApplyBindingOverride(
                new InputBinding
                {
                    overrideProcessors = overrideProcessor
                });
            
            inputProvider.XYAxis = InputActionReference.Create(ia.Player.Look);
            
            virtualCamera.gameObject.SetActive(false);
            virtualCamera.gameObject.SetActive(true);
        }
        
        public void SetLookAtTarget(Transform target)
        {
            if (virtualCamera != null)
            {
                virtualCamera.LookAt = target;
            }
        }
        
        public CinemachineVirtualCamera GetVirtualCamera()
        {
            return virtualCamera;
        }
        
        public ThirdPersonController GetThirdPersonController()
        {
            return thirdPersonController;
        }
        
        private void HandleCameraRotationInput()
        {
            // Right mouse button detection
            if (Input.GetMouseButtonDown(1))
            {
                isRightClicking = true;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isRightClicking = false;
            }
            
            // Get mouse delta
            if (isRightClicking)
            {
                var deltaMouseXY = Mouse.current.delta.ReadValue();
                RotateCamera(rotateSpeed * deltaMouseXY);
            }
        }

        private void RotateCamera(Vector2 rotationDelta)
        {
            if (isRightClicking == false)
                return;

            var newAngles = virtualCamera.transform.eulerAngles;
            newAngles.x -= rotationDelta.y;
            newAngles.y += rotationDelta.x;
            newAngles.z = 0f;
            virtualCamera.transform.eulerAngles = newAngles;
        }
        
        private void MoveCameraTarget()
        {
            if (thirdPersonController.CinemachineCameraTarget != null)
            {
                // カメラの向いている方向を取得
                Vector3 cameraTargetPosition =  virtualCamera.transform.position + 
                                                virtualCamera.transform.forward * followOffset;
                // ターゲットからプレイヤーの方向を計算
                Vector3 cameraToPlayerDirection = thirdPersonController.transform.position - cameraTargetPosition;
                
                // cimemachineTargetの位置をcameraToPlayerDirection分移動
                var cinemachineTarget = thirdPersonController.CinemachineCameraTarget.transform;
                var cinemachineTargetMovePosition = cinemachineTarget.position + cameraToPlayerDirection;
                cinemachineTargetMovePosition.y += transposer.m_FollowOffset.y;
                
                cinemachineTarget.position = Vector3.Lerp(
                    cinemachineTarget.position,
                    cinemachineTargetMovePosition,
                    Time.deltaTime * 10f
                );
            }
        }
        
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
        public void SetManualRotationEnabled(bool enabled)
        {
            enableManualRotation = enabled;
        }
        
        public void SetRotateSpeed(float speed)
        {
            rotateSpeed = speed;
        }
    }
}