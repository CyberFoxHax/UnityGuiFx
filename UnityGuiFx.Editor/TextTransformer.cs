using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ImGuiFx.Editor {
	public class TextTransformer {
		public TextTransformer(string xml) {
			XmlDocument = new XmlDocument();
			XmlDocument.LoadXml(xml);

			var root = XmlDocument.DocumentElement;
			if (root.Name != "ImGuiFx") {
				XmlDocument = null;
				return;
			}

			var title = root.Attributes["Title"].Value;
			var fullClassName = root.Attributes["Class"].Value;
			var className = fullClassName.Split('.').Last();
			var namespaceStr = fullClassName.Replace("." + className, "");
			var menuItem = root.Attributes["MenuItem"] != null ? root.Attributes["MenuItem"].Value : "Window";
		}

		public XmlDocument XmlDocument { get; set; }
	}
}