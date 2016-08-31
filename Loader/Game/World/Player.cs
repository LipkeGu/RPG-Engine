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

		Sprite walk_texture;
		Sprite bike_texture;
		Sprite dive_texture;

		public Vector2<float> camera;
		public Vector2<float> Position;

		Vector2<int> Frames, frame;

		public Player(IntPtr renderer, string filename, Vector2<float> camera, Vector2<float> startPos)
		{
			this.player_file = new IniFile(filename);
			var name = player_file.WertLesen("Info", "Name");
			this.movingtype = MovingType.Walk;
			this.renderer = renderer;
			this.debug = false;

			this.camera = camera;


			var walk_filename = Path.Combine("Data/Actors/", name, this.player_file.WertLesen("Textures", "Walk"));
			var bike_filename = Path.Combine("Data/Actors/", name, this.player_file.WertLesen("Textures", "Bike"));
			var dive_filename = Path.Combine("Data/Actors/", name, this.player_file.WertLesen("Textures", "Dive"));

			this.walk_texture = new Sprite(walk_filename, name, this.renderer);
			this.bike_texture = new Sprite(bike_filename, name, this.renderer);
			this.dive_texture = new Sprite(dive_filename, name, this.renderer);

			this.walk_texture.SourceRect.y += 14;
			this.bike_texture.SourceRect.y += 14;
			this.dive_texture.SourceRect.y += 14;

			this.walk_texture.SourceRect.h -= 14;
			this.bike_texture.SourceRect.h -= 14;
			this.dive_texture.SourceRect.h -= 14;

			var frames = this.player_file.WertLesen("Info", "Frames").Split(',');
			this.Frames = new Vector2<int>(int.Parse(frames[0]), int.Parse(frames[1]));
			this.frame = new Vector2<int>(0,0);
			this.Position = startPos;

			switch (this.movingtype)
			{
				case MovingType.Walk:
					this.walk_texture.FrameSize = this.Frames;

					this.collisionRect.Width = this.walk_texture.FrameSize.X;
					this.collisionRect.Height = this.walk_texture.FrameSize.Y;
					break;
				case MovingType.Bike:
					this.bike_texture.FrameSize = this.Frames;

					this.collisionRect.Width = this.bike_texture.FrameSize.X;
					this.collisionRect.Height = this.bike_texture.FrameSize.Y;
					break;
				case MovingType.Dive:
					this.dive_texture.FrameSize = this.Frames;

					this.collisionRect.Width = this.dive_texture.FrameSize.X;
					this.collisionRect.Height = this.dive_texture.FrameSize.Y;
					break;
				default:
					this.walk_texture.FrameSize = this.Frames;

					this.collisionRect.Width = this.walk_texture.FrameSize.X;
					this.collisionRect.Height = this.walk_texture.FrameSize.Y;

					break;
			}

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
				if (layer.type == 2 && t.collisionRect.IntersectsWith(collisionRect))
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
			var layer = layers["Layer2"];
			var direction = ResolveCollision(ref layer);
			var movingspeed = 0;

			switch (this.movingtype)
			{
				case MovingType.Walk:
					movingspeed = 4;
					break;
				case MovingType.Bike:
					movingspeed = 8;
					break;
				case MovingType.Dive:
					movingspeed = 2;
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
						if (frame.X < 3)
							frame.X += 1;
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
				switch (this.movingtype)
				{
					case MovingType.Walk:
						this.walk_texture.FrameSize = this.Frames;

						this.walk_texture.SourceRect.x = (frame.X * this.walk_texture.FrameSize.X);
						this.walk_texture.SourceRect.y = +14 + (frame.Y * this.walk_texture.FrameSize.Y);
						this.walk_texture.SourceRect.h -= 14;

						this.collisionRect.X = this.walk_texture.TargetRect.x = (int)this.Position.X;
						this.collisionRect.Y = this.walk_texture.TargetRect.y = 10 + (int)this.Position.Y;

						break;
					case MovingType.Bike:
						this.bike_texture.FrameSize = this.Frames;

						this.bike_texture.SourceRect.x = (frame.X * this.bike_texture.FrameSize.X);
						this.bike_texture.SourceRect.y = + 14 + (frame.Y * this.bike_texture.FrameSize.Y);
						this.bike_texture.SourceRect.h -= 14;

						this.collisionRect.X = this.bike_texture.TargetRect.x = (int)this.Position.X;
						this.collisionRect.Y = this.bike_texture.TargetRect.y = (int)this.Position.Y;

						break;
					case MovingType.Dive:
						this.dive_texture.FrameSize = this.Frames;

						this.dive_texture.SourceRect.x = (frame.X * this.dive_texture.FrameSize.X);
						this.dive_texture.SourceRect.y = (frame.Y * this.dive_texture.FrameSize.Y);

						this.collisionRect.X = this.dive_texture.TargetRect.x = (int)this.Position.X;
						this.collisionRect.Y = this.dive_texture.TargetRect.y = (int)this.Position.Y;

						break;
					default:
						break;
				}

				this.camera.X = (this.Position.X * 2);
				this.camera.Y = (this.Position.Y * 2);

				switch (this.movingtype)
				{
					case MovingType.Walk:
						this.walk_texture.Events(e);
						break;
					case MovingType.Bike:
						this.bike_texture.Events(e);
						break;
					case MovingType.Dive:
						this.dive_texture.Events(e);
						break;
					default:
						break;
				}
			}

			#endregion
		}

		public void Update(ref Dictionary<string, Layer> layers)
		{
			switch (this.movingtype)
			{
				case MovingType.Walk:
					this.walk_texture.Update();
					break;
				case MovingType.Bike:
					this.bike_texture.Update();
					break;
				case MovingType.Dive:
					this.dive_texture.Update();
					break;
				default:
					break;
			}
		}

		public void Render(Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			switch (this.movingtype)
			{
				case MovingType.Walk:
					this.collisionRect.Width = this.walk_texture.TargetRect.w;
					this.collisionRect.Height = this.walk_texture.TargetRect.h;

					this.collisionRect.X = this.walk_texture.TargetRect.x = (screensize.X / 2) - (this.walk_texture.FrameSize.X / 2);
					this.collisionRect.Y = this.walk_texture.TargetRect.y = (screensize.Y / 2) - (this.walk_texture.FrameSize.Y / 2);

					this.walk_texture.Render();
					break;
				case MovingType.Bike:
					this.collisionRect.X = this.bike_texture.TargetRect.x = (screensize.X / 2) - (this.bike_texture.FrameSize.X / 2);
					this.collisionRect.Y = this.bike_texture.TargetRect.y = (screensize.Y / 2) - (this.bike_texture.FrameSize.Y / 2);

					this.collisionRect.Width = this.bike_texture.TargetRect.w;
					this.collisionRect.Height = this.bike_texture.TargetRect.h;

					this.bike_texture.Render();

					break;
				case MovingType.Dive:
					this.collisionRect.X = this.dive_texture.TargetRect.x = (screensize.X / 2) - (this.dive_texture.FrameSize.X / 2);
					this.collisionRect.Y = this.dive_texture.TargetRect.y = (screensize.Y / 2) - (this.dive_texture.FrameSize.Y / 2);

					this.collisionRect.Width = this.dive_texture.TargetRect.w;
					this.collisionRect.Height = this.dive_texture.TargetRect.h;

					this.dive_texture.Render();
					break;
				default:
					break;
			}
			
			if (debug)
			{
				Graphic.DrawRect(renderer, (int)collisionRect.X, (int)collisionRect.Y, (int)collisionRect.Width, (int)collisionRect.Height, Color.Red);
				this.collision_overlay.x = (int)collisionRect.X;
				this.collision_overlay.y = (int)collisionRect.Y;
			}
		}

		public void Close()
		{
			this.walk_texture.Close();
			this.bike_texture.Close();
			this.dive_texture.Close();
		}
	}
}
