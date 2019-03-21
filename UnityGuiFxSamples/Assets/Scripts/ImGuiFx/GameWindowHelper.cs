using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImGuiFx{
	public abstract class GameWindowHelper : MonoBehaviour{
		public Vector2 Size
		{
			get { return new Vector2(Screen.width, Screen.height); }
		}

		public List<Control> Controls { get; private set; }

		protected virtual IEnumerable<Control> OnInitialize() { yield break; }
		protected virtual void OnLoaded() { }

		private void OnLoad() {
			Controls = OnInitialize().ToList();
			OnLoaded();
		}

		private void OnGUI() {
			if (GUI.Button(new Rect(Screen.width - 50, 0, 50, 20), "Reset"))
				Controls = null;

			if (Controls == null || Controls.Any() == false) {
				OnLoad();
				InitControls(Controls);
			}

			RendererEngine.RenderControls(Controls);
		}

		private static void InitControls(IEnumerable<Control> ctrls) {
			foreach (var control in ctrls) {
				control.OnLoaded();
				if (control.Children.Any())
					InitControls(control.Children);
			}
		}
	}
}