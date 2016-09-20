using System;
using System.Drawing;

using SDL2;

namespace RPGEngine
{
	public class UserInferface
	{
		private SDL.SDL_Rect tileScreen;
		private bool renderUi;

		public UserInferface(bool render_ui = true)
		{
			this.renderUi = render_ui;

			this.tileScreen.h = 56;
			this.tileScreen.w = 128;
			this.tileScreen.x = 20;
			this.tileScreen.y = 20;
		}

		public void Update()
		{
		}

		public void Events(ref SDL.SDL_Event e)
		{
			if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F11)
			{
				if (this.renderUi)
					this.renderUi = false;
				else
					this.renderUi = true;
			}

			switch (e.button.type)
			{
				case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
					switch (e.button.button)
					{
						case (byte)SDL.SDL_BUTTON_LEFT:
							Console.WriteLine("Left Button clicked!");
							break;
						case (byte)SDL.SDL_BUTTON_RIGHT:
							Console.WriteLine("Right Button clicked!");
							break;
						case (byte)SDL.SDL_BUTTON_MIDDLE:
							Console.WriteLine("Middle Button clicked!");
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}

		}

		public void Close()
		{
		}

		public int Render(ref IntPtr windows_surface, ref IntPtr renderer, Color color)
		{
			var retval = -1;

			if (!this.renderUi)
				return retval;

			retval = SDL.SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
			retval = SDL.SDL_RenderFillRect(renderer, ref this.tileScreen);
			retval = SDL.SDL_SetRenderDrawColor(renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

			return retval;
		}
	}
}
