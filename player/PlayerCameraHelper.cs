using Godot;

namespace GodotFPS.player_helpers
{
    public class PlayerCameraHelper
    {
        private const float JoypadDeadzone = 0.15f;
        private const float JoypadSensitivity = 2;
        private const float MouseSensitivity = 0.05f;
        private Camera _camera;
        private Spatial _rotationNode;
        private Player _player;

        public void OnReady(Player player)
        {
            _player = player;
            _camera = player.GetNode<Camera>("Rotation_Helper/Camera");
            _rotationNode = player.GetNode<Spatial>("Rotation_Helper");
        }

        public void ProcessViewInput(float delta)
        {
            if (Input.GetMouseMode() != Input.MouseMode.Captured) return;
            if (Input.GetConnectedJoypads().Count == 0) return;

            // Joypad rotation
            var joypadVec = new Vector2();
            
            var osName = OS.GetName();
            if (osName == "Windows")
                joypadVec = new Vector2(Input.GetJoyAxis(0, 2), Input.GetJoyAxis(0, 3));
            else if (osName == "X11" || osName == "OSX")
                joypadVec = new Vector2(Input.GetJoyAxis(0, 3), Input.GetJoyAxis(0, 4));

            if (joypadVec.Length() < JoypadDeadzone)
                joypadVec = new Vector2(0,0);
            else
                joypadVec = joypadVec.Normalized() * ((joypadVec.Length() - JoypadDeadzone) / (1 - JoypadDeadzone));
            
            _rotationNode.RotateX(Mathf.Deg2Rad(joypadVec.y * JoypadSensitivity));
            _player.RotateY(Mathf.Deg2Rad(joypadVec.x * JoypadSensitivity * -1));

            var cameraRot = _rotationNode.RotationDegrees;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -70, 70);
            _rotationNode.RotationDegrees = cameraRot;
        }

        public void ProcessMouseInput(Vector2 relativeMotion)
        {
            _rotationNode.RotateX(Mathf.Deg2Rad(relativeMotion.y * MouseSensitivity));
            _player.RotateY(Mathf.Deg2Rad(relativeMotion.x * MouseSensitivity * -1));
            var cameraRotation = _rotationNode.RotationDegrees;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70, 70);
            _rotationNode.RotationDegrees = cameraRotation;
        }
    }
}