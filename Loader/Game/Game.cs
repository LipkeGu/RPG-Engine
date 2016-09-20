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

		private int curCursorPosition_X;
		private int curCursorPosition_Y;
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
			if (string.IsNullOrEmpty(msg))
				return;

			var source = src.Split('.');
			var stext = string.Empty;

			switch (type)
			{
				case LogType.Info:
					Console.ForegroundColor = ConsoleColor.White;
					stext = "[I] {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Warn:
					Console.ForegroundColor = ConsoleColor.Yellow;
					stext = "[W] {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Error:
					throw new InvalidOperationException("{0}: {1}".F(source[source.Length - 1], msg));
				case LogType.Debug:
					Console.ForegroundColor = ConsoleColor.Green;
					stext = "[D] {0}: {1}".F(source[source.Length - 1], msg);
					break;
				case LogType.Notice:
					Console.ForegroundColor = ConsoleColor.White;
					stext = "[N] {0}: {1}".F(source[source.Length - 1], msg);
					break;
				default:
					stext = "{0} {1}".F(source[source.Length - 1], msg);
					break;
			}

			Console.WriteLine(stext);
		}

		public void Events(ref bool running, ref bool paused)
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
						if (this.events.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE)
						{
							if (paused)
								paused = false;
							else
								paused = true;
						}

						if (!paused)
							this.world.Events(ref this.events);
						break;
					case SDL_EventType.SDL_MOUSEMOTION:
					case SDL_EventType.SDL_MOUSEBUTTONDOWN:
					case SDL_EventType.SDL_MOUSEBUTTONUP:
						if (this.curCursorPosition_X != this.events.motion.x || this.curCursorPosition_Y != this.events.motion.y)
						{
							this.curCursorPosition_X = this.events.motion.x;
							Engine.MousePosition.X = this.curCursorPosition_X;

							this.curCursorPosition_Y = this.events.motion.y;
							Engine.MousePosition.Y = this.curCursorPosition_Y;
						}

						this.ui.Events(ref this.events);
						break;
					default:
						break;
				}
			}
		}

		public int Start()
		{
			this.world = new World(this.config.Player.Name, this.video.Renderer, this.WindowSize());

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
			var surface = this.video.WindowSurface();
			var windowSize = this.WindowSize();

			retval = this.video.Begin(Color.Black);
			retval = this.world.Render(ref windowSize, ref surface, ref renderer);
			retval = this.ui.Render(ref surface, ref renderer, Color.BlanchedAlmond);

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
