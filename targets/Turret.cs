using Godot;

namespace GodotFPS
{
	public class Turret : Spatial
	{
		[Export] public bool UseRaycast { get; set; }

		private const int TurretDamageBullet = 20;
		private const int TurretDamageRaycast = 5;
		private const float FlashTime = 0.1f;
		private const float FireTime = 0.8f;
		private const int AmmoInFullTurret = 20;
		private const float AmmoReloadTime = 4;
		private const float PlayerHeight = 3;
		private const int MaxTurretHealth = 60;
		private const float DestroyedTime = 20;

		private float _flashTimer;
		private float _fireTimer;

		private Spatial _nodeTurretHead;
		private RayCast _nodeRayCast;
		private Spatial _nodeFlashOne;
		private Spatial _nodeFlashTwo;
		private Spatial _currentTarget;
		private Particles _smokeParticles;
		private PackedScene _bulletScene = GD.Load<PackedScene>("weapons/Bullet_Scene.tscn");

		private int _ammoInTurret = 20;
		private float _ammoReloadTimer;

		private bool _isActive;

		private int _turretHealth = 60;
		private float _destroyedTimer;

		public override void _Ready()
		{
			GetNode("Vision_Area").Connect("body_entered", this, nameof(BodyEnteredVision));
			GetNode("Vision_Area").Connect("body_exited", this, nameof(BodyExitedVision));
			_nodeTurretHead = GetNode<Spatial>("Head");
			_nodeRayCast = GetNode<RayCast>("Head/Ray_Cast");
			_nodeFlashOne = GetNode<Spatial>("Head/Flash");
			_nodeFlashTwo = GetNode<Spatial>("Head/Flash_2");
			_nodeRayCast.AddException(this);
			_nodeRayCast.AddException(GetNode("Base/Static_Body"));
			_nodeRayCast.AddException(GetNode("Head/Static_Body"));
			_nodeRayCast.AddException(GetNode("Vision_Area"));
			_smokeParticles = GetNode<Particles>("Smoke");
			_smokeParticles.Emitting = false;
			_turretHealth = MaxTurretHealth;
		}

		public override void _PhysicsProcess(float delta)
		{
			if (_isActive)
			{
				if (_flashTimer > 0)
				{
					_flashTimer -= delta;
					if (_flashTimer <= 0)
					{
						_nodeFlashOne.Visible = false;
						_nodeFlashTwo.Visible = false;
					}
				}

				if (_currentTarget != null)
				{
					_nodeTurretHead.LookAt(_currentTarget.GlobalTransform.origin
										   + new Vector3(0, PlayerHeight, 0),
						new Vector3(0, 1, 0));
					if (_turretHealth > 0)
					{
						if (_ammoInTurret > 0)
						{
							if (_fireTimer > 0) _fireTimer -= delta;
							else FireBullet();
						}
						else
						{
							if (_ammoReloadTimer > 0) _ammoReloadTimer -= delta;
							else _ammoInTurret = AmmoInFullTurret;
						}
					}
				}
			}

			if (_turretHealth > 0) return;

			if (_destroyedTimer > 0)
			{
				_destroyedTimer -= delta;
			}
			else
			{
				_turretHealth = MaxTurretHealth;
				_smokeParticles.Emitting = false;
			}
		}

		public void BulletHit(int damage, Transform bulletHitPos)
		{
			_turretHealth -= damage;
			if (_turretHealth > 0) return;
			_smokeParticles.Emitting = true;
			_destroyedTimer = DestroyedTime;
		}

		private void FireBullet()
		{
			if (UseRaycast)
			{
				_nodeRayCast.LookAt(_currentTarget.GlobalTransform.origin
									+ new Vector3(0, PlayerHeight, 0), new Vector3(0, 1, 0));
				_nodeRayCast.ForceRaycastUpdate();
				if (_nodeRayCast.IsColliding())
				{
					var body = _nodeRayCast.GetCollider();
					if (body.HasMethod("BulletHit"))
					{
						body.Call("BulletHit", TurretDamageRaycast, _nodeRayCast.GetCollisionPoint());
					}
				}
			}
			else
			{
				var clone = _bulletScene.Instance<Bullet>();
				var sceneRoot = GetTree().Root.GetChildren()[0] as Node;
				if (sceneRoot == null) return;
				sceneRoot.AddChild(clone);
				clone.GlobalTransform = GetNode<Spatial>("Head/Barrel_End").GlobalTransform;
				clone.Scale = new Vector3(8, 8, 8);
				clone.BulletDamage = TurretDamageBullet;
				clone.BulletSpeed = 60;
			}

			GetNode<Globals>("/root/Globals").PlaySound("Pistol_shot", false, GlobalTransform.origin);
			_ammoInTurret -= 1;

			_nodeFlashOne.Visible = true;
			_nodeFlashTwo.Visible = true;

			_flashTimer = FlashTime;
			_fireTimer = FireTime;

			if (_ammoInTurret <= 0)
				_ammoReloadTimer = AmmoReloadTime;
		}

		private void BodyEnteredVision(Spatial body)
		{
			if (_currentTarget != null || !(body is KinematicBody)) return;
			_currentTarget = body;
			_isActive = true;
		}

		private void BodyExitedVision(Spatial body)
		{
			if (_currentTarget == null || body != _currentTarget) return;
			_currentTarget = null;
			_isActive = false;
			_flashTimer = 0;
			_fireTimer = 0;
			_nodeFlashOne.Visible = false;
			_nodeFlashTwo.Visible = false;
		}
	}
}
