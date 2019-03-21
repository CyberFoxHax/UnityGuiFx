using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImGuiFx.Controls {
	// sorry no official documentation for this class
	// https://forum.unity.com/threads/editorgui-preview-renderer.255403/
	public class PreviewRenderControl : Control, IControl, IDisposable {

		public PreviewRenderControl() {
			CameraLookAt = true;
			RotateSpeed = 0.35f;
			//CameraPosition = new Vector3(0, 1.5f, -5);
			Size = new Vector2(250, 250);
		}

		protected override void OnLoaded() {
			PreviewRenderUtility = new PreviewRenderUtility(true);
			GC.SuppressFinalize(PreviewRenderUtility);
			PreviewRenderUtility.lights[0].transform.Rotate(30, 160, 0);
			PreviewRenderUtility.lights[0].intensity = 2;
			var cam = PreviewRenderUtility.camera;
			cam.fieldOfView = 30f;
			cam.nearClipPlane = 0.01f;
			cam.farClipPlane = 1000;

			// Bugfix After Unity3D 2018.1.0a3 Camera.backgroundColor didn't do jackshit anymore
			// https://github.com/Unity-Technologies/UnityCsReference/blob/d9c97528e802409c023a4be9a0dca16a5d75e986/Editor/Mono/Inspector/PreviewRenderUtility.cs
			var tex = new Texture2D(1, 1, TextureFormat.RGBA32, true);
			const float correction = 0.615f; // apparantly U3D can't render the right color...
			tex.SetPixel(0, 0, new Color(0.8509804f * correction, 0.8509804f * correction, 0.827451f * correction, 1f));
			tex.Apply();
			_backgroundStyle = new GUIStyle {
				normal = {
					background = tex
				}
			};

			base.OnLoaded();
		}

		public void AddAnyObject(Object unityObject) {
			var gameObject = unityObject as GameObject;
			if (gameObject != null) {
				AddObject(gameObject);
				return;
			}

			var material = unityObject as Material;
			if (material != null) {
				AddObject(material);
				return;
			}

			var mesh = unityObject as Mesh;
			if (mesh != null) {
				AddObject(mesh);
				return;
			}

			throw new Exception("Object type not supported");
		}

		public void AddObject(Material material) {
			if (IsLoaded)
				AddMaterialInternal(material);
			else
				Loaded += p => AddMaterialInternal(material);
		}

		public void AddObject(GameObject gameObject) {
			if(IsLoaded)
				AddGameObjectInternal(gameObject);
			else
				Loaded += p => AddGameObjectInternal(gameObject);
		}

		public void AddObject(Mesh mesh) {
			if (IsLoaded)
				AddMeshInternal(mesh);
			else
				Loaded += p => AddMeshInternal(mesh);
		}

		public PreviewRenderUtility PreviewRenderUtility { get; set; }

		private GameObject _renderTarget;
		public GameObject RenderTarget {
			get { return _renderTarget; }
			set {
				if(_renderTarget != value)
					PreviewRenderUtility.AddSingleGO(value);
				_renderTarget = value;
			} 
		}

		public RenderTexture RenderTexture { get; set; }
		public bool CameraLookAt { get; set; }
		public float RotateSpeed { get; set; }
		public bool AutoRotate { get; set; } = true;
		public float Rotation { get; set; }

		public event Action<PreviewRenderControl> Rendered;

		private GUIStyle _backgroundStyle;
		private float _cameraDistance;
		private float _cameraAngle = 20 * Mathf.Deg2Rad;
		private Rect _cachedRect;
		private DateTime _lastRenderTime = DateTime.Now;

		public Texture2D Screenshot() {
			if (PreviewRenderUtility == null)
				throw new Exception("PreviewRenderUtility is not ready");
			var renderTexture = PreviewRenderUtility.camera.targetTexture;
			RenderTexture.active = renderTexture;
			if (renderTexture == null)
				throw new Exception("PreviewRenderUtility is not ready");
			var tex = new Texture2D(renderTexture.width, renderTexture.height);
			tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			tex.Apply();
			RenderTexture.active = null;
			return tex;
		}

		public void Render() {
			var now = DateTime.Now;
			var timeDiff = now - _lastRenderTime;
			var deltaTime = (float)timeDiff.TotalMilliseconds / 1000;
			
			var rect = GUILayoutUtility.GetRect(Width, Height);
			if (Event.current.type != EventType.Repaint)
				return;
			_cachedRect = rect;

			PreviewRenderUtility.BeginPreview(_cachedRect, _backgroundStyle);
			var cam = PreviewRenderUtility.camera;

			if(RenderTarget != null) {
				if(RotateSpeed != 0 && AutoRotate)
					Rotation += deltaTime * RotateSpeed;


				var rot = _renderTarget.transform.localEulerAngles;
				rot.y = Rotation * Mathf.Rad2Deg;
				_renderTarget.transform.localEulerAngles = rot;

				if (EditorWindow.focusedWindow != null)
					EditorWindow.focusedWindow.Repaint();
			}

			cam.transform.localPosition = new Vector3(
				0,
				Mathf.Sin(_cameraAngle) * _cameraDistance,
				Mathf.Cos(_cameraAngle) * _cameraDistance
			);

			if (CameraLookAt)
				cam.transform.LookAt(Vector3.zero);

			cam.Render();
			PreviewRenderUtility.EndAndDrawPreview(rect);
			_lastRenderTime = now;
			Rendered?.Invoke(this);
		}

		private void AddMaterialInternal(Material mat) {
			var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Object.DestroyImmediate(gameObject.GetComponent<SphereCollider>());
			var meshRenderer = gameObject.GetComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = mat;
			var mf = gameObject.GetComponent<MeshFilter>();
			var sharedMesh = mf.sharedMesh;
			var mesh = new Mesh {
				vertices = sharedMesh.vertices,
				normals = sharedMesh.normals,
				tangents = sharedMesh.tangents,
				uv = sharedMesh.uv.Select(p => {
					var s = p;
					s.x *= 2;
					return s;
				}).ToArray(),
				triangles = sharedMesh.triangles
			};
			mf.sharedMesh = mesh;
			AddObject(gameObject);
		}

		private void AddMeshInternal(Mesh mesh) {
			if (RenderTarget != null)
				UnityEngine.Object.DestroyImmediate(RenderTarget);
			var gameObjectWrapper = new GameObject("RenderingMeshWrapper");

			var gameObject = new GameObject("MeshContainer");
			gameObject.transform.parent = gameObjectWrapper.transform;

			var meshFilter = gameObject.AddComponent<MeshFilter>();
			var meshRenderer = gameObject.AddComponent<MeshRenderer>();

			meshFilter.sharedMesh = mesh;
			meshRenderer.sharedMaterials = Enumerable.Range(0, mesh.subMeshCount)
				.Select(p => new Material(Shader.Find("Standard")))
				.ToArray();

			AddObject(gameObjectWrapper);
		}

		private void AddGameObjectInternal(GameObject gameObject) {
			if (RenderTarget != null)
				Object.DestroyImmediate(RenderTarget);
			var vertices = gameObject.GetComponentsInChildren<MeshFilter>()
				.Select(p=>p.sharedMesh.vertices.Select(v=>new Vector3(
					v.x * p.transform.localScale.x,
					v.y * p.transform.localScale.y,
					v.z * p.transform.localScale.z
				)))
				.SelectMany(p => p)
				.ToArray();

			if(vertices.Length == 0)
				throw new Exception("There's nothing to render");

			var min = new Vector3(
				vertices.Select(p => p.x).Min(),
				vertices.Select(p => p.y).Min(),
				vertices.Select(p => p.z).Min()
			);
			var max = new Vector3(
				vertices.Select(p => p.x).Max(),
				vertices.Select(p => p.y).Max(),
				vertices.Select(p => p.z).Max()
			);
			var maxMinAbs = new Vector3(
				Mathf.Abs(max.x - min.x),
				Mathf.Abs(max.y - min.y),
				Mathf.Abs(max.z - min.z)
			);
			var center = -(min + maxMinAbs / 2);

			var bounds = new Bounds(center, maxMinAbs);

			gameObject.transform.localPosition += bounds.center;

			RenderTarget = gameObject;


			// https://stackoverflow.com/a/32836605/1148434
			var vec0 = bounds.min;
			var vec1 = bounds.max;

			var boundSphereRadius = new []{
				new Vector3(vec0.x, vec0.y, vec0.z),
				new Vector3(vec1.x, vec0.y, vec0.z),
				new Vector3(vec0.x, vec1.y, vec0.z),
				new Vector3(vec0.x, vec0.y, vec1.z),
				new Vector3(vec1.x, vec1.y, vec0.z),
				new Vector3(vec1.x, vec0.y, vec1.z),
				new Vector3(vec0.x, vec1.y, vec1.z),
				new Vector3(vec1.x, vec1.y, vec1.z)
			}.Select(x => Vector3.Distance(x, bounds.center)).Max();
			var camDistance = (boundSphereRadius) / Mathf.Tan((PreviewRenderUtility.cameraFieldOfView * Mathf.Deg2Rad) / 2.0f);

			_cameraDistance = camDistance;
		}

		public void Dispose() {
			// todo doesn't actually work
			PreviewRenderUtility.Cleanup();
			//Debug.Log("Preview cleanup");
			GC.ReRegisterForFinalize(PreviewRenderUtility);
		}
	}
}
