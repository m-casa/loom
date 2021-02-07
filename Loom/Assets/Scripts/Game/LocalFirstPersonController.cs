using UnityEngine;

namespace ECM.Controllers
{
    /// <summary>
    /// Base First Person Controller.
    /// 
    /// Base class for a first person controller.
    /// It inherits from 'BaseCharacterController' and extends it to perform classic FPS movement.
    /// 
    /// As the base character controllers, this default behaviour can easily be modified or completely replaced in a derived class. 
    /// </summary>

    public class LocalFirstPersonController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("First Person")]
        [Tooltip("Speed when moving forward.")]
        [SerializeField]
        private float _forwardSpeed = 7.0f;

        [Tooltip("Speed when moving backwards.")]
        [SerializeField]
        private float _backwardSpeed = 7.0f;

        [Tooltip("Speed when moving sideways.")]
        [SerializeField]
        private float _strafeSpeed = 7.0f;

        [Tooltip("Speed multiplier while running.")]
        [SerializeField]
        private float _runSpeedMultiplier = 1.5f;

        [Tooltip("Speed multiplier for syncing player.")]
        [SerializeField]
        private float _lerpSpeed = 1.0f;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached buffer size.
        /// </summary>

        public int bufferSize { get; private set; }

        /// <summary>
        /// Cached player state buffer.
        /// </summary>

        public PlayerState[] playerStateBuffer { get; private set; }

        /// <summary>
        /// Cached timer.
        /// </summary>

        public float timer { get; private set; }

        /// <summary>
        /// Cached tick number.
        /// </summary>

        public int tickNumber { get; private set; }

        /// <summary>
        /// Cached camera pivot transform.
        /// </summary>

        public Transform cameraPivotTransform { get; private set; }

        /// <summary>
        /// Cached camera transform.
        /// </summary>

        public Transform cameraTransform { get; private set; }

        /// <summary>
        /// Cached MouseLook component.
        /// </summary>

        public Components.MouseLook mouseLook { get; private set; }

        /// <summary>
        /// Speed when moving forward.
        /// </summary>

        public float forwardSpeed
        {
            get { return _forwardSpeed; }
            set { _forwardSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Speed when moving backwards.
        /// </summary>

        public float backwardSpeed
        {
            get { return _backwardSpeed; }
            set { _backwardSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Speed when moving sideways.
        /// </summary>

        public float strafeSpeed
        {
            get { return _strafeSpeed; }
            set { _strafeSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Speed multiplier while running.
        /// </summary>

        public float runSpeedMultiplier
        {
            get { return _runSpeedMultiplier; }
            set { _runSpeedMultiplier = Mathf.Max(value, 1.0f); }
        }

        /// <summary>
        /// Run input command.
        /// </summary>

        public bool run { get; set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Perform character animation.
        /// </summary>

        protected override void Animate()
        {
            if (animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("horizontal", Mathf.Round(moveDirection.x), 1f, Time.deltaTime * 10f);
                animator.SetFloat("vertical", Mathf.Round(moveDirection.z), 1f, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// Use this method to animate camera.
        /// The default implementation use this to animate camera's when crouching.
        /// Called on LateUpdate.
        /// </summary>

        protected virtual void AnimateView()
        {
            // Scale camera pivot to simulate crouching

            var yScale = isCrouching ? Mathf.Clamp01(crouchingHeight / standingHeight) : 1.0f;

            cameraPivotTransform.localScale = Vector3.MoveTowards(cameraPivotTransform.localScale,
                new Vector3(1.0f, yScale, 1.0f), 5.0f * Time.deltaTime);
        }

        /// <summary>
        /// Perform 'Look' rotation.
        /// This rotate the character along its y-axis (yaw) and a child camera along its local x-axis (pitch).
        /// </summary>

        protected virtual void RotateView()
        {
            mouseLook.LookRotation(movement, cameraTransform);
        }

        /// <summary>
        /// Override the default ECM UpdateRotation to perform typical fps rotation.
        /// </summary>

        protected override void UpdateRotation()
        {
            RotateView();
        }

        /// <summary>
        /// Get target speed, relative to input moveDirection,
        /// eg: forward, backward or strafe.
        /// </summary>

        protected virtual float GetTargetSpeed()
        {
            // Defaults to forward speed

            var targetSpeed = forwardSpeed;

            // Strafe

            if (moveDirection.x > 0.0f || moveDirection.x < 0.0f)
                targetSpeed = strafeSpeed;

            // Backwards

            if (moveDirection.z < 0.0f)
                targetSpeed = backwardSpeed;

            // Forward handled last as if strafing and moving forward at the same time,
            // forward speed should take precedence

            if (moveDirection.z > 0.0f)
                targetSpeed = forwardSpeed;

            // Handle run speed modifier

            return run ? targetSpeed * runSpeedMultiplier : targetSpeed;
        }

        /// <summary>
        /// Overrides CalcDesiredVelocity to generate a velocity vector relative to view direction
        /// eg: forward, backward or strafe.
        /// </summary>

        protected override Vector3 CalcDesiredVelocity()
        {
            // Set character's target speed (eg: moving forward, backward or strafe)

            speed = GetTargetSpeed();

            // Return desired velocity relative to view direction and target speed

            return transform.TransformDirection(base.CalcDesiredVelocity());
        }

        /// <summary>
        /// Perform character movement logic.
        /// 
        /// NOTE: Must be called in FixedUpdate.
        /// </summary>

        protected override void Move()
        {
            // Send a copy of the player's movement status to the server for verification
            ClientSend.PlayerState(moveDirection, transform.rotation, tickNumber);

            // Store a copy of the player's state at this point in time
            // We use the remainder as our buffer slot to signify the state's point in time
            uint bufferSlot = (uint)(tickNumber % bufferSize);
            playerStateBuffer[bufferSlot].moveDirection = moveDirection;
            playerStateBuffer[bufferSlot].position = transform.position;

            // Apply movement

            // If using root motion and root motion is being applied (eg: grounded),
            // move without acceleration / deceleration, let the animation takes full control
            var desiredVelocity = CalcDesiredVelocity();

            if (useRootMotion && applyRootMotion)
                movement.Move(desiredVelocity, speed, !allowVerticalMovement);
            else
            {
                // Move with acceleration and friction
                var currentFriction = isGrounded ? groundFriction : airFriction;
                var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

                movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction,
                    currentBrakingFriction, !allowVerticalMovement);
            }

            // Jump logic
            Jump();
            MidAirJump();
            UpdateJumpTimer();

            // Update root motion state,
            // should animator root motion be enabled? (eg: is grounded)
            applyRootMotion = useRootMotion && movement.isGrounded;

            // Update the tick that our next physics simulation will begin at
            tickNumber++;
        }

        /// <summary>
        /// Handles input.
        /// </summary>

        protected override void HandleInput()
        {
            // Store a copy of the player's input for local use
            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical"),
            };
        }

        /// <summary>
        /// Syncs player's location to the server.
        /// </summary>

        public virtual void SyncPlayer(Vector3 _position, int _tickNumber)
        {
            //This buffer signifies the point in time we are correcting
            uint bufferSlot = (uint)(_tickNumber % bufferSize);

            // Check for a difference in position at the specified point in time (the tick received)
            Vector3 positionDifference = _position - playerStateBuffer[bufferSlot].position;

            // If there is a margin of error, rewind and replay
            if (positionDifference.sqrMagnitude > 0.0000001f)
            {
                // If the current position is more than 1.5 units away from the server position then
                //  snap back (rewind) since smoothing wouldn't help a large correction
                // Else apply smoothing if more than 0.5 units, and no correction if less than that
                if ((_position - transform.position).sqrMagnitude >= 2.25f)
                {
                    transform.position = _position;
                }
                else if ((_position - transform.position).sqrMagnitude >= 0.255f)
                {
                    transform.position = Vector3.Slerp(transform.position, _position, _lerpSpeed * Time.deltaTime);
                }

                // The tick number of the server when its version of the player was done calculating
                uint rewindTickNumber = (uint)_tickNumber;

                // Correct the margin of error until we are caught back up (replay)
                while (rewindTickNumber < tickNumber)
                {
                    // Revise the player's state at this point in time
                    // We use the remainder as our buffer slot to signify the state's point in time
                    bufferSlot = (uint)(rewindTickNumber % bufferSize);
                    playerStateBuffer[bufferSlot].moveDirection = moveDirection;
                    playerStateBuffer[bufferSlot].position = transform.position;

                    // Update the tick that our next physics simulation will begin at
                    rewindTickNumber++;
                }

                // Reset the tick number to signify we will begin recording new player states
                tickNumber = 0;
            }
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Validate this editor exposed fields.
        /// </summary>

        public override void OnValidate()
        {
            // Call the parent class' version of method

            base.OnValidate();

            // Validate this editor exposed fields

            forwardSpeed = _forwardSpeed;
            backwardSpeed = _backwardSpeed;
            strafeSpeed = _strafeSpeed;

            runSpeedMultiplier = _runSpeedMultiplier;
        }

        /// <summary>
        /// Initialize this.
        /// </summary>

        public override void Awake()
        {
            // Call the parent class' version of method
            base.Awake();

            // Cache and initialize this components
            mouseLook = GetComponent<Components.MouseLook>();
            if (mouseLook == null)
            {
                Debug.LogError(
                    string.Format(
                        "BaseFPSController: No 'MouseLook' found. Please add a 'MouseLook' component to '{0}' game object",
                        name));
            }

            cameraPivotTransform = transform.Find("Camera_Pivot");
            if (cameraPivotTransform == null)
            {
                Debug.LogError(string.Format(
                    "BaseFPSController: No 'Camera_Pivot' found. Please parent a transform gameobject to '{0}' game object.",
                    name));
            }

            var cam = GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogError(
                    string.Format(
                        "BaseFPSController: No 'Camera' found. Please parent a camera to '{0}' game object.", name));
            }
            else
            {
                cameraTransform = cam.transform;
                mouseLook.Init(transform, cameraTransform);
            }

            // The buffer size divided by the tick rate is equal to the amount of time 
            //  that can be recorded into a player's state buffer
            // EX. 1024 divided by 60 is about 17 seconds to work with
            bufferSize = 1024;

            // A buffer that can hold a player's various states
            playerStateBuffer = new PlayerState[bufferSize];

            // Set up the player's buffer states
            for (int i = 0; i < playerStateBuffer.Length; i++)
            {
                playerStateBuffer[i] = new PlayerState();
            }

            // Will be used to keep track of Time.deltaTime
            timer = 0.0f;

            // Will be used to keep track of player states
            tickNumber = 0;
        }

        public override void FixedUpdate()
        {
            // Perform character movement
            Move();
        }

        public override void Update()
        {
            // Handle input
            HandleInput();

            // Update character rotation (if not paused)
            UpdateRotation();

            // Perform character animation (if not paused)
            Animate();
        }

        public virtual void LateUpdate()
        {
            // Perform camera's (view) animation

            AnimateView();
        }

        #endregion
    }
}
