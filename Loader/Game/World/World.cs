using System;
using System.Collections.Generic;
using System.IO;

namespace RPGEngine
{
	public class World
	{
		Vector2<float> camera;
		Map map;
		Player player;

		Vector2<int> ScreenBsize;
		Dictionary<string, Map> Maps;

		Worldtype worldtype;

		public Worldtype WorldType
		{
			get { return this.worldtype; }
			set { this.worldtype = value; }
		}

		public World(string Playername, string MapDirectory, string actorsDirectory, IntPtr renderer,
			Vector2<int> screenSize, Worldtype worldtype = Worldtype.Normal)
		{
			this.ScreenBsize = screenSize;
			this.worldtype = worldtype;
			this.Maps = new Dictionary<string, Map>();

			this.ReadMaps(MapDirectory, renderer);
			
			if (this.Maps.Count > 0)
			{
				this.map = this.Maps["TestMap"];
				this.map.Mapcreated += OnMapCreated;

				this.map.Load(renderer);
			}
			
			this.ReadActors(Playername, actorsDirectory, renderer);
		}

		public void ReadActors(string Playername, string path, IntPtr renderer)
		{
			var actorsDir = new DirectoryInfo(path);
			var players = new Dictionary<string, Player>();

			foreach (var fil in actorsDir.GetFiles("{0}.ini".F(Playername), SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					players.Add(Playername, new Player(renderer, "Data/Actors/{0}".F(fil.Name), this.camera, this.map.StartPosition));
					this.player = players[Playername];
					break;
				}
				else
					throw new FileNotFoundException("File Not found: Data/Actors/{0}".F(fil.Name));
		}

		public void ReadMaps(string path, IntPtr renderer)
		{
			var MapsDir = new DirectoryInfo(path);
			var mapname = string.Empty;

			foreach (var fil in MapsDir.GetFiles("*.map", SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					mapname = fil.Name.Split('.')[0];
					this.Maps.Add(mapname, new Map(mapname, fil.FullName, "{0}/Data/Tileset/".F(Environment.CurrentDirectory)));
				}

			Game.Print(LogType.Debug, GetType().ToString(), "Found {0} Map(s)!".F(this.Maps.Count));
		}

		public void OnMapCreated(object source, EventArgs e)
		{
			Game.Print(LogType.Debug, GetType().ToString(), "Map '{0}' loaded!".F(this.map.Name));
		}

		public void Update()
		{
			if (this.map == null || this.player == null)
				return;

			this.player.Update();
			this.map.Update();
		}

		public void Events(SDL2.SDL.SDL_Event e)
		{
			if (this.map == null)
				return;

			this.map.Events(e);
			this.player.Events(e, ref this.map.Layers);
		}

		public void Render(Vector2<int> screensize, IntPtr screen_surface, ref IntPtr renderer)
		{
			if (this.player == null || this.map == null)
				return;

			this.ScreenBsize = screensize;
			this.camera = this.player.Camera;

			this.map.Render(ref renderer, this.camera, ref screen_surface, this.ScreenBsize, ref this.player, this.worldtype);
		}

		public void Close()
		{
			if (this.player != null)
				this.player.Close();

			if (this.map != null)
				this.map.Close();
		}

		/*
		public void CreateMap(int width, int height, int tw, int th)
		{
			var tiles = new Dictionary<string, Tile>();

			Directory.CreateDirectory("Data/Maps/TestMap");

			this.Map = new Map("Data/Maps/TestMap/TestMap.map", this.renderer, "");
			this.Map.tileset = SDL2.SDL_image.IMG_LoadTexture(this.renderer, "Data/Tileset/town_0.png");
			this.Map.Layers = new Dictionary<string, Layer>();

			var src_posX = 3F * tw;
			var src_posY = 1F * th;

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					if (y == 0)
					{
						src_posX = 5 * tw;
						src_posY = 3 * th;
					}

					tiles.Add("{0}-{1}-{2}".F(x, y, 0), new Tile(this.Map.tileset, new Vector2(src_posX, src_posY), th, tw, new Vector2(x, y), 0, true, this.camera));
				}

				src_posX = 3F * tw;
				src_posY = 1F * th;
			}

			this.Map.Layers.Add("base", new Layer(tiles, 0, width, height));
			Console.Beep();

			var inifile = new IniFile("Data/Maps/TestMap/TestMap.map");

			inifile.WertSchreiben("Info", "Layers", "{0}".F(this.Map.Layers.Count));
			inifile.WertSchreiben("Info", "Tileset", "routes_0");

			inifile.WertSchreiben("Info", "Width", "{0}".F(width));
			inifile.WertSchreiben("Info", "Height", "{0}".F(height));

			inifile.WertSchreiben("Info", "TWidth", "{0}".F(tw));
			inifile.WertSchreiben("Info", "THeight", "{0}".F(th));

			inifile.WertSchreiben("Info", "Tiles", "{0}".F(tiles.Count));
			inifile.WertSchreiben("Info", "Groups", "0");

			inifile.WertSchreiben("Layer0", "Type", "0".F(tiles.Count));
			inifile.WertSchreiben("Layer0", "Image", "clear".F(tiles.Count));
			var i = 0;

			foreach (var t in tiles)
			{
				if (t.Value.type != 0)
					inifile.WertSchreiben("Tiles", "Tile{0}".F(i), "{0};{1};{2};clear".F(t.Value.type, (t.Value.TargetRect.x / tw), (t.Value.TargetRect.y / th)));
				i++;
			}
		}
		*/
	}
}
