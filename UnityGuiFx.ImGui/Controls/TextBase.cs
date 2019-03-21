using UnityEngine;

namespace ImGuiFx.Controls {
	public abstract class TextBase : Control, IControl {
		protected TextBase() {
			MaxLength = -1;
		}

		protected override void OnLoaded() {
			if (Style == null)
				Style = GUI.skin.textArea;
			base.OnLoaded();
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
