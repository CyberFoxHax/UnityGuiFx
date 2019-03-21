using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx{
	public abstract class EditorWindowHelper : EditorWindow {
		protected static void StaticStart<T>() where T : EditorWindowHelper {
			GetWindow(typeof(T));
		}

		public Vector2 Size {
			get { return new Vector2(position.width, position.height); }
		}

		public List<Control> Controls { get; private set; }

		public string Title { get; set; }

		protected virtual IEnumerable<Control> OnInitialize() { yield break; }
		protected virtual void OnLoaded() { }

		private void OnLoad(){
			Controls = OnInitialize().ToList();
			OnLoaded();
			titleContent = new GUIContent(Title ?? GetType().Name);
		}

		private void OnGUI() {
			if (GUI.Button(new Rect(position.width - 50, 0, 50, 20), "Reset"))
				Controls = null;

			if (Controls == null || Controls.Any() == false){
				OnLoad();
				InitControls(Controls);
			}

			RendererEngine.RenderControls(Controls);
		}

		private static void InitControls(IEnumerable<Control> ctrls){
			foreach (var control in ctrls){
				control.OnLoaded();
				if(control.Children.Any())
					InitControls(control.Children);
			}
		}
	}
}