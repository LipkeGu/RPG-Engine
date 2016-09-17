using System;
using System.Drawing;
using System.Linq;

using static SDL2.SDL;

namespace RPGEngine
{
	public class Tile
	{
		SDL_Rect TargetRect, SourceRect, offset;
		Vector2<float> camera;

		Sprite image;
		Portal portal;

		bool debug, passable, mouse_focus;

		RectangleF collisionRect, movetypeRect;
		TileType tiletype;
		public MovingType movingtype = MovingType.Walk;

		public int walk_speed, dive_speed, bike_speed;
		LayerType layertype;

		public Tile(ref Sprite image, Vector2<uint> source, Vector2<uint> size, Vector2<ulong> target,
			LayerType layertype, bool passable, Vector2<float> camera, TileType tiletype)
		{
			this.camera = camera;
			this.image = image;

			this.passable = passable;
			this.layertype = layertype;
			this.tiletype = tiletype;

			switch (this.layertype)
			{
				case LayerType.Ground:
					switch (this.tiletype)
					{
						case TileType.Clear:
						case TileType.Grass:
							this.movingtype = MovingType.Walk;
							break;
						case TileType.Water:
							this.movingtype = MovingType.Dive;
							break;
						case TileType.Road:
							this.movingtype = MovingType.Bike;
							break;
						case TileType.None:
							this.movingtype = MovingType.Walk;
							break;
						default:
							break;
					}
					break;
				case LayerType.Ground_Overlay:
					switch (this.tiletype)
					{
						case TileType.Clear:
						case TileType.Grass:
							this.movingtype = MovingType.Walk;
							break;
						case TileType.Water:
							this.movingtype = MovingType.Dive;
							break;
						case TileType.Road:
							this.movingtype = MovingType.Bike;
							break;
						case TileType.None:
							this.movingtype = MovingType.Walk;
							break;
						default:
							break;
					}
					break;
				case LayerType.Collision:
					this.tiletype = TileType.None;
					break;
				case LayerType.PlayerOverlay:
					switch (this.tiletype)
					{
						case TileType.Clear:
						case TileType.Grass:
							this.movingtype = MovingType.Walk;
							break;
						case TileType.Water:
							this.movingtype = MovingType.Dive;
							break;
						case TileType.Road:
							this.movingtype = MovingType.Bike;
							break;
						case TileType.None:
							this.movingtype = MovingType.Walk;
							break;
						default:
							break;
					}
					break;
				default:
					switch (this.tiletype)
					{
						case TileType.Clear:
						case TileType.Grass:
							this.movingtype = MovingType.Walk;
							break;
						case TileType.Water:
							this.movingtype = MovingType.Dive;
							break;
						case TileType.Road:
							this.movingtype = MovingType.Bike;
							break;
						case TileType.None:
							this.movingtype = MovingType.Walk;
							break;
						default:
							break;
					}
					break;
			}

			this.TargetRect.x = (int)(target.X * size.Y);
			this.TargetRect.y = (int)(target.Y * size.Y);

			this.SourceRect.x = (int)source.X;
			this.SourceRect.y = (int)source.Y;

			this.offset.h = this.TargetRect.h = this.SourceRect.h = (int)size.X;
			this.offset.w = this.TargetRect.w = this.SourceRect.w = (int)size.Y;

			this.mouse_focus = false;

			switch (this.tiletype)
			{
				case TileType.Clear:
					this.walk_speed = 4;
					this.bike_speed = 2;
					this.dive_speed = 0;
					break;
				case TileType.Grass:
					this.walk_speed = 2;
					this.bike_speed = 1;
					this.dive_speed = 0;
					break;
				case TileType.Water:
					this.walk_speed = 1;
					this.bike_speed = 0;
					this.dive_speed = 2;
					break;
				case TileType.Road:
					this.walk_speed = 4;
					this.bike_speed = 6;
					this.dive_speed = 0;
					break;
				case TileType.None:
				default:
					this.walk_speed = 1;
					this.bike_speed = 1;
					this.dive_speed = 1;
					break;
			}

			this.movetypeRect.Height = this.offset.h;
			this.movetypeRect.Width = this.offset.w;

			this.collisionRect.Height = (this.offset.h - 2);
			this.collisionRect.Width = (this.offset.w - 2);

			this.movetypeRect.X = this.offset.x;
			this.movetypeRect.Y = this.offset.y;

			this.collisionRect.X = (this.offset.x + 1);
			this.collisionRect.Y = (this.offset.y + 1);
		}

		public LayerType Type
		{
			get { return this.layertype; }
			set { this.layertype = value; }
		}

		public bool Passable
		{
			get { return this.passable; }
		}

		public TileType Tiletype
		{
			get { return this.tiletype; }
		}

		public void Update()
		{
			if (Engine.MousePosition.X >= this.collisionRect.Left && Engine.MousePosition.X <= this.collisionRect.Right &&
				Engine.MousePosition.Y >= this.collisionRect.Top && Engine.MousePosition.Y <= this.collisionRect.Bottom)
				this.mouse_focus = true;
			else
				this.mouse_focus = false;
		}

		public void Events(ref SDL_Event e)
		{
		}

		public int Render(ref IntPtr renderer, ref IntPtr screen_surface, Vector2<float> camera, Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			var retval = -1;

			this.camera = camera;
			this.offset.x = (int)(TargetRect.x - this.camera.X + (screensize.X / 2));
			this.offset.y = (int)(TargetRect.y - this.camera.Y + (screensize.Y / 2));

			this.movetypeRect.X = this.offset.x;
			this.movetypeRect.Y = this.offset.y;

			if (this.portal != null)
			{
				this.portal.Position.X = this.movetypeRect.X;
				this.portal.Position.Y = this.movetypeRect.Y;
			}
			
			this.collisionRect.X = this.offset.x;
			this.collisionRect.Y = this.offset.y;

			retval = SDL_RenderCopy(renderer, this.image.Image, ref this.SourceRect, ref this.offset);

			if (type == Worldtype.Debug && this.mouse_focus == false)
			{
				var color = Color.Transparent;

				switch (this.tiletype)
				{
					case TileType.Clear:
						color = Color.LightGreen;
						break;
					case TileType.Grass:
						color = Color.DarkGreen;
						break;
					case TileType.Water:
						color = Color.LightBlue;
						break;
					case TileType.Road:
						color = Color.DarkGray;
						break;
					case TileType.None:
						color = Color.Red;
						break;
					default:
						break;
				}

				retval = Video.DrawRect(ref renderer, this.offset.x, this.offset.y, this.offset.w, this.offset.h, color);

				if (this.mouse_focus)
					retval = Video.DrawRect(ref renderer, this.offset.x, this.offset.y, this.offset.w, this.offset.h, Color.DarkOrange, true);
			}

			if (this.portal != null)
				Video.DrawRect(ref renderer, (int)this.portal.Position.X, (int)this.portal.Position.Y, 
					(int)this.portal.Position.Width, (int)this.portal.Position.Height, Color.DeepSkyBlue);

			return retval;
		}

		public void Close()
		{
		}

		public RectangleF CollisionBox
		{
			get { return this.collisionRect; }
		}

		public RectangleF MovingBox
		{
			get { return this.movetypeRect; }
		}

		public Vector2<int> Cords
		{
			get {
				var c = new Vector2<int>(this.offset.x, this.offset.y);
				return c;
			}
		}

		public Portal Portal
		{
			get { return this.portal; }
			set { this.portal = value; }
		}

		public bool DebugMode
		{
			get { return this.debug; }
			set { this.debug = value; }
		}
	}
}
