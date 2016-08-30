using System;
using System.Collections.Generic;
using System.IO;

namespace RPGEngine
{
	public class World
	{
		Vector2<float> camera;
		Map Map;
		Player Player;

		Vector2<int> ScreenBsize;
		Dictionary<string, Map> Maps;
		Dictionary<string, Player> Players;

		IntPtr renderer;

		Worldtype type;

		public Worldtype Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		public World(string Playername, string MapDirectory, string actorsDirectory, IntPtr renderer,
			Vector2<int> screenSize, Worldtype type = Worldtype.Normal)
		{
			this.ScreenBsize = screenSize;
			this.type = type;


			this.renderer = renderer;

			this.Maps = new Dictionary<string, Map>();
			this.Players = new Dictionary<string, Player>();

			this.ReadMaps(MapDirectory);


			if (this.Maps.Count > 0)
			{
				this.Map = this.Maps["TestMap"];
				this.Map.Mapcreated += OnMapCreated;

				this.Map.Load();
			}
			
			this.ReadActors(Playername, actorsDirectory);
		}

		public void ReadActors(string Playername, string path)
		{
			var actorsDir = new DirectoryInfo(path);

			foreach (var fil in actorsDir.GetFiles("{0}.ini".F(Playername), SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					this.Players.Add(Playername, new Player(Playername, this.renderer, "Data/Actors/{0}".F(fil.Name), this.camera, this.Map.StartPosition));
					this.Player = this.Players[Playername];
					break;
				}
		}

		public void ReadMaps(string path)
		{
			var MapsDir = new DirectoryInfo(path);

			foreach (var fil in MapsDir.GetFiles("*.map", SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
					this.Maps.Add(fil.Name.Split('.')[0], new Map(fil.FullName, this.renderer, Environment.CurrentDirectory + "/Data/Tileset/"));

			Game.Print(LogType.Debug, GetType().ToString(), "Found {0} Maps!".F(this.Maps.Count));
		}

		public void OnMapCreated(object source, EventArgs e)
		{
			this.Map.loaded = true;
			Game.Print(LogType.Debug, GetType().ToString(), "Map loaded!");
		}

		public void Update()
		{
			this.Player.Update(ref this.Map.Layers);

			if (this.Map != null)
				this.Map.Update();
		}

		public void Events(SDL2.SDL.SDL_Event e)
		{
			if (this.Map != null)
			{
				this.Map.Events(e);
				this.Player.Events(e, this.Map.TileSize, ref this.Map.Layers);
			}
		}

		public void Render(Vector2<int> screensize, IntPtr screen_surface)
		{
			if (this.Player == null)
				return;

			this.ScreenBsize = screensize;
			this.camera = this.Player.camera;

			if (this.Map != null)
				this.Map.Render(this.camera, screen_surface, this.ScreenBsize, ref this.Player, this.type);
		}

		public void Close()
		{
			this.Player.Close();

			if (this.Map != null)
				this.Map.Close();
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
