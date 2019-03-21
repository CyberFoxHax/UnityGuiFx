using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImGuiFx{
	public abstract class GameWindowHelper : MonoBehaviour{
		protected GameWindowHelper() {
			PrivateDispatcher = new DispatcherOnGUI();
			RendererEngine = new RendererEngine();
		}

		public Vector2 Size
		{
			get { return new Vector2(Screen.width, Screen.height); }
		}

		private List<Control> Controls { get; set; }
		private RendererEngine RendererEngine { get; set; }
		private DispatcherOnGUI PrivateDispatcher { get; set; }
		public DispatcherOnGUI.IInvoker Dispatcher {
			get { return PrivateDispatcher; }
		}

		protected virtual IEnumerable<Control> OnInitialize() { yield break; }
		protected virtual void OnLoaded() { }

		private void OnLoad() {
			Controls = OnInitialize().ToList();
			OnLoaded();
		}

		private void OnGUI() {
			#if UNITY_EDITOR
			if (GUI.Button(new Rect(Screen.width - 50, 0, 50, 20), "Reset"))
				Controls = null;
			#endif

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