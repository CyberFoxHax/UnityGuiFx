using UnityEditor;
using UnityEngine;

namespace ImGuiFx.Controls {
	public class Vector3Field : Control, IControl {

		public string Label { get; set; }
		public Vector3 Vector3 { get; set; }

		public void Render() {
			EditorGUILayout.Vector3Field(Label, Vector3, WidthAndHeight);
		}
	}
}
