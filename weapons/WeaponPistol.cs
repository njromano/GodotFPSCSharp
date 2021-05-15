using Godot;

namespace GodotFPS
{
	public class WeaponPistol : WeaponBase
	{
		public override int Damage => 15;
		public override int AmmoInMag => 10;
		public override bool CanReload => true;
		public override bool CanRefill => true;
		public override string ReloadingAnimationName => "Pistol_reload";
		public override string IdleAnimationName => "Pistol_idle";
		public override string FireAnimationName => "Pistol_fire";
		public override bool Enabled { get; set; }
		public override int AmmoInWeapon { get; set; } = 10;
		public override int SpareAmmo { get; set; } = 20;
		public override Player Player { get; set; }

		private PackedScene _bulletScene = GD.Load<PackedScene>("weapons/Bullet_Scene.tscn");

		public override void Fire()
		{
			var clone = _bulletScene.Instance<Bullet>();
			var sceneRoot = (Node) GetTree().Root.GetChildren()[0];
			sceneRoot.AddChild(clone);

			clone.GlobalTransform = GlobalTransform;
			clone.Scale = new Vector3(4, 4, 4);
			clone.BulletDamage = Damage;
			AmmoInWeapon -= 1;
			Player.CreateSound("Pistol_shot", GlobalTransform.origin);
		}

		public override bool Equip()
		{
			if (Player.AnimationManager.CurrentState == IdleAnimationName)
			{
				Enabled = true;
				return true;
			}

			if (Player.AnimationManager.CurrentState == "Idle_unarmed")
				Player.AnimationManager.SetAnimation("Pistol_equip");

			return false;
		}

		public override bool Unequip()
		{
			if (Player.AnimationManager.CurrentState == IdleAnimationName
				&& Player.AnimationManager.CurrentState != "Pistol_unequip")
				Player.AnimationManager.SetAnimation("Pistol_unequip");

			if (Player.AnimationManager.CurrentState != "Idle_unarmed")
				return false;

			Enabled = false;
			return true;
		}

		public override void Reset()
		{
			AmmoInWeapon = 10;
			SpareAmmo = 20;
		}
	}
}
