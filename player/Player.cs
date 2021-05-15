using Godot;
using GodotFPS.player_helpers;

namespace GodotFPS
{
	public class Player : KinematicBody
	{
		private const int MaxHealth = 150;
		
		private PlayerInputHelper _inputHelper = new PlayerInputHelper();
		private PlayerWeaponHelper _weaponHelper = new PlayerWeaponHelper();
		private PlayerHudHelper _hudHelper = new PlayerHudHelper();
		private PlayerCameraHelper _cameraHelper = new PlayerCameraHelper();
		private PlayerRespawnHelper _respawnHelper = new PlayerRespawnHelper();

		private Globals _globals;

		public int Health { get; set; } = 100;
		public bool IsDead { get; set; }
		public Camera Camera { get; set; }
		public AnimationPlayerManager AnimationManager { get; set; }

		public override void _Ready()
		{
			_inputHelper.OnReady(this);
			_weaponHelper.OnReady(this);
			_hudHelper.OnReady(this);
			_cameraHelper.OnReady(this);
			AnimationManager = GetNode<AnimationPlayerManager>("Rotation_Helper/Model/Animation_Player");
			AnimationManager.CallbackFunction = GD.FuncRef(this, nameof(FireBullet));
			_respawnHelper.OnReady(this);
			_globals = GetNode<Globals>("/root/Globals");
			Camera = GetNode<Camera>("Rotation_Helper/Camera");
		}

		public override void _PhysicsProcess(float delta)
		{
			if (!IsDead)
			{
				_inputHelper.ProcessInput(delta);
				_weaponHelper.ProcessInput(delta, AnimationManager);
				_inputHelper.ProcessMovement(delta);
				_cameraHelper.ProcessViewInput(delta);
			}

			_weaponHelper.ProcessChangingAndReloading();
			_hudHelper.ProcessHud(delta, _weaponHelper);
			_respawnHelper.ProcessRespawn(delta, _weaponHelper);
		}

		public override void _Input(InputEvent @event)
		{
			if (IsDead) return;
			if (Input.GetMouseMode() != Input.MouseMode.Captured) return;
			
			if (@event is InputEventMouseMotion motionEvent)
			{
				_cameraHelper.ProcessMouseInput(motionEvent.Relative);
			}

			if (@event is InputEventMouseButton buttonEvent)
			{
				_weaponHelper.ProcessScrollWheelInput((ButtonList)buttonEvent.ButtonIndex);
			}
		}

		public void BulletHit(int damage, Transform bulletHitPos)
		{
			Health -= damage;
		}

		public void FireBullet()
		{
			_weaponHelper.FireBullet();
		}

		public void AddHealth(int additionalHealth)
		{
			Health += additionalHealth;
			Health = Mathf.Clamp(Health, 0, MaxHealth);
		}

		public void AddAmmo(int additionalAmmo)
		{
			_weaponHelper.AddAmmo(additionalAmmo);
			_globals.PlaySound("Gun_cock", false, GlobalTransform.origin);
		}

		public void AddGrenade(int additionalGrenades)
		{
			_weaponHelper.AddGrenades(additionalGrenades);
		}
		
		public void CreateSound(string soundName, Vector3 position)
		{
			_globals.PlaySound(soundName, false, position);
		}
	}
}
