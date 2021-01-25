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
        private float _forwardSpeed = 5.0f;

        [Tooltip("Speed when moving backwards.")]
        [SerializeField]
        private float _backwardSpeed = 5.0f;

        [Tooltip("Speed when moving sideways.")]
        [SerializeField]
        private float _strafeSpeed = 5.0f;

        [Tooltip("Speed multiplier while running.")]
        [SerializeField]
        private float _runSpeedMultiplier = 1.5f;

        #endregion

        #region PROPERTIES

        // An array that holds the player's various states with a state buffer size of 512; 
        // This buffer size divided by the tick rate is how much time is recorded for the player
        // 512 divided by 30 is about 17 seconds to work with

        private PlayerManager[] clientStateBuffer = new PlayerManager[512];

        /// <summary>
        /// Cached timer.
        /// </summary>

        public float timer { get; private set; }

        /// <summary>
        /// Cached tick number.
        /// </summary>
        /// 

        public int tickNumber { get; private set; }

        /// <summary>
        /// Cached target position.
        /// </summary>
        /// 

        public Vector3 targetPosition { get; set; }

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
                if (run && moveDirection != Vector3.zero)
                {
                    animator.speed = 1.5f;
                }
                else
                {
                    animator.speed = 1f;
                }

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
        }

        /// <summary>
        /// Handles input.
        /// </summary>

        protected override void HandleInput()
        {
            // Subtract from the timer to get closer to the point in time that
            //  the last FixedUpdate was called

            timer -= Time.fixedDeltaTime;

            // Store a copy of the player's input for local use

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical"),
            };

            // Send a copy of the player's movement status to the server for verification

            ClientSend.PlayerMovement(moveDirection, transform.rotation, tickNumber);

            // Store a copy of the player's state at this point in time
            // We use the remainder as our buffer slot to signify the state's point in time

            uint bufferSlot = (uint)tickNumber % 512;
            clientStateBuffer[bufferSlot].moveDirection = moveDirection;
            clientStateBuffer[bufferSlot].position = transform.position;

            // Simulate movement for the character by calling Physics.Simulate
            // This will esentially run FixedUpdate manually

            Physics.Simulate(Time.fixedDeltaTime);

            // Update the tick that our next physics simulation will begin at

            tickNumber++;
        }

        /// <summary>
        /// Syncs player's location to the server.
        /// </summary>
        /// 

        public virtual void SyncPlayer(Vector3 _position, int _tickNumber)
        {
            //This buffer signifies the point in time we are correcting

            uint bufferSlot = (uint)_tickNumber % 512;

            // Check for the margin of error in the player's position at the specified point in time (the tick)

            Vector3 positionMarginOfError = _position - clientStateBuffer[bufferSlot].position;

            // If there is a slight margin of error, rewind and replay

            if (positionMarginOfError.sqrMagnitude > 0.0000001f)
            {
                // Snap the player to the correct position in the state returned

                Rigidbody playerRigidbody = GetComponent<Rigidbody>();
                //playerRigidbody.position = _position;
                playerRigidbody.position = Vector3.Slerp(playerRigidbody.position, _position, 0.8f);

                // The tick number of the server when its version of the player was done calculating

                uint rewindTickNumber = (uint)_tickNumber;

                // Correct the margin of error until we are caught back up

                while (rewindTickNumber < tickNumber)
                {
                    bufferSlot = rewindTickNumber % 512;
                    clientStateBuffer[bufferSlot].moveDirection = moveDirection;
                    clientStateBuffer[bufferSlot].position = playerRigidbody.position;

                    // Execute the above calculations

                    Physics.Simulate(Time.fixedDeltaTime);

                    rewindTickNumber++;
                }
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

            // Set up the player's buffer states

            for (int i = 0; i < clientStateBuffer.Length; i++)
            {
                clientStateBuffer[i] = gameObject.AddComponent<PlayerManager>();
            }
        }

        /// <summary>
        /// Initialize this.
        /// </summary>

        public virtual void Start()
        {
            timer = 0.0f;
            tickNumber = 0;
        }

        public override void FixedUpdate()
        {
            // Perform character movement

            Move();
        }

        public override void Update()
        {
            // Record the amount of time it took to finish the last frame (Time.deltaTime)

            timer += Time.deltaTime;

            // Record which tick we will begin the next physics simulation at

            tickNumber = 0;

            // Execute this while loop so long as the timer is greater than the amount of time it took
            //  to finish the last call to FixedUpdate (Time.fixedDeltaTime)
            // NOTE: The time it takes to finish the last frame is usually greater than
            //  the time it takes to finish the last call to FixedUpdate

            while (timer >= Time.fixedDeltaTime)
            {
                // Handle input

                HandleInput();
            }

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
