using System;
using System.Drawing;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Game
	{
		private Video video;
		private Audio audio;
		private Text text;
		private Settings config;
		private UserInferface ui;
		private World world;
		private SDL_Event events;

		public Game(ref Video video, ref Text text, ref Audio audio, ref Settings config, ref UserInferface ui)
		{
			this.video = video;
			this.text = text;
			this.audio = audio;
			this.config = config;
			this.ui = ui;
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
			var stext = string.Empty;

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
					throw new InvalidOperationException("{0}: {1}".F(source[source.Length - 1], msg));
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

		public void Events(ref bool running)
		{
			while (SDL_PollEvent(out this.events) != 0)
			{
				switch (this.events.type)
				{
					case SDL_EventType.SDL_QUIT:
						running = false;
						break;
					case SDL_EventType.SDL_KEYDOWN:
					case SDL_EventType.SDL_KEYUP:
						this.world.Events(ref this.events);
						this.ui.Events(ref this.events);
						break;
					case SDL_EventType.SDL_MOUSEMOTION:
						Engine.MousePosition.X = this.events.motion.x;
						Engine.MousePosition.Y = this.events.motion.y;
						break;
					default:
						break;
				}
			}
		}

		public int Start()
		{
			this.world = new World(this.config.Player.Name, this.config.Engine.MapDirectory, 
				this.config.Engine.ActorDirectory, this.video.Renderer, this.WindowSize());

			return this.world != null ? 0 : -1;
		}

		public void Update()
		{
			this.world.Update();
			this.ui.Update();
		}

		public void Render(IntPtr renderer)
		{
			var retval = -1;

			retval = this.video.Begin(Color.Black);

			retval = this.world.Render(this.WindowSize(), this.video.WindowSurface, ref renderer);

			retval = this.ui.Render(this.video.WindowSurface, ref renderer, Color.BlanchedAlmond);

			this.video.End();
		}

		public void Close()
		{
			if (this.world != null)
				this.world.Close();

			if (this.ui != null)
				this.ui.Close();
		}

		public Vector2<int> WindowSize()
		{
			var size = new Vector2<int>(0, 0);

			SDL_GetWindowSize(this.video.Window, out size.X, out size.Y);
			return size;
		}
	}
}
