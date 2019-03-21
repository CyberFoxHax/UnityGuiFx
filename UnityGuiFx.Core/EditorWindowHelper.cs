#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImGuiFx{
	public abstract class EditorWindowHelper : EditorWindow {
		protected EditorWindowHelper() {
			PrivateDispatcher = new DispatcherOnGUI();
			RendererEngine = new RendererEngine();
		}

		protected static void StaticStart<T>() where T : EditorWindowHelper {
			GetWindow(typeof(T));
		}

		public Vector2 Size {
			get { return new Vector2(position.width, position.height); }
		}

		protected List<Control> Controls { get; set; }
		private RendererEngine RendererEngine { get; set; }
		private DispatcherOnGUI PrivateDispatcher { get; set; }
		public DispatcherOnGUI.IInvoker Dispatcher {
			get { return PrivateDispatcher; }
		}

		public string Title { get; set; }

		protected virtual IEnumerable<Control> OnInitialize() { yield break; }
		protected virtual void OnLoaded() { }

		protected void Reload(){
			Controls = null;
		}

		private void OnLoad(){
			Controls = OnInitialize().ToList();
			OnLoaded();
			titleContent = new GUIContent(Title ?? GetType().Name);
		}

		private void OnGUI() {
			if (Controls == null || Controls.Any() == false)
				OnLoad();

			PrivateDispatcher.DispatchAllStart();
			RendererEngine.Controls = Controls;
			RendererEngine.Dispatcher = PrivateDispatcher;
			RendererEngine.RenderControls();
			PrivateDispatcher.DispatchAllEnd();
		}
	}
}
#endif