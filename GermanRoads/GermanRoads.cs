using ICities;

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GermanRoads
{
	public class GermanRoadsMod : IUserMod {

		public string Name {
			get { return "German Roads"; }
		}

		public string Description {
			get { return "Typical German Roads like the Autobahn or Bundesstrasse."; }
		}

	}

	public class GermanRoadsModLoader : LoadingExtensionBase {
		public static GameObject GermanRoadsContainer = null;

		public override void OnCreated (ILoading loading) {
			base.OnCreated(loading);
//			Debug.Log("German Roads: Loading Container");
			if(GermanRoadsModLoader.GermanRoadsContainer == null) {
				GermanRoadsModLoader.GermanRoadsContainer = new GameObject("German Roads Prefabs");
				GermanRoadsContainer.AddComponent<GermanRoadsContainer>();
			}
		}

		public override void OnReleased () {
			base.OnReleased();
			if(GermanRoadsModLoader.GermanRoadsContainer != null) {
				UnityEngine.Object.Destroy(GermanRoadsContainer);
			}
		}
	}

	public class GermanRoadsContainer : MonoBehaviour {
		public bool initialized = false;
		public int initialized_tab = 100; 
		public static bool initialized_locale = false;
		public static bool initialized_locale_category = false;
		public static UITextureAtlas thumbnails = null;

		private void Awake() {
			UnityEngine.Object.DontDestroyOnLoad(this);
		}

		private void OnLevelWasLoaded(int level) {
			if(level == 6) {
				initialized = false;
			}
		}

		private void Update() {
			if(initialized == false)
			{
				try
				{
					GameObject.Find("Road").GetComponent<NetCollection>();
					Debug.Log ("German Roads: Found Road Collection");
					GameObject.Find("Beautification").GetComponent<NetCollection>();
					Debug.Log ("German Roads: Found Beautification Collection");
					typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(SingletonLite<LocaleManager>.instance);
				}
				catch (Exception)
				{
					return;
				}
				Debug.Log ("German Roads: Found collections");
				initialized = true;
//				loadThumbnailAtlas();
				buildRoads();
				initialized_locale = true;
				Debug.Log("German Roads: Finished installing components");
				return;
			}
			if (initialized_tab > 1) {
				initialized_tab--;
			}
			else if (initialized_tab == 1) {
				initialized_tab = 100;
				Debug.Log("German Roads: Trying to locate panels");
				RoadsGroupPanel[] array = GameObject.FindObjectsOfType<RoadsGroupPanel>();
//				PublicTransportGroupPanel[] array2 = UnityEngine.Object.FindObjectsOfType<PublicTransportGroupPanel>();
//				if (array != null && array2 != null)
				if (array != null)
				{
					int num = 0;
					try {
						RoadsGroupPanel[] array3 = array;
						for (int i = 0; i < array3.Length; i++) {
							RoadsGroupPanel roadsGroupPanel = array3 [i];
							UIButton uIButton = roadsGroupPanel.Find<UIButton> ("German Roads");
							if (uIButton != null) {
								if (uIButton.name == "German Roads") {
									uIButton.text = "GR";
									num++;
									Debug.Log ("German Roads: Found tab button & changed text in roads panel");
								}
							} else {
								Debug.Log ("German Roads: Found roads panel, but not our button");
							}
						}
						if (num >= 1) {
							initialized_tab = 0;
						}
					} catch {
					}
				}
			}
		}
			
//		public static string getModPath() {
//			string workshopPath = ".";
//			foreach(PublishedFileId mod in Steam.workshop.GetSubscribedItems()) {
//				if(mod.AsUInt64 == GermanRoadsMod.workshop_id) {
//					workshopPath = Steam.workshop.GetSubscribedItemPath(mod);
//					Debug.Log("German Roads: Workshop path: " + workshopPath);
//					break;
//				}
//			}
//			string localPath = DataLocation.modsPath + "/GermanRoads";
//			Debug.Log("German Roads: " + localPath);
//			if(System.IO.Directory.Exists(localPath)) {
//				Debug.Log("German Roads: Local path exists, looking for assets here: " + localPath);
//				return localPath;
//			}
//			return workshopPath;
//		}

		public void buildRoads() {
			RoadAI ai = null;

			// Bundesstrasse
			Debug.Log("German Roads: buildRoads()");
			NetInfo bundesstrasse = clonePrefab("Road", "Basic Road", "Bundesstrasse", "A highway with two lanes. 100% as high as the original, but only 12% as way.");
//			replaceTexture(bundesstrasse, "road_small", true, false);
//			bundesstrasse.m_Atlas = thumbnails;
//			bundesstrasse.m_Thumbnail = "SOME4";
			later(() => {
				bundesstrasse.m_createPavement = false;
				bundesstrasse.m_createGravel = true;
				bundesstrasse.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasse.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasse.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;
					} else {
						bundesstrasse.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasse.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse.name));
			});
			NetInfo bundesstrasseElevated = clonePrefab("Road", "Basic Road Elevated", "Bundesstrasse (Elevated)", "");
//			replaceTexture(bundesstrasseElevated, "road_small", true, false);
			later(() => {
				bundesstrasseElevated.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasseElevated.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasseElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						bundesstrasseElevated.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasseElevated.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_elevatedInfo = bundesstrasseElevated;
				RoadBridgeAI bai = bundesstrasseElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseElevated.name));
			});
			NetInfo bundesstrasseBridge = clonePrefab("Road", "Basic Road Bridge", "Bundesstrasse (Bridge)", "");
//			replaceTexture(bundesstrasseBridge, "road_small", true, false);
			later(() => {
				bundesstrasseBridge.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasseBridge.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasseBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						bundesstrasseBridge.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasseBridge.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_bridgeInfo = bundesstrasseBridge;
				RoadBridgeAI bai = bundesstrasseBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseBridge.name));
			});

			// Autobahn
			NetInfo autobahn = clonePrefab("Road", "Medium Road", "Autobahn", "A Autobahn with four lanes. 200% the speed of the highway!");
			//			replaceTexture(autobahn, "road_small", true, false);
			//			autobahn.m_Atlas = thumbnails;
			//			autobahn.m_Thumbnail = "SOME4";
			later(() => {
				autobahn.m_createPavement = false;
				autobahn.m_createGravel = true;
				autobahn.m_averageVehicleLaneSpeed = 8f;
				for(int i = 0; i<autobahn.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahn.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 8f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;
					} else {
						autobahn.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahn.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn.name));
			});
			NetInfo autobahnElevated = clonePrefab("Road", "Medium Road Elevated", "autobahn (Elevated)", "");
			//			replaceTexture(autobahnElevated, "road_small", true, false);
			later(() => {
				autobahnElevated.m_averageVehicleLaneSpeed = 8f;
				for(int i = 0; i<autobahnElevated.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahnElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 8f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahnElevated.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahnElevated.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_elevatedInfo = autobahnElevated;
				RoadBridgeAI bai = autobahnElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnElevated.name));
			});
			NetInfo autobahnBridge = clonePrefab("Road", "Medium Road Bridge", "autobahn (Bridge)", "");
			//			replaceTexture(autobahnBridge, "road_small", true, false);
			later(() => {
				autobahnBridge.m_averageVehicleLaneSpeed = 8f;
				for(int i = 0; i<autobahnBridge.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahnBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 8f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahnBridge.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahnBridge.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_bridgeInfo = autobahnBridge;
				RoadBridgeAI bai = autobahnBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnBridge.name));
			});
		}

//		public void loadThumbnailAtlas() {
//			if(thumbnails != null) return;
//
//			thumbnails = ScriptableObject.CreateInstance<UITextureAtlas>();
//			thumbnails.padding = 0;
//			thumbnails.name = "German Roads Thumbnails";
//
//			Shader shader = Shader.Find("UI/Default UI Shader");
//			if(shader != null) thumbnails.material = new Material(shader);
//
////			string path = getModPath() + "/thumbnail_atlas.png";
//			Texture2D tex = new Texture2D(1, 1);
////			tex.LoadImage(System.IO.File.ReadAllBytes(path));
//			thumbnails.material.mainTexture = tex;
//
//			Texture2D tx = new Texture2D(109, 100);
//
//			string[] ts = new string[] { "", "Disabled", "Focused", "Hovered", "Pressed" };
//			for(int i = 0; i < 10; ++i) {
//				for(int j = 0; j < ts.Length; ++j) {
//					UITextureAtlas.SpriteInfo sprite = new UITextureAtlas.SpriteInfo();
//					sprite.name = string.Format("SOME{0}{1}", i, ts[j]);
//					sprite.region = new Rect( (j*109f)/1024f, (i*100f)/1024f, 109f/1024f, 100f/1024f );
//					sprite.texture = tx;
//					thumbnails.AddSprite(sprite);
//				}
//			}
//		}

//		public void replaceTexture(NetInfo ni, string texName, bool segments, bool nodes) {
//			string path = getModPath() + "/{0}.png";
//			Texture2D tex = new Texture2D(1, 1);
//			tex.LoadImage(System.IO.File.ReadAllBytes(string.Format(path, texName)));
//			replaceTexture(ni, tex, segments, nodes);
//		}

//		public void replaceTexture(NetInfo ni, Texture tex, bool segments, bool nodes) {
//			if(segments) {
//				Material mat = new Material(ni.m_segments[0].m_material);
//				mat.shader = ni.m_segments[0].m_material.shader;
//				mat.SetTexture("_MainTex", tex);
//				for(int i = 0; i < ni.m_segments.Length; ++i) {
//					ni.m_segments[i].m_material = mat;
//					ni.m_segments[i].m_lodRenderDistance = 2500;
//				}
//			}
//			if(nodes) {
//				Material mat = new Material(ni.m_nodes[0].m_material);
//				mat.shader = ni.m_nodes[0].m_material.shader;
//				mat.SetTexture("_MainTex", tex);
//				for(int i = 0; i < ni.m_nodes.Length; ++i) {
//					ni.m_nodes[i].m_material = mat;
//					ni.m_nodes[i].m_lodRenderDistance = 2500;
//				}
//			}
//		}

		public void removeNull<T>(ref T[] array) {
			int count = 0;
			for(int i = 0; i < array.Length; ++i) {
				if(array[i] != null) ++count;
			}
			T[] nu = new T[count];
			count = 0;
			for(int i = 0; i < array.Length; ++i) {
				if(array[i] != null) {
					nu[count] = array[i];
					++count;
				}
			}
			array = nu;
		}

		public void cloneArray<T>(ref T[] source) where T: new() {
			T[] new_array = new T[source.Length];
			for(int i = 0; i < new_array.Length; ++i) {
				T original = source[i];
				T copy = new T();
				foreach(FieldInfo fi in typeof(T).GetAllFields()) {
					fi.SetValue(copy, fi.GetValue(original));
				}
				new_array[i] = copy;
			}
			source = new_array;
		}

		public void shallowCopy<T>(T source, ref T clone) where T: new() {
			foreach(FieldInfo f in typeof(T).GetAllFields()) {
				f.SetValue(clone, f.GetValue(source));
			}
		}

		public void later(Action a) {
			Singleton<LoadingManager>.instance.QueueLoadingAction(inCoroutine(a));
		}

		public IEnumerator inCoroutine(Action a) {
			a.Invoke();
			yield break;
		}

		public PropInfo findProp(string name) {
			foreach(PropCollection collection in UnityEngine.Object.FindObjectsOfType<PropCollection>()) {
				Debug.Log(string.Format("German Roads: PropCollection {0}", collection.name));
				foreach(PropInfo prop in collection.m_prefabs) {
					Debug.Log(string.Format("German Roads: - PropInfo {0}", prop.name));
					if(prop.name == name) {
						return prop;
					}
				}
			}
			return null;
		}

		public NetInfo clonePrefab(string collectionName, string sourceName, string name, string desc)
		{
			Debug.Log(string.Format("German Roads: Cloning {1} -> {2}, adding to collection: {0}", collectionName, sourceName, name));
			foreach(NetCollection collection in NetCollection.FindObjectsOfType<NetCollection>()) {
				foreach(NetInfo prefab in collection.m_prefabs) {
					if(prefab.name == sourceName) {
						return clonePrefab(prefab, collectionName, name, desc);
					}
				}
			}
			return null;
		}
		public NetInfo clonePrefab(NetInfo prefab, string collectionName, string name, string desc)
		{
			Debug.Log ("German Roads: Creating GR Tab and populating it.");
//			NetInfo originalInfo = prefab;

			Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(SingletonLite<LocaleManager>.instance);
			GameObject GRObject = GameObject.Instantiate<GameObject>(prefab.gameObject);
			GRObject.transform.SetParent(transform);
			GRObject.name = name;
			NetInfo GRInfo = GRObject.GetComponent<NetInfo>();
			GRInfo.m_prefabInitialized = false;
			GRInfo.m_netAI = null;
			typeof(NetInfo).GetField("m_UICategory", BindingFlags.Instance|BindingFlags.NonPublic).SetValue(GRInfo, "German Roads");

			if (!initialized_locale)
			{
				Locale.Key key = new Locale.Key
				{
					m_Identifier = "NET_TITLE",
					m_Key = name
				};
				locale.AddLocalizedString(key, name);
				key = new Locale.Key
				{
					m_Identifier = "NET_DESC",
					m_Key = name
				};
				locale.AddLocalizedString(key, desc);
				if (!GermanRoadsContainer.initialized_locale_category)
				{
					key = new Locale.Key
					{
						m_Identifier = "MAIN_CATEGORY",
						m_Key = "German Roads"
					};
					locale.AddLocalizedString(key, "German Roads");
					GermanRoadsContainer.initialized_locale_category = true;
				}
			}
//			if (collectionName != null)
//			{
//				MethodInfo method = typeof(NetCollection).GetMethod("InitializePrefabs", BindingFlags.Static | BindingFlags.NonPublic);
//				Singleton<LoadingManager>.instance.QueueLoadingAction((IEnumerator)method.Invoke(null, new object[]
//					{
//						collectionName,
//						new NetInfo[]
//						{
//							component
//						},
//						new string[0]
//					}));
//				NetInfo.Lane[] lanes = component.m_lanes;
//				for (int i = 0; i < lanes.Length; i++)
//				{
//					GermanRoadsContainer.<>c__DisplayClass15 <>c__DisplayClass = new GermanRoadsContainer.<>c__DisplayClass15();
//					<>c__DisplayClass.l = lanes[i];
//					NetLaneProps p = <>c__DisplayClass.l.m_laneProps;
//					this.later(delegate
//						{
//							<>c__DisplayClass.l.m_laneProps = p;
//						});
//					<>c__DisplayClass.l.m_laneProps = null;
//				}
//			}

//			if(!initialized_locale) {
//				Locale.Key k = new Locale.Key(){ m_Identifier = "NET_TITLE", m_Key = name };
//				Locale.AddLocalizedString(k, name);
//				k = new Locale.Key() { m_Identifier = "NET_DESC", m_Key = name };
//				k.AddLocalizedString(k, desc);
//				if(!initialized_locale_category) {
//					k = new Locale.Key() { m_Identifier = "MAIN_CATEGORY", m_Key = "German Roads" };
//					k.AddLocalizedString(k, "German Roads");
//					initialized_locale_category = true;
//				}
//			}

			Debug.Log (string.Format("German Roads: collectionName: {0}", collectionName));
			if(collectionName != null) {
				MethodInfo initMethod = typeof(NetCollection).GetMethod("InitializePrefabs", BindingFlags.Static | BindingFlags.NonPublic);
				Singleton<LoadingManager>.instance.QueueLoadingAction((IEnumerator)initMethod.Invoke(null, new object[] {
					collectionName,
					new[] { GRInfo },
					new string[] { }
				}));

				// temporarily remove props reference and reset after InitializePrefabs() is done
				foreach(NetInfo.Lane l in GRInfo.m_lanes) {
					NetLaneProps p = l.m_laneProps;
					later(() => {
						l.m_laneProps = p;
					});
					l.m_laneProps = null;
				}
			}

//			return Component;
			return GRInfo;
		}
	}

	public static class Debug
	{
		public static void Out(ColossalFramework.Plugins.PluginManager.MessageType messageType, bool useComma, params System.Object[] o)
		{
			string s = "";
			for (int i = 0; i < o.Length; i++)
			{
				s += o[i].ToString();
				if (i < o.Length - 1 && useComma)
					s += "  ,  ";
			}
			DebugOutputPanel.AddMessage(messageType, s);
		}

		public static void Log(params System.Object[] o)
		{
			Message(o);
		}

		public static void Message(params System.Object[] o)
		{
			Message(true, o);
		}

		public static void Message(bool useComma, params System.Object[] o)
		{
			Out(ColossalFramework.Plugins.PluginManager.MessageType.Message, useComma, o);
		}

		public static void Warning(params System.Object[] o)
		{
			Warning(true, o);
		}

		public static void Warning(bool useComma, params System.Object[] o)
		{
			Out(ColossalFramework.Plugins.PluginManager.MessageType.Warning, useComma, o);
		}

		public static void Error(params System.Object[] o)
		{
			Error(true, o);
		}

		public static void Error(bool useComma, params System.Object[] o)
		{
			Out(ColossalFramework.Plugins.PluginManager.MessageType.Error, useComma, o);
		}
	}
}