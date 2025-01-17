﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class CameraController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
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
        public bool Grounded = true;

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

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private bool _cameraRotating = false;
        [SerializeField] float _cameraSmoothing = 1f;
        [SerializeField] float _cameraRotationSpeed = 2f;
        [SerializeField] float _minXFrame = .1f;
        [SerializeField] float _maxXFrame = .9f;
        [SerializeField] float _minYFrame = .75f;
        [SerializeField] float _maxYFrame = .25f;
        Quaternion isometricOffset;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private Transform _playerTransform;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private GameObject[] _virtualCameras;
        private ICinemachineCamera _vcamOverTheShoulder;
        private ICinemachineCamera _vcamLowAngle;
        private ICinemachineCamera _vcamIsometric;
        private ICinemachineCamera _vcamMigratingIsometric;
        private CinemachineFramingTransposer _vcamMigratingIsometricFrame;
        private GameObject _isoRotation;
        private HUDUpdater _hud;
        private GameObject _flashlight;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
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

                _isoRotation = GameObject.FindGameObjectWithTag("IsometricRotation");

                _flashlight = GameObject.FindGameObjectWithTag("Flashlight");

                _hud = FindObjectOfType<HUDUpdater>();

                _virtualCameras = GameObject.FindGameObjectsWithTag("Vcam");
                foreach (GameObject Vcam in _virtualCameras)
                {
                    switch (Vcam.name)
                    {
                        case "OverTheShoulder":
                        {
                            _vcamOverTheShoulder = Vcam.GetComponent<ICinemachineCamera>();
                            break;
                        }
                        case "LowAngle":
                        {
                            _vcamLowAngle = Vcam.GetComponent<ICinemachineCamera>();
                            break;
                        }
                        case "Isometric":
                        {
                            _vcamIsometric = Vcam.GetComponent<ICinemachineCamera>();
                            break;
                        }
                        case "MigratingIsometric":
                        {
                            _vcamMigratingIsometric = Vcam.GetComponent<ICinemachineCamera>();
                            _vcamMigratingIsometricFrame = Vcam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
                            break;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _input.vcam1 = true;
            _playerTransform = GetComponent<Transform>();
            _flashlight.SetActive(true);
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            ToggleFlashlight();
            Quit();
        }

        private void LateUpdate()
        {
            CameraRotation();
            CameraMigration();
            CameraSelection();
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

        private void CameraRotation()
        {
            //If the main camera is in 3rd person perspective mode, offer full rotations
            if (!Camera.main.orthographic)
            {
                // if there is an input and camera position is not fixed
                if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
                {
                    //Don't multiply mouse input by Time.deltaTime;
                    float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                    _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                    _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
                }

                // clamp our rotations so our values are limited 360 degrees
                _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                // If we are currently looking at over the shoulder, take pitch into account
                if (CinemachineCore.Instance.IsLive(_vcamOverTheShoulder))
                {
                    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                }
                else
                {
                    _cinemachineTargetPitch = 0.0f;
                }

                // Cinemachine will follow this target
                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw, 0.0f);
            }
            // otherwise, if in orthographic mode
            else
            {
                if (_input.rotateCameraRight && !_cameraRotating)
                {
                    _cameraRotating = true;
                    _input.rotateCameraRight = false;
                    StartCoroutine(CameraQuarterRotation(90f));
                }
                if (_input.rotateCameraLeft && !_cameraRotating)
                {
                    _cameraRotating = true;
                    _input.rotateCameraLeft = false;
                    StartCoroutine(CameraQuarterRotation(-90f));
                }
            }
        }

        IEnumerator CameraQuarterRotation(float rotation)
        {
            float count = 0;
            if (rotation > 0)
            {
                while (count < rotation)
                {
                    _isoRotation.GetComponent<Transform>().Rotate(0f, _cameraRotationSpeed, 0f, Space.World);
                    count += _cameraRotationSpeed;
                    yield return null;
                }
            }
            else
            {
                while (count > rotation)
                {
                    _isoRotation.GetComponent<Transform>().Rotate(0f, -_cameraRotationSpeed, 0f, Space.World);
                    count -= _cameraRotationSpeed;
                    yield return null;
                }
            }
            _cameraRotating = false;
        }

        private void CameraMigration()
        {
            // Initialize current camera values
            float oldX;
            float oldY;
            float newX;
            float newY;
            oldX = _vcamMigratingIsometricFrame.m_ScreenX;
            oldY = _vcamMigratingIsometricFrame.m_ScreenY;
            switch ((int)_isoRotation.transform.eulerAngles.y)
            {
                case 0:
                {
                    isometricOffset = Quaternion.AngleAxis(-45, Vector3.up);
                    break;
                }
                case 90:
                {
                    isometricOffset = Quaternion.AngleAxis(-135, Vector3.up);
                    break;
                }
                case 180 or -180:
                {
                    isometricOffset = Quaternion.AngleAxis(-225, Vector3.up);
                    break;
                }
                case 270 or -90:
                {
                    isometricOffset = Quaternion.AngleAxis(-315, Vector3.up);
                    break;
                }
            }
            Vector3 isometricForward = isometricOffset * _playerTransform.forward;

            // Determine the new X position
            if (isometricForward.x > 0)
                {
                    newX = _minXFrame;
                }
            else
                {
                    newX = _maxXFrame;
                }

            // If different than previous, create an interpolation set to the new X position
            if (oldX != newX)
            {
                float lerpResult = Mathf.Lerp(oldX, newX, Time.deltaTime * _cameraSmoothing);
                _vcamMigratingIsometricFrame.m_ScreenX = lerpResult;
            }

            // Determine the new Y position
            if (isometricForward.z > 0)
                {
                    newY = _minYFrame;
                }
            else
                {
                    newY = _maxYFrame;
                }

            // If different than previous, create an interpolation set to the Y position
            if (oldY != newY)
            {
                float lerpResult = Mathf.Lerp(oldY, newY, Time.deltaTime * _cameraSmoothing);
                _vcamMigratingIsometricFrame.m_ScreenY = lerpResult;
            }

        }

        private void CameraSelection()
        {
            if (_input.vcam1)
            {
                Debug.Log("Vcam1 Selected");
                Camera.main.orthographic = false;
                _vcamOverTheShoulder.Priority = 1;
                _vcamLowAngle.Priority = 0;
                _vcamIsometric.Priority = 0;
                _vcamMigratingIsometric.Priority = 0;
                _input.vcam1 = false;
                Cursor.visible = false;
                _input.cursorLocked = true;
                _input.cursorInputForLook = true;
                _hud.UpdateHUD("OverTheShoulder");
            }
            else if (_input.vcam2)
            {
                Debug.Log("Vcam2 Selected");
                Camera.main.orthographic = true;
                _vcamOverTheShoulder.Priority = 0;
                _vcamLowAngle.Priority = 0;
                _vcamIsometric.Priority = 1;
                _vcamMigratingIsometric.Priority = 0;
                _input.vcam2 = false;
                Cursor.visible = true;
                _input.cursorLocked = false;
                _input.cursorInputForLook = false;
                _hud.UpdateHUD("Isometric");
            }
            else if (_input.vcam3)
            {
                Debug.Log("Vcam3 Selected");
                Camera.main.orthographic = true;
                _vcamOverTheShoulder.Priority = 0;
                _vcamLowAngle.Priority = 0;
                _vcamIsometric.Priority = 0;
                _vcamMigratingIsometric.Priority = 1;
                _input.vcam3 = false;
                Cursor.visible = true;
                _input.cursorLocked = false;
                _input.cursorInputForLook = false;
                _hud.UpdateHUD("MigratingIsometric");
            }
            else if (_input.vcam4)
            {
                Debug.Log("Vcam4 Selected");
                Camera.main.orthographic = false;
                _vcamOverTheShoulder.Priority = 0;
                _vcamLowAngle.Priority = 1;
                _vcamIsometric.Priority = 0;
                _vcamMigratingIsometric.Priority = 0;
                _input.vcam4 = false;
                Cursor.visible = false;
                _input.cursorLocked = true;
                _input.cursorInputForLook = true;
                _hud.UpdateHUD("LowAngle");
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
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

        private void ToggleFlashlight()
        {
            if (_input.flashlight)
            {
                if (_flashlight.activeInHierarchy)
                {
                    _flashlight.SetActive(false);
                }
                else
                {
                    _flashlight.SetActive(true);
                }
                _input.flashlight = false;
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
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void Quit()
        {
            if (_input.quit)
            {
                Debug.Log("Quitting App");
                Application.Quit();
            }
        }
    }
}
