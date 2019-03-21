using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ImGuiFx.Editor {
	public class AssemblyScanner {
		public AssemblyScanner() {
			_homeAssemly = Assembly.GetAssembly(StaticBaseControlType);
		}

		private readonly Assembly _homeAssemly;

		private static readonly Type StaticBaseControlType = typeof(Control);
		private static readonly Type StaticInterfaceType = typeof(ILayout);

		public IEnumerable<Type> GetLocalDerivatives() {
			return _homeAssemly
				.GetExportedTypes()
				.Where(p => p.IsAbstract == false && StaticBaseControlType.IsAssignableFrom(p))
				.ToArray();
		}

		public IEnumerable<Type> GetForeignDerivatives() {
			var asm = Assembly.GetEntryAssembly();
			return asm
				.GetExportedTypes()
				.Where(p => p.IsAbstract == false && StaticBaseControlType.IsAssignableFrom(p))
				.ToArray();
		}
	}
}
