using SDL2;
using System;
using System.IO;

namespace RPGEngine
{
	public class Sprite
	{
		IntPtr image;
		int width, height;
		
		Vector2<int> frames, frameSize, offset;
		
		public SDL.SDL_Rect SourceRect, TargetRect;

		string filename;

		public Sprite(string filename, ref IntPtr renderer, Vector2<int> frames, Vector2<int> offset)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(filename);

			var format = uint.MinValue;
			var access = 0;

			this.filename = filename;
			
			this.image = SDL_image.IMG_LoadTexture(renderer, this.filename);
			SDL.SDL_QueryTexture(this.image, out format, out access, out this.width, out this.height);

			this.frames = frames;
			this.offset = offset;

			this.frameSize = new Vector2<int>((this.width / frames.X), (this.height / frames.Y));

			this.TargetRect.h = this.SourceRect.h = (int)this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = (int)this.frameSize.X;

			this.TargetRect.x = this.SourceRect.x = 0 * (int)this.frameSize.X;
			this.TargetRect.y = this.SourceRect.y = 0 * (int)this.frameSize.Y;
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

		public int Render(ref IntPtr renderer)
		{
			return SDL.SDL_RenderCopy(renderer, this.image, ref this.SourceRect, ref this.TargetRect);
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

		public Vector2<int> Offset
		{
			get { return this.offset; }
			set { this.offset = value; }
		}

		public void Close()
		{
			Game.Print(LogType.Debug, this.GetType().ToString(), "Unloading Texture: {0}".F(this.filename));
			SDL.SDL_DestroyTexture(this.image);
		}
	}
}
