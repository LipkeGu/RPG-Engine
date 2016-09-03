using SDL2;
using System;
using System.IO;

namespace RPGEngine
{
	public class Sprite
	{
		IntPtr image;
		IntPtr renderer;

		int width;
		int height;

		Vector2<int> frames;

		Vector2<int> frameSize;

		public SDL.SDL_Rect SourceRect;
		public SDL.SDL_Rect TargetRect;

		uint format;
		int access;

		string filename;

		public Sprite(string filename, IntPtr renderer, Vector2<int> frames)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(filename);

			this.filename = filename;
			this.renderer = renderer;

			this.image = SDL_image.IMG_LoadTexture(this.renderer, this.filename);
			SDL.SDL_QueryTexture(this.image, out this.format, out this.access, out this.width, out this.height);

			this.frames = frames;
			this.frameSize = new Vector2<int>((this.width / frames.X), (this.height / frames.Y));

			this.TargetRect.h = this.SourceRect.h = (int)this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = (int)this.frameSize.X;

			this.TargetRect.x = this.SourceRect.x = 0 * (int)this.frameSize.X;
			this.TargetRect.y = this.SourceRect.y = 0 * (int)this.frameSize.Y;

			Game.Print(LogType.Debug, this.GetType().ToString(), "Using Texture '{0}' (W*H: {1}*{3} | Frames: {2}*{4})"
				.F(this.filename, this.width, this.frames.X, this.height, this.frames.Y));
		}

		public IntPtr Renderer
		{
			set { this.renderer = value; }
			get { return this.renderer; }
		}

		public Vector2<int> FrameSize
		{
			set
			{
				this.frameSize.X = (this.width / value.X);
				this.frameSize.Y = (this.height / value.Y);
			}

			get { return this.frameSize; }
		}

		public int Width
		{
			get { return this.width; }
		}

		public int Height
		{
			get { return this.height; }
		}

		public string Filename
		{
			get { return this.filename; }
		}

		public Vector2<int> Frames
		{
			get { return this.frames; }
		}

		public IntPtr Image
		{
			set { this.image = value; }
			get { return this.image; }
		}

		public void Render()
		{
			SDL.SDL_RenderCopy(this.renderer, this.image, ref this.SourceRect, ref this.TargetRect);
		}

		public void Update()
		{
			this.frameSize = new Vector2<int>((this.width / frames.X), (this.height / frames.Y));

			this.TargetRect.h = this.SourceRect.h = this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = this.frameSize.X;
		}

		public void Events(SDL.SDL_Event e)
		{

		}

		public void Close()
		{
			SDL.SDL_DestroyTexture(this.image);
		}
	}
}
