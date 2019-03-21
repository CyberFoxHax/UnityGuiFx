using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.ImGuiFx {
	public static class RendererEngine {

		public static void RenderControls(List<Control> controls){
			for (var i = 0; i < controls.Count; i++){
				var baseControl = controls[i];
				if (baseControl.Hidden) continue;
				var layout = baseControl as ILayout;
				if (layout != null)
					layout.LayoutBegin();

				if (baseControl.Children.Any())
					RenderControls(baseControl.Children);

				var control = baseControl as IControl;
				if (control != null)
					control.Render();

				if (layout != null)
					layout.LayoutEnd();
			}
		}
	}
}
