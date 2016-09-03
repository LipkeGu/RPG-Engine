using SDL2;
using System;

namespace RPGEngine
{
	public class UserInferface
	{
		SDL.SDL_Rect TileScreen;

		public UserInferface()
		{
			this.TileScreen.h = 32;
			this.TileScreen.w = 32;
			this.TileScreen.x = 50;
			this.TileScreen.y = 50;
		}

		public void Update()
		{
		}

		public void Events(ref SDL.SDL_Event e)
		{
		}

		public void Close()
		{
		}

		public void Render(ref IntPtr windows_surface, ref IntPtr renderer)
		{
			SDL.SDL_SetRenderDrawColor(renderer, byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
			SDL.SDL_RenderDrawRect(renderer, ref this.TileScreen);
			SDL.SDL_SetRenderDrawColor(renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

		}
	}
}
