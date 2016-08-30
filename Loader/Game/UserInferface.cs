using SDL2;
using System;

using System.Collections.Generic;

namespace RPGEngine
{
	class UserInferface
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
			if (!this.Textures.ContainsKey(texture.Alias))
				this.Textures.Add(texture.Alias, texture);
		}

		public void Update()
		{
			foreach (var texture in this.Textures)
				texture.Value.Update();
		}

		public void Events(SDL.SDL_Event e)
		{
			foreach (var texture in this.Textures)
				texture.Value.Events(e);
		}

		public void Close()
		{
			foreach (var texture in this.Textures)
				texture.Value.Close();

			this.Textures.Clear();
		}

		public void Render(IntPtr windows_surface)
		{
			SDL.SDL_SetRenderDrawColor(this.renderer, 255, 0, 0, 255);
			SDL.SDL_RenderDrawRect(this.renderer, ref this.TileScreen);

			foreach (var texture in this.Textures)
				texture.Value.Render();



		}
	}
}
