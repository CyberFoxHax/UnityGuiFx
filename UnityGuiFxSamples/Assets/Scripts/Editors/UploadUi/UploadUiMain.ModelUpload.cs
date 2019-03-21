using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Assets.Scripts.DataModels;
using Assets.Scripts.DataModels.Database;
using Assets.Scripts.Lib;
using Assets.Scripts.Lib.NewJson;
using Assets.Scripts.Lib.UnityBinaryMesh;
using UnityEngine;

namespace Assets.Scripts.Editors.UploadUi {
	public partial class UploadUiMain {
		private void ModelFieldOnChange(GameObject gameObject){
			// make db model
			var model = new Model{
				Name = gameObject.name,
				ModelCreator = new ModelCreator{
					Name = Login.GetGlobal().Username
				}
			};
			var modelFile = model.ModelFile = new ModelFile();
			model.ModelFile = modelFile;

			var meshRenderers = gameObject.GetComponentInChildren<MeshRenderer>();
			var meshFilters = gameObject.GetComponentInChildren<MeshFilter>();
			var mesh = meshFilters.sharedMesh;
			var submeshes = meshFilters.sharedMesh.subMeshCount;

			modelFile.Name = mesh.name;
			modelFile.BinaryStream = UnityBinaryMesh.GetMeshBinary(mesh);
			modelFile.Checksum = Crc32.Checksum(modelFile.BinaryStream);

			var materials = meshRenderers.sharedMaterials.Select(p => new MaterialConverter(p).GetDbMaterial()).ToArray();

			var dataSubmeshes = materials.Select((p, i) => new SubMesh{
				Material = p,
				Channel = i,
				OwnerModel = model,
			});

			var textures = materials.SelectMany(p => new[]{
				p.MapAlbedo,
				p.MapNormal,
				p.MapOcclusion,
				p.MapSmooth,
				p.MapMetal,
				p.MapHeight,
				p.MapEmissive
			}).Where(p => p != null).ToArray();

			_previewDataStruct.Content = string.Format(
				"Model: {0}\n" +
				"FileSize: {1}kb\n" +
				"SubMeshes: {2}\n" +
				"Materials: {3}\n" +
				"Textures: {4}\n",
				model,
				Math.Round(modelFile.BinaryStream.Length/1024f),
				dataSubmeshes.Count(),
				materials.Count(),
				string.Join("", textures.Select(p => "\n\t" + p.Name).ToArray())
			);


			Func<Stream, byte[]> getBytes = p =>{
				p.Seek(0, SeekOrigin.Begin);
				var binaryReader = new BinaryReader(p);
				return binaryReader.ReadBytes((int) p.Length);
			};

			Thread.Sleep(500);

			var newForm = new WWWForm();
			for (var i = 0; i < textures.Length; i++)
				newForm.AddBinaryData("texture[" + i + "]", getBytes(textures[i].BinaryStream), textures[i].Name,
					"application/octet-stream");
			newForm.AddBinaryData("modelFile", getBytes(modelFile.BinaryStream), "modelFile", "application/octet-stream");

			newForm.AddBinaryData("jsonData", Encoding.UTF8.GetBytes(Json.Serialize(new{
				textures,
				materials,
				Submeshes = dataSubmeshes,
				model,
				materialTags = _tagsWrapper.CheckedTags.Select(p => p.Id)
			})), "jsonData", "application/json");

			_formData = newForm;

			_uploadButton.Hidden = false;
		}
	}
}
