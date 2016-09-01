using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RPGEngine
{
	public class Player
	{
		bool debug;

		IniFile player_file;
		MovingType movingtype;
		IntPtr renderer;

		RectangleF collisionRect;
		SDL.SDL_Rect collision_overlay;

		Sprite texture;
	
		public Vector2<float> Camera;
		public Vector2<float> Position;

		string name;

		int speed_walk = 2;
		int speed_bike = 2;
		int speed_dive = 0;

		Vector2<int> frame;

		public Player(IntPtr renderer, string filename, Vector2<float> camera, Vector2<float> startPos)
		{
			this.player_file = new IniFile(filename);
			this.name = player_file.WertLesen("Info", "Name");
			this.movingtype = MovingType.Walk;
			this.renderer = renderer;
			this.debug = false;

			if (camera.X == 0 && camera.Y == 0)
				this.Camera = startPos;
			else
				this.Camera = camera;
			
			var frames = this.player_file.WertLesen("Info", "Frames").Split(',');
			this.texture = Engine.GetTexture(Path.Combine("Data/Actors/", this.name, this.player_file.WertLesen("Textures", "Walk")), 
				renderer, new Vector2<int>(int.Parse(frames[0]), int.Parse(frames[1])));

			this.frame = new Vector2<int>(0,0);
			this.Position = startPos;

			this.texture.FrameSize = this.texture.Frames;
			this.collisionRect.Width = this.texture.FrameSize.X;
			this.collisionRect.Height = this.texture.FrameSize.Y;

			this.collision_overlay.w = (int)this.collisionRect.Width;
			this.collision_overlay.h = (int)this.collisionRect.Height;
		}

		Direction ResolveCollision(ref Layer layer)
		{
			var p_left = this.collisionRect.Left;
			var p_right = this.collisionRect.Right;
			var p_top = this.collisionRect.Top;
			var p_bottom = this.collisionRect.Bottom;

			foreach (var t in layer.Tiles.Values)
			{
				if (layer.LayerType == LayerType.Collision && t.collisionRect.IntersectsWith(collisionRect))
				{
					if (p_top < t.collisionRect.Bottom + 3)
						return Direction.Up;

					if (p_bottom > t.collisionRect.Top - 3)
						return Direction.Down;

					if (p_left > (t.collisionRect.Right + 3))
						return Direction.Left;

					if (p_right < (t.collisionRect.Left - 3))
						return Direction.Right;
				}
			}

			return Direction.None;
		}
		
		public void Events(SDL.SDL_Event e, ref Dictionary<string, Layer> layers)
		{
			var col_layer = layers["Layer2"];
			var ground_layer = layers["Layer0"];

			var direction = ResolveCollision(ref col_layer);
			var tiletype = TileType.Clear;

			this.GetTileType(ref ground_layer, out tiletype, out speed_walk, out speed_bike, out speed_dive);
			var movingspeed = 0;

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
						if (this.Position.Y > -1 && this.Position.Y < float.MaxValue)
						{
							switch (direction)
							{
								case Direction.Up:
									this.Position.Y += movingspeed;
									break;
								case Direction.Down:
									this.Position.Y -= movingspeed;
									break;
								case Direction.Left:
								case Direction.Right:
								case Direction.None:
								default:
									this.Position.Y -= movingspeed;
									break;
							}
						}
						else
							this.Position.Y = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
					{
						frame.Y = 0;
						if (this.Position.Y > -1 && this.Position.Y < float.MaxValue)
						{
							switch (direction)
							{
								case Direction.Down:
									this.Position.Y -= movingspeed;
									break;
								case Direction.Up:
									this.Position.Y += movingspeed;
									break;
								case Direction.Left:
								case Direction.Right:
								case Direction.None:
								default:
									this.Position.Y += movingspeed;
									break;
							}
						}
						else
							this.Position.Y = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT)
					{
						frame.Y = 1;
						if (this.Position.X > -1 && this.Position.X < float.MaxValue)
						{
							switch (direction)
							{
								case Direction.Left:
									this.Position.X += movingspeed;
									break;
								case Direction.Right:
									this.Position.X -= movingspeed;
									break;
								case Direction.Up:
								case Direction.Down:
								case Direction.None:
								default:
									this.Position.X -= movingspeed;
									break;
							}

						}
						else
							this.Position.X = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT)
					{
						frame.Y = 2;
						if (this.Position.X >= 0 && this.Position.X < float.MaxValue)
						{
							switch (direction)
							{
								case Direction.Right:
									this.Position.X -= movingspeed;
									break;
								case Direction.Left:
									this.Position.X += movingspeed;
									break;
								case Direction.Up:
								case Direction.Down:
								case Direction.None:
								default:
									this.Position.X += movingspeed;
									break;
							}
						}
						else
							this.Position.X = 0;
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F4)
					{
						switch (this.movingtype)
						{
							case MovingType.Walk:
								this.movingtype = MovingType.Bike;
								break;
							case MovingType.Bike:
								this.movingtype = MovingType.Dive;
								break;
							case MovingType.Dive:
								this.movingtype = MovingType.Walk;
								break;
							default:
								break;
						}
					}

					if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_F8)
					{
						if (this.debug)
							this.debug = false;
						else
							this.debug = true;
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
					return;
			}

			#region "Movement"
			if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT ||
				e.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP || e.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
			{

				this.texture.FrameSize = this.texture.Frames;

				this.texture.SourceRect.x = (frame.X * this.texture.FrameSize.X);
				this.texture.SourceRect.y = (frame.Y * this.texture.FrameSize.Y);

				this.collisionRect.X = this.texture.TargetRect.x = (int)this.Position.X;
				this.collisionRect.Y = this.texture.TargetRect.y = (int)this.Position.Y;

				this.Camera.X = (this.Position.X * 2);
				this.Camera.Y = (this.Position.Y * 2);

				this.texture.Events(e);
			}

			#endregion
		}

		public void Update()
		{
			texture = Engine.GetTexture(Path.Combine("Data/Actors/", this.name, this.player_file.WertLesen("Textures", this.movingtype.ToString())), this.renderer, new Vector2<int>(4, 4));
			this.texture.Update();
		}

		public void Render(Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			this.collisionRect.Width = this.texture.TargetRect.w;
			this.collisionRect.Height = this.texture.TargetRect.h;

			this.collisionRect.X = this.texture.TargetRect.x = (screensize.X / 2) - (this.texture.FrameSize.X / 2);
			this.collisionRect.Y = this.texture.TargetRect.y = (screensize.Y / 2) - (this.texture.FrameSize.Y / 2);

			this.texture.Render();

			if (debug)
			{
				Video.DrawRect(renderer, (int)collisionRect.X, (int)collisionRect.Y, (int)collisionRect.Width, (int)collisionRect.Height, Color.Red);
				this.collision_overlay.x = (int)collisionRect.X;
				this.collision_overlay.y = (int)collisionRect.Y;
			}
		}

		public void Close()
		{
			this.texture.Close();
		}

		public void GetTileType(ref Layer layer, out TileType tiletype, out int walk_speed, out int bike_speed, out int dive_speed)
		{
			var w_speed = 0;
			var b_speed = 0;
			var d_speed = 0;
			var ttype = TileType.Clear;

			foreach (var tile in layer.Tiles.Values)
				if (this.collisionRect.IntersectsWith(tile.collisionRect) && tile.Type == LayerType.Ground)
				{
					ttype = tile.Tiletype;
					w_speed = tile.walk_speed;
					b_speed = tile.bike_speed;
					d_speed = tile.dive_speed;

					break;
				}

			tiletype = ttype;

			walk_speed = w_speed;
			bike_speed = b_speed;
			dive_speed = d_speed;
		}
	}
}
