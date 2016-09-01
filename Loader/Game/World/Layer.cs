using System;
using System.Collections.Generic;

namespace RPGEngine
{
	public class Layer
	{
		public Dictionary<string, Tile> Tiles;
		public LayerType LayerType;
		public float width;
		public float height;

		public Layer(Dictionary<string, Tile> tiles, LayerType layertype, float width, float height)
		{
			this.LayerType = layertype;
			this.width = width;
			this.height = height;
			this.Tiles = tiles;
		}

		public void Update()
		{
			foreach (var tile in this.Tiles.Values)
				tile.Update();
		}

		public void Events(SDL2.SDL.SDL_Event e)
		{
			foreach (var tile in this.Tiles.Values)
				tile.Events(e);
		}

		public void Render(IntPtr renderer, ref IntPtr screen_surface, Vector2<float> camera, Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			foreach (var tile in this.Tiles.Values)
				tile.Render(renderer, ref screen_surface, camera, screensize, type);
		}

		public void Close()
		{
			foreach (var tile in this.Tiles.Values)
				tile.Close();

			this.Tiles.Clear();
		}
	}
}
