using Godot;
using Godot.Collections;

namespace GodotFPS
{
	public class Spawn_Points : Spatial
	{
		public override void _Ready()
		{
			var globals = GetNode<Globals>("/root/Globals");
			globals.RespawnPoints = new Array<Spatial>(GetChildren());
		}
	}
}
