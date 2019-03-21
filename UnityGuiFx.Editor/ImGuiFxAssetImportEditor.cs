using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using ImGuiFx.Controls;
using ImGuiFx.Layouts;
using UnityEngine;

namespace ImGuiFx.Editor {
	public partial class ImGuiFxAssetImportEditor {

		public class ImGuiFxType {
			public string Name { get; set; }
			public string[] Attributes { get; set; }
			public bool SupportsChildren { get; set; }
			public string FullTypeName { get; set; }
		}

		public List<ImGuiFxType> TypesList { get; set; } = new List<ImGuiFxType>();
		public string OriginalText { get; set; }
		public XmlDocument Document { get; set; }

		private void OnLoaded() {
			var asset = (TextAsset)target;
			{
				var parseText = asset.text;
				if (parseText.Contains("<?xml") == false)
					parseText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" + parseText;
				Document = new XmlDocument();
				Document.LoadXml(parseText);
				if (Document.DocumentElement?.Name == "ImGuiFx") {
					
				}
				else {
					_disableUi = true;
					return;
				}
			}

			{
				var showText = asset.text;
				if (showText.Contains("<?xml")) {
					var xmlPos = showText.IndexOf("<?xml");
					var firstLineBreak = showText.Substring(xmlPos).IndexOf("\n");
					showText = showText.Substring(0, xmlPos) + showText.Substring(firstLineBreak + 1);
				}

				showText = showText
					.Replace("<", "<<b></b>")
					.Replace("\t", "    ");

				OriginalText = showText;
				EditorMainTextArea.Text = showText;
			}

			var asm = Assembly.GetAssembly(typeof(Control));

			var controlBaseClass = typeof(Control);
			var layoutBaseClass = typeof(ILayout);

			var allChildren = asm
				.GetExportedTypes()
				.Where(p => p.IsAbstract == false && controlBaseClass.IsAssignableFrom(p))
				.ToArray();

			foreach (var type in allChildren) {
				var tagDefinition = new ImGuiFxType {
					Name = type.Name,
					Attributes = type
						.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name)
						.ToArray(),
					SupportsChildren = layoutBaseClass.IsAssignableFrom(type),
					FullTypeName = type.FullName
				};

				TypesList.Add(tagDefinition);
			}

			TypesList = TypesList
				.OrderBy(p => p.SupportsChildren)
				.ThenBy(p => p.Name)
				.ToList();

			var groups = TypesList
				.GroupBy(p => {
					var split = p.FullTypeName.Split('.');
					return split.ElementAt(split.Length - 2);
				})
				.ToArray();

			foreach (var group in groups) {
				ControlListLayout.Children.Add(new Foldout {
					IsOpen = true,
					Disabled = true,
					Label = "    " + group.Key
				});

				foreach (var imGuiFxType in group.OrderBy(p => p.SupportsChildren))
					ControlListLayout.Children.Add(new Button {
						Content = imGuiFxType.Name
					});
			}
		}

		private void OnSaveButtonClick(Button obj) {
			throw new System.NotImplementedException();
		}

		private void OnRevertButtonClick(Button obj) {
			EditorMainTextArea.Text = OriginalText;
			GUI.FocusControl(null);
		}
	}

}
