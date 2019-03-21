using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.DataModels.Database;
using Assets.Scripts.Editors.UploadUi;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.Lib;
using Assets.Scripts.Lib.UnityBinaryMesh;
using UnityEditor;
using UnityEngine;
using Material = Assets.Scripts.DataModels.Database.Material;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editors.DownloadUi {
	public partial class DownloadUiMain {
		protected override void OnLoaded(){
			var req = new JesusWebRequest(ServerUrl.Url, "Model/List");
			req.SendWait();
			var resp = req.GetResponse<List<Model>>();
			if (resp == null) return;
			foreach (var model in resp){
				var button = new Button{
					Content = string.Join(" - ", new []{
						model.Id + "",
						model.Name,
						model.ModelCreator.Name
					}),
					Data = model
				};
				button.Click += ModelButtonClick;
				_modelsList.Children.Add(button);
			}
		}

		private void ModelButtonClick(Button button){
			var data = (Model) button.Data;
			var req = new JesusWebRequest(ServerUrl.Url, "Model/Detail?id=" + data.Id);
			req.SendWait();
			var jsonData = req.GetResponsePart<ResponseJsonModel>("JsonData");
			var meshData = req.GetResponsePart<Stream>("ModelData");
			if (jsonData == null) {
				Debug.LogError("Response json is null");
				return;
			}

			if (meshData == null){
				Debug.LogError("Response mesh is null");
				return;
			}

			var mesh = UnityBinaryMesh.GetMesh(meshData);
			mesh.name = "FromServer: " + data.ModelFile.Id + (data.Name != null ? " - " + data.Name : "");

			var newGo = new GameObject(mesh.name);
		//	Func<UnityEngine.Material> getNewMat = () => new UnityEngine.Material(Shader.Find("IdMei/WorldSpaceMix"));
			Func<UnityEngine.Material> getNewMat = () => new UnityEngine.Material(Shader.Find("Standard"));
			var matList = new List<UnityEngine.Material>();
			for (int i = 0; i < jsonData.Submeshes.Count; i++){
				var subMesh = jsonData.Submeshes[i];
				var material = jsonData.Materials[i];
				var newMat = getNewMat();

				// transfer properties

				matList.Add(newMat);
			}


			newGo.AddComponent<MeshRenderer>().materials = matList.ToArray();
			newGo.AddComponent<MeshFilter>().mesh = mesh;
			Undo.RegisterCreatedObjectUndo(newGo, "Object download");
			Selection.objects = new Object[]{newGo};
		}

		public class ResponseJsonModel {
			public Model Model { get; set; }
			public ModelFile ModelFile { get; set; }
			public ModelCreator ModelCreator { get; set; }
			public List<SubMesh> Submeshes { get; set; }
			public List<Material> Materials { get; set; }
		}
	}
}
