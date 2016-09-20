using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;

namespace RPGEngine
{
	public class Player
	{
		bool debug;

		IniFile player_file;
		MovingType movingtype;
		Direction move_direction;

		RectangleF collisionRect, movetypeRect;

		Sprite texture;

		public Vector2<float> Location, Position;
		
		string name;

		Vector2<int> frame;

		public Player(ref IntPtr renderer, string filename, Vector2<float> camera, Vector2<float> startPos)
		{
			this.player_file = new IniFile(filename);
			this.name = player_file.WertLesen("Info", "Name");
			this.movingtype = MovingType.Walk;
			this.debug = false;

			if (camera.X == 0 && camera.Y == 0)
				this.Location = startPos;
			else
				this.Location = camera;

			var frames = this.player_file.WertLesen("Info", "Frames").Split(',');

			var texture_entry_file = this.player_file.WertLesen("Textures", "Walk");
			var texture_entry_offset = this.player_file.WertLesen("Info", "Offset").Split(',');

			var texture_frames = new Vector2<int>(int.Parse(frames[0]), int.Parse(frames[1]));
			var texture_offset = new Vector2<int>(int.Parse(texture_entry_offset[0]), int.Parse(texture_entry_offset[1]));

			this.texture = Engine.GetTexture(Path.Combine("Data/Actors/", this.name, 
				texture_entry_file), ref renderer, ref texture_frames, ref texture_offset);

			this.frame = new Vector2<int>(0, 0);
			this.Position = startPos;

			this.texture.FrameSize = this.texture.Frames;
			this.movetypeRect.Width = this.collisionRect.Width = this.texture.FrameSize.X;
			this.movetypeRect.Height = this.collisionRect.Height = this.texture.FrameSize.Y;
		}

		Direction ResolveCollision(ref Layer layer, ref Direction m_direction, SDL.SDL_Keycode key)
		{
			var p_left = this.collisionRect.Left;
			var p_right = this.collisionRect.Right;
			var p_top = this.collisionRect.Top;
			var p_bottom = this.collisionRect.Bottom;

			var dir = Direction.None;

			var t = (from ct in layer.Tiles.Values
					 where ct.CollisionBox.IntersectsWith(this.collisionRect)
					 where ct.Type == LayerType.Collision
					 select ct).FirstOrDefault();

			if (t == null)
				return dir;

			if (p_top <= t.CollisionBox.Bottom && m_direction == Direction.Up) // Actor is moving from bottom to top
			{
				dir = Direction.Up;
			}

			if (p_bottom <= t.CollisionBox.Top && m_direction == Direction.Down) // Actor is moving to bottom from top
			{
				dir = Direction.Down;
			}

			if (p_left >= t.CollisionBox.Right && m_direction == Direction.Left)
			{
				dir = Direction.Left;
			}

			if (p_right <= t.CollisionBox.Left && m_direction == Direction.Right)
			{
				dir = Direction.Right;
			}

			return dir;
		}

		public void Events(ref SDL.SDL_Event e, ref Dictionary<string, Layer> layers)
		{
			var direction = Direction.None;
			var speed_walk = 2;
			var speed_bike = 2;
			var speed_dive = 0;
			var movingspeed = 0;
			var tiletype = TileType.None;

			if (layers.ContainsKey("Layer2"))
			{
				var col_layer = layers["Layer2"];
				direction = ResolveCollision(ref col_layer, ref this.move_direction, e.key.keysym.sym);
			}
			else
				Game.Print(LogType.Error, this.GetType().ToString(), "Undefined Definition: 'Layer2'");

			var ground_layer = layers["Layer0"];

			this.GetTileType(ref ground_layer, out tiletype, out speed_walk, 
				out speed_bike, out speed_dive, out movingtype);

			switch (this.movingtype)
			{
				case MovingType.Walk:
					movingspeed = speed_walk;
					break;
				case MovingType.Bike:
					movingspeed = speed_bike;
					break;
				case MovingType.Dive:
					movingspeed = speed_dive;
					break;
				default:
					movingspeed = 0;
					break;
			}

			switch (e.type)
			{
				case SDL.SDL_EventType.SDL_KEYDOWN:
					#region "Movement"
					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
					{
						frame.Y = 3;
						this.move_direction = Direction.Up;

						if (this.Position.Y > -1 && this.Position.Y < float.MaxValue)
						{
							if (direction == move_direction)
								this.Position.Y += movingspeed;
							else
								this.Position.Y -= movingspeed;
						}
						else
							this.Position.Y = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
					{
						frame.Y = 0;
						this.move_direction = Direction.Down;

						if (this.Position.Y > -1 && this.Position.Y < float.MaxValue)
						{
							if (direction == move_direction)
								this.Position.Y -= movingspeed;
							else
								this.Position.Y += movingspeed;
						}
						else
							this.Position.Y = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT)
					{
						frame.Y = 1;
						this.move_direction = Direction.Left;

						if (this.Position.X > -1 && this.Position.X < float.MaxValue)
						{
							if (direction == move_direction)
								this.Position.X += movingspeed;
							else
								this.Position.X -= movingspeed;
						}
						else
							this.Position.X = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT)
					{
						frame.Y = 2;
						this.move_direction = Direction.Right;

						if (this.Position.X > -1 && this.Position.X < float.MaxValue)
						{
							if (direction == move_direction)
								this.Position.X -= movingspeed;
							else
								this.Position.X += movingspeed;
						}
						else
							this.Position.X = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT ||
					e.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
					{
						if (frame.X < (this.texture.Frames.X - 1))
						{
							if (movingspeed > 0)
								frame.X += 1;
							else
								frame.X = 1;
						}
						else
							frame.X = 1;
					}
					#endregion
					break;
				case SDL.SDL_EventType.SDL_KEYUP:
					#region "Movement"
					frame.X = 0;

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
						frame.Y = 3;

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
						frame.Y = 0;

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT)
						frame.Y = 1;

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT)
						frame.Y = 2;
					#endregion
					break;
				default:
					break;
			}

			#region "Movement"
			if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT ||
				e.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
			{
				this.texture.FrameSize = this.texture.Frames;

				this.texture.SourceRect.x = (frame.X * this.texture.FrameSize.X);
				this.texture.SourceRect.y = (frame.Y * this.texture.FrameSize.Y);

				this.movetypeRect.X = this.collisionRect.X = this.texture.TargetRect.x = (int)this.Position.X;
				this.movetypeRect.X = this.collisionRect.Y = this.texture.TargetRect.y = (int)this.Position.Y;

				this.Location.X = (this.Position.X * 2);
				this.Location.Y = (this.Position.Y * 2);
			}

			#endregion
		}

		public void Update(ref IntPtr renderer)
		{
			texture = Engine.GetTexture(Path.Combine("Data/Actors/", this.name, this.player_file.WertLesen("Textures",
				this.movingtype.ToString())), ref renderer, ref this.texture.Frames, ref this.texture.Offset);

			this.texture.Update();
		}

		public int Render(ref IntPtr renderer, Vector2<int> screensize, Vector2<int> size)
		{
			var retval = -1;

			this.collisionRect.Width = (size.X - 4);
			this.collisionRect.Height = (size.Y - 4);

			this.movetypeRect.Width = size.X;
			this.movetypeRect.Height = size.Y;


			this.texture.TargetRect.x = (screensize.X / 2) - (this.texture.FrameSize.X / 2);
			this.texture.TargetRect.y = (screensize.Y / 2) - (this.texture.FrameSize.Y / 2);

			this.collisionRect.X = this.texture.Offset.X + this.texture.TargetRect.x;
			this.collisionRect.Y = this.texture.Offset.Y + +this.texture.TargetRect.y;

			this.movetypeRect.X = this.texture.TargetRect.x;
			this.movetypeRect.Y = this.texture.TargetRect.y;

			retval = this.texture.Render(ref renderer);

			if (debug)
			{
				retval = Video.DrawRect(ref renderer, (int)collisionRect.X, (int)collisionRect.Y,
					(int)collisionRect.Width, (int)collisionRect.Height, Color.Red);
			}

			return retval;
		}

		public void Close()
		{
			this.texture.Close();
		}

		public void GetTileType(ref Layer layer, out TileType tiletype, out int walk_speed,
			out int bike_speed, out int dive_speed, out MovingType movingtype)
		{
			var tiles = (from t in layer.Tiles.Values
						 where t.Type == LayerType.Ground || t.Type == LayerType.Ground_Overlay
						 select t).ToList();

			foreach (var tile in tiles)
				if (this.movetypeRect.IntersectsWith(tile.MovingBox))
				{
					tiletype = tile.Tiletype;
					walk_speed = tile.walk_speed;
					bike_speed = tile.bike_speed;
					dive_speed = tile.dive_speed;
					movingtype = tile.movingtype;

					return;
				}
			
			tiletype = TileType.None;
			movingtype = MovingType.Walk;
			walk_speed = 1;
			bike_speed = 1;
			dive_speed = 1;
		}

		public RectangleF MovingBox
		{
			get { return this.movetypeRect; }
		}

		public bool DebugMode
		{
			get { return this.debug; }
			set { this.debug = value; }
		}
	}
}
