using Godot;

namespace GodotFPS
{
	public class RigidBodyHitTest : RigidBody
	{
		private const int BaseBulletBoost = 9;

		public void BulletHit(int damage, Transform bulletGlobalTransform)
		{
			var directionVector = bulletGlobalTransform.basis.z.Normalized() * BaseBulletBoost;
			var impulsePosition = (bulletGlobalTransform.origin - GlobalTransform.origin).Normalized();
			var impulseDirection = directionVector * damage;
			ApplyImpulse(impulsePosition, impulseDirection);
		}
	}
}
