using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.Lib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editors.UploadUi {
	public partial class UploadUiMain{
		private WWWForm _formData;

		protected override void OnLoaded(){
		}

		private void UploadButtonOnClick(Button obj){
			var req = JesusWebRequest.Post(ServerUrl.Url + "Model", _formData);
			req.Send();
			//var resp = req.GetResponse();
		}

		private void ObjectFieldOnChange(ObjectField<Object> objectField, Object oldobject){
			var material = objectField.Object as Material;
			if (material != null)
				MaterialFieldOnChange(material);

			var gameObject = objectField.Object as GameObject;
			if (gameObject != null)
				ModelFieldOnChange(gameObject);
		}
	}
}
