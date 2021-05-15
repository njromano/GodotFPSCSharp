using Godot;
using Godot.Collections;

namespace GodotFPS
{
	public class Globals : Node
	{
		public double MouseSensitivity = 0.08f;
		public double JoypadSensitivity = 2f;

		public CanvasLayer CanvasLayer = null;
		public PackedScene DebugDisplayScene = GD.Load<PackedScene>("menus/Debug_Display.tscn");
		public Control DebugDisplay;

		public const string MainMenuPath = "res://menus/Main_Menu.tscn";

		public PackedScene PopupScene = GD.Load<PackedScene>("menus/Pause_Popup.tscn");
		public WindowDialog Popup = null;

		public Array<Spatial> RespawnPoints = null;

		public Dictionary<string, AudioStream> AudioClips = new Dictionary<string, AudioStream>
		{
			{"Pistol_shot", GD.Load<AudioStream>("res://assets/audio/gun_revolver_pistol_shot_04.wav")},
			{"Rifle_shot", GD.Load<AudioStream>("res://assets/audio/gun_rifle_sniper_shot_01.wav")},
			{"Gun_cock", GD.Load<AudioStream>("res://assets/audio/gun_semi_auto_rifle_cock_02.wav")}
		};

		public PackedScene SimpleAudioPlayerScene = GD.Load<PackedScene>("res://Simple_Audio_Player.tscn");
		public Array<Node> CreatedAudio = new Array<Node>();

		public override void _Ready()
		{
			CanvasLayer = new CanvasLayer();
			AddChild(CanvasLayer);
			GD.Randomize();
		}

		public override void _Process(float delta)
		{
			if (!Input.IsActionJustPressed("ui_cancel") || Popup != null)
				return;
			Popup = PopupScene.Instance<WindowDialog>();
			Popup.GetNode("Button_quit").Connect("pressed", this, nameof(PopupQuit));
			Popup.Connect("popup_hide", this, nameof(PopupClosed));
			Popup.GetNode("Button_resume").Connect("pressed", this, nameof(PopupClosed));
			CanvasLayer.AddChild(Popup);
			Popup.PopupCentered();
			Input.SetMouseMode(Input.MouseMode.Visible);
			GetTree().Paused = true;
		}

		public void LoadNewScene(string newScenePath)
		{
			GetTree().ChangeScene(newScenePath);
			RespawnPoints = null;
			foreach (var sound in CreatedAudio)
			{
				sound?.QueueFree();
			}

			CreatedAudio = new Array<Node>();
		}

		public void SetDebugDisplay(bool displayOn)
		{
			if (displayOn && DebugDisplay == null)
			{
				DebugDisplay = DebugDisplayScene.Instance<Control>();
				CanvasLayer.AddChild(DebugDisplay);
			}
			else if (DebugDisplay != null)
			{
				DebugDisplay.QueueFree();
				DebugDisplay = null;
			}
		}

		public Vector3 GetRespawnPosition()
		{
			if (RespawnPoints == null) return new Vector3(0, 0, 0);
			var respawnPoint = (int) GD.RandRange(0, RespawnPoints.Count - 1);
			return RespawnPoints[respawnPoint].GlobalTransform.origin;
		}

		private void PopupClosed()
		{
			GetTree().Paused = false;
			if (Popup == null) return;
			Popup.QueueFree();
			Popup = null;
		}

		private void PopupQuit()
		{
			GetTree().Paused = false;
			Input.SetMouseMode(Input.MouseMode.Visible);
			if (Popup != null)
			{
				Popup.QueueFree();
				Popup = null;
			}

			LoadNewScene(MainMenuPath);
		}

		public void PlaySound(string soundName, bool loopSound, Vector3 soundPosition)
		{
			if (AudioClips.ContainsKey(soundName))
			{
				var newAudio = SimpleAudioPlayerScene.Instance<SimpleAudioPlayer>();
				newAudio.ShouldLoop = loopSound;
				AddChild(newAudio);
				newAudio.PlaySound(AudioClips[soundName], soundPosition);
			}
			else
				GD.Print("ERROR: cannot play sound that does not exist in audio_clips!");
		}
	}
}
