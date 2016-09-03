using System;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Audio
	{
		public delegate void AudioInitDoneEventHandler(object source, FinishEventArgs args);
		public delegate void AudioInitErrorEventHandler(object source, ErrorEventArgs args);
		public delegate void AudioInitStateEventHandler(object source, StateEventArgs args);

		public event AudioInitDoneEventHandler AudioInitDone;
		public event AudioInitStateEventHandler AudioInitState;
		public event AudioInitErrorEventHandler AudioInitError;

		int errnum = -1;
		uint audioDevice = uint.MaxValue;
		string deviceName;

		SDL_AudioSpec audioSpec;
		SDL_AudioSpec obtainedSpec;
		SDL_AudioCallback audiocallback;

		public void Init(object obj)
		{
			var config = (Settings)obj;
			this.errnum = SDL_Init(SDL_INIT_AUDIO);

			if (this.errnum != 0)
				OnAudioInitError(GetType().ToString(), SDL_GetError());
			
			this.audiocallback = new SDL_AudioCallback(this.Audiocallback);
			this.audioSpec.freq = config.Engine.Frequence;
			this.audioSpec.format = config.Engine.format;
			this.audioSpec.channels = config.Engine.AudioChannels;
			this.audioSpec.samples = config.Engine.Samples;
			this.audioSpec.silence = config.Engine.Silence;
			this.audioSpec.size = config.Engine.Size;

			this.audioSpec.callback = this.audiocallback;

			this.errnum = SDL_OpenAudio(ref this.audioSpec, out this.obtainedSpec);
			if (this.errnum != 0)
				OnAudioInitError(GetType().ToString(), SDL_GetError());

			this.deviceName = SDL_GetAudioDeviceName(SDL_GetNumAudioDevices(0) - 1, 0);

			this.audioDevice = SDL_OpenAudioDevice(this.deviceName, 0, ref this.audioSpec, out this.obtainedSpec, 1);
			if (this.audioDevice != uint.MaxValue)
				OnAudioInitState(GetType().ToString(), "using Audiodevice '{0}'".F(this.deviceName));

			OnAudioInitDone(GetType().ToString(), "Audio initialized!");
		}

		void Audiocallback(IntPtr userdata, IntPtr stream, int len)
		{
		}

		public void Close()
		{
			Game.Print(LogType.Debug, GetType().ToString(), "Closing Audio Subsystem");

			if (this.audioDevice != uint.MaxValue)
				SDL_CloseAudioDevice(this.audioDevice);
			
			SDL_CloseAudio();
		}

		#region Events
		protected virtual void OnAudioInitDone(string source, string message)
		{
			var fe = new FinishEventArgs();
			fe.Source = source;
			fe.Message = message;

			AudioInitDone?.Invoke(this, fe);
		}

		protected virtual void OnAudioInitError(string source, string ErrorMessage)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = ErrorMessage;
			errvtargs.Source = source;

			AudioInitError?.Invoke(this, errvtargs);
		}

		protected virtual void OnAudioInitState(string source, string ErrorMessage)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = ErrorMessage;
			statevtargs.Source = source;

			AudioInitState?.Invoke(this, statevtargs);
		}
		#endregion

		public void Play_audio()
		{
			SDL_PauseAudio(0);
		}

		public void Pause_Audio()
		{
			SDL_PauseAudio(1);
		}
	}
}
