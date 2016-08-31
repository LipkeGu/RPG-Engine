using SDL2;
using System;
using System.Collections.Generic;
using System.IO;


namespace RPGEngine
{
	class Map
	{
		IniFile inifile;
		Vector2<float> camera;

		IntPtr renderer;
		public IntPtr tileset;
		string tileset_directory;

		public Dictionary<string, Layer> Layers;
		
		public Vector2<float> StartPosition
		{
			get { return this.camera; }
		}

		public delegate void MapCreatedEventHandler(object source, EventArgs args);
		public event MapCreatedEventHandler Mapcreated;

		public Map(string fileName, IntPtr renderer, string parentDir)
		{
			this.inifile = new IniFile(fileName);
			this.renderer = renderer;
			this.tileset_directory = parentDir;
		}

		public void Load()
		{
			var num_layers = int.Parse(this.inifile.WertLesen("Info", "Layers"));
			var tileset_file = Path.Combine(this.tileset_directory, this.inifile.WertLesen("Info", "Tileset") + ".png");

			var tileset_width = 0;
			var tileset_height = 0;
			var tileset_format = 0U;
			var tileset_access = 0;
			var groups = new Dictionary<string, List<Vector2<int>>>();

			var map_w = int.Parse(this.inifile.WertLesen("Info", "Width"));
			var map_h = int.Parse(this.inifile.WertLesen("Info", "Height"));

			var tw = int.Parse(this.inifile.WertLesen("Info", "TWidth"));
			var th = int.Parse(this.inifile.WertLesen("Info", "THeight"));

			var stpos = this.inifile.WertLesen("Info", "StartPos").Split(',');
			this.camera = new Vector2<float>(float.Parse(stpos[0]) * tw, float.Parse(stpos[1]) * th);

			this.tileset = SDL_image.IMG_LoadTexture(this.renderer, tileset_file);

			SDL.SDL_QueryTexture(this.tileset, out tileset_format, out tileset_access, out tileset_width, out tileset_height);

			#region "TileGroups (randomaze tiles)"
			var num_groups = int.Parse(inifile.WertLesen("Info", "Groups"));
			var rand = new Random();

			if (num_groups > 0)
			{
				var imageoffsets = string.Empty.Split(',');
				var imageoffset = string.Empty.Split(':');
				var groupentry = string.Empty.Split(';');
;
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating TileGroups...");
				for (var g = 0; g < num_groups; g++)
				{
					groupentry = this.inifile.WertLesen("TileGroups", "group" + g.ToString()).Split(';');
					var offsets = new List<Vector2<int>>();
					if (groupentry.Length > 1)
					{
						imageoffsets = groupentry[1].ToString().Split(',');
						for (var o = 0; o < imageoffsets.Length; o++)
						{
							imageoffset = imageoffsets[o].ToString().Split(':');
							offsets.Add(new Vector2<int>(int.Parse(imageoffset[0]), int.Parse(imageoffset[1])));
						}
					}

					Game.Print(LogType.Debug, this.GetType().ToString(), "Adding TileGroup \"{0}\" ({1} offsets) ...".F(groupentry[0], offsets.Count));

					if (!groups.ContainsKey(groupentry[0]))
						groups.Add(groupentry[0], offsets);
					else
						throw new ArgumentException("Undifined Group: '{0}'".F(groupentry[0]));
				}

				Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} TileGroups...".F(groups.Count));

			}
			#endregion

			if (!File.Exists(tileset_file))
				throw new FileNotFoundException("Tileset not found: {0}".F(tileset_file));

			var imgX = 0;
			var imgY = 0;

			var targetX = 0;
			var targetY = 0;
			var passable = true;

			#region "Layer"
			this.Layers = new Dictionary<string, Layer>();
			Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layers...");

			var layertype = 0;
			var groundimage = string.Empty;
			var imgoffset = string.Empty.Split(':');
			var tileentry = string.Empty.Split(';');

			for (var l = 0; l < num_layers; l++)
			{
				layertype = int.Parse(inifile.WertLesen("Layer" + l.ToString(), "Type"));
				groundimage = inifile.WertLesen("Layer" + l.ToString(), "Image");
				var tiles = new Dictionary<string, Tile>();
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Tiles...");

				if (l > 0)
				{
					for (var ti = 0; ti < (map_w * map_h); ti++)
					{
						passable = layertype == 2 ? true : false;
						tileentry = inifile.WertLesen("Tiles", "Tile" + ti.ToString()).Split(';');

						if (tileentry.Length == 0 || tileentry == null)
							break;

						if (tileentry.Length > 2 && int.Parse(tileentry[0]) == layertype)
						{
							targetX = int.Parse(tileentry[1]);
							targetY = int.Parse(tileentry[2]);

							if (tileentry[3].Contains(":"))
							{
								imgoffset = tileentry[3].Split(":".ToCharArray());
								imgX = (int.Parse(imgoffset[0]) * tw);
								imgY = (int.Parse(imgoffset[1]) * th);
							}
						}

						if (imgX <= tileset_width && imgY <= tileset_height)
						{
							if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype),
									new Tile(this.tileset, new Vector2<int>(imgX, imgY), new Vector2<int>(th, tw),
									new Vector2<int>(targetX, targetY), layertype, passable, this.camera));
						}
						else
							throw new IndexOutOfRangeException("Malformed definition for Tile \"{0}-{1}-{2}\"".F(targetX, targetY, layertype));
					}
				}
				else
				{
					var imageoffset = new Vector2<int>(0, 0);

					for (var y = 0; y < map_h; y++)
						for (var x = 0; x < map_w; x++)
						{
							imageoffset = groups[groundimage][rand.Next(0, groups[groundimage].Count)];

							imgX = (imageoffset.X * tw);
							imgY = (imageoffset.Y * th);

							targetX = x;
							targetY = y;

							if (imgX <= tileset_width && imgY <= tileset_height)
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, 0), new Tile(this.tileset,
									new Vector2<int>(imgX, imgY), new Vector2<int>(th, tw),
									new Vector2<int>(targetX, targetY), 0, true, this.camera));
						}
				}

				if (tiles.Count > 0)
				{
					Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layer {0}...".F(l));

					this.Layers.Add("Layer{0}".F(l), new Layer(tiles, layertype, map_w, map_h));

					Game.Print(LogType.Debug, GetType().ToString(), "Added {0} Tiles to Layer {1}!".F(tiles.Count, l));
				}
			}

			Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Layers!".F(this.Layers.Count));
			#endregion

			OnMapCreated();
		}

		public void Update()
		{
			foreach (var layer in this.Layers)
				layer.Value.Update();
		}

		public void Render(Vector2<float> camera, IntPtr screen_surface, Vector2<int> screensize, ref Player player, Worldtype type = Worldtype.Normal)
		{
			foreach (var layer in this.Layers)
				if (layer.Value.type < 2 && layer.Value.Tiles.Count > 0)
					layer.Value.Render(this.renderer, screen_surface, camera, screensize, type);

			if (type != Worldtype.Editor)
				player.Render(screensize);

			foreach (var layer in this.Layers)
				if (layer.Value.type >= 2 && layer.Value.Tiles.Count > 0)
					layer.Value.Render(this.renderer, screen_surface, camera, screensize, type);
		}

		public void Events(SDL.SDL_Event e)
		{
			foreach (var layer in this.Layers)
				if (layer.Value.type != 0)
					layer.Value.Events(e);
		}

		public void Close()
		{
			SDL.SDL_DestroyTexture(this.tileset);

			foreach (var layer in this.Layers)
				layer.Value.Close();

			this.Layers.Clear();
		}

		protected virtual void OnMapCreated()
		{
			Mapcreated?.Invoke(this, EventArgs.Empty);
		}
	}
}
