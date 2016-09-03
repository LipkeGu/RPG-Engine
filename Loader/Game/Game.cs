using System;
using System.Drawing;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Game
	{
		Video video;
		Audio audio;
		Text text;
		Settings config;

		static string stext;
		World world;
		SDL_Event events;

		public Game(Video video, Text text, Audio audio, Settings config)
		{
			this.video = video;
			this.text = text;
			this.audio = audio;
			this.config = config;
		}

		public void Events(ref bool running)
		{
			while(SDL_PollEvent(out this.events) != 0)
			{
				switch (this.events.type)
				{
					case SDL_EventType.SDL_QUIT:
						running = false;
						break;
					case SDL_EventType.SDL_KEYDOWN:
					case SDL_EventType.SDL_KEYUP:
						this.world.Events(this.events);
						break;
					default:
						break;
				}
			}
		}

		public int Start()
		{
			this.world = new World(
				this.config.Player.Name,
				this.config.Engine.MapDirectory,
				this.config.Engine.ActorDirectory,
				this.video.Renderer,
				this.WindowSize(),
				this.config.Engine.Worldmode);

			return world != null ? 0 : -1;
		}

		public void Update()
		{
			this.world.Update();
		}

		public void Render(IntPtr renderer)
		{
			this.video.Begin(Color.Black);

			this.world.Render(this.WindowSize(), this.video.WindowSurface, ref renderer);
		
			this.video.End();
		}

		public void Close()
		{
			if (world != null)
				this.world.Close();
		}

		/// <summary>
		/// Prints Output to the IDE or Console
		/// </summary>
		/// <param name="src">Source</param>
		/// <param name="msg">Message</param>
		/// <param name="fmt">Arguments (like int etc)</param>
		public static void Print(LogType type, string src, string msg)
		{
			var source = src.Split('.');
			switch (type)
			{
				case LogType.Info:
					Console.ForegroundColor = ConsoleColor.White;
					stext = "[I]: {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Warn:
					Console.ForegroundColor = ConsoleColor.Yellow;
					stext = "[W]: {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					stext = "[E]: {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Debug:
					Console.ForegroundColor = ConsoleColor.White;
					stext = "[D]: {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Notice:
					stext = "[N]: {0}: {1}".F(source[source.Length - 1], msg);
					break;
				default:
					stext = "{0}: {1}".F(source[source.Length - 1], msg);
					break;
			}

			Console.WriteLine(stext);
		}

		public Vector2<int> WindowSize()
		{
			var w = 0;
			var h = 0;

			SDL_GetWindowSize(this.video.Window, out w, out h);
			return new Vector2<int>(w, h);
		}


	}
}
