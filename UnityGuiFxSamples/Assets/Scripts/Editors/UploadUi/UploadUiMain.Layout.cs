using System;
using System.Collections.Generic;
using Assets.Scripts.ImGuiFx;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.ImGuiFx.Layouts;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editors.UploadUi {
	public partial class UploadUiMain : EditorWindowHelper {

		[MenuItem("IdMei/UploadUI")]
		public static void ShowWindow() {
			StaticStart<UploadUiMain>();
		}

		private static T New<T>(Action<T> f) where T:new(){
			var o = new T();
			f(o);
			return o;
		}

		private Label _previewDataStruct;
		private ObjectField<Texture2D> _normalMap;
		private TagsUi _tagsWrapper;
		private Button _uploadButton;

		protected override IEnumerable<Control> OnInitialize(){
			Title = "UploadUI";
			var layout = new VerticalLayout {
				Children = {
					new Label { Content = "Upload" },
					New<ObjectField<UnityEngine.Object>>(p=>{
						p.Label = "Drop Object";
						p.AllowSceneObjects = true;
						p.Change += ObjectFieldOnChange;
					}),
					(_previewDataStruct = new Label()),
					New<Button>(p =>{
						p.Content = "Upload Object";
						p.Hidden = true;
						p.Click += UploadButtonOnClick;
						_uploadButton = p;
					}),
					new Label { Content = "Tags" },
					New<TagsUi>(p => {
						_tagsWrapper = p;
					})
				}
			};

			yield return layout;
		}

	}
}
