using Cinemachine;
using Landscape2.Runtime;
using Landscape2.Runtime.CameraPositionMemory;
using StarterAssets;
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
        public ThirdPersonController ThirdPersonController { get; private set; }
        
        const int CAMERA_FARCLIP_VALUE = 20000;
        
        // カメラの移動完了監視用の変数をクラスのフィールドとして追加
        private Vector3 lastCameraPosition = Vector3.zero;
        private Quaternion lastCameraRotation = Quaternion.identity;
        private float stillTime = 0f;
        private float moveCompleteThreshold = 0.5f; // 位置・回転の変化許容値
        private float moveCompleteWait = 0.2f;      // 何秒間止まったら「完了」とみなす

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
            var walkerPrefab = Resources.Load<GameObject>("PlayerComponent");
            var walker = GameObject.Instantiate(walkerPrefab);
            var playerCameraController = walker.GetComponent<PlayerCameraController>();
            playerCameraController.Initialize();

            var cameraMoveSpeedData = Resources.Load<CameraMoveData>("CameraMoveSpeedData_Slow");

            LandscapeCamera = new LandscapeCamera(mainCamVC, playerCameraController.VirtualCamera, playerCameraController.ThirdPersonController.gameObject);
            WalkerMoveByUserInput = new WalkerMoveByUserInput(
                playerCameraController.VirtualCamera,
                playerCameraController.ThirdPersonController.gameObject,
                false, (moveDelta) =>
                {
                    // 一瞬で指定された距離だけ移動
                    playerCameraController.ThirdPersonController.MoveInstantly(-moveDelta);
                });
            CameraPositionMemory = new CameraPositionMemory(
                mainCamVC, 
                playerCameraController.VirtualCamera,
                LandscapeCamera,
                4.0f);
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
                
                // 初期位置と回転を記録
                LandscapeCamera.SetCameraDefaults();
            });
            
            // プレイヤーの ThirdPersonController を取得
            ThirdPersonController controller = playerCameraController.ThirdPersonController;
            ThirdPersonController = controller;
            if (controller != null)
            {
                LandscapeCamera.OnSetCameraCalled += () =>
                {
                    if (LandscapeCamera.cameraState == LandscapeCameraState.Walker)
                    {
                        controller.SetViewMode(ThirdPersonController.ViewMode.Pedestrian);
                    }
                    else
                    {
                        controller.SetViewMode(ThirdPersonController.ViewMode.Overhead);
                    }

                };
                controller.SetViewMode(ThirdPersonController.ViewMode.Overhead);
            
                // // カメラの移動通知
                // CameraMoveByUserInput.OnCameraMoved.AddListener(() =>
                // {
                //     CheckCameraMoveComplete(controller);
                // });
                // 歩行者カメラの移動を無効化
                WalkerMoveByUserInput.IsActive = false;
            }
        }

        // カメラ移動完了を監視する関数
        private void CheckCameraMoveComplete(ThirdPersonController controller)
        {
            var camObj = GameObject.Find("CameraParent");
            if (camObj == null) return;
            var camTrans = camObj.transform;

            // 位置・回転の変化量を計算
            float posDiff = Vector3.Distance(camTrans.position, lastCameraPosition);
            float rotDiff = Quaternion.Angle(camTrans.rotation, lastCameraRotation);

            if (posDiff < moveCompleteThreshold && rotDiff < moveCompleteThreshold)
            {
                stillTime += Time.deltaTime;
                if (stillTime > moveCompleteWait)
                {
                    // ここで「カメラ移動完了」イベントを発火
                    controller.OnCameraMoved();
                    
                    // 必要な処理をここに追加
                    stillTime = 0f; // 1回だけ通知したい場合
                }
            }
            else
            {
                stillTime = 0f;
            }

            lastCameraPosition = camTrans.position;
            lastCameraRotation = camTrans.rotation;
        }
    }
}