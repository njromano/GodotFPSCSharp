using Godot;
using Array = Godot.Collections.Array;

namespace GodotFPS
{
	public class MainMenu : Control
	{
		[Export(PropertyHint.File)] public string TestingAreaScene { get; set; }
		[Export(PropertyHint.File)] public string SpaceAreaScene { get; set; }
		[Export(PropertyHint.File)] public string RuinsAreaScene { get; set; }

		private Panel _startMenu;
		private Panel _levelSelectMenu;
		private Panel _optionsMenu;

		public override void _Ready()
		{
			_startMenu = GetNode<Panel>("Start_Menu");
			_levelSelectMenu = GetNode<Panel>("Level_Select_Menu");
			_optionsMenu = GetNode<Panel>("Options_Menu");

			GetNode("Start_Menu/Button_Start").Connect("pressed", this,
				nameof(StartMenuButtonPressed), new Array {"start"});
			GetNode("Start_Menu/Button_Open_Godot").Connect("pressed", this,
				nameof(StartMenuButtonPressed), new Array {"open_godot"});
			GetNode("Start_Menu/Button_Options").Connect("pressed", this,
				nameof(StartMenuButtonPressed), new Array {"options"});
			GetNode("Start_Menu/Button_Quit").Connect("pressed", this,
				nameof(StartMenuButtonPressed), new Array {"quit"});

			GetNode("Level_Select_Menu/Button_Back").Connect("pressed", this,
				nameof(LevelSelectMenuButtonPressed), new Array {"back"});
			GetNode("Level_Select_Menu/Button_Level_Testing_Area").Connect("pressed", this,
				nameof(LevelSelectMenuButtonPressed), new Array {"testing_scene"});
			GetNode("Level_Select_Menu/Button_Level_Space").Connect("pressed", this,
				nameof(LevelSelectMenuButtonPressed), new Array {"space_level"});
			GetNode("Level_Select_Menu/Button_Level_Ruins").Connect("pressed", this,
				nameof(LevelSelectMenuButtonPressed), new Array {"ruins_level"});

			GetNode("Options_Menu/Button_Back").Connect("pressed", this,
				nameof(OptionsMenuButtonPressed), new Array {"back"});
			GetNode("Options_Menu/Button_Fullscreen").Connect("pressed", this,
				nameof(OptionsMenuButtonPressed), new Array {"fullscreen"});
			GetNode("Options_Menu/Check_Button_VSync").Connect("pressed", this,
				nameof(OptionsMenuButtonPressed), new Array {"vsync"});
			GetNode("Options_Menu/Check_Button_Debug").Connect("pressed", this,
				nameof(OptionsMenuButtonPressed), new Array {"debug"});

			Input.SetMouseMode(Input.MouseMode.Visible);

			var globals = GetNode<Globals>("/root/Globals");
			GetNode<HSlider>("Options_Menu/HSlider_Mouse_Sensitivity").Value = globals.MouseSensitivity;
			GetNode<HSlider>("Options_Menu/HSlider_Joypad_Sensitivity").Value = globals.JoypadSensitivity;
		}

		private void StartMenuButtonPressed(string buttonName)
		{
			switch (buttonName)
			{
				case "start":
					_levelSelectMenu.Visible = true;
					_startMenu.Visible = false;
					break;
				case "open_godot":
					OS.ShellOpen("https://godotengine.org/");
					break;
				case "options":
					_optionsMenu.Visible = true;
					_startMenu.Visible = false;
					break;
				case "quit":
					GetTree().Quit();
					break;
			}
		}

		private void LevelSelectMenuButtonPressed(string buttonName)
		{
			if (buttonName == "back")
			{
				_startMenu.Visible = true;
				_levelSelectMenu.Visible = false;
				return;
			}

			SetMouseAndJoypadSensitivity();
			var globals = GetNode<Globals>("/root/Globals");
			
			switch (buttonName)
			{
				case "testing_scene":
					globals.LoadNewScene(TestingAreaScene);
					break;
				case "space_level":
					globals.LoadNewScene(SpaceAreaScene);
					break;
				case "ruins_level":
					globals.LoadNewScene(RuinsAreaScene);
					break;
			}
		}

		public void OptionsMenuButtonPressed(string buttonName)
		{
			switch (buttonName)
			{
				case "back":
					_startMenu.Visible = true;
					_optionsMenu.Visible = false;
					break;
				case "fullscreen":
					OS.WindowFullscreen = !OS.WindowFullscreen;
					break;
				case "vsync":
					OS.VsyncEnabled = GetNode<Button>("Options_Menu/Check_Button_VSync").Pressed;
					break;
				case "debug":
					var globals = GetNode<Globals>("/root/Globals");
					var pressed = GetNode<Button>("Options_Menu/Check_Button_Debug").Pressed;
					globals.SetDebugDisplay(pressed);
					break;
			}
		}

		public void SetMouseAndJoypadSensitivity()
		{
			var globals = GetNode<Globals>("/root/Globals");
			globals.MouseSensitivity = GetNode<HSlider>("Options_Menu/HSlider_Mouse_Sensitivity").Value;
			globals.JoypadSensitivity = GetNode<HSlider>("Options_Menu/HSlider_Joypad_Sensitivity").Value;
		}
	}
}
