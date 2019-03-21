using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.IO;
using UnityEngine;

namespace ImGuiFx.Editor {
	public class XmlAsset {

		private const string CreateMenuPath = "Assets/Create/ImGuiFx/";

		[MenuItem(CreateMenuPath+"Layout")]
		public static void CreateLayoutAsset() {
			var path = GetNewAssetPath("New ImGuiFx Layout.xml");
			Debug.Log(path);
		}

		[MenuItem(CreateMenuPath+"Editor Panel")]
		public static void CreateEditorPanelAsset() {
			var path = GetNewAssetPath("New ImGuiFx Editor Panel.xml");
			Debug.Log(path);
		}

		[MenuItem(CreateMenuPath+"Game Panel")]
		public static void CreateGamePanelAsset() {
			var path = GetNewAssetPath("New ImGuiFx Game Panel.xml");
			Debug.Log(path);
		}

		// http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
		public static string GetNewAssetPath(string fileName) {
			var selection = Selection.activeObject;
			var selectionPath = AssetDatabase.GetAssetPath(selection);
			var path = AssetDatabase.GetAssetPath(selection);
			if (path == "") {
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "") {
				var selectionFileName = Path.GetFileName(selectionPath);
				if (selectionFileName == null)
					throw new Exception("Path filename whatever error");

				path = path.Replace(selectionFileName, "");
			}

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName);
			return assetPathAndName;
		}
	}
}
