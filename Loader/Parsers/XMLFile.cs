using System;
using System.IO;
using System.Xml.Serialization;

namespace RPGEngine
{
	public class XmlManager<T>
	{
		public Type Type;

		public XmlManager()
		{
			this.Type = typeof(T);
		}

		public T Load(string path)
		{
			T instance;
			using (var reader = new StreamReader(path))
			{
				var xml = new XmlSerializer(this.Type);
				instance = (T)xml.Deserialize(reader);
			}

			return instance;
		}

		public void Save(string path, object obj)
		{
			try
			{
				using (var writer = new StreamWriter(path))
				{
					var xml = new XmlSerializer(this.Type);
					xml.Serialize(writer, obj);
				}
			}
			catch(Exception ex)
			{
				Game.Print(LogType.Error, this.GetType().ToString(), ex.Message);
			}
		}
	}
}
