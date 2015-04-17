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

		public const UInt64 workshop_id = 426854617;

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
			Debug.Log("German Roads: Loading Container");
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
			
		public void buildRoads() {
			RoadAI ai = null;

			// Bundesstrasse
			Debug.Log("German Roads: buildRoads()");
			NetInfo bundesstrasse = clonePrefab("Road", "Basic Road", "Bundesstrasse", "The German Bundesstrasse. Same as the Basic Road, but Speed 100! No Zoning!");
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
//					} else {
//						bundesstrasse.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasse.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse.name));
			});
			NetInfo bundesstrasseElevated = clonePrefab("Road", "Basic Road Elevated", "Bundesstrasse (Elevated)", "");
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
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseElevated.name));
			});
			NetInfo bundesstrasseBridge = clonePrefab("Road", "Basic Road Bridge", "Bundesstrasse (Bridge)", "");
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
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseBridge.name));
			});

			// Bundesstrasse four lanes, two each direction, devided by a grass lane in the middle
			NetInfo bundesstrasse4lane = clonePrefab("Road", "Medium Road Decoration Grass", "Bundesstrasse Four Lanes", "The German Bundesstrasse with four lanes, two for each direction. Speed still 100. No Zoning!");
			later(() => {
				bundesstrasse4lane.m_createPavement = false;
				bundesstrasse4lane.m_createGravel = true;
				bundesstrasse4lane.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasse4lane.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasse4lane.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;
//					} else {
//						bundesstrasse4lane.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasse4lane.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4lane.name));
			});
			NetInfo bundesstrasse4laneElevated = clonePrefab("Road", "Medium Road Elevated", "Bundesstrasse Four Lanes (Elevated)", "");
			later(() => {
				bundesstrasse4laneElevated.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasse4laneElevated.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasse4laneElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						bundesstrasse4laneElevated.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasse4laneElevated.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_elevatedInfo = bundesstrasse4laneElevated;
				RoadBridgeAI bai = bundesstrasse4laneElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneElevated.name));
			});
			NetInfo bundesstrasse4laneBridge = clonePrefab("Road", "Medium Road Bridge", "Bundesstrasse Four Lanes (Bridge)", "");
			later(() => {
				bundesstrasse4laneBridge.m_averageVehicleLaneSpeed = 2f;
				for(int i = 0; i<bundesstrasse4laneBridge.m_lanes.Length; ++i) {
					NetInfo.Lane l = bundesstrasse4laneBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						bundesstrasse4laneBridge.m_lanes[i] = null;
					}
				}
				removeNull(ref bundesstrasse4laneBridge.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_bridgeInfo = bundesstrasse4laneBridge;
				RoadBridgeAI bai = bundesstrasse4laneBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneBridge.name));
			});

			// Autobahn four lanes, two for each direction, devided by a concrete lane in the middle
			NetInfo autobahn = clonePrefab("Road", "Medium Road", "Autobahn", "The German Autobahn with four lanes, two for each direction. Speed 200!!! No Zoning!");
			later(() => {
				autobahn.m_createPavement = false;
				autobahn.m_createGravel = true;
				autobahn.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahn.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahn.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;
//					} else {
//						autobahn.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahn.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn.name));
			});

			NetInfo autobahnElevated = clonePrefab("Road", "Medium Road Elevated", "Autobahn (Elevated)", "");
			later(() => {
				autobahnElevated.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahnElevated.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahnElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahnElevated.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahnElevated.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_elevatedInfo = autobahnElevated;
				RoadBridgeAI bai = autobahnElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnElevated.name));
			});

			NetInfo autobahnBridge = clonePrefab("Road", "Medium Road Bridge", "Autobahn (Bridge)", "");
			later(() => {
				autobahnBridge.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahnBridge.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahnBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahnBridge.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahnBridge.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_bridgeInfo = autobahnBridge;
				RoadBridgeAI bai = autobahnBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnBridge.name));
			});

			// Autobahn three-lane, one-way
			NetInfo autobahn3lane1way = clonePrefab("Road", "Highway", "Autobahn Three Lanes One-way", "The German Autobahn with three lanes but only one-way. So basically the same as the highway, but with Speed 200!!!");
			later(() => {
				autobahn3lane1way.m_createPavement = false;
				autobahn3lane1way.m_createGravel = true;
				autobahn3lane1way.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahn3lane1way.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahn3lane1way.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;
//					} else {
//						autobahn3lane1way.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahn3lane1way.m_lanes);
				ai = autobahn3lane1way.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn3lane1way.name));
			});

			NetInfo autobahn3lane1wayElevated = clonePrefab("Road", "Highway Elevated", "Autobahn Three Lanes One-way (Elevated)", "");
			later(() => {
				autobahn3lane1wayElevated.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahn3lane1wayElevated.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahn3lane1wayElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahn3lane1wayElevated.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahn3lane1wayElevated.m_lanes);
				ai = autobahn3lane1way.m_netAI as RoadAI;
				ai.m_trafficLights = false;
				ai.m_elevatedInfo = autobahn3lane1wayElevated;
				RoadBridgeAI bai = autobahn3lane1wayElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn3lane1wayElevated.name));
			});

			NetInfo autobahn3lane1wayBridge = clonePrefab("Road", "Highway Bridge", "Autobahn Three Lanes One-way (Bridge)", "");
			later(() => {
				autobahn3lane1wayBridge.m_averageVehicleLaneSpeed = 4f;
				for(int i = 0; i<autobahn3lane1wayBridge.m_lanes.Length; ++i) {
					NetInfo.Lane l = autobahn3lane1wayBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 4f;
					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian || l.m_laneType == NetInfo.LaneType.PublicTransport) {
						autobahn3lane1wayBridge.m_lanes[i] = null;
					}
				}
				removeNull(ref autobahn3lane1wayBridge.m_lanes);
				ai = autobahn3lane1way.m_netAI as RoadAI;
				ai.m_trafficLights = false;
				ai.m_bridgeInfo = autobahn3lane1wayBridge;
				RoadBridgeAI bai = autobahn3lane1wayBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn3lane1wayBridge.name));
			});
		}

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

			return GRInfo;
		}
	}

}