using Godot;
using Godot.Collections;

namespace GodotFPS
{
	public class Target : StaticBody
	{
		[Export] public PackedScene DestroyedTarget { get; set; }

		private const int TargetHealth = 40;
		private const float TargetRespawnTime = 14;
		private float _targetRespawnTimer;

		private int _currentHealth = 40;
		private Spatial _brokenTargetHolder;
		private CollisionShape _targetCollisionShape;

		public override void _Ready()
		{
			_brokenTargetHolder = GetNode<Spatial>("../Broken_Target_Holder");
			_targetCollisionShape = GetNode<CollisionShape>("Collision_Shape");
		}

		public override void _PhysicsProcess(float delta)
		{
			if (_targetRespawnTimer <= 0) return;
			_targetRespawnTimer -= delta;
			if (_targetRespawnTimer > 0) return;
			foreach (var child in new Array<Node>(_brokenTargetHolder.GetChildren()))
			{
				child.QueueFree();
			}

			_targetCollisionShape.Disabled = false;
			Visible = true;
			_currentHealth = TargetHealth;
		}

		public void BulletHit(int damage, Transform bulletTransform)
		{
			_currentHealth -= damage;
			if (_currentHealth > 0) return;
			var clone = DestroyedTarget.Instance();
			_brokenTargetHolder.AddChild(clone);
			foreach (var rigid in clone.GetChildren())
			{
				if (!(rigid is RigidBody body)) continue;
				var centerRigidSpace = _brokenTargetHolder.GlobalTransform.origin - body.GlobalTransform.origin;
				var direction = (body.Transform.origin - centerRigidSpace).Normalized();
				body.ApplyImpulse(centerRigidSpace, direction * 12 * damage);
			}

			_targetRespawnTimer = TargetRespawnTime;
			_targetCollisionShape.Disabled = true;
			Visible = false;
		}
	}
}
