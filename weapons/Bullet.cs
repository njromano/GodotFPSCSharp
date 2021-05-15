using Godot;

namespace GodotFPS
{
	public class Bullet : Spatial
	{
		public int BulletSpeed = 200;
		public int BulletDamage = 15;
		private const float KillTimer = 4;
		private float _timer = 0;
		private bool _hitSomething = false;

		public override void _Ready()
		{
			GetNode("Area").Connect("body_entered", this, nameof(Collided));
		}

		public override void _PhysicsProcess(float delta)
		{
			var forwardDir = GlobalTransform.basis.z.Normalized();
			GlobalTranslate(forwardDir * BulletSpeed * delta);
			_timer += delta;
			if (_timer >= KillTimer)
				QueueFree();
		}

		private void Collided(Object body)
		{
			if (!_hitSomething && body.HasMethod("BulletHit"))
				body.Call("BulletHit", BulletDamage, GlobalTransform);
			_hitSomething = true;
			QueueFree();
		}
	}
}
