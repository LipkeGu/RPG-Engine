using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using static SDL2.SDL;

namespace RPGEngine
{
	public class VideoErrorEventArgs : EventArgs
	{
		public string Source
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}
	}

	public class SDLEventEventArgs : EventArgs
	{
		public SDL_Keycode Key
		{
			get;
			set;
		}
	}

	public class Engine
	{
		public static Vector2<int> MousePosition = new Vector2<int>(0, 0);
		public static Dictionary<string, Sprite> Textures = new Dictionary<string, Sprite>();

		public Video Video;
		public Audio Audio;
		public Text Text;
		public Input Input;
		public UserInferface UI;
		public Settings Config;

		private bool haveVideo, paused, started, running;
		private uint fps;
		private uint starttime;
		private uint pausetime;
		private string[] args;
		private Game game;

		public Engine(string[] args)
		{
			this.args = args;

			this.Config = new Settings();
			this.Video = new Video();
			this.Audio = new Audio();
			this.Input = new Input();
			this.UI = new UserInferface();

			this.game = new Game(ref this.Video, ref this.Text, ref this.Audio, ref this.Config, ref this.UI);
			var xmlSettingsReader = new XmlManager<Settings>();
			this.haveVideo = this.running = false;

			var settingsFile = Path.Combine(Environment.CurrentDirectory, "Data/Settings.xml");
			if (File.Exists(settingsFile))
				this.Config = xmlSettingsReader.Load(settingsFile);
			else
				xmlSettingsReader.Save(settingsFile, this.Config);

			this.Video.VideoInitDone += this.OnVideoInitDone;
			this.Video.VideoInitError += this.OnVideoInitError;
			this.Video.VideoInitState += this.OnVideoInitState;

			this.Audio.AudioInitDone += this.Audio_AudioInitDone;
			this.Audio.AudioInitError += this.Audio_AudioInitError;
			this.Audio.AudioInitState += this.Audio_AudioInitState;

			this.Input.InputInitDone += this.Input_InputInitDone;
			this.Input.InputInitError += this.Input_InputInitError;
			this.Input.InputInitState += this.Input_InputInitState; 
		}

		public bool IsStarted
		{
			get { return this.started; }
		}

		public bool IsPaused
		{
			get { return this.paused; }
		}

		public static void ConvertSurface(IntPtr surface, ref IntPtr renderer, string filename, Vector2<int> frames, Vector2<int> offset)
		{
			var t = loadTexture(ref renderer, filename, frames, offset);

			if (!Textures.ContainsKey(filename))
				Textures.Add(filename, t);
		}

		public static Sprite GetTexture(string filename, ref IntPtr renderer, Vector2<int> frames, Vector2<int> offset)
		{
			if (Textures.ContainsKey(filename))
				return Textures[filename];
			else
				return loadTexture(ref renderer, filename, frames, offset);
		}

		public static void UnloadTextures(bool clear_cache = false)
		{
			foreach (var texture in Textures.Values)
				texture.Close();

			if (clear_cache)
				Textures.Clear();
		}

		public void Init()
		{
			if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
				Game.Print(LogType.Error, this.GetType().ToString(), SDL_GetError());

			var inputInit = new Thread(new ParameterizedThreadStart(this.Input.Init));
			inputInit.Start(this.Config);

			var audioInit = new Thread(new ParameterizedThreadStart(this.Audio.Init));
			audioInit.Start(this.Config);

			var videoInit = new Thread(new ParameterizedThreadStart(this.Video.Init));
			videoInit.Start(this.Config);
		}
		
		public void Close()
		{
			this.game.Close();

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

		public void Pause()
		{
			if (this.started && !this.paused)
			{
				this.paused = true;
				this.pausetime = SDL_GetTicks() - this.starttime;
			}
		}

		public void Unpause()
		{
			if (this.paused)
			{
				this.paused = false;
				this.starttime = SDL_GetTicks() - this.pausetime;
				this.pausetime = uint.MinValue;
			}
		}

		public void Stop()
		{
			this.started = false;
			this.paused = false;
		}

		private static Sprite loadTexture(ref IntPtr renderer, string filename, Vector2<int> frames, Vector2<int> offset)
		{
			var t = new Sprite(filename, ref renderer, frames, offset);

			if (!Textures.ContainsKey(filename))
			{
				Textures.Add(filename, t);
				return t;
			}
			else
				return GetTexture(filename, ref renderer, frames, offset);
		}

		private void start()
		{
			this.started = true;
			this.paused = false;

			this.starttime = SDL_GetTicks();
		}

		private void regulate()
		{
			if (this.fps >= this.Config.Engine.FPS)
				this.fps = uint.MinValue;

			this.fps++;

			if (!this.IsStarted)
				this.start();

			if (this.GetTime() < 1000 / this.Config.Engine.FPS)
				SDL_Delay((1000 / this.Config.Engine.FPS) - this.GetTime());
			else
				this.start();
		}

		private void Input_InputInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Input_InputInitError(object source, ErrorEventArgs args)
		{
			throw new Exception("{0}: {1}".F(args.Source, args.Message));
		}

		private void Input_InputInitDone(object source, FinishEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Audio_AudioInitState(object source, StateEventArgs args)
		{
			Game.Print(LogType.Debug, args.Source, args.Message);
		}

		private void Audio_AudioInitError(object source, ErrorEventArgs args)
		{
			throw new Exception("{0}: {1}".F(args.Source, args.Message));
		}

		private void Audio_AudioInitDone(object source, FinishEventArgs args)
		{
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

		private void OnVideoInitDone(object source, FinishEventArgs e)
		{
			this.haveVideo = true;
			if (this.haveVideo && this.game.Start() == 0)
			{
				this.running = true;

				while (this.running)
				{
					this.game.Events(ref this.running);
					this.game.Update();
					this.game.Render(this.Video.Renderer);
					this.regulate();
				}
			}

			this.Close();
		}
	}
}