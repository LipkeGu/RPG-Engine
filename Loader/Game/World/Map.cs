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
			var tilesetfile = Path.Combine("{0}/Data/Tileset/".F(Environment.CurrentDirectory), "{0}.png".F(inifile.WertLesen("Info", "Tileset")));

			if (!File.Exists(tilesetfile))
				throw new FileNotFoundException("Tileset not found: {0}".F(tilesetfile));

			var tileset = Engine.GetTexture(tilesetfile, ref renderer,
				new Vector2<int>(20, 20), new Vector2<int>(0, 0));

			var tilegroups = new Dictionary<string, List<Vector2<uint>>>();

			var tw = uint.Parse(inifile.WertLesen("Info", "TWidth"));
			var th = uint.Parse(inifile.WertLesen("Info", "THeight"));

			var stpos = inifile.WertLesen("Info", "StartPos").Split(',');
			this.camera = new Vector2<float>(float.Parse(stpos[0]) * tw, float.Parse(stpos[1]) * th);

			#region "TileGroups (random tiles)"
			var num_groups = uint.Parse(inifile.WertLesen("Info", "Groups"));
			var rand = new Random();

			if (num_groups > uint.MinValue)
			{
				var imageoffsets = string.Empty.Split(',');
				var imageoffset = string.Empty.Split(':');
				var groupentry = string.Empty.Split(';');

				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Groups...");
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

					Game.Print(LogType.Debug, this.GetType().ToString(), "Adding Group '{0}' ({1} entries)...".F(groupentry[0], offsets.Count));

					if (!tilegroups.ContainsKey(groupentry[0]))
						tilegroups.Add(groupentry[0], offsets);
					else
						throw new ArgumentException("Undefined Group: '{0}'".F(groupentry[0]));
				}

				Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Groups...".F(tilegroups.Count));

			}
			#endregion

			var imgX = uint.MinValue;
			var imgY = uint.MinValue;

			var targetX = ulong.MinValue;
			var targetY = ulong.MinValue;

			var passable = true;

			#region "Portals"
			this.Portals = new Dictionary<string, Portal>();
			Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Portals...");
			var p_string = inifile.WertLesen("Info", "Portals");
			var num_portals = uint.Parse(p_string);
			var portal_entry = string.Empty.Split(',');
			var portal = new Portal(new Vector2<ulong>(0,0), string.Empty, new Vector2<uint>(tw, th));
			var portal_values = string.Empty.Split(',');

			for (var p = 0; p < num_portals; p++)
			{
				portal_entry = inifile.WertLesen("Portals", "portal{0}".F(p)).Split(';');
				portal_values = portal_entry[0].Split(',');

				portal.Position.X = ulong.Parse(portal_values[0]) * tw;
				portal.Position.Y = ulong.Parse(portal_values[1]) * th;

				if (!this.Portals.ContainsKey(portal_entry[1]))
					this.Portals.Add("{0}-{1}".F(portal_entry[0], portal_entry[1]), portal);
			}

			if (this.Portals.Count > 0)
				Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Portals".F(this.Portals.Count));
			#endregion;

			#region "Layer"
			this.Layers = new Dictionary<string, Layer>();
			Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layers...");

			var layertype = LayerType.Ground;
			var image = string.Empty;

			var tileentry = string.Empty.Split(';');
			var tiletype = TileType.Clear;

			for (var l = uint.MinValue; l < num_layers; l++)
			{
				layertype = (l == uint.MinValue) ? LayerType.Ground : (LayerType)int.Parse(inifile.WertLesen("Layer{0}".F(l), "Type"));

				image = inifile.WertLesen("Layer{0}".F(l), "Image").Split(',')[0];
				var tiles = new Dictionary<string, Tile>();
				Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Tiles...");

				#region "Fill the Map with Ground Tiles"
				if (l == uint.MinValue)
					for (var y = ulong.MinValue; y < this.mapHeight; y++)
						for (var x = ulong.MinValue; x < this.mapWidth; x++)
						{
							image = "clear";

							if (!tilegroups.ContainsKey(image))
								throw new Exception("Undefined definition '{0}'".F(image));

							var imageoffset = new Vector2<uint>(0, 0);

							if (layertype == LayerType.Collision)
								tiletype = TileType.None;
							else
								if (tileentry.Length > 3)
									tiletype = (TileType)int.Parse(tileentry[4]);

							imageoffset = tilegroups[image][rand.Next(0, tilegroups[image].Count - 1)];

							imgX = (imageoffset.X * tw);
							imgY = (imageoffset.Y * th);

							targetX = x;
							targetY = y;


							if (imgX <= tileset.Width && imgY <= tileset.Height)
							{
								var tile = new Tile(ref tileset, new Vector2<uint>(imgX, imgY), new Vector2<uint>(th, tw),
								new Vector2<ulong>(targetX, targetY), layertype, true, this.camera, tiletype);

								foreach (var p in this.Portals.Values)
								{
									if (p.Position.X == targetX && p.Position.Y == targetY)
									{
										tile.Portal = p;
										Game.Print(LogType.Debug, "Map", "Adding Portal at {0}x{1}".F(tile.Portal.Position.X, tile.Portal.Position.Y));
										break;
									}
								}

								if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
									tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
								else
								{
									tiles.Remove("{0}-{1}-{2}".F(targetX, targetY, layertype));
									tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
								}
							}
							else
								throw new ArgumentException("Malformed definition for Tile \"{0}-{1}-{2}\"".F(targetX, targetY, layertype));
						}
				#endregion

				#region "Read and add Tiles from file" 
				for (var ti = ulong.MinValue; ti < (this.mapWidth * this.mapHeight); ti++)
				{
					passable = layertype == LayerType.Collision ? false : true;
					tileentry = inifile.WertLesen("Tiles", "Tile{0}".F(ti)).Split(';');

					if (tileentry.Length == 0 || tileentry == null)
						break;

					if (tileentry.Length > 3 && (LayerType)int.Parse(tileentry[0]) == layertype)
					{
						targetX = uint.Parse(tileentry[1]);
						targetY = uint.Parse(tileentry[2]);

						if (tileentry[3].Contains(":"))
						{
							var imgoffset = tileentry[3].Split(":".ToCharArray());
							imgX = (uint.Parse(imgoffset[0]) * tw);
							imgY = (uint.Parse(imgoffset[1]) * th);
						}
						else
						{
							if (!tilegroups.ContainsKey(image))
								throw new Exception("Undefined definition '{0}'".F(image));

							var imageoffset = tilegroups[image][rand.Next(0, tilegroups[image].Count - 1)];
							imgX = (imageoffset.X * tw);
							imgY = (imageoffset.Y * th);
						}
					}

					if (layertype == LayerType.Collision)
						tiletype = TileType.None;
					else
						if (tileentry.Length > 3)
							tiletype = (TileType)int.Parse(tileentry[4]);
					
					if (imgX <= tileset.Width && imgY <= tileset.Height)
					{
						var tile = new Tile(ref tileset, new Vector2<uint>(imgX, imgY), new Vector2<uint>(th, tw),
							new Vector2<ulong>(targetX, targetY), layertype, true, this.camera, tiletype);

						foreach (var p in this.Portals.Values)
						{
							if (p.Position.X == targetX && p.Position.Y == targetY)
							{
								tile.Portal = p;
								Game.Print(LogType.Debug, "Map", "Adding Portal at {0}x{1}".F(tile.Portal.Position.X, tile.Portal.Position.Y));
								break;
							}
						}

						if (!tiles.ContainsKey("{0}-{1}-{2}".F(targetX, targetY, layertype)))
							tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
						else
						{
							tiles.Remove("{0}-{1}-{2}".F(targetX, targetY, layertype));
							tiles.Add("{0}-{1}-{2}".F(targetX, targetY, layertype), tile);
						}
					}
					else
						throw new IndexOutOfRangeException("Malformed definition for Tile \"{0}-{1}-{2}\"".F(targetX, targetY, layertype));
				}

				if (tiles.Count > 0)
				{
					Game.Print(LogType.Debug, this.GetType().ToString(), "Creating Layer {0}...".F(l));

					this.Layers.Add("Layer{0}".F(l), new Layer(tiles, layertype, this.mapWidth, this.mapHeight));

					Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Tiles to Layer {1}!".F(tiles.Count, l));
				}
				else
					throw new NullReferenceException("No Tiles defined (added) for this Map");
				#endregion
			}

			if (this.Layers.Count > 0)
				Game.Print(LogType.Debug, this.GetType().ToString(), "Added {0} Layers!".F(this.Layers.Count));
			#endregion

			if (player == null)
				return;

			player.Position.X = float.Parse(stpos[0]);
			player.Position.Y = float.Parse(stpos[1]);
		}

		public void Update(ref Player player)
		{
			foreach (var layer in this.Layers.Values)
				layer.Update();
		}

		public int Render(ref IntPtr renderer, Vector2<float> camera, ref IntPtr screen_surface, Vector2<int> screensize, ref Player player)
		{
			var retval = -1;

			this.worldtype = (this.debug ? Worldtype.Debug : Worldtype.Normal);

			foreach (var layer in this.Layers.Values)
				if (layer.LayerType != LayerType.PlayerOverlay)
					retval = layer.Render(ref renderer, ref screen_surface, camera, screensize, this.worldtype);

			if (player != null)
				retval = player.Render(ref renderer, screensize);

			foreach (var layer in this.Layers.Values)
				if (layer.LayerType == LayerType.PlayerOverlay)
					retval = layer.Render(ref renderer, ref screen_surface, camera, screensize, this.worldtype);

			return retval;
		}

		public void Events(ref SDL.SDL_Event e)
		{
			foreach (var layer in this.Layers)
					layer.Value.Events(ref e);
		}

		public void Close()
		{
			foreach (var layer in this.Layers.Values)
				layer.Close();

			this.Layers.Clear();

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
