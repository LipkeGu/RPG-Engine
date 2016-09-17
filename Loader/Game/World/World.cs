using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace RPGEngine
{
	public class World
	{
		Vector2<float> camera;
		Map map;
		Player player;
		bool debug;
		IntPtr renderer;

		Dictionary<string, Map> Maps;
		
		public World(string Playername, string MapDirectory, string actorsDirectory, IntPtr renderer, Vector2<int> screenSize)
		{
			this.Maps = new Dictionary<string, Map>();
			this.debug = false;
			this.renderer = renderer;

			this.ReadMaps(MapDirectory);
			
			if (this.Maps.Count > 0)
			{
				this.map = this.Maps["TestMap"];
				this.map.Mapcreated += OnMapCreated;

				this.map.Load(ref this.renderer, ref player);
				this.ReadActors(Playername, actorsDirectory, ref this.renderer);
			}
		}

		private void ReadActors(string Playername, string path, ref IntPtr renderer)
		{
			var actorsDir = new DirectoryInfo(path);
			var players = new Dictionary<string, Player>();

			foreach (var fil in actorsDir.GetFiles("{0}.ini".F(Playername), SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					players.Add(Playername, new Player(ref renderer, "Data/Actors/{0}".F(fil.Name), 
						this.camera, this.map.StartPosition));

					this.player = players[Playername];
					break;
				}
				else
					throw new FileNotFoundException("File Not found: Data/Actors/{0}".F(fil.Name));
		}

		private void ReadMaps(string path)
		{
			var MapsDir = new DirectoryInfo(path);
			var mapname = string.Empty;

			foreach (var fil in MapsDir.GetFiles("*.map", SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					mapname = fil.Name.Split('.')[0];
					this.Maps.Add(mapname, new Map(mapname, fil.FullName, Worldtype.Normal));
				}

			Game.Print(LogType.Debug, GetType().ToString(), "Found {0} Map(s)!".F(this.Maps.Count));
		}

		private void OnMapCreated(object source, EventArgs e)
		{
			Game.Print(LogType.Debug, GetType().ToString(), "Map '{0}' loaded!".F(this.map.Name));
		}

		public void Update()
		{
			if (this.player != null)
			{
				this.player.DebugMode = this.debug;
				this.player.Update(ref this.renderer);
			}

			if (this.map != null)
			{
				this.map.DebugMode = this.debug;
				this.map.Update(ref this.player);

			}
			else
				Game.Print(LogType.Debug, "Map", "Map is null");
		}

		public void Events(ref SDL.SDL_Event e)
		{
			this.player.Events(ref e, ref this.map.Layers);

			if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F8)
				this.debug = this.debug ? false : true;

			if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F7)
			{
				var pc = Maps["PokeCenter"];
				var x = new ChangeMapLogic(ref this.map, ref pc, ref player, ref this.renderer);
				return;
			}

			if (this.map != null)
			{
				this.map.Events(ref e);
				var portal = (from p in map.Portals.Values
							  where p.Position.IntersectsWith(this.player.MovingBox)
							  where p.Map != null
							  select p).ToList();

				if (portal.Count > 0)
				{
					var m = this.Maps[portal[0].Map];
					var x = new ChangeMapLogic(ref this.map, ref m, ref this.player, ref this.renderer);
				}
			}
		}

		public int Render(Vector2<int> screensize, IntPtr screen_surface, ref IntPtr renderer)
		{
			var retval = -1;

			this.camera = this.player.Camera;
			retval = this.map.Render(ref renderer, this.camera, ref screen_surface, screensize, ref this.player);

			return retval;
		}

		public void Close()
		{
			if (this.player != null)
				this.player.Close();

			if (this.map != null)
				this.map.Close();
		}
	}
}
