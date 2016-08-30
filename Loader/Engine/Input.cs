using System;

using static SDL2.SDL;

namespace RPGEngine
{
	public class Input
	{
		int errnum = -1;

		IntPtr joystick = IntPtr.Zero;
		IntPtr gameController = IntPtr.Zero;

		public int Init()
		{
			errnum = SDL_Init(SDL_INIT_GAMECONTROLLER | SDL_INIT_JOYSTICK);

			this.joystick = SDL_JoystickOpen(0);
			this.gameController = SDL_GameControllerOpen(0);

			return errnum;
		}

		public void Close()
		{
			if (this.joystick != IntPtr.Zero)
				SDL_JoystickClose(this.joystick);

			if (this.gameController != IntPtr.Zero)
				SDL_GameControllerClose(this.gameController);
		}
	}
}
