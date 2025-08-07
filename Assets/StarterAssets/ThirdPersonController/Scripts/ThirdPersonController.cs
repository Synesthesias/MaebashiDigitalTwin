using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using Landscape2.Runtime;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [SerializeField]
        private CharacterController _controller;
        public float CharacterHeight => _controller.height;
        
        [SerializeField]
        private StarterAssetsInputs _input;
        
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        
        [Header("Audio")]
        [Tooltip("Audio clip to play when landing")]
        public AudioClip LandingAudioClip;
        
        [Tooltip("Array of footstep audio clips")]
        public AudioClip[] FootstepAudioClips;
        
        [Tooltip("Volume of footstep and landing sounds")]
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = false;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // fall detection and reset
        [SerializeField]
        private Vector3 _initialPosition;
        [Header("Fall Detection")]
        [Tooltip("Y position threshold for fall detection. Player resets when falling below this value")]
        public float FallThreshold = -100f;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        [SerializeField]
        private PlayerInput _playerInput;
        private IInputFocusHandler focusHandler;
#endif
        private Animator _animator;
        
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private Vector2 _lastInputMove = Vector2.zero;
        private Vector2 _lastCameraPositionXZ = Vector2.zero;
        private float _cameraDistanceThreshold = 0.1f; // カメラ移動の閾値

        public enum ViewMode
        {
            Pedestrian, // 歩行者モード
            Overhead    // 俯瞰モード
        }
        [Header("View Mode")]
        public ViewMode CurrentViewMode = ViewMode.Pedestrian;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            if (_controller == null)
            {
                _controller = GetComponent<CharacterController>();
            }
            if (_input == null)
            {
                _input = GetComponent<StarterAssetsInputs>();
                if (_input == null)
                {
                    Debug.LogError("StarterAssetsInputs component is missing from the GameObject.");
                }
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

#if ENABLE_INPUT_SYSTEM
            // フォーカス制御の登録
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }
            
            if (_playerInput != null)
            {
                try
                {
                    focusHandler = new PlayerInputFocusHandler(_playerInput);
                    InputFocusManager.RegisterHandler(focusHandler);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"PlayerInputFocusHandler の初期化に失敗しました: {ex.Message}");
                }
            }
#endif
        }

        private void Update()
        {
            if (CurrentViewMode == ViewMode.Overhead)
            {
                return;
            }
            
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            
            // Check if player has fallen below threshold
            float currentYPosition = transform.position.y;
            if (currentYPosition < FallThreshold)
            {
                ResetToInitialPosition();
            }
        }

        public void OnCameraMoved()
        {
            if (CurrentViewMode == ViewMode.Overhead)
            {
                OverheadMovePlayerToCameraFront();
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        public void Move()
        {
            if (_input == null || _controller == null || _mainCamera == null)
            {
                return;
            }

            // Input Systemを使用してmove inputを取得
            Vector2 inputMove = _input.move;
            MoveWithInput(inputMove);
        }
        
        // 外部から移動入力を与えて移動させるメソッド
        public void MoveWithInput(Vector2 inputMove)
        {
            if (_controller == null || _mainCamera == null)
            {
                return;
            }

            bool hasMovementInput = inputMove.magnitude >= _threshold;
            
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = hasMovementInput ? MoveSpeed : 0.0f;
            
            // 入力に基づいて直接速度を設定（斜め移動でも速度が一定になるようにクランプ）
            float inputMagnitude = hasMovementInput ? Mathf.Min(inputMove.magnitude, 1.0f) : 0f;
            _speed = targetSpeed * inputMagnitude;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 入力に基づいて移動方向を計算
            Vector3 inputDirection = new Vector3(inputMove.x, 0.0f, inputMove.y).normalized;
            
            if (hasMovementInput)
            {
                // カメラの向きを基準に、入力方向へプレイヤーを向ける
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                
                // プレイヤーの回転を更新
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = hasMovementInput ? Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward : Vector3.zero;
            Vector3 moveVector = targetDirection.normalized * (_speed * Time.deltaTime);
            
            _controller.Move(moveVector + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }
        
        // 直接移動ベクトルを与えて移動させるメソッド（カメラの回転を考慮しない）
        public void MoveDirectly(Vector3 moveDirection)
        {
            if (_controller == null)
            {
                return;
            }
            
            // 重力を加えて移動
            _controller.Move(moveDirection + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        
        // 一瞬で指定された距離だけ移動する（UI入力用）
        public void MoveInstantly(Vector2 inputMove)
        {
            if (_controller == null || _mainCamera == null)
            {
                return;
            }
            
            // 入力に基づいて移動方向を計算
            Vector3 inputDirection = new Vector3(inputMove.x, 0.0f, inputMove.y);
            
            // カメラの向きを基準に移動方向を変換
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;
            
            // Y軸回転のみ考慮
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // 移動方向を計算
            Vector3 moveDirection = cameraRight * inputDirection.x + cameraForward * inputDirection.z;
            
            // 一瞬で移動（Time.deltaTimeを使わない）
            _controller.Move(moveDirection);
            
            // プレイヤーの向きを移動方向に合わせる
            if (moveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }

                    // ジャンプ入力をリセット
                    _input.jump = false;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
        
        private void OnFootstep(AnimationEvent animationEvent)
        {
            // animationEventがnullまたはweightが低い場合は処理しない
            if (animationEvent == null || animationEvent.animatorClipInfo.weight <= 0.5f)
            {
                return;
            }
            
            // FootstepAudioClipsが設定されていて、有効なクリップがある場合
            if (FootstepAudioClips != null && FootstepAudioClips.Length > 0 && _controller != null)
            {
                // ランダムにクリップを選択
                var index = Random.Range(0, FootstepAudioClips.Length);
                var clip = FootstepAudioClips[index];
                
                // クリップがnullでない場合は再生
                if (clip != null)
                {
                    // プレイヤーの中心位置で音を再生
                    AudioSource.PlayClipAtPoint(clip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }
        
        private void OnLand(AnimationEvent animationEvent)
        {
            // animationEventがnullまたはweightが低い場合は処理しない
            if (animationEvent == null || animationEvent.animatorClipInfo.weight <= 0.5f)
            {
                return;
            }
            
            // LandingAudioClipが設定されている場合
            if (LandingAudioClip != null && _controller != null)
            {
                // プレイヤーの中心位置で音を再生
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }


        // 俯瞰モード時にカメラのforward方向にRayを飛ばし、地面に当たった位置にプレイヤーを移動
        private void OverheadMovePlayerToCameraFront()
        {
            if (_mainCamera == null) return;

            Vector2 currentCameraPositionXZ = new Vector2(_mainCamera.transform.position.x, _mainCamera.transform.position.z);
            float cameraMovementDistance = Vector2.Distance(currentCameraPositionXZ, _lastCameraPositionXZ);
            
            // 移動入力が変化したか、カメラが閾値以上移動したときRaycast
            if (_input.move != _lastInputMove || cameraMovementDistance > _cameraDistanceThreshold)
            {
                _lastInputMove = _input.move;
                _lastCameraPositionXZ = currentCameraPositionXZ;

                Vector3 rayOrigin = _mainCamera.transform.position;
                Vector3 rayDirection = _mainCamera.transform.forward;
                float rayDistance = 1000f;

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, GroundLayers))
                {
                    // 地面の上に適切な高さでプレイヤーを配置（俯瞰モード用）
                    Vector3 targetPos = hit.point + Vector3.up * (GroundedRadius * 0.5f);
                    
                    // カメラの移動距離に応じて補間速度を調整（高速移動時は追従を早める）
                    float lerpSpeed = cameraMovementDistance > 5.0f ? 25.0f : 10.0f;
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);

                    float targetYaw = _mainCamera.transform.eulerAngles.y;
                    float rotationSpeed = cameraMovementDistance > 5.0f ? 20.0f : 10.0f;
                    float smoothYaw = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, Time.deltaTime * rotationSpeed);
                    transform.rotation = Quaternion.Euler(0.0f, smoothYaw, 0.0f);
                }
            }
        }

        // 外部からモードを切り替えるためのメソッド
        public void SetViewMode(ViewMode mode)
        {
            ViewMode previousMode = CurrentViewMode;
            CurrentViewMode = mode;
            gameObject.SetActive(CurrentViewMode == ViewMode.Pedestrian);
#if ENABLE_INPUT_SYSTEM 
            if (_playerInput != null)
            {
                _playerInput.enabled = CurrentViewMode == ViewMode.Pedestrian;
            }
#endif
        }
        
        // 歩行者モード用にプレイヤーの位置を調整
        private void AdjustPlayerPositionForWalkerMode()
        {
            // 現在の位置から下方向にRaycastして地面を探す
            Vector3 rayOrigin = transform.position + Vector3.up * 2.0f; // 少し上から開始
            Vector3 rayDirection = Vector3.down;
            float rayDistance = 10.0f;
            
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, GroundLayers))
            {
                // 地面の上に適切な高さでプレイヤーを配置（GroundedOffsetは負の値なので注意）
                Vector3 targetPos = hit.point + Vector3.up * (GroundedRadius + 0.1f);
                transform.position = targetPos;
                
                // 垂直速度をリセット
                _verticalVelocity = 0f;
            }
        }

        private void ResetToInitialPosition()
        {
            // Disable character controller to allow position change
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            // Reset position and rotation
            transform.position = _initialPosition;

            // Reset velocities
            _verticalVelocity = 0f;
            _speed = 0f;
            _animationBlend = 0f;

            // Reset camera rotation if needed
            if (CinemachineCameraTarget != null)
            {
                _cinemachineTargetPitch = 0f;
            }

            // Re-enable character controller
            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }

#if ENABLE_INPUT_SYSTEM
        private void OnDestroy()
        {
            // フォーカス制御の登録解除
            if (focusHandler != null)
            {
                InputFocusManager.UnregisterHandler(focusHandler);
            }
        }
#endif
    }
}