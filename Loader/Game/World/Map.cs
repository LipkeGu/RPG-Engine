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
		public Sprite tileset;
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
			var tileset_file = Path.Combine(this.tileset_directory, "{0}.png".F(this.inifile.WertLesen("Info", "Tileset")));

			if (!File.Exists(tileset_file))
				throw new FileNotFoundException("Tileset not found: {0}".F(tileset_file));

			var tileset_width = 0;
			var tileset_height = 0;
			var tileset_format = uint.MinValue;
			var tileset_access = 0;

			this.tileset = Engine.GetTexture(tileset_file, this.renderer,new Vector2<int>(20,20));
			SDL.SDL_QueryTexture(this.tileset.Image, out tileset_format, out tileset_access, out tileset_width, out tileset_height);

			var groups = new Dictionary<string, List<Vector2<int>>>();

			var map_w = int.Parse(this.inifile.WertLesen("Info", "Width"));
			var map_h = int.Parse(this.inifile.WertLesen("Info", "Height"));

			var tw = int.Parse(this.inifile.WertLesen("Info", "TWidth"));
			var th = int.Parse(this.inifile.WertLesen("Info", "THeight"));

			var stpos = this.inifile.WertLesen("Info", "StartPos").Split(',');
			this.camera = new Vector2<float>(float.Parse(stpos[0]) * tw, float.Parse(stpos[1]) * th);

			#region "TileGroups (random tiles)"
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
					groupentry = this.inifile.WertLesen("TileGroups", "group{0}".F(g)).Split(';');
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



			var imgX = 0;
			var imgY = 0;

			var targetX = 0;
			var targetY = 0;
			var passable = true;

			#region "Layer"
			this.Layers = new Dictionary<string, Layer>();
			Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layers...");

			var layertype = LayerType.Ground;
			var image = string.Empty;
			
			var tileentry = string.Empty.Split(';');
			var tiletype = TileType.Clear;

			for (var l = 0; l < num_layers; l++)
			{
				layertype = (LayerType)int.Parse(inifile.WertLesen("Layer{0}".F(l), "Type"));
				image = inifile.WertLesen("Layer{0}".F(l), "Image");
				var tiles = new Dictionary<string, Tile>();
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Tiles...");

				if (l > 0)
				{
					for (var ti = 0; ti < (map_w * map_h); ti++)
					{
						passable = layertype == LayerType.Collision ? false : true;
						tileentry = inifile.WertLesen("Tiles", "Tile{0}".F(ti)).Split(';');

						if (tileentry.Length == 0 || tileentry == null)
							break;

						if (tileentry.Length > 2 && (LayerType)int.Parse(tileentry[0]) == layertype)
						{
							targetX = int.Parse(tileentry[1]);
							targetY = int.Parse(tileentry[2]);
							
							if (tileentry[3].Contains(":"))
							{
								var imgoffset = tileentry[3].Split(":".ToCharArray());
								imgX = (int.Parse(imgoffset[0]) * tw);
								imgY = (int.Parse(imgoffset[1]) * th);
							}
							else
							{
								if (!groups.ContainsKey(image))
									throw new Exception("Undefined definition '{0}'".F(image));

								var imageoffset = groups[image][rand.Next(0, groups[image].Count - 1)];
								imgX = (imageoffset.X * tw);
								imgY = (imageoffset.Y * th);
							}
						}

						if (tileentry.Length > 3)
							tiletype = (TileType)int.Parse(tileentry[4]);

						if (imgX <= tileset_width && imgY <= tileset_height)
						{
							if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype),
									new Tile(ref this.tileset, new Vector2<int>(imgX, imgY), new Vector2<int>(th, tw),
									new Vector2<int>(targetX, targetY), layertype, passable, this.camera, tiletype));
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
							if (!groups.ContainsKey(image))
								throw new Exception("Undefined definition '{0}'".F(image));

							imageoffset = groups[image][rand.Next(0, groups[image].Count - 1)];

							imgX = (imageoffset.X * tw);
							imgY = (imageoffset.Y * th);

							targetX = x;
							targetY = y;

							if (imgX <= tileset_width && imgY <= tileset_height)
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), new Tile(ref this.tileset,
									new Vector2<int>(imgX, imgY), new Vector2<int>(th, tw),
									new Vector2<int>(targetX, targetY), layertype, true, this.camera, tiletype));
						}
				}

				if (tiles.Count > 0)
				{
					Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layer {0}...".F(l));

					this.Layers.Add("Layer{0}".F(l), new Layer(tiles, layertype, map_w, map_h));

					Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Tiles to Layer {1}!".F(tiles.Count, l));
				}
			}

			Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Layers!".F(this.Layers.Count));
			#endregion

			OnMapCreated();
		}

		public void Update()
		{
			foreach (var layer in this.Layers.Values)
				layer.Update();
		}

		public void Render(Vector2<float> camera, ref IntPtr screen_surface, Vector2<int> screensize, ref Player player, Worldtype type = Worldtype.Normal)
		{
			foreach (var layer in this.Layers.Values)
				if (layer.LayerType == LayerType.Ground || layer.LayerType == LayerType.Overlay)
					layer.Render(this.renderer, ref screen_surface, camera, screensize, type);

			if (type != Worldtype.Editor)
				player.Render(screensize);

			foreach (var layer in this.Layers.Values)
				if (layer.LayerType == LayerType.Collision)
					layer.Render(this.renderer, ref screen_surface, camera, screensize, type);
		}

		public void Events(SDL.SDL_Event e)
		{
			foreach (var layer in this.Layers)
				if (layer.Value.LayerType != LayerType.Ground)
					layer.Value.Events(e);
		}

		public void Close()
		{
			this.tileset.Close();

			foreach (var layer in this.Layers.Values)
				layer.Close();

			this.Layers.Clear();
		}

		protected virtual void OnMapCreated()
		{
			Mapcreated?.Invoke(this, EventArgs.Empty);
		}
	}
}
