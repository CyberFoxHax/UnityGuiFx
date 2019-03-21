using System;
using System.Collections.Generic;
using Assets.Scripts.ImGuiFx;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.ImGuiFx.Layouts;
using UnityEditor;

namespace Assets.Scripts.Editors.DownloadUi {
	public partial class DownloadUiMain : EditorWindowHelper {

		[MenuItem("IdMei/DownloadUI")]
		public static void ShowWindow() {
			StaticStart<DownloadUiMain>();
		}

		public virtual VerticalLayout	_modelsList	{ get; set; }

		private static T New<T>(Action<T> f) where T:new(){
			var o = new T();
			f(o);
			return o;
		}

		protected override IEnumerable<Control> OnInitialize(){
			Title = "DownloadUI";
			yield return New<VerticalLayout>(p3=>{
				p3.Children.AddRange(new Control[]{
					New<Label>(p5=>{
						p5.Content="Model list:";
					}),
					New<VerticalLayout>(p5=>{
						_modelsList=p5;
					}),
				});
			});
		}
	}
}
