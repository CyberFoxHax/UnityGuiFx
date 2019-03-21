namespace ImGuiFx {
	public interface IInternalInitializers {
		void Initialize();
		void OnLoaded();
		bool Alive { get; }
	}
}