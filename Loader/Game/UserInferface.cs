using SDL2;
using System;

using System.Collections.Generic;

namespace RPGEngine
{
	public class UserInferface
	{
		public Dictionary<string, Sprite> Textures;
		IntPtr renderer;

		SDL.SDL_Rect TileScreen;

		public UserInferface(IntPtr renderer)
		{
			this.renderer = renderer;

			this.TileScreen.h = 32;
			this.TileScreen.w = 32;
			this.TileScreen.x = 50;
			this.TileScreen.y = 50;

			this.Textures = new Dictionary<string, Sprite>();
		}

		public void AddTexture(Sprite texture)
		{
			if (!this.Textures.ContainsKey(texture.Filename))
				this.Textures.Add(texture.Filename, texture);
		}

		public void Update()
		{
			foreach (var texture in this.Textures.Values)
				texture.Update();
		}

		public void Events(SDL.SDL_Event e)
		{
			foreach (var texture in this.Textures.Values)
				texture.Events(e);
		}

		public void Close()
		{
			foreach (var texture in this.Textures.Values)
				texture.Close();

			this.Textures.Clear();
		}

		public void Render(IntPtr windows_surface)
		{
			SDL.SDL_SetRenderDrawColor(this.renderer, byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
			SDL.SDL_RenderDrawRect(this.renderer, ref this.TileScreen);

			foreach (var texture in this.Textures.Values)
				texture.Render();

			SDL.SDL_SetRenderDrawColor(this.renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

		}
	}
}
