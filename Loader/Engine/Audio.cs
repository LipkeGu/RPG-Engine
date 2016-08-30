using static SDL2.SDL;

namespace RPGEngine
{
	public class Audio
	{
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

			if (this.errnum == 0)
			{
				this.audiocallback = new SDL_AudioCallback(this.Audiocallback);
				this.audioSpec.freq = 48000;
				this.audioSpec.format = AUDIO_F32;
				this.audioSpec.channels = 2;
				this.audioSpec.samples = 4096;
				this.audioSpec.callback = this.audiocallback;

				this.errnum = SDL_OpenAudio(ref this.audioSpec, out this.obtainedSpec);
				if (this.errnum == 0)
				{
					this.deviceName = SDL_GetAudioDeviceName(SDL_GetNumAudioDevices(0) - 1, 0);
					this.audioDevice = SDL2.SDL.SDL_OpenAudioDevice(this.deviceName,
						0, ref this.audioSpec, out this.obtainedSpec, (int)SDL_AUDIO_ALLOW_ANY_CHANGE);

					Game.Print(LogType.Error, GetType().ToString(), "using AudioDevice {0}".F(this.deviceName));
				}
				else
					Game.Print(LogType.Error, GetType().ToString(), "OpenAudio-Error: {0}".F(SDL_GetError()));

			}
			else
				Game.Print(LogType.Error, GetType().ToString(), "Init-Error: {0}".F(SDL_GetError()));

		}

		void Audiocallback(System.IntPtr userdata, System.IntPtr stream, int len)
		{

		}

		public void Close()
		{
			SDL_CloseAudio();

			if (this.audioDevice != uint.MaxValue)
				SDL_CloseAudioDevice(this.audioDevice);
		}
	}
}
