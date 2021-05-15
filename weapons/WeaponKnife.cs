using Godot;
using GodotFPS;

namespace DefaultNamespace
{
	public class WeaponKnife : WeaponBase
	{
		public override int Damage => 40;
		public override int AmmoInMag => 1;
		public override bool CanReload => false;
		public override bool CanRefill => false;
		public override string ReloadingAnimationName => "";
		public override string IdleAnimationName => "Knife_idle";
		public override string FireAnimationName => "Knife_fire";
		
		public override int AmmoInWeapon { get; set; } = 1;
		public override int SpareAmmo { get; set; } = 1;
		public override bool Enabled { get; set; } = false;

		public override Player Player { get; set; }

		public override void Fire()
		{
			var area = GetNode<Area>("Area");
			foreach (Node body in area.GetOverlappingBodies())
			{
				if (body == Player) continue;
				if (body is RigidBodyHitTest hit)
				{
					hit.BulletHit(Damage, area.GlobalTransform);
				}
			}
		}

		public override bool Equip()
		{
			if (Player.AnimationManager.CurrentState == IdleAnimationName)
			{
				Enabled = true;
				return true;
			}

			if (Player.AnimationManager.CurrentState == "Idle_unarmed")
				Player.AnimationManager.SetAnimation("Knife_equip");

			return false;
		}

		public override bool Unequip()
		{
			if (Player.AnimationManager.CurrentState == IdleAnimationName)
				Player.AnimationManager.SetAnimation("Knife_unequip");

			if (Player.AnimationManager.CurrentState != "Idle_unarmed") 
				return false;
			
			Enabled = false;
			return true;

		}

		public override bool Reload() => false;

		public override void Reset()
		{
			AmmoInWeapon = 1;
			SpareAmmo = 1;
		}
	}
}
