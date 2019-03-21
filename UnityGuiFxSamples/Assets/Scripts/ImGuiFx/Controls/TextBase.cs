using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls {
	public abstract class TextBase : Control, IControl {
		protected TextBase() {
			Style = GUI.skin.textArea;
			MaxLength = -1;
		}
		public string Text { get; set; }
		public int MaxLength { get; set; }

		public GUIStyle Style { get; set; }

		public delegate void TextChangedEvent(TextBase sender, string oldValue);
		public event TextChangedEvent Changed;

		public void Render(){
			var oldText = Text;
			var newText = RenderText();
			Text = newText;
			if (Changed != null && newText != oldText)
				Changed(this, oldText);
		}

		protected abstract string RenderText();
	}
}
