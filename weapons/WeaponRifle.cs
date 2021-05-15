using Godot;

namespace GodotFPS
{
	public class WeaponRifle : WeaponBase
	{
		public override int Damage => 4;
		public override int AmmoInMag => 50;
		public override bool CanReload => true;
		public override bool CanRefill => true;
		public override string ReloadingAnimationName => "Rifle_reload";
		public override string IdleAnimationName => "Rifle_idle";
		public override string FireAnimationName => "Rifle_fire";
		public override bool Enabled { get; set; }
		public override int AmmoInWeapon { get; set; } = 50;
		public override int SpareAmmo { get; set; } = 100;
		public override Player Player { get; set; }

		public override void Fire()
		{
			var ray = GetNode<RayCast>("Ray_Cast");
			ray.ForceRaycastUpdate();
			AmmoInWeapon -= 1;

			if (ray.IsColliding())
			{
				var body = ray.GetCollider();
				if (body is RigidBodyHitTest rigidHit)
				{
					rigidHit.BulletHit(Damage, ray.GlobalTransform);
				}
				else if (body is TurretBodies turretHit)
				{
					turretHit.BulletHit(Damage, ray.GlobalTransform);
				}
				else if (body is Target targetHit)
				{
					targetHit.BulletHit(Damage, ray.GlobalTransform);
				}
			}

			Player.CreateSound("Rifle_shot", ray.GlobalTransform.origin);
		}

		public override bool Equip()
		{
			var animationState = Player.AnimationManager.CurrentState;
			if (animationState == IdleAnimationName)
			{
				Enabled = true;
				return true;
			}

			if (animationState == "Idle_unarmed")
				Player.AnimationManager.SetAnimation("Rifle_equip");

			return false;
		}

		public override bool Unequip()
		{
			var animationState = Player.AnimationManager.CurrentState;
			if (animationState == IdleAnimationName
				&& animationState != "Rifle_unequip")
				Player.AnimationManager.SetAnimation("Rifle_unequip");

			if (animationState != "Idle_unarmed") return false;
			
			Enabled = false;
			return true;

		}

		public override void Reset()
		{
			AmmoInWeapon = 50;
			SpareAmmo = 100;
		}
	}
}
