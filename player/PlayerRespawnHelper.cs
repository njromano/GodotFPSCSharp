using System;
using Godot;
using Godot.Collections;

namespace GodotFPS.player_helpers
{
    public class PlayerRespawnHelper
    {
        private const float RespawnTime = 4;

        private Player _player;
        private Globals _globals;
        private Camera _camera;
        private float _deadTime;

        public void OnReady(Player player)
        {
            _player = player;
            _globals = GetNode<Globals>("/root/Globals");
            _camera = GetNode<Camera>("Rotation_Helper/Camera");
            player.GlobalTransform = new Transform(
                player.GlobalTransform.basis,
                _globals.GetRespawnPosition());
        }

        public void ProcessRespawn(float delta, PlayerWeaponHelper weaponHelper)
        {
            if (_player.Health <= 0 && !_player.IsDead)
            {
                GetNode<CollisionShape>("Body_CollisionShape").Disabled = true;
                GetNode<CollisionShape>("Feet_CollisionShape").Disabled = true;
                weaponHelper.ChangingWeapon = true;
                weaponHelper.ChangingWeaponName = "UNARMED";
                GetNode<ColorRect>("HUD/Death_Screen").Visible = true;
                GetNode<Panel>("HUD/Panel").Visible = false;
                GetNode<Control>("HUD/Crosshair").Visible = false;
                _deadTime = RespawnTime;
                _player.IsDead = true;
                var grabbedObject = weaponHelper.GrabbedObject;
                if (grabbedObject != null)
                {
                    grabbedObject.Mode = RigidBody.ModeEnum.Rigid;
                    grabbedObject.ApplyImpulse(new Vector3(0, 0, 0),
                        -_camera.GlobalTransform.basis.z.Normalized()
                        * PlayerWeaponHelper.ObjectThrowForce / 2);
                    grabbedObject.CollisionLayer = 1;
                    grabbedObject.CollisionMask = 1;
                    grabbedObject = null;
                }
            }

            if (!_player.IsDead) return;
            
            _deadTime -= delta;
            var deadTimePretty = $"{_deadTime:0.}";
            GetNode<Label>("HUD/Death_Screen/Label").Text = $"You died\n{deadTimePretty} seconds until respawn";
            if (_deadTime > 0) return;
            _player.GlobalTransform = new Transform(
                _player.GlobalTransform.basis,
                _globals.GetRespawnPosition());
            GetNode<CollisionShape>("Body_CollisionShape").Disabled = false;
            GetNode<CollisionShape>("Feet_CollisionShape").Disabled = false;
            GetNode<ColorRect>("HUD/Death_Screen").Visible = false;
            GetNode<Panel>("HUD/Panel").Visible = true;
            GetNode<Control>("HUD/Crosshair").Visible = true;
            foreach (var weapon in weaponHelper.Weapons)
            {
                weapon.Value?.Call("reset_weapon");
            }

            _player.Health = 100;
            weaponHelper.GrenadeAmounts = new Dictionary<string, int>
            {
                {"Grenade", 2}, {"Sticky Grenade", 2}
            };
            weaponHelper.CurrentGrenadeName = "Grenade";
            _player.IsDead = false;
        }

        private T GetNode<T>(string path) where T : class
        {
            return _player.GetNode<T>(path);
        }
    }
}