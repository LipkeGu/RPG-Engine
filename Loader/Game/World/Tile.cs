﻿using System;
using System.Drawing;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Tile
	{
		public SDL_Rect TargetRect, SourceRect, offset;
		Vector2<float> camera;

		Sprite image;

		bool debug;
		bool passable;
		RectangleF collisionRect, movetypeRect;
		SDL_Rect collision_overlay;
		TileType tiletype;
		public MovingType movingtype = MovingType.Walk;

		public int walk_speed;
		public int dive_speed;
		public int bike_speed;

		LayerType layertype;

		public Tile(ref Sprite image, Vector2<uint> source, Vector2<uint> size, Vector2<uint> target, 
			LayerType layertype, bool passable, Vector2<float> camera, TileType tiletype)
		{
			this.camera = camera;
			this.image = image;

			this.passable = passable;
			this.layertype = layertype;
			this.tiletype = tiletype;

			this.TargetRect.x = (int)(target.X * size.Y);
			this.TargetRect.y = (int)(target.Y * size.Y);

			this.SourceRect.x = (int)source.X;
			this.SourceRect.y = (int)source.Y;

			this.offset.h = this.TargetRect.h = this.SourceRect.h = (int)size.X;
			this.offset.w = this.TargetRect.w = this.SourceRect.w = (int)size.Y;

			switch (this.tiletype)
			{
				case TileType.Clear:
					this.movingtype = MovingType.Walk;
					this.walk_speed = 4;
					this.bike_speed = 2;
					this.dive_speed = 0;
					break;
				case TileType.Grass:
					this.movingtype = MovingType.Walk;
					this.walk_speed = 2;
					this.bike_speed = 1;
					this.dive_speed = 0;
					break;
				case TileType.Water:
					this.movingtype = MovingType.Dive;
					this.walk_speed = 1;
					this.bike_speed = 0;
					this.dive_speed = 2;
					break;
				case TileType.Road:
					this.movingtype = MovingType.Bike;
					this.walk_speed = 4;
					this.bike_speed = 6;
					this.dive_speed = 0;
					break;
				default:
					this.movingtype = MovingType.Walk;
					this.walk_speed = 0;
					this.bike_speed = 0;
					this.dive_speed = 0;
					break;
			}


			if (this.layertype == LayerType.Collision)
			{
				this.movetypeRect.Height = this.offset.h;
				this.movetypeRect.Width = this.offset.w;

				this.collisionRect.Height = (this.offset.h - 2);
				this.collisionRect.Width = (this.offset.w - 2);

				this.collision_overlay.h = (int)this.collisionRect.Height;
				this.collision_overlay.w = (int)this.collisionRect.Width;

				this.movetypeRect.X = this.offset.x;
				this.movetypeRect.Y = this.offset.y;

				this.collisionRect.X = this.collision_overlay.x = (this.offset.x + 1);
				this.collisionRect.Y = this.collision_overlay.y = (this.offset.y + 1);
			}
		}

		public LayerType Type
		{
			get { return this.layertype; }
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
		}

		public void Events(SDL_Event e)
		{
			if (e.key.keysym.sym == SDL_Keycode.SDLK_F8)
			{
				if (this.debug)
					this.debug = false;
				else
					this.debug = true;
			}
		}

		public int Render(ref IntPtr renderer, ref IntPtr screen_surface, Vector2<float> camera, Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			this.camera = camera;
			this.offset.x = (int)(TargetRect.x - this.camera.X + (screensize.X / 2));
			this.offset.y = (int)(TargetRect.y - this.camera.Y + (screensize.Y / 2));

			this.movetypeRect.X = this.offset.x;
			this.movetypeRect.Y = this.offset.y;

			this.collisionRect.X = this.collision_overlay.x = this.offset.x;
			this.collisionRect.Y = this.collision_overlay.y = this.offset.y;

			this.collision_overlay.h = (int)this.collisionRect.Height;
			this.collision_overlay.w = (int)this.collisionRect.Width;

			SDL_RenderCopy(renderer, this.image.Image, ref this.SourceRect, ref this.offset);
			
			if (this.layertype == LayerType.Collision && this.debug)
				Video.DrawRect(renderer, (int)this.collisionRect.X, (int)this.collisionRect.Y, 
					(int)this.collisionRect.Width, (int)this.collisionRect.Height, Color.Red);

			return 0;
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
	}
}
