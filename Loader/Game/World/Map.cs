using System;
using System.Collections.Generic;
using System.IO;
using SDL2;

namespace RPGEngine
{
	public class Map
	{
		public Dictionary<string, Layer> Layers;
		public Dictionary<string, Portal> Portals;

		private Vector2<float> camera;
		private Worldtype worldtype;
		private string name, filename;
		private bool debug;
		private ulong mapWidth, mapHeight;
		private Vector2<int> tileSize;

		public delegate void MapCreatedEventHandler(object source, EventArgs args);

		public event MapCreatedEventHandler Mapcreated;
		
		public Map(string name, string fileName, Worldtype worldtype = Worldtype.Normal)
		{
			this.name = name;
			this.filename = fileName;

			this.worldtype = worldtype;
			this.debug = this.worldtype == Worldtype.Debug ? true : false;
		}

		public void Load(ref IntPtr renderer, ref Player player)
		{
			var inifile = new IniFile(this.filename);
			this.mapHeight = ulong.Parse(inifile.WertLesen("Info", "Height"));
			this.mapWidth =  ulong.Parse(inifile.WertLesen("Info", "Width"));
			
			var num_layers = uint.Parse(inifile.WertLesen("Info", "Layers"));
			var tilesetfile = Path.Combine("Data/Tileset/",	"{0}.png".F(inifile.WertLesen("Info", "Tileset")));

			if (!File.Exists(tilesetfile))
				Game.Print(LogType.Error, this.GetType().ToString(), "Tilesheet not found: {0}".F(tilesetfile));

			/// TODO: load tileset info from file...
			var frames = new Vector2<int>(20, 20);
			var offset = new Vector2<int>(0, 0);
			var tileset = Engine.GetTexture(tilesetfile, ref renderer, ref frames, ref offset);

			var tilegroups = new Dictionary<string, List<Vector2<uint>>>();

			tileSize.X = int.Parse(inifile.WertLesen("Info", "TWidth"));
			tileSize.Y = int.Parse(inifile.WertLesen("Info", "THeight"));

			var stpos = inifile.WertLesen("Info", "StartPos").Split(',');
			this.camera = new Vector2<float>(float.Parse(stpos[0]) * tileSize.X, float.Parse(stpos[1]) * tileSize.Y);

			#region "TileGroups (random tiles)"
			var num_groups = uint.Parse(inifile.WertLesen("Info", "Groups"));
			var rand = new Random();

			if (num_groups > uint.MinValue)
			{
				var imageoffsets = string.Empty.Split(',');
				var imageoffset = string.Empty.Split(':');
				var groupentry = string.Empty.Split(';');

				for (var g = uint.MinValue; g < num_groups; g++)
				{
					groupentry = inifile.WertLesen("TileGroups", "group{0}".F(g)).Split(';');
					var offsets = new List<Vector2<uint>>();

					if (groupentry.Length > 1)
					{
						imageoffsets = groupentry[1].ToString().Split(',');
						for (var o = 0; o < imageoffsets.Length; o++)
						{
							imageoffset = imageoffsets[o].ToString().Split(':');
							offsets.Add(new Vector2<uint>(uint.Parse(imageoffset[0]), uint.Parse(imageoffset[1])));
						}
					}

					if (!tilegroups.ContainsKey(groupentry[0]))
						tilegroups.Add(groupentry[0], offsets);
				}
			}
			#endregion

			var imagePos = new Vector2<uint>(uint.MinValue, uint.MinValue);

			var targetX = ulong.MinValue;
			var targetY = ulong.MinValue;

			#region "Portals"
			this.Portals = new Dictionary<string, Portal>();

			var num_portals = uint.Parse(inifile.WertLesen("Info", "Portals"));
			var portal_entry = string.Empty.Split(',');
			var portal = new Portal(new Vector2<ulong>(0,0), string.Empty,
				new Vector2<uint>((uint)tileSize.X, (uint)tileSize.Y));

			var portal_values = string.Empty.Split(',');

			for (var p = 0U; p < num_portals; p++)
			{
				portal_entry = inifile.WertLesen("Portals", "portal{0}".F(p)).Split(';');
				portal_values = portal_entry[0].Split(',');

				portal.Position.X = ulong.Parse(portal_values[0]) * (ulong)tileSize.X;
				portal.Position.Y = ulong.Parse(portal_values[1]) * (ulong)tileSize.Y;

				if (!this.Portals.ContainsKey(portal_entry[1]))
					this.Portals.Add("{0}-{1}".F(portal_entry[0], portal_entry[1]), portal);
			}
			#endregion;

			#region "Layer"
			this.Layers = new Dictionary<string, Layer>();

			var layertype = LayerType.Ground;
			var image = string.Empty;

			var tileentry = string.Empty.Split(';');
			var tiletype = TileType.Clear;
			var imageoffset_a = new Vector2<uint>(0, 0);
			var tile = new Tile();
			var tiles = new Dictionary<string, Tile>();

			for (var l = uint.MinValue; l < num_layers; l++)
			{
				layertype = (l == uint.MinValue) ? LayerType.Ground : (LayerType)int.Parse(inifile.WertLesen("Layer{0}".F(l), "Type"));

				image = inifile.WertLesen("Layer{0}".F(l), "Image").Split(',')[0];
				tiles = new Dictionary<string, Tile>();

/// TODO "Put this in a simple Function..."

				#region "Fill the Map with Ground Tiles"
				if (l == uint.MinValue)
					for (var y = ulong.MinValue; y < this.mapHeight; y++)
						for (var x = ulong.MinValue; x < this.mapWidth; x++)
						{
							image = "clear";

							if (!tilegroups.ContainsKey(image))
								Game.Print(LogType.Error, this.GetType().ToString(), "Undefined definition '{0}'".F(image));

							if (layertype == LayerType.Collision)
								tiletype = TileType.None;
							else
								if (tileentry.Length > 3)
									tiletype = (TileType)int.Parse(tileentry[4]);

							imageoffset_a = tilegroups[image][rand.Next(0, tilegroups[image].Count - 1)];

							imagePos.X = (imageoffset_a.X * (uint)tileSize.X);
							imagePos.Y = (imageoffset_a.Y * (uint)tileSize.Y);

							targetX = x * (ulong)tileSize.X;
							targetY = y * (ulong)tileSize.X;

							if (imagePos.X <= tileset.Width && imagePos.Y <= tileset.Height)
								addTiles(ref tiles, ref Portals, ref targetX, ref targetY, ref tileset, 
									ref layertype, ref tiletype, ref imagePos);
						}
				#endregion

				#region "Read and add Tiles from file"
				var imgoffset = string.Empty.Split(':');
				for (var ti = ulong.MinValue; ti < (this.mapWidth * this.mapHeight); ti++)
				{
					tileentry = inifile.WertLesen("Tiles", "Tile{0}".F(ti)).Split(';');

					if (tileentry.Length == 0)
						continue;

					if (tileentry.Length > 3 && (LayerType)int.Parse(tileentry[0]) == layertype)
					{
						targetX = ulong.Parse(tileentry[1]) * (ulong)tileSize.X;
						targetY = ulong.Parse(tileentry[2]) * (ulong)tileSize.Y;

						if (tileentry[3].Contains(":"))
						{
							imgoffset = tileentry[3].Split(':');
							imagePos.X = (uint.Parse(imgoffset[0]) * (uint)tileSize.X);
							imagePos.Y = (uint.Parse(imgoffset[1]) * (uint)tileSize.Y);
						}
						else
						{
							if (!tilegroups.ContainsKey(image))
								Game.Print(LogType.Error, this.GetType().ToString(), "Undefined definition '{0}'".F(image));

							imageoffset_a = tilegroups[image][rand.Next(0, tilegroups[image].Count - 1)];
							imagePos.X = (imageoffset_a.X * (uint)tileSize.X);
							imagePos.Y = (imageoffset_a.Y * (uint)tileSize.Y);
						}
					}

					if (layertype == LayerType.Collision)
						tiletype = TileType.None;
					else
						if (tileentry.Length > 3)
							tiletype = (TileType)int.Parse(tileentry[4]);
					
					if (imagePos.X <= tileset.Width && imagePos.Y <= tileset.Height)
						addTiles(ref tiles, ref Portals, ref targetX, ref targetY, ref tileset,ref layertype, ref tiletype, ref imagePos);	
				}

				if (tiles.Count > 0)
					this.Layers.Add("Layer{0}".F(l), new Layer(tiles, layertype));
				#endregion
			}
			#endregion

			if (player == null)
				return;

			player.Position = this.camera;
		}

		private void addTiles(ref Dictionary<string, Tile> tiles, ref Dictionary<string, Portal> portals, 
			ref ulong targetX, ref ulong targetY, ref Sprite tileset, ref LayerType layertype, ref TileType tiletype, 
			ref Vector2<uint> srcImage)
		{
			var tile = new Tile(ref tileset, srcImage, new Vector2<uint>((uint)tileSize.X, (uint)tileSize.Y),
			new Vector2<ulong>(targetX, targetY), layertype, true, this.camera, tiletype);

			foreach (var p in this.Portals.Values)
			{
				if (p.Position.X == targetX && p.Position.Y == targetY)
				{
					tile.Portal = p;
					break;
				}

				if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
					tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
				else
				{
					tiles.Remove("{0}-{1}-{2}".F(targetX, targetY, layertype));
					tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
				}
			}
		}

		public void Update(ref Player player)
		{
			foreach (var layer in this.Layers.Values)
				layer.Update();
		}

		public int Render(ref IntPtr renderer, ref Vector2<float> camera, ref IntPtr screen_surface,
			ref Vector2<int> screensize, ref Player player)
		{
			var retval = -1;

			this.worldtype = (this.debug ? Worldtype.Debug : Worldtype.Normal);

			foreach (var layer in this.Layers.Values)
				if (layer.LayerType != LayerType.PlayerOverlay)
					retval = layer.Render(ref renderer, ref screen_surface, ref camera, ref screensize, this.worldtype);

			if (player != null)
				retval = player.Render(ref renderer, screensize, new Vector2<int>(this.tileSize.X, this.tileSize.Y));

			foreach (var layer in this.Layers.Values)
				if (layer.LayerType == LayerType.PlayerOverlay)
					retval = layer.Render(ref renderer, ref screen_surface, ref camera, ref screensize, this.worldtype);

			return retval;
		}

		public void Events(ref SDL.SDL_Event e)
		{
			foreach (var layer in this.Layers.Values)
					layer.Events(ref e);
		}

		public void Close()
		{
			if (this.Layers != null)
			{
				foreach (var layer in this.Layers.Values)
					layer.Close();

				this.Layers.Clear();
			}
			
			if (this.Portals != null)
				this.Portals.Clear();
		}

		public string Name
		{
			get { return this.name; }
		}

		protected virtual void OnMapCreated()
		{
			this.Mapcreated?.Invoke(this, EventArgs.Empty);
		}

		public bool DebugMode
		{
			get { return this.debug; }
			set { this.debug = value; }
		}

		public Vector2<float> StartPosition
		{
			get { return this.camera; }
		}

	}
}
