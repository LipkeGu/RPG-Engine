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
		
		public World(string Playername, IntPtr renderer, Vector2<int> screenSize)
		{
			this.Maps = new Dictionary<string, Map>();
			this.debug = false;
			this.renderer = renderer;

			this.ReadMaps("Data/Maps/");
			
			if (this.Maps.Count > 0)
			{
				this.map = this.Maps["TestMap"];
				this.map.Mapcreated += OnMapCreated;

				this.map.Load(ref this.renderer, ref player);
				this.ReadActors(Playername, "Data/Actors/", ref this.renderer);
			}
		}

		private void ReadActors(string Playername, string path, ref IntPtr renderer)
		{
			var actorsDir = new DirectoryInfo(path);
			var players = new Dictionary<string, Player>();

			foreach (var fil in actorsDir.GetFiles("{0}.ini".F(Playername), SearchOption.AllDirectories))
				if (fil.Length > 0 && fil.Exists)
				{
					players.Add(Playername, new Player(ref renderer, "{0}".F(fil.FullName), 
						this.camera, this.map.StartPosition));

					this.player = players[Playername];
					break;
				}
				else

					throw new FileNotFoundException("File Not found: {0}".F(fil.FullName));
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
		}

		private void OnMapCreated(object source, EventArgs e)
		{
			Game.Print(LogType.Notice, GetType().ToString(), "Map '{0}' loaded!".F(this.map.Name));
		}

		public void Update()
		{
			if (this.player != null)
			{
				this.player.DebugMode = this.debug;
				this.player.Update(ref this.renderer);
				this.camera = this.player.Location;
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

			if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F7 && this.debug)
			{
				var pc = this.Maps["PokeCenter"];
				var x = new ChangeMapLogic(ref this.map, ref pc, ref player, ref this.renderer);
				return;
			}

			if (this.map != null)
			{
				this.map.Events(ref e);

				var portal = (from p in map.Portals.Values
							  where p.Position.IntersectsWith(this.player.MovingBox)
							  where !string.IsNullOrEmpty(p.Map)
							  where p.Enabled
							  select p).ToList();

				if (portal.Count > 0)
				{
					var m = this.Maps[portal[0].Map];
					var x = new ChangeMapLogic(ref this.map, ref m, ref this.player, ref this.renderer);
				}
			}
		}

		public int Render(ref Vector2<int> screensize, ref IntPtr screen_surface, ref IntPtr renderer)
		{
			var retval = -1;
			
			retval = this.map.Render(ref renderer, ref this.camera, ref screen_surface, ref screensize, ref this.player);

			return retval;
		}

		public void Close()
		{
			if (this.player != null)
				this.player.Close();

			if (this.map != null)
				this.map.Close();

			foreach (var map in this.Maps.Values)
			{
				map.Close();
			}
		}
	}
}
