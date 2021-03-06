﻿using System;
using System.Collections.Generic;

using SDL2;

namespace RPGEngine
{
	public class Layer
	{
		public Dictionary<string, Tile> Tiles;
		public LayerType LayerType;

		public Layer(Dictionary<string, Tile> tiles, LayerType layertype)
		{
			this.LayerType = layertype;
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

		public int Render(ref IntPtr renderer, ref IntPtr screen_surface, ref Vector2<float> camera, 
			ref Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			var retval = -1;

			foreach (var tile in this.Tiles.Values)
				retval = tile.Render(ref renderer, ref screen_surface, ref camera, ref screensize, ref type);

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
