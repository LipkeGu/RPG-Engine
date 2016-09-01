using SDL2;

namespace RPGEngine
{
	public class Settings
	{
		public class EngineSettings
		{
			public int Width, Height, FontSize, Frequence;
			public string Map, FontFile, MapDirectory, ActorDirectory, FontDirectory, TilesetDirectory;
			public uint FPS, Size;
			public SDL.SDL_WindowFlags WindowFlags;
			public Worldtype Worldmode;
			public byte AudioChannels, Silence;
			public ushort Samples, format;
			
			public EngineSettings()
			{
				this.Worldmode = Worldtype.Normal;

				#region "Video"
				this.Width = 1024;
				this.Height = 768;
				this.FPS = 30;
				this.WindowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
				#endregion

				this.MapDirectory = "Data/Maps/";
				this.ActorDirectory = "Data/Actors/";
				this.FontDirectory = "Data/Fonts/";
				this.TilesetDirectory = "Data/Tilesets/";

				#region "Text"
				this.FontFile = "regular.ttf";
				this.FontSize = 12;
				#endregion

				#region "Audio"
				this.AudioChannels = 2;
				this.Samples = 4096;
				this.Frequence = 48000;
				this.format = 33056;
				this.Silence = byte.MinValue;
				this.Size = uint.MinValue;
				#endregion
			}
		}

		public class GameSettings
		{
			string title;
			public GameSettings()
			{
				this.title = "RPG-Engine";

			}

			public string Title
			{
				get { return this.title; }
			}
		}

		public class PlayerSettings
		{
			string name;
			uint health;

			public PlayerSettings()
			{
				this.name = "Brendan";
				this.health = 100;
			}

			public string Name
			{
				get { return this.name; }
				set { this.name = value; }
			}

			public uint Health
			{
				get { return this.health; }
				set { this.health = value; }
			}
		}

		public PlayerSettings Player;
		public GameSettings Game;
		public EngineSettings Engine;

		public Settings()
		{
			this.Engine = new EngineSettings();
			this.Game = new GameSettings();
			this.Player = new PlayerSettings();
		}
	}
}
