﻿using SDL2;
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
		Vector2<int> frameSize;

		public SDL.SDL_Rect SourceRect;
		public SDL.SDL_Rect TargetRect;

		uint format;
		int access;

		string alias;
		string filename;

		public Sprite(string filename, string alias, IntPtr renderer)
		{
			this.filename = filename;
			if (!File.Exists(this.filename))
				throw new FileNotFoundException(this.filename);

			this.alias = alias;
			this.renderer = renderer;

			this.image = SDL_image.IMG_LoadTexture(this.renderer, this.filename);
			SDL.SDL_QueryTexture(this.image, out format, out access, out width, out height);

			this.frameSize = new Vector2<int>(this.width / 4, this.height / 4);

			this.TargetRect.h = this.SourceRect.h = (int)this.frameSize.Y;
			this.TargetRect.w = this.SourceRect.w = (int)this.frameSize.X;

			this.TargetRect.x = this.SourceRect.x = 0 * (int)this.frameSize.X;
			this.TargetRect.y = this.SourceRect.y = 0 * (int)this.frameSize.Y;
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

				this.TargetRect.h = this.SourceRect.h = this.frameSize.Y;
				this.TargetRect.w = this.SourceRect.w = this.frameSize.X;
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

		public string Alias
		{
			get { return this.alias; }
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
