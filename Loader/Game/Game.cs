using System;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Game
	{
		RPGEngine Engine;
		Text Text;

		UserInferface Interface;
		static string stext;

		World World;
		SDL_Event engine_event;

		public Game(RPGEngine Engine, Text text)
		{
			this.Engine = Engine;
			this.Text = text;
			this.Interface = new UserInferface(Engine.Graphics.Renderer);
		}

		public void Events()
		{
			while(SDL_PollEvent(out this.engine_event) != 0)
			{
				switch (this.engine_event.type)
				{
					case SDL_EventType.SDL_QUIT:
						this.Engine.running = false;
						break;
					case SDL_EventType.SDL_KEYDOWN:
					case SDL_EventType.SDL_KEYUP:
						this.World.Events(this.engine_event);
						this.Interface.Events(this.engine_event);
						break;
					default:
						break;
				}
			}
		}

		public int Start()
		{
			this.World = new World(
				this.Engine.Config.Player.Name,
				this.Engine.Config.Engine.MapDirectory,
				this.Engine.Config.Engine.ActorDirectory,
				this.Engine.Graphics.Renderer,
				this.WindowSize(),
				this.Engine.Config.Engine.Worldmode);

			return World != null ? 0 : -1;
		}

		public void Update()
		{
			this.World.Update();
			this.Interface.Update();
		}

		public void Render(IntPtr renderer)
		{
			this.Engine.Graphics.Begin();

			this.Interface.Render(Engine.Graphics.WindowSurface);
			this.World.Render(this.WindowSize(), Engine.Graphics.WindowSurface);

			this.Engine.Graphics.End();
		}

		public void Close()
		{
			if (World != null)
				this.World.Close();

			this.Interface.Close();
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

			SDL_GetWindowSize(this.Engine.Graphics.Window, out w, out h);
			return new Vector2<int>(w, h);
		}


	}
}
