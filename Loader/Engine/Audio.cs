using System;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Audio
	{
		private uint audioDevice = uint.MaxValue;
		private string deviceName;

		private SDL_AudioSpec audioSpec, obtainedSpec;
		private SDL_AudioCallback callback;

		public delegate void AudioInitDoneEventHandler(object source, FinishEventArgs args);

		public delegate void AudioInitErrorEventHandler(object source, ErrorEventArgs args);

		public delegate void AudioInitStateEventHandler(object source, StateEventArgs args);

		public event AudioInitDoneEventHandler AudioInitDone;

		public event AudioInitStateEventHandler AudioInitState;

		public event AudioInitErrorEventHandler AudioInitError;

		public Audio()
		{
			SDL_Init(SDL_INIT_AUDIO);
		}

		public void Init(object obj)
		{
			var config = (Settings)obj;
			var errnum = SDL_Init(SDL_INIT_AUDIO);

			if (errnum != 0)
				this.onAudioInitError(this.GetType().ToString(), SDL_GetError());
			
			this.callback = new SDL_AudioCallback(this.audiocallback);
			this.audioSpec.freq = config.Engine.Frequence;
			this.audioSpec.format = config.Engine.Format;
			this.audioSpec.channels = config.Engine.AudioChannels;
			this.audioSpec.samples = config.Engine.Samples;
			this.audioSpec.silence = config.Engine.Silence;
			this.audioSpec.size = config.Engine.Size;

			this.audioSpec.callback = this.audiocallback;

			errnum = SDL_OpenAudio(ref this.audioSpec, out this.obtainedSpec);
			if (errnum != 0)
				this.onAudioInitError(this.GetType().ToString(), SDL_GetError());

			this.deviceName = SDL_GetAudioDeviceName(SDL_GetNumAudioDevices(0) - 1, 0);
			this.audioDevice = SDL_OpenAudioDevice(this.deviceName, 0, ref this.audioSpec, out this.obtainedSpec, 1);

			if (this.audioDevice != uint.MaxValue)
				this.onAudioInitState(this.GetType().ToString(), "using Audiodevice '{0}'".F(this.deviceName));

			this.onAudioInitDone(this.GetType().ToString(), "Audio initialized!");
		}

		public void Close()
		{
			Game.Print(LogType.Debug, this.GetType().ToString(), "Closing Audio Subsystem");

			if (this.audioDevice != uint.MaxValue)
				SDL_CloseAudioDevice(this.audioDevice);
			
			SDL_CloseAudio();
		}

		public void Play_audio()
		{
			SDL_PauseAudio(0);
		}

		public void Pause_Audio()
		{
			SDL_PauseAudio(1);
		}

		#region Events
		protected virtual void onAudioInitDone(string source, string message)
		{
			var fe = new FinishEventArgs();
			fe.Source = source;
			fe.Message = message;

			this.AudioInitDone?.Invoke(this, fe);
		}

		protected virtual void onAudioInitError(string source, string message)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = message;
			errvtargs.Source = source;

			this.AudioInitError?.Invoke(this, errvtargs);
		}

		protected virtual void onAudioInitState(string source, string message)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = message;
			statevtargs.Source = source;

			this.AudioInitState?.Invoke(this, statevtargs);
		}
		#endregion

		private void audiocallback(IntPtr userdata, IntPtr stream, int len)
		{
		}
	}
}
