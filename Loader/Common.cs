using System;
using System.IO;
using System.Threading;

using System.Xml.Serialization;
using static SDL2.SDL;

namespace RPGEngine
{
	public class VideoErrorEventArgs : EventArgs
	{
		public string Source { get; set; }
		public string Message { get; set; }
	}

	public class SDLEventEventArgs : EventArgs
	{
		public SDL_Keycode key { get; set; }
	}

	public class RPGEngine
	{
		bool paused;
		bool started;

		uint FPS;
		uint starttime;
		uint pausetime;
		string[] args;

		public Graphic Graphics;
		public Audio Audio;
		public Text Text;

		Game Game;
		public Settings Config;

		SDL_Event Events;
		[XmlIgnore]
		public string SettingsFile;
		XmlManager<Settings> XMLSettingsReader;

		public bool haveVideo, running;

		public RPGEngine(string[] args)
		{
			this.args = args;

			this.Config = new Settings();
			this.Graphics = new Graphic();
			this.Audio = new Audio();

			this.Game = new Game(this, this.Text);
			this.Events = new SDL_Event();
			this.XMLSettingsReader = new XmlManager<Settings>();
			this.haveVideo = this.running = false;

			this.SettingsFile = Path.Combine(Environment.CurrentDirectory, "Data/Settings.xml");

			if (!File.Exists(this.SettingsFile))
				this.XMLSettingsReader.Save(this.SettingsFile, this.Config);
			else
				try
				{
					this.Config = this.XMLSettingsReader.Load(this.SettingsFile);
				}
				catch (Exception e)
				{
					this.XMLSettingsReader.Save(this.SettingsFile, this.Config);
					Game.Print(LogType.Error, GetType().ToString(), "Failed to open \"{0}\". Default values was written and used. Error: {1}".F(this.SettingsFile, e.Message));
				}


			this.Graphics.VideoInitDone += OnVideoInitDone;
			this.Graphics.VideoInitError += OnVideoInitError;
			this.Graphics.VideoInitState += OnVideoInitState;
		}

		private void OnVideoInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void OnVideoInitError(object source, ErrorEventArgs e)
		{
			this.haveVideo = false;

			Game.Print(LogType.Error, e.Source, e.Message);
		}

		[STAThread]
		public void OnVideoInitDone(object source, FinishEventArgs e)
		{
			this.haveVideo = true;

			if (this.haveVideo)
			{
				var fontfile = Path.Combine(this.Config.Engine.FontDirectory, this.Config.Engine.FontFile);
				this.Text = new Text(this.Graphics.Renderer, fontfile, 12, System.Drawing.Color.Black, System.Drawing.Color.White);

				if (this.Game.Start() == 0)
				{
					this.running = true;
					
					while (this.running)
					{
						this.Game.Events();
						this.Game.Update();
						this.Game.Render(this.Graphics.Renderer);
						this.regulate();
					}
				}

				this.Close();
			}
		}

		public void Init()
		{
			var videoInit = new Thread(new ParameterizedThreadStart(this.Graphics.Init));
			videoInit.Start(this.Config);
		}

		public void Close()
		{
			if (this.Game != null)
				this.Game.Close();

			this.Graphics.Close();

			SDL_Quit();
		}

		public uint GetTime()
		{
			if (this.started)
				if (this.paused)
					return this.pausetime;
				else
					return (SDL_GetTicks() - this.starttime);
			else
				return 0;
		}

		public void pause()
		{
			if (this.started && !this.paused)
			{
				this.paused = true;
				this.pausetime = SDL_GetTicks() - this.starttime;
			}
		}

		public void start()
		{
			this.started = true;
			this.paused = false;

			this.starttime = SDL_GetTicks();
		}


		void unpause()
		{
			if (this.paused)
			{
				this.paused = false;
				this.starttime = SDL_GetTicks() - this.pausetime;
				this.pausetime = 0;
			}
		}

		public void stop()
		{
			this.started = false;
			this.paused = false;
		}

		bool isStarted()
		{
			return this.started;
		}

		bool isPaused()
		{
			return this.paused;
		}

		void regulate()
		{
			if (this.FPS >= this.Config.Engine.FPS)
				this.FPS = 0;

			this.FPS++;

			if (!this.isStarted())
				this.start();

			if (this.GetTime() < 1000 / this.Config.Engine.FPS)
				SDL_Delay((1000 / this.Config.Engine.FPS) - this.GetTime());
			else
				this.start();
		}
	}
}