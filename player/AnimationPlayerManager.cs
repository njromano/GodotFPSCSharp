using System.Linq;
using Godot;
using Godot.Collections;

namespace GodotFPS
{
	public class AnimationPlayerManager : AnimationPlayer
	{
		private readonly Dictionary<string, string[]> _states = new Dictionary<string, string[]>
		{
			{"Idle_unarmed", new[] {"Knife_equip", "Pistol_equip", "Rifle_equip", "Idle_unarmed"}},
			{"Pistol_equip", new[] {"Pistol_idle"}},
			{"Pistol_fire", new[] {"Pistol_idle"}},
			{"Pistol_idle", new[] {"Pistol_fire", "Pistol_reload", "Pistol_unequip", "Pistol_idle"}},
			{"Pistol_reload", new[] {"Pistol_idle"}},
			{"Pistol_unequip", new[] {"Idle_unarmed"}},

			{"Rifle_equip", new[] {"Rifle_idle"}},
			{"Rifle_fire", new[] {"Rifle_idle"}},
			{"Rifle_idle", new[] {"Rifle_fire", "Rifle_reload", "Rifle_unequip", "Rifle_idle"}},
			{"Rifle_reload", new[] {"Rifle_idle"}},
			{"Rifle_unequip", new[] {"Idle_unarmed"}},

			{"Knife_equip", new[] {"Knife_idle"}},
			{"Knife_fire", new[] {"Knife_idle"}},
			{"Knife_idle", new[] {"Knife_fire", "Knife_unequip", "Knife_idle"}},
			{"Knife_unequip", new[] {"Idle_unarmed"}},
		};

		private readonly Dictionary<string, float> _animationSpeeds = new Dictionary<string, float>
		{
			{"Idle_unarmed", 1},
			{"Pistol_equip", 1.4f},
			{"Pistol_fire", 1.8f},
			{"Pistol_idle", 1},
			{"Pistol_reload", 1},
			{"Pistol_unequip", 1.4f},
			{"Rifle_equip", 2},
			{"Rifle_fire", 6},
			{"Rifle_idle", 1},
			{"Rifle_reload", 1.45f},
			{"Rifle_unequip", 2},
			{"Knife_equip", 1},
			{"Knife_fire", 1.35f},
			{"Knife_idle", 1},
			{"Knife_unequip", 1},
		};

		public string CurrentState = null;

		public FuncRef CallbackFunction = null;

		public override void _Ready()
		{
			SetAnimation("Idle_unarmed");
			Connect("animation_finished", this, nameof(AnimationEnded));
		}

		public bool SetAnimation(string animationName)
		{
			if (animationName == CurrentState)
			{
				GD.Print("AnimationPlayer_Manager.cs -- WARNING: Animation is already ", animationName);
				return true;
			}

			if (!HasAnimation(animationName)) return false;

			if (CurrentState != null)
			{
				var possibleAnimations = _states[CurrentState];
				if (possibleAnimations.Contains(animationName))
				{
					CurrentState = animationName;
					Play(animationName, -1, _animationSpeeds[animationName]);
					return true;
				}

				GD.Print("AnimationPlayer_Manager.gd -- WARNING: Cannot change to ", animationName, " from ",
					CurrentState);
				return false;
			}

			CurrentState = animationName;
			Play(animationName, -1, _animationSpeeds[animationName]);
			return true;
		}

		private void AnimationEnded(string animationName)
		{
			switch (CurrentState)
			{
				// UNARMED
				case "Idle_unarmed":
					return;
				// KNIFE
				case "Knife_equip":
					SetAnimation("Knife_idle");
					break;
				case "Knife_idle":
					return;
				case "Knife_fire":
					SetAnimation("Knife_idle");
					break;
				// PISTOL
				case "Knife_unequip":
					SetAnimation("Idle_unarmed");
					break;
				case "Pistol_equip":
					SetAnimation("Pistol_idle");
					break;
				case "Pistol_idle":
					return;
				case "Pistol_fire":
					SetAnimation("Pistol_idle");
					break;
				case "Pistol_unequip":
					SetAnimation("Idle_unarmed");
					break;
				// RIFLE transitions
				case "Pistol_reload":
					SetAnimation("Pistol_idle");
					break;
				case "Rifle_equip":
					SetAnimation("Rifle_idle");
					break;
				case "Rifle_idle":
					return;
				case "Rifle_fire":
					SetAnimation("Rifle_idle");
					break;
				case "Rifle_unequip":
					SetAnimation("Idle_unarmed");
					break;
				case "Rifle_reload":
					SetAnimation("Rifle_idle");
					break;
			}
		}

		private void AnimationCallback()
		{
			if (CallbackFunction == null)
				GD.Print("AnimationPlayer_Manager.gd -- WARNING: No callback function for the animation to call!");
			else
				CallbackFunction.CallFunc();
		}
	}
}
