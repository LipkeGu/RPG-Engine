using System;
using System.Collections.Generic;

namespace RPGEngine
{
	public class Layer
	{
		public Dictionary<string, Tile> Tiles;
		public int type;
		public float width;
		public float height;

		public Layer(Dictionary<string, Tile> tiles, int type, float width, float height)
		{
			this.type = type;
			this.width = width;
			this.height = height;
			this.Tiles = tiles;
		}

		public void Update()
		{
			foreach (var tile in this.Tiles)
				tile.Value.Update();
		}

		public void Events(SDL2.SDL.SDL_Event e)
		{
			foreach (var tile in this.Tiles)
				tile.Value.Events(e);
		}

		public void Render(IntPtr renderer, IntPtr screen_surface, Vector2<float> camera, 
			Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			foreach (var tile in this.Tiles)
				tile.Value.Render(renderer, screen_surface, camera, screensize, type);
		}

		public void Close()
		{
			foreach (var tile in this.Tiles)
				tile.Value.Close();

			this.Tiles.Clear();
		}
	}
}
