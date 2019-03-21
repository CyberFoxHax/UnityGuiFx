using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/* todo
 * 
 * split into 2 modules
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
	public class CodeEditor : EditorWindow {

		private int _toolbarMenuIndex;
		private static readonly string[] FileOptions = {
			"New",
			"Load",
			"Save",
			"Save as"
		};

		private CodeTextArea _codeTextArea;

		private List<Tab> _fileTabs;
		private int _fileTabIndex;

		private Vector2 _scrollPos;
		private string _currentDocumentPath;

		// Use this for initialization
		[MenuItem("Window/Script Editor")]
		private static void Init() {
			GetWindow(typeof(CodeEditor), false, "Script Editor");
		}

		private void Initialize() {
			minSize = new Vector2(200, 100);
			_codeTextArea = new CodeTextArea();
			_codeTextArea.Rect = position;
			_toolbarMenuIndex = -1;
			_fileTabs = new List<Tab>();
			_fileTabIndex = -1;
		}

		private class Tab {
			public readonly string FileName;
			public readonly string FilePath;
			public string FileText;

			public Tab(string name, string path, string text) {
				FileName = name;
				FilePath = path;
				FileText = text;
			}
		}

		// Update is called once per frame
		private void OnGUI() {
			if (_fileTabs == null)
				Initialize();

			//backup color
			var backupColor = Color.white;
			var backupContentColor = Color.black;
			var backupBackgroundColor = GUI.backgroundColor;

			GUI.Box(new Rect(0, 0, position.width, 20), "", "toolbar");

			_toolbarMenuIndex = EditorGUI.Popup(new Rect(0, 0, 40, 20), _toolbarMenuIndex, FileOptions, "toolbarButton");
			GUI.Label(new Rect(0, 0, 40, 20), "File");
			if (GUI.changed) {
				CheckFileOptions();
			}

			GUILayout.BeginArea(new Rect(0, 18, position.width, 20));
			GUILayout.BeginHorizontal();
			if (_fileTabs.Count > 0) {
				for (var i = 0; i < _fileTabs.Count; i++) {
					var tab = _fileTabs[i];
					if (GUILayout.Button(tab.FileName, "toolbarButton")) {
						_fileTabs[_fileTabIndex].FileText = _codeTextArea.Text;
						_fileTabIndex = i;
						_codeTextArea.SetText(tab.FileText);
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			var calcHeight = _codeTextArea.CalculatedHeight();
			var rect = position;
			rect.height = calcHeight;
			_codeTextArea.Rect = rect;

			_scrollPos = GUI.BeginScrollView(
				new Rect(
					0,
					36,
					position.width,
					position.height - 36
				),
				_scrollPos,
				new Rect(
					0, 0,
					position.width,
					_codeTextArea.CalculatedHeight()
				)
			);

			_codeTextArea.OnGUI();

			GUI.EndScrollView();

			//Reset color
			GUI.color = backupColor;
			GUI.contentColor = backupContentColor;
			GUI.backgroundColor = backupBackgroundColor;
		}

		private void CheckFileOptions() {
			if (_toolbarMenuIndex == -1)
				return;
			FileStream fs;
			StreamWriter sw;
			string path;
			switch (_toolbarMenuIndex) {
				//New
				case 0:
					_currentDocumentPath = "";
					_codeTextArea.SetText("");
					break;

				//Open
				case 1:
					path = EditorUtility.OpenFilePanel("Load...", "", "*.*");
					if (path != "") {
						_currentDocumentPath = path;

						if (_fileTabs.Count == 0 || _fileTabs.Any(p => p.FilePath == path) == false) {
							_codeTextArea.SetText("");
							LoadFile(path);
						}
					}
					break;

				//Save
				case 2:
					fs = new FileStream(_currentDocumentPath, FileMode.Open);
					fs.SetLength(0);
					fs.Close();
					sw = new StreamWriter(_currentDocumentPath);
					sw.Write(_codeTextArea.Text);
					sw.Close();
					AssetDatabase.Refresh();
					break;

				//Save as
				case 3:
					path = EditorUtility.SaveFilePanel("Save as...", "", "Untitled", "*.*");
					if (path == "")
						break;
					_currentDocumentPath = path;
					fs = new FileStream(path, FileMode.OpenOrCreate);
					fs.SetLength(0);
					fs.Close();
					sw = new StreamWriter(path);
					sw.Write(_codeTextArea.Text);
					sw.Close();
					AssetDatabase.Refresh();
					break;
			}
			_toolbarMenuIndex = -1;
		}

		private void LoadFile(string path) {
			var sr = new StreamReader(path);
			var newText = "";
			while (sr.EndOfStream == false)
				newText += sr.ReadLine() + "\n";
			var index = path.LastIndexOf("/", StringComparison.Ordinal);
			var file = path.Substring(index + 1);
			sr.Close();
			_fileTabs.Add(new Tab(file, path, newText));
			_fileTabIndex = _fileTabs.Count - 1;
			_codeTextArea.SetText(newText);
			Repaint();
		}
	}
}