using Godot;

namespace GodotFPS
{
	public class AmmoPickup : Spatial
	{
		[Export(PropertyHint.Enum, "full size,small")]
		private int _kitSize;

		public int KitSize
		{
			get => _kitSize;
			set => KitSizeChange(value);
		}

		private readonly int[] _ammoAmounts = {4, 1};
		private readonly int[] _grenadeAmounts = {2, 0};
		private const int RespawnTime = 20;

		private bool _isReady;

		private float _respawnTimer;

		public override void _Ready()
		{
			GetNode("Holder/Ammo_Pickup_Trigger")
				.Connect("body_entered", this, nameof(TriggerBodyEntered));
			_isReady = true;
			KitSizeChangeValues(0, false);
			KitSizeChangeValues(1, false);
			KitSizeChangeValues(_kitSize, true);
		}

		public override void _PhysicsProcess(float delta)
		{
			if (_respawnTimer <= 0) return;
			_respawnTimer -= delta;
			if (_respawnTimer > 0) return;
			KitSizeChangeValues(_kitSize, true);
		}

		private void KitSizeChange(int value)
		{
			if (_isReady)
			{
				KitSizeChangeValues(_kitSize, false);
				_kitSize = value;
				KitSizeChangeValues(_kitSize, true);
			}
			else
			{
				_kitSize = value;
			}
		}

		private void KitSizeChangeValues(int size, bool enable)
		{
			if (size == 0)
			{
				GetNode<CollisionShape>("Holder/Ammo_Pickup_Trigger/Shape_Kit").Disabled = !enable;
				GetNode<Spatial>("Holder/Ammo_Kit").Visible = enable;
			}
			else
			{
				GetNode<CollisionShape>("Holder/Ammo_Pickup_Trigger/Shape_Kit_Small").Disabled = !enable;
				GetNode<Spatial>("Holder/Ammo_Kit_Small").Visible = enable;
			}
		}

		private void TriggerBodyEntered(Object body)
		{
			if (!(body is Player player)) return;
			player.AddAmmo(_ammoAmounts[_kitSize]);
			player.AddGrenade(_grenadeAmounts[_kitSize]);
			_respawnTimer = RespawnTime;
			KitSizeChangeValues(_kitSize, false);
		}
	}
}
