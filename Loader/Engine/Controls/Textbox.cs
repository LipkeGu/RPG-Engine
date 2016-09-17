using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGEngine
{
	public class Textbox
	{
		string text;
		string name;
		
		SDL.SDL_Rect body;

		public Textbox()
		{
			this.body = new SDL.SDL_Rect();
			this.body.h = 32;
			this.body.w = 128;

			this.body.x = 10;
			this.body.y = 10;

			this.text = string.Empty;
		}

		public int Width
		{
			get { return this.body.w; }
			set { this.body.w = value; }
		}

		public int Height
		{
			get { return this.body.h; }
			set { this.body.h = value; }
		}

		public Vector2<int> Location
		{
			get
			{
				var l = new Vector2<int>();
				l.X = this.body.x;
				l.Y = this.body.y;

				return l;
			}

			set
			{
				this.body.h = value.X;
				this.body.y = value.Y;
			}
		}

		public string Text
		{
			get { return this.text; }
			set { this.Text = value; }
		}
	}
}
