using System;
using System.Drawing;
using static SDL2.SDL;

namespace RPGEngine
{
	public class Tile
	{
		public SDL_Rect TargetRect, SourceRect, offset;
		Vector2<float> camera;

		IntPtr image;

		bool debug;
		bool passable;
		public RectangleF collisionRect;
		SDL_Rect collision_overlay;

		int type;

		public Tile(IntPtr image, Vector2<int> source, Vector2<int> size, Vector2<int> target, int type, bool passable, Vector2<float> camera)
		{
			this.camera = camera;
			this.image = image;

			this.passable = passable;
			this.type = type;

			this.TargetRect.x = target.X * size.Y;
			this.TargetRect.y = target.Y * size.Y;

			this.SourceRect.x = source.X;
			this.SourceRect.y = source.Y;

			this.offset.h = this.TargetRect.h = this.SourceRect.h = size.X;
			this.offset.w = this.TargetRect.w = this.SourceRect.w = size.Y;

			if (this.type == 2)
			{
				this.collisionRect.Height = this.offset.h;
				this.collisionRect.Width = this.offset.w;

				this.collision_overlay.h = (int)this.collisionRect.Height;
				this.collision_overlay.w = (int)this.collisionRect.Width;

				this.collisionRect.X = this.collision_overlay.x = this.offset.x;
				this.collisionRect.Y = this.collision_overlay.y = this.offset.y;
			}
		}

		public int Type
		{
			get { return this.type; }
		}

		public bool Passable
		{
			get { return this.passable; }
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

		public int Render(IntPtr renderer, IntPtr screen_surface, Vector2<float> camera, Vector2<int> screensize, Worldtype type = Worldtype.Normal)
		{
			this.camera = camera;
			this.offset.x = (int)(TargetRect.x - this.camera.X + (screensize.X / 2));
			this.offset.y = (int)(TargetRect.y - this.camera.Y + (screensize.Y / 2));

			this.collisionRect.X = this.collision_overlay.x = this.offset.x;
			this.collisionRect.Y = this.collision_overlay.y = this.offset.y;

			this.collision_overlay.h = (int)this.collisionRect.Height;
			this.collision_overlay.w = (int)this.collisionRect.Width;

			SDL_RenderCopy(renderer, this.image, ref this.SourceRect, ref this.offset);
			
			if (this.type == 2 && this.debug)
				Graphic.DrawRect(renderer, (int)this.collisionRect.X, (int)this.collisionRect.Y, 
					(int)this.collisionRect.Width, (int)this.collisionRect.Height, Color.Red);

			return 0;
		}

		public void Close()
		{
			SDL_DestroyTexture(this.image);
		}
	}
}
