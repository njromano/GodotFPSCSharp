using Godot;
using Godot.Collections;

namespace GodotFPS.player_helpers
{
    public class PlayerWeaponHelper
    {
        public const float ObjectThrowForce = 120;
        private const float GrenadeThrowForce = 50;
        private const float ObjectGrabDistance = 7;
        private const float ObjectGrabRayDistance = 10;
        private const float MouseSensitivityScrollWheel = 0.08f;

        private KinematicBody _player;
        private SpotLight _flashlight;

        private float _mouseScrollValue;
        private bool _reloadingWeapon;

        private PackedScene _grenadeScene = GD.Load<PackedScene>("res://weapons/Grenade.tscn");
        private PackedScene _stickyGrenadeScene = GD.Load<PackedScene>("res://weapons/Sticky_Grenade.tscn");
        private Camera _camera;

        public Dictionary<string, WeaponBase> Weapons { get; set; } =
            new Dictionary<string, WeaponBase> {{"UNARMED", null}, {"KNIFE", null}, {"PISTOL", null}, {"RIFLE", null}};

        public Dictionary<string, int> WeaponNameToNumber => new Dictionary<string, int>
        {
            {"UNARMED", 0}, {"KNIFE", 1}, {"PISTOL", 2}, {"RIFLE", 3}
        };

        public Dictionary<int, string> WeaponNumberToName { get; set; } = new Dictionary<int, string>
        {
            {0, "UNARMED"}, {1, "KNIFE"}, {2, "PISTOL"}, {3, "RIFLE"}
        };

        public Dictionary<string, int> GrenadeAmounts { get; set; } = new Dictionary<string, int>
        {
            {"Grenade", 2}, {"Sticky Grenade", 2}
        };

        public string CurrentWeaponName { get; set; } = "UNARMED";
        public string CurrentGrenadeName { get; set; } = "Sticky Grenade";
        public WeaponBase CurrentWeapon => Weapons[CurrentWeaponName];

        public int CurrentGrenadeAmount
        {
            get => GrenadeAmounts[CurrentGrenadeName];
            set => GrenadeAmounts[CurrentGrenadeName] = value;
        }

        public bool ChangingWeapon { get; set; }
        public string ChangingWeaponName { get; set; } = "UNARMED";
        public RigidBody GrabbedObject { get; set; }

        public void OnReady(Player player)
        {
            _player = player;
            var firePoints = player.GetNode("Rotation_Helper/Gun_Fire_Points");
            Weapons["KNIFE"] = firePoints.GetNode<WeaponBase>("Knife_Point");
            Weapons["PISTOL"] = firePoints.GetNode<WeaponBase>("Pistol_Point");
            Weapons["RIFLE"] = firePoints.GetNode<WeaponBase>("Rifle_Point");

            var gunAimPoint = player.GetNode<Spatial>("Rotation_Helper/Gun_Aim_Point");
            var gunAimPointPos = gunAimPoint.GlobalTransform.origin;
            foreach (var w in Weapons)
            {
                var weapon = w.Value;
                if (weapon == null) continue;
                weapon.Player = player;
                weapon.LookAt(gunAimPointPos, new Vector3(0, 1, 0));
                weapon.RotateObjectLocal(new Vector3(0, 1, 0), Mathf.Deg2Rad(180));
            }

            _flashlight = player.GetNode<SpotLight>("Rotation_Helper/Flashlight");
            _camera = player.GetNode<Camera>("Rotation_Helper/Camera");

            CurrentWeaponName = "UNARMED";
            ChangingWeaponName = "UNARMED";
        }

        public void ProcessInput(float delta, AnimationPlayerManager animationPlayerManager)
        {
            // Flashlight
            if (Input.IsActionPressed("flashlight"))
            {
                if (_flashlight.IsVisibleInTree())
                    _flashlight.Hide();
                else
                    _flashlight.Show();
            }

            // Changing weapons
            var weaponChangeNumber = WeaponNameToNumber[CurrentWeaponName];
            if (Input.IsKeyPressed((int) KeyList.Key1))
                weaponChangeNumber = 0;
            if (Input.IsKeyPressed((int) KeyList.Key2))
                weaponChangeNumber = 1;
            if (Input.IsKeyPressed((int) KeyList.Key3))
                weaponChangeNumber = 2;
            if (Input.IsKeyPressed((int) KeyList.Key4))
                weaponChangeNumber = 3;
            if (Input.IsActionJustPressed("shift_weapon_positive"))
                weaponChangeNumber += 1;
            if (Input.IsActionJustPressed("shift_weapon_negative"))
                weaponChangeNumber -= 1;
            weaponChangeNumber = Mathf.Clamp(weaponChangeNumber, 0, WeaponNumberToName.Count - 1);
            var weaponName = WeaponNumberToName[weaponChangeNumber];
            if (!ChangingWeapon && !_reloadingWeapon && weaponName != CurrentWeaponName)
            {
                ChangingWeaponName = weaponName;
                ChangingWeapon = true;
                _mouseScrollValue = weaponChangeNumber;
            }

            // Firing weapons
            if (Input.IsActionPressed("fire") && !ChangingWeapon)
            {
                if (CurrentWeapon != null)
                {
                    if (CurrentWeapon.AmmoInWeapon > 0)
                    {
                        if (animationPlayerManager.CurrentState == CurrentWeapon.IdleAnimationName)
                        {
                            animationPlayerManager.SetAnimation(CurrentWeapon.FireAnimationName);
                        }
                    }
                    else
                    {
                        _reloadingWeapon = true;
                    }
                }
            }

            // Grenades
            if (Input.IsActionJustPressed("change_grenade"))
                CurrentGrenadeName = CurrentGrenadeName == "Grenade" ? "Sticky Grenade" : "Grenade";
            if (Input.IsActionJustPressed("fire_grenade") && GrenadeAmounts[CurrentGrenadeName] > 0)
            {
                GrenadeAmounts[CurrentGrenadeName] -= 1;
                GrenadeBase grenadeClone = null;
                if (CurrentGrenadeName == "Grenade")
                {
                    grenadeClone = _grenadeScene.Instance<Grenade>();
                }
                else if (CurrentGrenadeName == "Sticky Grenade")
                {
                    grenadeClone = _stickyGrenadeScene.Instance<StickyGrenade>();
                    grenadeClone.PlayerBody = _player;
                }

                if (grenadeClone != null)
                {
                    _player.GetTree().Root.AddChild(grenadeClone);
                    grenadeClone.GlobalTransform =
                        _player.GetNode<Spatial>("Rotation_Helper/Grenade_Toss_Pos").GlobalTransform;
                    grenadeClone.ApplyImpulse(new Vector3(0, 0, 0),
                        grenadeClone.GlobalTransform.basis.z * GrenadeThrowForce);
                }
            }

            // Reloading weapons
            if (Input.IsActionJustPressed("reload") && !_reloadingWeapon && !ChangingWeapon)
            {
                var currentWeapon = Weapons[CurrentWeaponName];
                if (currentWeapon != null && currentWeapon.CanReload)
                {
                    var currentAnimationState = animationPlayerManager.CurrentState;
                    var isReloading = false;
                    foreach (var w in Weapons)
                    {
                        var weaponNode = w.Value;
                        if (weaponNode != null
                            && currentAnimationState != null
                            && currentAnimationState == weaponNode.ReloadingAnimationName)
                        {
                            isReloading = true;
                        }
                    }

                    if (!isReloading)
                        _reloadingWeapon = true;
                }
            }

            // Grabbing and throwing objects
            if (Input.IsActionJustPressed("fire") && CurrentWeaponName == "UNARMED")
            {
                if (GrabbedObject == null)
                {
                    var state = _player.GetWorld().DirectSpaceState;
                    var centerPos = _player.GetViewport().Size / 2;
                    var rayFrom = _camera.ProjectRayOrigin(centerPos);
                    var rayTo = rayFrom + _camera.ProjectRayNormal(centerPos) * ObjectGrabRayDistance;
                    var rayResult = state.IntersectRay(rayFrom, rayTo,
                        new Array {_player, _player.GetNode("Rotation_Helper/Gun_Fire_Points/Knife_Point/Area")});
                    if (rayResult.Count > 0 && rayResult["collider"] is RigidBody)
                    {
                        GrabbedObject = rayResult["collider"] as RigidBody;
                        GrabbedObject.Mode = RigidBody.ModeEnum.Static;
                        GrabbedObject.CollisionLayer = 0;
                        GrabbedObject.CollisionMask = 0;
                    }
                }
                else
                {
                    GrabbedObject.Mode = RigidBody.ModeEnum.Rigid;
                    GrabbedObject.ApplyImpulse(new Vector3(0, 0, 0),
                        -_camera.GlobalTransform.basis.z.Normalized() * ObjectThrowForce);
                    GrabbedObject.CollisionLayer = 1;
                    GrabbedObject.CollisionMask = 1;
                    GrabbedObject = null;
                }
            }

            if (GrabbedObject != null)
            {
                GrabbedObject.GlobalTransform = new Transform(
                    GrabbedObject.GlobalTransform.basis,
                    _camera.GlobalTransform.origin
                    + (-_camera.GlobalTransform.basis.z.Normalized()
                       * ObjectGrabDistance));
            }
        }

        public void ProcessChangingAndReloading()
        {
            if (ChangingWeapon)
            {
                bool unequipped;

                var currentWeapon = Weapons[CurrentWeaponName];
                if (currentWeapon == null)
                {
                    unequipped = true;
                }
                else
                {
                    unequipped = !currentWeapon.Enabled || currentWeapon.Unequip();
                }

                if (unequipped)
                {
                    bool weaponEquipped;
                    var weaponToEquip = Weapons[ChangingWeaponName];
                    if (weaponToEquip == null)
                    {
                        weaponEquipped = true;
                    }
                    else
                    {
                        weaponEquipped = weaponToEquip.Enabled || weaponToEquip.Equip();
                    }

                    if (weaponEquipped)
                    {
                        ChangingWeapon = false;
                        CurrentWeaponName = ChangingWeaponName;
                        ChangingWeaponName = "";
                    }
                }
            }

            if (_reloadingWeapon)
            {
                var currentWeapon = Weapons[CurrentWeaponName];
                currentWeapon?.Reload();
                _reloadingWeapon = false;
            }
        }

        public void ProcessScrollWheelInput(ButtonList button)
        {
            if (button != ButtonList.WheelUp && button != ButtonList.WheelDown)
                return;
            if (button == ButtonList.WheelUp) _mouseScrollValue += MouseSensitivityScrollWheel;
            if (button == ButtonList.WheelDown) _mouseScrollValue -= MouseSensitivityScrollWheel;
            _mouseScrollValue = Mathf.Clamp(_mouseScrollValue, 0, WeaponNumberToName.Count - 1);
            if (!ChangingWeapon && !_reloadingWeapon)
            {
                var roundMouseScrollValue = (int) (Mathf.Round(_mouseScrollValue));
                if (WeaponNumberToName[roundMouseScrollValue] != CurrentWeaponName)
                {
                    ChangingWeaponName = WeaponNumberToName[roundMouseScrollValue];
                    ChangingWeapon = true;
                    _mouseScrollValue = roundMouseScrollValue;
                }
            }
        }

        public void FireBullet()
        {
            if (ChangingWeapon) return;
            CurrentWeapon?.Fire();
        }

        public void AddAmmo(int additionalAmmo)
        {
            if (CurrentWeaponName == "UNARMED") return;
            if (!CurrentWeapon.CanRefill) return;
            CurrentWeapon.SpareAmmo += CurrentWeapon.AmmoInMag * additionalAmmo;
        }

        public void AddGrenades(int additionalGrenades)
        {
            CurrentGrenadeAmount += additionalGrenades;
            CurrentGrenadeAmount = Mathf.Clamp(CurrentGrenadeAmount, 0, 4);
        }
    }
}