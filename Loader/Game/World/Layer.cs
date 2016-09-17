using System;
using System.Collections.Generic;
using SDL2;

namespace RPGEngine
{
	public class Layer
	{
		public Dictionary<string, Tile> Tiles;
		public LayerType LayerType;
		public float Width, Height;

		public Layer(Dictionary<string, Tile> tiles, LayerType layertype, float width, float height)
		{
			this.LayerType = layertype;
			this.Width = width;
			this.Height = height;
			this.Tiles = tiles;
		}

		public void Update()
		{
			foreach (var tile in this.Tiles.Values)
			{
				tile.Type = this.LayerType;
				tile.Update();
			}
		}

		public void Events(ref SDL.SDL_Event e)
		{
			foreach (var tile in this.Tiles.Values)
				tile.Events(ref e);
		}

		public int Render(ref IntPtr renderer, ref IntPtr screen_surface, Vector2<float> camera, 
			Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			var retval = -1;

			foreach (var tile in this.Tiles.Values)
				retval = tile.Render(ref renderer, ref screen_surface, camera, screensize, type);

			return retval;
		}

		public void Close()
		{
			foreach (var tile in this.Tiles.Values)
				tile.Close();

			this.Tiles.Clear();
		}
	}
}
