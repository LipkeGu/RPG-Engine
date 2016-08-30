namespace RPGEngine
{
	public class Settings
	{
		public class EngineSettings
		{
			public int Width, Height;
			public string Map, FontFile, MapDirectory, ActorDirectory, FontDirectory;
			public int FontSize;
			public uint FPS;
			public SDL2.SDL.SDL_WindowFlags WindowFlags;
			public Worldtype Worldmode;

			public EngineSettings()
			{
				this.Worldmode = Worldtype.Normal;
				this.Width = 1024;
				this.Height = 768;
				this.FPS = 60;
				this.WindowFlags = SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

				this.MapDirectory = "Data/Maps/";
				this.ActorDirectory = "Data/Actors/";
				this.FontDirectory = "Data/Fonts/";
				this.FontFile = "regular.ttf";
				this.FontSize = 12;
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
			public string Name;
			public PlayerSettings()
			{
				this.Name = "Brendan";
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
