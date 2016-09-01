using System;
using System.Collections.Generic;
using System.Drawing;
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

	public class Engine
	{
		bool paused;
		bool started;

		uint FPS;
		uint starttime;
		uint pausetime;
		string[] args;

		public Video Video;
		public Audio Audio;
		public Text Text;
		public Input Input;
		public UserInferface UI;

		public static Dictionary<string, Sprite> Textures = new Dictionary<string, Sprite>();

		Game Game;
		public Settings Config;

		[XmlIgnore]
		public string SettingsFile;
		XmlManager<Settings> XMLSettingsReader;

		public bool haveVideo, haveAudio, haveInput, running;

		public Engine(string[] args)
		{
			this.args = args;

			this.Config = new Settings();
			this.Video = new Video();
			this.Audio = new Audio();
			this.Input = new Input();
			this.UI = new UserInferface(Video.Renderer);

			this.Game = new Game(this.Video, this.Text, this.Audio, this.Config);
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


			this.Video.VideoInitDone += OnVideoInitDone;
			this.Video.VideoInitError += OnVideoInitError;
			this.Video.VideoInitState += OnVideoInitState;

			this.Audio.AudioInitDone += Audio_AudioInitDone;
			this.Audio.AudioInitError += Audio_AudioInitError;
			this.Audio.AudioInitState += Audio_AudioInitState;

			this.Input.InputInitDone += Input_InputInitDone;
			this.Input.InputInitError += Input_InputInitError;
			this.Input.InputInitState += Input_InputInitState; 
		}

		private void Input_InputInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Input_InputInitError(object source, ErrorEventArgs args)
		{
			this.haveInput = false;
			throw new Exception("{0}: {1}".F(args.Source, args.Message));
		}

		private void Input_InputInitDone(object source, FinishEventArgs args)
		{
			this.haveInput= true;
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Audio_AudioInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Audio_AudioInitError(object source, ErrorEventArgs args)
		{
			this.haveAudio = false;
			throw new Exception("{0}: {1}".F(args.Source, args.Message));
		}

		private void Audio_AudioInitDone(object source, FinishEventArgs args)
		{
			this.haveAudio = true;
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void OnVideoInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void OnVideoInitError(object source, ErrorEventArgs e)
		{
			this.haveVideo = false;
			throw new Exception("{0}: {1}".F(e.Source, e.Message));
		}

		[STAThread]
		public void OnVideoInitDone(object source, FinishEventArgs e)
		{
			this.haveVideo = true;

			if (this.haveVideo)
			{
				var fontfile = Path.Combine(this.Config.Engine.FontDirectory, this.Config.Engine.FontFile);
				this.Text = new Text(this.Video.Renderer, fontfile, 12, Color.Black, Color.White);

				if (this.Game.Start() == 0)
				{
					this.running = true;
					
					while (this.running)
					{
						this.Game.Events(ref this.running);
						this.Game.Update();
						this.Game.Render(this.Video.Renderer);
						this.regulate();
					}
				}

				this.Close();
			}
		}

		public void Init()
		{
			SDL_Init(SDL_INIT_EVERYTHING);

			var InputInit = new Thread(new ParameterizedThreadStart(this.Input.Init));
			InputInit.Start(this.Config);

			var AudioInit = new Thread(new ParameterizedThreadStart(this.Audio.Init));
			AudioInit.Start(this.Config);

			var videoInit = new Thread(new ParameterizedThreadStart(this.Video.Init));
			videoInit.Start(this.Config);
		}

		public void Close()
		{
			this.Game.Close();
			this.Video.Close();
			this.Audio.Close();
			this.Input.Close();
			this.UI.Close();

			foreach (var texture in Textures.Values)
				texture.Close();

			Textures.Clear();
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
				return uint.MinValue;
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
				this.pausetime = uint.MinValue;
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
				this.FPS = uint.MinValue;

			this.FPS++;

			if (!this.isStarted())
				this.start();

			if (this.GetTime() < 1000 / this.Config.Engine.FPS)
				SDL_Delay((1000 / this.Config.Engine.FPS) - this.GetTime());
			else
				this.start();
		}

		private static Sprite LoadTexture(IntPtr renderer, string filename, Vector2<int> frames)
		{
			var t = new Sprite(filename, renderer, frames);

			if (!Textures.ContainsKey(filename))
			{
				Textures.Add(filename, t);
				Game.Print(LogType.Debug, "Engine","Have now {0} Textures...".F(Textures.Count));
				return t;
			}
			else
				return GetTexture(filename, renderer, frames);
		}

		public static Sprite GetTexture(string filename, IntPtr renderer, Vector2<int> frames)
		{
			if (Textures.ContainsKey(filename))
				return Textures[filename];
			else
				return LoadTexture(renderer, filename, frames);
		}

		public static void UnloadTextures(bool clear_cache = false)
		{
			foreach (var texture in Textures.Values)
				texture.Close();

			if (clear_cache)
				Textures.Clear();
		}
	}
}