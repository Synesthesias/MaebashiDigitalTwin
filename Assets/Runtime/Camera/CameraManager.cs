using Cinemachine;
using Landscape2.Runtime;
using Landscape2.Runtime.CameraPositionMemory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Landscape2.Maebashi.Runtime
{
    public class CameraManager
    {
        public LandscapeCamera LandscapeCamera { get; private set; }
        public WalkerMoveByUserInput WalkerMoveByUserInput { get; private set; }
        public CameraPositionMemory CameraPositionMemory { get; private set; }
        public CameraMoveByUserInput CameraMoveByUserInput { get; private set; }
        
        const int CAMERA_FARCLIP_VALUE = 4000;
        
        public CameraManager()
        {
            // MainCameraを取得
            GameObject mainCamera = Camera.main.gameObject;

            // MainCameraにCinemachineBrainがアタッチされていない場合は追加
            if (mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                mainCamera.AddComponent<CinemachineBrain>();
            }

            //俯瞰視点用のカメラの生成と設定
            GameObject mainCam = new GameObject("PointOfViewCamera");
            CinemachineVirtualCamera mainCamVC = mainCam.AddComponent<CinemachineVirtualCamera>();
            mainCamVC.m_Lens.FieldOfView = 60;
            mainCamVC.m_Lens.NearClipPlane = 0.3f;
            mainCamVC.m_Lens.FarClipPlane = CAMERA_FARCLIP_VALUE;

            //歩行者視点用のオブジェクトの生成と設定
            GameObject walker = new GameObject("Walker");
            CharacterController characterController = walker.AddComponent<CharacterController>();
            characterController.slopeLimit = 90;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.05f;

            //歩行者視点用のカメラの生成と設定
            GameObject walkerCam = new GameObject("WalkerCamera");
            CinemachineVirtualCamera walkerCamVC = walkerCam.AddComponent<CinemachineVirtualCamera>();
            walkerCamVC.m_Lens.FieldOfView = 60;
            walkerCamVC.m_Lens.NearClipPlane = 0.3f;

            walkerCamVC.m_Lens.FarClipPlane = CAMERA_FARCLIP_VALUE;
            walkerCamVC.Priority = 9;
            walkerCamVC.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
            walkerCamVC.AddCinemachineComponent<CinemachineTransposer>();
            CinemachineInputProvider walkerCamInput = walkerCam.AddComponent<CinemachineInputProvider>();

            var cameraMoveSpeedData = Resources.Load<CameraMoveData>("CameraMoveSpeedData_Slow");
            
            // 歩行者視点時カメラ回転の移動量補正
            var ia = new DefaultInputActions();
            {
                float val = cameraMoveSpeedData.walkerCameraRotateSpeed;
                string overrideProcessor = $"ClampVector2Processor(minX={-val}, minY={-val}, maxX={val}, maxY={val})";

                ia.Player.Look.ApplyBindingOverride(
                    new InputBinding
                    {
                        overrideProcessors = overrideProcessor
                    });
            }
            walkerCamInput.XYAxis = InputActionReference.Create(ia.Player.Look);

            walkerCam.SetActive(false);
            walkerCam.SetActive(true);
            walkerCamVC.Follow = walker.transform;

            LandscapeCamera = new LandscapeCamera(mainCamVC, walkerCamVC, walker);
            WalkerMoveByUserInput = new WalkerMoveByUserInput(walkerCamVC, walker);
            CameraPositionMemory = new CameraPositionMemory(mainCamVC, walkerCamVC, LandscapeCamera);
            CameraMoveByUserInput = new CameraMoveByUserInput(mainCamVC);
            
            // CameraMoveByUserInputのStart完了イベントを購読
            CameraMoveByUserInput.OnStartCompleted.AddListener(() =>
            {
                if (cameraMoveSpeedData != null)
                {
                    CameraMoveByUserInput.UpdateCameraMoveSpeedData(cameraMoveSpeedData);
                }
                
                // 特定の位置にカメラをセット
                var cameraParent = GameObject.Find("CameraParent");
                if (cameraParent != null)
                {
                    cameraParent.transform.position = new Vector3(-77.6360397f ,206.438171f, -778.114075f);
                    cameraParent.transform.rotation = Quaternion.Euler(47.8f, -2.4f, 0f);
                }
            });
        }
    }
}