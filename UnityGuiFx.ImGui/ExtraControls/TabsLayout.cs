using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImGuiFx.ExtraControls {
	public class TabsLayout : Control, ILayout {
		
		public int SelectedIndex { get; set; }

		public List<Tab> Tabs { get; set; }

		protected override void OnLoaded() {
			Tabs = Children.OfType<Tab>().ToList();
			Children.Clear();
			OnTabChanged(SelectedIndex);
			base.OnLoaded();
		}

		public class Tab : Control {
			public string Title { get; set; }
		}

		private void OnTabChanged(int newIndex) {
			Children = Tabs[newIndex].Children;
		}

		// https://forum.unity.com/threads/gui-tab-style-controls.89085/
		public void LayoutBegin() {
			const byte darkGray = 0x66;
			const byte lightGray = 0xE5;

			var storeColor = GUI.backgroundColor;
			var highlightCol = new Color32(darkGray, darkGray, darkGray, 255);
			var bgCol = new Color32(lightGray, lightGray, lightGray, 255);

			var buttonStyle = new GUIStyle(GUI.skin.button) {
				padding = {bottom = 8}
			};

			GUILayout.BeginHorizontal();
			//Create a row of buttons
			for (var i = 0; i < Tabs.Count; ++i) {
				if (i == SelectedIndex) {
					GUI.backgroundColor = highlightCol;
					buttonStyle.normal.textColor = Color.white;
					buttonStyle.fontStyle = FontStyle.Bold;
				}
				else {
					GUI.backgroundColor = bgCol;
					buttonStyle.normal.textColor = Color.black;
					buttonStyle.fontStyle = FontStyle.Normal;
				}

				if (GUILayout.Button(Tabs[i].Title, buttonStyle) == false) continue;
				if (SelectedIndex != i)
					OnTabChanged(i);
				SelectedIndex = i; //Tab click
			}
			GUILayout.EndHorizontal();
			//Restore color
			GUI.backgroundColor = storeColor;
			//Draw a line over the bottom part of the buttons (ugly haxx)
			var texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, new Color32((byte) (darkGray * 0.8f), (byte) (darkGray * 0.8f), (byte) (darkGray * 0.8f), 255));
			texture.Apply();
			var lastRect = GUILayoutUtility.GetLastRect();
			GUI.DrawTexture(
				new Rect(
					0,
					lastRect.yMin+lastRect.height - 2,
					Screen.width,
					4
				),
				texture
			);
		}

		public void LayoutEnd() {
		}
	}
}
