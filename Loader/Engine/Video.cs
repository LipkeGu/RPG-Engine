using System;
using System.Drawing;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace RPGEngine
{
	public class Video
	{
		#region private Fields
		private IntPtr window, renderer;
		private SDL_DisplayMode dspMode;

		private int width, height, bpp;

		private string title;

		#endregion

		public delegate void VideoInitDoneEventHandler(object source, FinishEventArgs args);

		public delegate void VideoInitErrorEventHandler(object source, ErrorEventArgs args);

		public delegate void VideoInitStateEventHandler(object source, StateEventArgs args);

		public event VideoInitDoneEventHandler VideoInitDone;

		public event VideoInitStateEventHandler VideoInitState;

		public event VideoInitErrorEventHandler VideoInitError;

		/// <summary>
		/// Bit per Pixel (from Primary Display)
		/// </summary>
		public int BitsPerPixel
		{
			get
			{
				return this.bpp;
			}
		}

		/// <summary>
		///	Returns the current Window context 
		/// </summary>
		public IntPtr Window
		{
			get
			{
				return this.window;
			}
		}

		/// <summary>
		///	Returns the current Window surface
		/// </summary>
		public IntPtr WindowSurface()
		{
			return SDL_GetWindowSurface(this.window);
		}

		/// <summary>
		///	Returns the current Renderer context 
		/// </summary>
		public IntPtr Renderer
		{
			get
			{
				return this.renderer;
			}
		}

		/// <summary>
		/// Draws an Color filled Rectangle on the screen (Window)
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="x">position X</param>
		/// <param name="y">position Y</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">Color</param>
		public static int DrawRect(ref IntPtr renderer, int x, int y, int w, int h, Color color)
		{
			var retval = -1;
			var rect = new SDL_Rect();

			rect.h = h;
			rect.w = w;

			rect.x = x;
			rect.y = y;

			retval = SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
			retval = SDL_RenderFillRect(renderer, ref rect);
			retval = SDL_SetRenderDrawColor(renderer, byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

			return retval;
		}

		/// <summary>
		/// Initialize the SDL Video Context
		/// </summary>
		/// <param name="title">Window Title</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <returns>-1 on Error or 0 on success</returns>
		public void Init(object obj)
		{
			var config = (Settings)obj;

			this.width = config.Engine.Width;
			this.height = config.Engine.Height;
			this.title = config.Game.Title;

			this.OnVideoInitState(this.GetType().ToString(), "Waiting for the Video Subsystem...");
			
			var errnum = -1;
			var flags = config.Engine.WindowFlags;

			errnum = IMG_Init(IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_JPG);
			if (errnum == -1)
				this.OnVideoInitError(this.GetType().ToString(), "IMG_Init(): {0}".F(SDL_GetError()));

			this.window = SDL_CreateWindow(this.title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, this.width, this.height, flags);

			if (this.window == IntPtr.Zero)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_CreateWindow(): {0}".F(SDL_GetError()));

			this.renderer = SDL_CreateRenderer(this.window, 0, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

			if (this.renderer == IntPtr.Zero)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_CreateRenderer(): {0}".F(SDL_GetError()));

			if (SDL_SetHint("SDL_HINT_RENDER_VSYNC", "1") == SDL_bool.SDL_FALSE)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_SetHint(): 'Vsync' is not enabled!");

			if (SDL_SetHint("SDL_HINT_RENDER_SCALE_QUALITY", "1") == SDL_bool.SDL_FALSE)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_SetHint(): 'Linear texture filtering' is not enabled!");

			if (SDL_GetCurrentDisplayMode(0, out this.dspMode) != 0)
				this.OnVideoInitError(this.GetType().ToString(), "SDL_GetDesktopDisplayMode(): {0}".F(SDL_GetError()));

			SDL_SetWindowMinimumSize(this.window, this.width, this.height);
			SDL_SetWindowMaximumSize(this.window, (this.dspMode.w - 160), this.dspMode.h - 160);
			
			this.bpp = (int)SDL_BITSPERPIXEL(this.dspMode.format);

			this.OnVideoInitDone("Starting Game...");
		}

		/// <summary>
		/// Set (new) Window Title
		/// </summary>
		/// <param name="title">The (new) title of the Window</param>
		public void SetWindowTile(string title)
		{
			SDL_SetWindowTitle(this.Window, title);
		}

		/// <summary>
		/// Set Window position
		/// </summary>
		/// <param name="x">Horizontally Position</param>
		/// <param name="y">Vertical Position</param>
		public void SetWindowPosition(int x, int y)
		{
			SDL_SetWindowPosition(this.window, x, y);
		}

		/// <summary>
		/// Closes the SDL Instances.
		/// </summary>
		public void Close()
		{
			IMG_Quit();
			SDL_VideoQuit();

			SDL_DestroyRenderer(this.renderer);
			SDL_DestroyWindow(this.window);
		}

		/// <summary>
		///	Reports the Video subsystem to flip / update the Window...
		/// </summary>
		/// <returns></returns>
		public void End()
		{
			SDL_RenderPresent(this.renderer);
		}

		/// <summary>
		/// Reports the Video subsystem to "begin" the rendering...
		/// </summary>
		/// <param name="color">Draw color</param>
		public int Begin(Color color)
		{
			var retval = -1;
			retval = SDL_SetRenderDrawColor(this.renderer, color.R, color.G, color.B, color.A);
			retval = SDL_RenderClear(this.renderer);

			return retval;
		}

		#region Events
		protected virtual void OnVideoInitDone(string message)
		{
			var fe = new FinishEventArgs();

			fe.Source = this.GetType().ToString();
			fe.Message = message;

			this.VideoInitDone?.Invoke(this, fe);
		}

		protected virtual void OnVideoInitError(string source, string message)
		{
			var errvtargs = new ErrorEventArgs();

			errvtargs.Message = message;
			errvtargs.Source = source;

			this.VideoInitError?.Invoke(this, errvtargs);
		}

		protected virtual void OnVideoInitState(string source, string message)
		{
			var statevtargs = new StateEventArgs();

			statevtargs.Message = message;
			statevtargs.Source = source;

			this.VideoInitState?.Invoke(this, statevtargs);
		}
		#endregion
	}
}
