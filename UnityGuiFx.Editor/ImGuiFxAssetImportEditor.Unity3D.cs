using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace ImGuiFx.Editor {
	[CustomEditor(typeof(TextAsset))]
	public partial class ImGuiFxAssetImportEditor : AssetImporterEditor {

		protected override void OnHeaderGUI() {
			if (_disableUi) {
				base.OnInspectorGUI(); // appears to return blank page
				return;
			}
			DrawHeaderGUI(this, "Change ImGuiFx XML");
		}

		private Rect DrawHeaderGUI(UnityEditor.Editor editor, string title) {
			return (Rect) typeof(UnityEditor.Editor)
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.First(p=>p.Name == "DrawHeaderGUI" && p.GetParameters().Length == 2)
				.Invoke(this, new object[] {editor, title});
		}

		private List<Control> _controls;
		private readonly RendererEngine _renderEngine = new RendererEngine();
		private readonly DispatcherOnGUI _privateDispatcher = new DispatcherOnGUI();
		private bool _disableUi;

		public override void OnInspectorGUI() {
			if (_disableUi) {
				base.OnInspectorGUI(); // appears to return blank page
				return;
			}

			if (_controls == null || _controls.Any() == false) {
				OnCreateUi();
				OnLoaded();
				Repaint();
			}

			GUI.enabled = true; 
			  
			_privateDispatcher.DispatchAllStart();
			_renderEngine.Controls = _controls;
			_renderEngine.Dispatcher = _privateDispatcher;
			_renderEngine.RenderControls();
			_privateDispatcher.DispatchAllEnd();
		}
	}

}
