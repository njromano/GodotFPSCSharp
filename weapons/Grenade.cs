using Godot;
using Godot.Collections;

namespace GodotFPS
{
	public class Grenade : GrenadeBase
	{
		public const int GrenadeDamage = 60;
		public const float GrenadeTime = 2;
		private float _grenadeTimer = 0;

		public const float ExplosionWaitTime = 0.48f;
		private float _explosionWaitTimer = 0;

		private CollisionShape _rigidShape;
		private MeshInstance _grenadeMesh;
		private Area _blastArea;
		private Particles _explosionParticles;

		public override void _Ready()
		{
			_rigidShape = GetNode<CollisionShape>("Collision_Shape");
			_grenadeMesh = GetNode<MeshInstance>("Grenade");
			_blastArea = GetNode<Area>("Blast_Area");
			_explosionParticles = GetNode<Particles>("Explosion");
			_explosionParticles.Emitting = false;
			_explosionParticles.OneShot = true;
		}

		public override void _Process(float delta)
		{
			if (_grenadeTimer < GrenadeTime)
			{
				_grenadeTimer += delta;
				return;
			}

			if (_explosionWaitTimer <= 0)
			{
				_explosionParticles.Emitting = true;
				_grenadeMesh.Visible = false;
				_rigidShape.Disabled = true;
				Mode = ModeEnum.Static;
				var bodies = new Array<Spatial>(_blastArea.GetOverlappingBodies());
				foreach (var body in bodies)
				{
					if (!body.HasMethod("BulletHit")) continue;
					body.Call("BulletHit", GrenadeDamage,
						body.GlobalTransform.LookingAt(GlobalTransform.origin, 
							new Vector3(0, 1, 0)));
				}
				GetNode<Globals>("/root/Globals").PlaySound("Rifle_shot", false, GlobalTransform.origin);
			}

			if (_explosionWaitTimer >= ExplosionWaitTime) return;
			_explosionWaitTimer += delta;
			if (_explosionWaitTimer < ExplosionWaitTime) return;
			QueueFree();
		}
	}
}
