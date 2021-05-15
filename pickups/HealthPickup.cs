using Godot;

namespace GodotFPS
{
	public class HealthPickup : Spatial
	{
		[Export(PropertyHint.Enum, "Full Size, Small")]
		public int KitSize
		{
			get => _kitSize;
			set => KitSizeChange(value);
		}

		private int _kitSize;

		private const float RespawnTime = 20f;
		private readonly int[] _healthAmounts = {70, 30};

		private float _respawnTimer;
		private bool _isReady;

		public void TriggerBodyEntered(Spatial body)
		{
			if (!(body is Player player)) return;
			player.AddHealth(_healthAmounts[_kitSize]);
			_respawnTimer = RespawnTime;
			KitSizeChangeValues(_kitSize, false);
			GetNode<Globals>("/root/Globals").PlaySound("Gun_cock", false, GlobalTransform.origin);
		}

		public override void _Ready()
		{
			GetNode<Area>("Holder/Health_Pickup_Trigger").Connect("body_entered", this, nameof(TriggerBodyEntered));
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
			if (!_isReady)
			{
				_kitSize = value;
				return;
			}

			KitSizeChangeValues(_kitSize, false);
			_kitSize = value;
			KitSizeChangeValues(_kitSize, true);
		}

		private void KitSizeChangeValues(int size, bool enable)
		{
			switch (size)
			{
				case 0:
					GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit").Disabled = !enable;
					GetNode<Spatial>("Holder/Health_Kit").Visible = enable;
					break;
				case 1:
					GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit_Small").Disabled = !enable;
					GetNode<Spatial>("Holder/Health_Kit_Small").Visible = enable;
					break;
			}
		}
	}
}
