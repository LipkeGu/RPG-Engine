using SDL2;
using System;
using System.Collections.Generic;
using System.IO;


namespace RPGEngine
{
	class Map
	{
		IniFile inifile;
		Vector2<float> camera, startpos;

		IntPtr renderer;
		public IntPtr tileset;
		public bool loaded;

		string map_file;
		string tileset_directory;

		string tileset_file;

		int w, h, tw, th;

		public Dictionary<string, Layer> Layers;
		Dictionary<string, List<Vector2<int>>> Groups;

		int numlayers;

		int tsheight, tswidth, tsaccess;
		uint tsformat;

		public Vector2<int> TileSize
		{
			get { return new Vector2<int>(this.tw, this.th); }
		}

		public Vector2<float> StartPosition
		{
			get { return this.startpos; }
		}

		public delegate void MapCreatedEventHandler(object source, EventArgs args);
		public event MapCreatedEventHandler Mapcreated;

		public Map(string fileName, IntPtr renderer, string parentDir)
		{
			this.map_file = fileName;
			this.renderer = renderer;
			this.tileset_directory = parentDir;
			this.loaded = false;
		}

		public void Load()
		{
			this.inifile = new IniFile(this.map_file);

			this.numlayers = int.Parse(this.inifile.WertLesen("Info", "Layers"));
			this.tileset_file = Path.Combine(this.tileset_directory, this.inifile.WertLesen("Info", "Tileset") + ".png");

			this.w = int.Parse(this.inifile.WertLesen("Info", "Width"));
			this.h = int.Parse(this.inifile.WertLesen("Info", "Height"));

			this.tw = int.Parse(this.inifile.WertLesen("Info", "TWidth"));
			this.th = int.Parse(this.inifile.WertLesen("Info", "THeight"));

			var stpos = this.inifile.WertLesen("Info", "StartPos").Split(',');

			this.startpos = new Vector2<float>(float.Parse(stpos[0]) * this.tw, float.Parse(stpos[1]) * this.th);
			this.camera = this.startpos;

			this.tileset = SDL_image.IMG_LoadTexture(this.renderer, this.tileset_file);

			SDL.SDL_QueryTexture(this.tileset, out this.tsformat, out this.tsaccess, out this.tswidth, out this.tsheight);

			#region "TileGroups (randomaze tiles)"
			var groups = int.Parse(inifile.WertLesen("Info", "Groups"));
			var rand = new Random();

			if (groups > 0)
			{
				this.Groups = new Dictionary<string, List<Vector2<int>>>();
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating TileGroups...");
				for (var g = 0; g < groups; g++)
				{
					var groupentry = this.inifile.WertLesen("TileGroups", "group" + g.ToString()).Split(';');
					var offsets = new List<Vector2<int>>();
					if (groupentry.Length > 1)
					{
						var imageoffsets = groupentry[1].ToString().Split(',');
						for (var o = 0; o < imageoffsets.Length; o++)
						{
							var imageoffset = imageoffsets[o].ToString().Split(':');
							offsets.Add(new Vector2<int>(int.Parse(imageoffset[0]), int.Parse(imageoffset[1])));
						}
					}

					Game.Print(LogType.Debug, this.GetType().ToString(), "Adding TileGroup \"{0}\" ({1} offsets) ...".F(groupentry[0], offsets.Count));

					if (!this.Groups.ContainsKey(groupentry[0]))
						this.Groups.Add(groupentry[0], offsets);
				}
				Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} TileGroups...".F(Groups.Count));

			}
			#endregion

			if (!File.Exists(this.tileset_file))
				throw new FileNotFoundException("Tileset not found: {0}".F(this.tileset_file));

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

			for (var l = 0; l < this.numlayers; l++)
			{
				layertype = int.Parse(inifile.WertLesen("Layer" + l.ToString(), "Type"));
				groundimage = inifile.WertLesen("Layer" + l.ToString(), "Image");
				var tiles = new Dictionary<string, Tile>();
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Tiles...");

				if (l > 0)
				{
					for (var ti = 0; ti < (w * h); ti++)
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
								imgX = (int.Parse(imgoffset[0]) * this.tw);
								imgY = (int.Parse(imgoffset[1]) * this.th);
							}
						}

						if (imgX <= this.tswidth && imgY <= this.tsheight)
						{
							if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), 
									new Tile(this.tileset, new Vector2<int>(imgX, imgY), new Vector2<int>(this.th, this.tw),
									new Vector2<int>(targetX, targetY), layertype, passable, this.camera));
						}
						else
							throw new IndexOutOfRangeException("Malformed definition for Tile \"{0}-{1}-{2}\"".F(targetX, targetY, layertype));
					}
				}
				else
					for (var y = 0; y < h; y++)
						for (var x = 0; x < w; x++)
						{
							var imageoffset = this.Groups[groundimage][rand.Next(0, this.Groups[groundimage].Count)];

							imgX = (imageoffset.X * this.tw);
							imgY = (imageoffset.Y * this.th);

							targetX = x;
							targetY = y;

							if (imgX <= this.tswidth && imgY <= this.tsheight)
								tiles.Add("{0}-{1}-{2}".F(targetX, targetY, 0), new Tile(this.tileset,
									new Vector2<int>(imgX, imgY), new Vector2<int>(this.th, this.tw), 
									new Vector2<int>(targetX, targetY), 0, true, this.camera));
						}

				if (tiles.Count > 0)
				{
					Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layer {0}...".F(l));

					this.Layers.Add("Layer{0}".F(l),
						new Layer(tiles, layertype, this.w, this.h));

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
			if (this.Layers != null)
				foreach (var layer in this.Layers)
					if (layer.Value.type < 2 && layer.Value.Tiles.Count > 0)
						layer.Value.Render(this.renderer, screen_surface, camera, screensize, type);

			if (type != Worldtype.Editor)
				player.Render(screensize);

			if (this.Layers != null)
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
