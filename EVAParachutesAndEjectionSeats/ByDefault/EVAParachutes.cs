namespace EVAParachutes {
[KSPAddon(KSPAddon.Startup.MainMenu, true)]
public class initKerbalEVA : UnityEngine.MonoBehaviour {
	public void Awake() {
		ConfigNode EVA = new ConfigNode("MODULE");
		EVA.AddValue("name", "ModuleKrKerbalParachute");

		try {
			PartLoader.getPartInfoByName("kerbalEVA").partPrefab.AddModule(EVA);
		} catch{}

		EVA = new ConfigNode("MODULE");
		EVA.AddValue("name", "ModuleKrKerbalParachute");
		try {
			PartLoader.getPartInfoByName("kerbalEVAfemale").partPrefab.AddModule(EVA);
		} catch{}
	}
}}