using UnityEngine;

namespace ImGuiFx.Controls {
	public class Image : Control, IControl {
		protected override void OnLoaded() {
			Style = new GUIStyle();
			base.OnLoaded();
		}

		public GUIStyle Style { get; set; }
		public Texture2D Texture2D { get; set; }

		private string _url;
		public string Url {
			get { return _url; }
			set {
				var req = new WWW(value);
				// no apparent boost in performance??
				//new Thread(() => {
				//	while (req.isDone == false) { }
				//	Dispatcher.InvokeOnStart(() => Texture2D = req.texture);
				//}).Start();
				while (req.isDone == false) { }
				Texture2D = req.texture;
				_url = value;
			}
		}


		public void Render() {
			if (Texture2D == null)
				return;

			if (float.IsNaN(Width)) Width = Texture2D.width;
			if (float.IsNaN(Height)) Height = Texture2D.height;

			Style.normal.background = Texture2D;
			GUILayout.BeginVertical(Style, WidthAndHeight);
			GUILayout.Label(" ");
			GUILayout.EndVertical();
		}
	}
}
