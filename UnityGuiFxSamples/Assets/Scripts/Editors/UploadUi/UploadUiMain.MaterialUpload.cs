namespace Assets.Scripts.Editors.UploadUi {
	public partial class UploadUiMain {
		private void MaterialFieldOnChange(UnityEngine.Material material){
			var dbMaterial = new MaterialConverter(material).GetDbMaterial();

			//dbMaterial
		}
	}
}
