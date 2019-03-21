using UnityEditor;

namespace ImGuiFx.Controls {
	// https://docs.unity3d.com/ScriptReference/EditorGUILayout.Space.html
	public class Space : Control, IControl {
		public void Render() {
			EditorGUILayout.Space();
		}
	}
}