using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/* todo
 * 
 * disable wordwrap
 * fix wordwrap and line count issue
 * doesn't repaint properly when loaded
 * goes blank when using tab
 * doubleclick selects the wrong portion
 * tab length is way too big
 * syntax highlighter doesn't work
 *  * After investigation, GUI.contentColor has no effect aside from alpha in Editor
*/

// https://forum.unity.com/threads/opensource-script-editor.188059/
namespace ImGuiFx.Editor {
	public class CodeTextArea {
		public CodeTextArea() {
			_controlName = GUID.Generate().ToString();
		}

		public string Text {
			get { return _mainScriptText; }
		}

		public void SetText(string value) {
			_mainScriptText = value;
			_lines = value.Count(p => p == '\n');
			_redos.Clear();
		}

		private static int GetPos(TextEditor te) {
			return te.cursorIndex;
		}

		private static void SetPos(TextEditor te, int value) {
			te.cursorIndex = value;
		}

		private static int GetSelectPos(TextEditor te) {
			return te.selectIndex;
		}

		private static void SetSelectPos(TextEditor te, int value) {
			te.selectIndex = value;
		}

		public float CalculatedHeight() {
			return _textStyle.CalcHeight(new GUIContent(_mainScriptText), Rect.width);
		}

		public Rect Rect;
		private bool _hasLoaded;
		private string _mainScriptText = "";
		private int _lines;
		private Vector2 _scrollPos;

		public GUIStyle TextStyle {
			get { return _textStyle; }
			set { _textStyle = value; }
		}

		private GUIStyle _textStyle = new GUIStyle();
		private Vector2 _tSize;

		private bool _isCommenting;
		private bool _isString;
		private bool _commentAffectsNextLine;

		private readonly string _controlName;

		private readonly List<string> _redos = new List<string>();

		public void OnGUI() {
			if (_textStyle == null) {
				_textStyle = new GUIStyle(GUI.skin.textArea);
			}
			if (_hasLoaded == false) {
				_hasLoaded = true;
				_textStyle = new GUIStyle(GUI.skin.textArea);
				_mainScriptText = "";
				_redos.Clear();
			}

			//backup color
			var backupColor = Color.white;
			var backupContentColor = Color.black;
			var backupBackgroundColor = GUI.backgroundColor;

			_tSize.y = _textStyle.CalcHeight(new GUIContent(_mainScriptText), Rect.width);
			_tSize.x = Rect.width;

			for (var i = 0; i < _lines; i++) {
				// Generate line numbers
				var indexString = i.ToString();
				var lSize = _textStyle.CalcSize(new GUIContent(indexString));
				if (i % 2 == 1) {
					EditorGUI.DrawRect(new Rect(
						-2,
						13 * i + 13,
						Rect.width + 4 + _tSize.x,
						13
					), new Color32(184, 184, 184, 255));
				}
				GUI.color = Color.white;
				GUI.Label(new Rect(
					0,
					13 * i + 13,
					lSize.x + 50,
					13
				), indexString); // todo align text to right side
			}

			var ev = Event.current;

			//add textarea with transparent text
			GUI.contentColor = new Color(1f, 1f, 1f, 0f);
			var bounds = new Rect(60, 13, Rect.width - 80, Rect.height + _tSize.y);
			GUI.SetNextControlName(_controlName);

			_mainScriptText = GUI.TextArea(bounds, _mainScriptText);


			//get the texteditor of the textarea to control selection
			var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			CheckKeys(te, ev);
			//set background of all textfield transparent
			GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);

			//backup selection to remake it after process
			var backupPos = GetPos(te);
			var backupSelPos = GetSelectPos(te);

			//get last position in text
			te.MoveTextEnd();
			var endpos = GetPos(te);
			//draw textfield with color on top of text area
			UpdateText(te, ev, endpos, _textStyle, backupPos, backupSelPos);

			//Reset color
			GUI.color = backupColor;
			GUI.contentColor = backupContentColor;
			GUI.backgroundColor = backupBackgroundColor;
		}

		private void UpdateText(TextEditor te, Event ev, int endpos, GUIStyle textStyle, int backupPos, int backupSelPos) {
			te.MoveTextStart();
			_lines = 0;
			var teTextContent = new GUIContent(te.text);

			// highlighting by drawing read only text over the actual invisibile text
			while (GetPos(te) != endpos) {
				te.SelectToStartOfNextWord();
				var wordtext = te.SelectedText;

				//set word color
				GUI.contentColor = GetColor(wordtext);

				var pixelselpos = textStyle.GetCursorPixelPosition(te.position, teTextContent, GetSelectPos(te));
				var pixelpos = textStyle.GetCursorPixelPosition(te.position, teTextContent, GetPos(te));

				GUI.Label(
					new Rect(
						pixelselpos.x - textStyle.border.left - 2f,
						pixelselpos.y - textStyle.border.top,
						pixelpos.x,
						pixelpos.y
					),
					wordtext
				);
				if (wordtext.Contains("\n"))
					_lines++;

				te.MoveToStartOfNextWord();
			}
			_lines++;

			//Reposition selection
			var bkpixelselpos = textStyle.GetCursorPixelPosition(te.position, teTextContent, backupSelPos);
			te.MoveCursorToPosition(bkpixelselpos);

			//Remake selection
			var bkpixelpos = textStyle.GetCursorPixelPosition(te.position, teTextContent, backupPos);
			te.SelectToPosition(bkpixelpos);
		}

		private Color GetColor(string str) {
			str = str.TrimEnd(' ');

			if (Keywords.CSharp.Any(st => str == st && !_isCommenting)) {
				return Color.cyan;
			}

			if (str.StartsWith("//") || str.StartsWith("*/")) {
				_isCommenting = true;
				if (str.StartsWith("*/"))
					_commentAffectsNextLine = true;
				return Color.green;
			}

			if (str.StartsWith("\"") || str.StartsWith("\'")) {
				_isString = true;
				return Color.magenta;
			}

			if (str.StartsWith("/*")) {
				_commentAffectsNextLine = false;
				return Color.green;
			}

			if (_isString && str != "\n" && !_isCommenting)
				return Color.magenta;

			if (_isCommenting && str != "\n")
				return Color.green;

			if (_isCommenting && str == "\n" && _commentAffectsNextLine)
				return Color.green;

			if (str == "\n") {
				_isCommenting = false;
				_isString = false;
			}

			return Color.white;
		}

		private void CheckKeys(TextEditor te, Event ev) {
			if (GUIUtility.keyboardControl == te.controlID && ev.Equals(Event.KeyboardEvent("tab"))) {
				Debug.Log("tab pressed");
				GUI.FocusControl(_controlName);
				if (_mainScriptText.Length > GetPos(te)) {
					_mainScriptText = _mainScriptText.Insert(GetPos(te), "\t");
					SetPos(te, GetPos(te) + 1);
					SetSelectPos(te, GetPos(te));
				}
				ev.Use();
				GUI.FocusControl(_controlName);
			}

			if (Event.current.type == EventType.KeyUp && GUI.GetNameOfFocusedControl() == _controlName) {
				switch (Event.current.keyCode) {
					case KeyCode.Space:
						Debug.Log("space pressed");
						break;
					case KeyCode.Return:
						Debug.Log("Enter pressed");
						if (_mainScriptText.Length > GetPos(te)) {
							_mainScriptText = _mainScriptText.Insert(GetPos(te), "\t");
							SetPos(te, GetPos(te) + 1);
							SetSelectPos(te, GetPos(te));
						}
						ev.Use();
						GUI.FocusControl(_controlName);
						break;
				}
			}
		}
	}
}