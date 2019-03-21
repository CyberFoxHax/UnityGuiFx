using System;
using UnityEngine;

// http://www.createdbyx.com/createdbyx/post/2016/02/08/Unity-101-Tip-92-%e2%80%93-Adding-Tab-and-Shift-Tab-support-to-TextArea-and-TextField.aspx
namespace ImGuiFx.Editor {
	public static class TextAreaTabSupport {

		private static void CheckEvents(string textIn, int lastkeyboardFocusIn, out string textOut) {
			var current = Event.current;

			if (GUI.GetNameOfFocusedControl() != "testa" || lastkeyboardFocusIn != GUIUtility.keyboardControl) {
				textOut = textIn;
				return;
			}

			if (current.type != EventType.KeyDown && current.type != EventType.KeyUp) {
				textOut = textIn;
				return;
			}

			if (current.isKey == false || current.keyCode != KeyCode.Tab && current.character != '\t') {
				textOut = textIn;
				return;
			}

			if (current.type == EventType.KeyUp) {
				var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

				if (!current.shift) {
					for (var i = 0; i < 4; i++) {
						te.Insert(' ');
					}
				}
				else {
					var min = Math.Min(te.cursorIndex, te.selectIndex);
					var index = min;
					var temp = te.text;
					for (var i = 1; i < 5; i++) {
						if (min - i < 0 || temp[min - i] != ' ') {
							break;
						}

						index = min - i;
					}

					if (index < min) {
						te.selectIndex = index;
						te.cursorIndex = min;
						te.ReplaceSelection(string.Empty);
					}
				}

				textOut = te.text;
			}
			else {
				textOut = textIn;
			}

			current.Use();
		}

		public static void DrawTextArea(string textIn, int lastkeyboardFocusIn, out string textOut, out int lastkeyboardFocusOut) {
			var current = Event.current;

			CheckEvents(textIn, lastkeyboardFocusIn, out textOut);

			using (new GUI.GroupScope(new Rect(0, 0, 110, 110))) {
				GUI.SetNextControlName("testa");
				textOut = GUI.TextArea(new Rect(0, 4, 100, 100), textIn);
			}

			if (lastkeyboardFocusIn != GUIUtility.keyboardControl &&
			    (current.type == EventType.KeyDown || current.type == EventType.KeyUp)) {
				lastkeyboardFocusOut = GUIUtility.keyboardControl;
			}
			else
				lastkeyboardFocusOut = -1;
		}
	}
}