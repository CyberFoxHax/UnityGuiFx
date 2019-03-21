using System;
using System.Collections.Generic;
using ImGuiFx.Controls;
using ImGuiFx.Layouts;
using UnityEngine;

namespace ImGuiFx.Editor {
	public partial class ImGuiFxAssetImportEditor {

		public VerticalLayout ControlListLayout;
		public TextArea EditorMainTextArea;

		private static Texture2D CreateColor(byte r, byte g, byte b, byte a = 255) {
			var color = new Texture2D(1, 1);
			color.SetPixels32(new[] {
				new Color32(r, g, b, a)
			});
			color.Apply();
			return color;
		}

		private static T New<T>(Action<T> f) where T : new() {
			var o = new T();
			f(o);
			return o;
		}

		private void OnCreateUi() {
			_controls = new List<Control> {
				new HorizontalLayout { Children = {
					(ControlListLayout = new VerticalLayout {
						Style = new GUIStyle {
							stretchHeight = true,
							fixedWidth = 150
						},
					}),
					new ScrollView { Children = {
						(EditorMainTextArea = new TextArea {
							Style = new GUIStyle {
								richText = true,
								normal = {
									background = CreateColor(255, 255, 255)
								},
								font = Font.CreateDynamicFontFromOSFont(new []{ "Consolas", "Lucida Console" }, 12),
								fontSize = 12,
								padding = new RectOffset(10, 10, 10, 10),
								stretchWidth = true,
								stretchHeight = true
							}
						})
					}}
				}},
				new Controls.Space(),
				new AlignmentLayout{
					Width = 110,
					Height = 20,
					AlignmentHorizontal = AlignmentLayout.AlignHorizontal.Right,
					Padding = new RectOffset(0, 10, 0, 0),
					Children = {
						new HorizontalLayout{Children = {
							New<Button>(p=> {
								p.Content = "Save";
								p.Click += OnSaveButtonClick;
							}),
							New<Button>(p=> {
								p.Content = "Revert";
								p.Click += OnRevertButtonClick;
							})
						}}
					}
				}
			};
		}
	}

}
