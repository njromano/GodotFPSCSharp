using Godot;

namespace GodotFPS
{
	public class DebugDisplay : Control
	{
		public override void _Ready()
		{
			GetNode<Label>("OS_Label").Text = $"OS: {OS.GetName()}";
			GetNode<Label>("Engine_Label").Text = $"Godot version: {Engine.GetVersionInfo()["string"]}";
		}

		public override void _Process(float delta)
		{
			GetNode<Label>("FPS_Label").Text = $"FPS: {Engine.GetFramesPerSecond()}";
		}
	}
}
