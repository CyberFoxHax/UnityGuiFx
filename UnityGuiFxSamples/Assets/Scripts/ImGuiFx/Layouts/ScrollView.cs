namespace Assets.Scripts.ImGuiFx.Layouts{
	// https://docs.unity3d.com/ScriptReference/GUILayout.BeginScrollView.html
	// https://docs.unity3d.com/ScriptReference/GUILayout.EndScrollView.html
	public class ScrollView : Control, ILayout {
		private UnityEngine.Vector2 _scrollPosition;

		public void LayoutBegin(){
			_scrollPosition = UnityEngine.GUILayout.BeginScrollView(
				_scrollPosition,
				GuiLayoutOptions
			);
		}

		public void LayoutEnd(){
			UnityEngine.GUILayout.EndScrollView();
		}
	}
}