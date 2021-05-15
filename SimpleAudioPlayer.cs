using System.Linq;
using Godot;

namespace GodotFPS
{
	public class SimpleAudioPlayer : Spatial
	{
		public bool ShouldLoop { get; set; }
		private AudioStreamPlayer3D _audioNode;
		private Globals _globals;

		public override void _Ready()
		{
			_audioNode = GetNode<AudioStreamPlayer3D>("Audio_Stream_Player");
			_audioNode.Connect("finished", this, nameof(SoundFinished));
			_audioNode.Stop();
			_globals = GetNode<Globals>("/root/Globals");
		}

		public void PlaySound(AudioStream audioStream, Vector3 position)
		{
			if (audioStream == null)
			{
				GD.Print("No audio stream passed; cannot play sound");
				_globals.CreatedAudio.Remove(_globals.CreatedAudio.FirstOrDefault(a => a == this));
				QueueFree();
				return;
			}

			_audioNode.Stream = audioStream;
			_audioNode.GlobalTransform = new Transform(_audioNode.GlobalTransform.basis, position);
			_audioNode.Play();
		}

		private void SoundFinished()
		{
			if (ShouldLoop)
			{
				_audioNode.Play();
				return;
			}

			var audioIndex = _globals.CreatedAudio.FirstOrDefault(a => a == this);
			if (audioIndex != null) _globals.CreatedAudio.Remove(this);
			_audioNode.Stop();
			QueueFree();
		}
	}
}
