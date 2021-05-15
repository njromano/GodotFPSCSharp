using Godot;

namespace GodotFPS.player_helpers
{
    public class PlayerInputHelper
    {
        public const float JoypadDeadzone = 0.15f;
        public const float Gravity = -49.6f;
        public const float MaxSpeed = 40f;
        public const float MaxSprintSpeed = 60;
        public const float JumpSpeed = 18f;
        public const float Acceleration = 9f;
        public const float SprintAcceleration = 18f;
        public const float DeAcceleration = 16f;
        public const int MaxSlopeAngle = 40;

        private bool _isSprinting;
        private Vector3 _direction;
        private Player _player;
        private Camera _camera;
        private Vector3 _velocity;

        public void OnReady(Player player)
        {
            _player = player;
            _camera = player.GetNode<Camera>("Rotation_Helper/Camera");
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public void ProcessInput(float delta)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Visible)
                Input.SetMouseMode(Input.MouseMode.Captured);

            _direction = new Vector3();
            var canXForm = _camera.GlobalTransform;
            var inputMovementVector = new Vector2();

            // keyboard
            if (Input.IsActionPressed("movement_forward"))
                inputMovementVector.y += 1;
            if (Input.IsActionPressed("movement_backward"))
                inputMovementVector.y -= 1;
            if (Input.IsActionPressed("movement_left"))
                inputMovementVector.x -= 1;
            if (Input.IsActionPressed("movement_right"))
                inputMovementVector.x += 1;

            // joypad
            if (Input.GetConnectedJoypads().Count > 0)
            {
                var joypadVec = new Vector2(0, 0);
                var osName = OS.GetName();
                if (osName == "Windows")
                    joypadVec = new Vector2(Input.GetJoyAxis(0, 0), -Input.GetJoyAxis(0, 1));
                else if (osName == "X11" || osName == "OSX")
                    joypadVec = new Vector2(Input.GetJoyAxis(0, 1), Input.GetJoyAxis(0, 2));

                if (joypadVec.Length() < JoypadDeadzone)
                    joypadVec = new Vector2(0, 0);
                else
                {
                    var normalized = joypadVec.Normalized();
                    var multiplier = (joypadVec.Length() - JoypadDeadzone) / (1 - JoypadDeadzone);
                }

                inputMovementVector += joypadVec;
            }

            // normalize so diagonal directions don't go faster
            inputMovementVector = inputMovementVector.Normalized();

            // basis vectors already normalized
            _direction += -canXForm.basis.z * inputMovementVector.y;
            _direction += canXForm.basis.x * inputMovementVector.x;

            // Jumping
            if (IsOnFloor() && Input.IsActionJustPressed("movement_jump"))
                _velocity.y = JumpSpeed;

            // Sprinting
            _isSprinting = Input.IsActionPressed("movement_sprint");
        }

        public void ProcessMovement(float delta)
        {
            _direction.y = 0;
            _direction = _direction.Normalized();
            _velocity.y += delta * Gravity;

            var target = _direction;
            target *= _isSprinting ? MaxSprintSpeed : MaxSpeed;

            var hVelocity = _velocity;
            hVelocity.y = 0;

            float acceleration;
            if (_direction.Dot(hVelocity) > 0)
                acceleration = _isSprinting ? SprintAcceleration : Acceleration;
            else
                acceleration = DeAcceleration;

            hVelocity = hVelocity.LinearInterpolate(target, acceleration * delta);
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity = _player.MoveAndSlide(_velocity,
                new Vector3(0, 1, 0),
                true,
                4,
                Mathf.Deg2Rad(MaxSlopeAngle));
        }

        private bool IsOnFloor()
        {
            return true;
        }
    }
}