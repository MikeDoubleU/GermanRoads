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
using System.Runtime.Serialization.Formatters.Binary;
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
				loadThumbnailAtlas();
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
									sortGRPanel();
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
			
		private void buildRoads() {
			RoadAI ai = null;

			// Bundesstrasse
			Debug.Log("German Roads: buildRoads()");
			NetInfo bundesstrasse = clonePrefab("Road", "Basic Road", "Bundesstrasse", "The German Bundesstrasse. Same as the Basic Road, but Speed 100! No Zoning!");
			bundesstrasse.m_Atlas = thumbnails;
			bundesstrasse.m_Thumbnail = "GR0";
			later(() => {
				bundesstrasse.m_color.r = 0.4f;
				bundesstrasse.m_color.g = 0.4f;
				bundesstrasse.m_color.b = 0.4f;
				bundesstrasse.m_createPavement = false;
				bundesstrasse.m_createGravel = true;
				bundesstrasse.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse.m_lanes;

				for(int i = 0; i < bundesstrasse.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
								
								laneProps.name = string.Format("Bundesstrasse props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

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
				bundesstrasseElevated.m_color.r = 0.4f;
				bundesstrasseElevated.m_color.g = 0.4f;
				bundesstrasseElevated.m_color.b = 0.4f;
				bundesstrasseElevated.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasseElevated.m_lanes;

				for(int i = 0; i < bundesstrasseElevated.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasseElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {						
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref bundesstrasseElevated.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_elevatedInfo = bundesstrasseElevated;
				RoadBridgeAI bai = bundesstrasseElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseElevated.name));
			});
			NetInfo bundesstrasseBridge = clonePrefab("Road", "Basic Road Bridge", "Bundesstrasse (Bridge)", "");
			later(() => {
				bundesstrasseBridge.m_color.r = 0.4f;
				bundesstrasseBridge.m_color.g = 0.4f;
				bundesstrasseBridge.m_color.b = 0.4f;
				bundesstrasseBridge.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasseBridge.m_lanes;

				for(int i = 0; i<bundesstrasseBridge.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasseBridge.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {	
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref bundesstrasseBridge.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_bridgeInfo = bundesstrasseBridge;
				RoadBridgeAI bai = bundesstrasseBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseBridge.name));
			});

			NetInfo bundesstrasseSlope = clonePrefab("Road", "Basic Road Slope", "Bundesstrasse (Slope)", "");
			later(() => {
				bundesstrasseSlope.m_color.r = 0.4f;
				bundesstrasseSlope.m_color.g = 0.4f;
				bundesstrasseSlope.m_color.b = 0.4f;
				bundesstrasseSlope.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasseSlope.m_lanes;

				for(int i = 0; i<bundesstrasseSlope.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasseSlope.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {	
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref bundesstrasseSlope.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_slopeInfo = bundesstrasseSlope;
				RoadTunnelAI tai = bundesstrasseSlope.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseSlope.name));
			});

			NetInfo bundesstrasseTunnel = clonePrefab("Road", "Basic Road Tunnel", "Bundesstrasse (Tunnel)", "");
			later(() => {
				bundesstrasseTunnel.m_color.r = 0.4f;
				bundesstrasseTunnel.m_color.g = 0.4f;
				bundesstrasseTunnel.m_color.b = 0.4f;
				bundesstrasseTunnel.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasseTunnel.m_lanes;

				for(int i = 0; i<bundesstrasseTunnel.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasseTunnel.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;
					}

				}
				removeNull(ref bundesstrasseTunnel.m_lanes);
				ai = bundesstrasse.m_netAI as RoadAI;
				ai.m_tunnelInfo = bundesstrasseTunnel;
				RoadTunnelAI tai = bundesstrasseTunnel.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasseTunnel.name));
			});

			// Bundesstrasse four lanes, two each direction, devided by a grass lane in the middle
			NetInfo bundesstrasse4lane = clonePrefab("Road", "Medium Road Decoration Grass", "Bundesstrasse Four Lanes", "The German Bundesstrasse with four lanes, two for each direction. Speed still 100. No Zoning!");
			bundesstrasse4lane.m_Atlas = thumbnails;
			bundesstrasse4lane.m_Thumbnail = "GR1";
			later(() => {
				bundesstrasse4lane.m_color.r = 0.4f;
				bundesstrasse4lane.m_color.g = 0.4f;
				bundesstrasse4lane.m_color.b = 0.4f;
				bundesstrasse4lane.m_createPavement = false;
				bundesstrasse4lane.m_createGravel = true;
				bundesstrasse4lane.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse4lane.m_lanes;

				for(int i = 0; i<bundesstrasse4lane.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse4lane.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

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
				bundesstrasse4laneElevated.m_color.r = 0.4f;
				bundesstrasse4laneElevated.m_color.g = 0.4f;
				bundesstrasse4laneElevated.m_color.b = 0.4f;
				bundesstrasse4laneElevated.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse4laneElevated.m_lanes;

				for(int i = 0; i<bundesstrasse4laneElevated.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse4laneElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref bundesstrasse4laneElevated.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_elevatedInfo = bundesstrasse4laneElevated;
				RoadBridgeAI bai = bundesstrasse4laneElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneElevated.name));
			});
			NetInfo bundesstrasse4laneBridge = clonePrefab("Road", "Medium Road Bridge", "Bundesstrasse Four Lanes (Bridge)", "");
			later(() => {
				bundesstrasse4laneBridge.m_color.r = 0.4f;
				bundesstrasse4laneBridge.m_color.g = 0.4f;
				bundesstrasse4laneBridge.m_color.b = 0.4f;
				bundesstrasse4laneBridge.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse4laneBridge.m_lanes;

				for(int i = 0; i<bundesstrasse4laneBridge.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse4laneBridge.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref bundesstrasse4laneBridge.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_bridgeInfo = bundesstrasse4laneBridge;
				RoadBridgeAI bai = bundesstrasse4laneBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneBridge.name));
			});

			NetInfo bundesstrasse4laneSlope = clonePrefab("Road", "Medium Road Slope", "Bundesstrasse Four Lanes (Slope)", "");
			later(() => {
				bundesstrasse4laneSlope.m_color.r = 0.4f;
				bundesstrasse4laneSlope.m_color.g = 0.4f;
				bundesstrasse4laneSlope.m_color.b = 0.4f;
				bundesstrasse4laneSlope.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse4laneSlope.m_lanes;

				for(int i = 0; i<bundesstrasse4laneSlope.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse4laneSlope.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 2f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
											//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Bundesstrasse Four Lanes Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
											//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("100 Speed Limit");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(2.9f,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(-2.9f,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref bundesstrasse4laneSlope.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_slopeInfo = bundesstrasse4laneSlope;
				RoadTunnelAI tai = bundesstrasse4laneSlope.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneSlope.name));
			});

			NetInfo bundesstrasse4laneTunnel = clonePrefab("Road", "Medium Road Tunnel", "Bundesstrasse Four Lanes (Tunnel)", "");
			later(() => {
				bundesstrasse4laneTunnel.m_color.r = 0.4f;
				bundesstrasse4laneTunnel.m_color.g = 0.4f;
				bundesstrasse4laneTunnel.m_color.b = 0.4f;
				bundesstrasse4laneTunnel.m_averageVehicleLaneSpeed = 2f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = bundesstrasse4laneTunnel.m_lanes;

				for(int i = 0; i<bundesstrasse4laneTunnel.m_lanes.Length; i++) {
					NetInfo.Lane l = bundesstrasse4laneTunnel.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 2f;
					}

				}
				removeNull(ref bundesstrasse4laneTunnel.m_lanes);
				ai = bundesstrasse4lane.m_netAI as RoadAI;
				ai.m_tunnelInfo = bundesstrasse4laneTunnel;
				RoadTunnelAI tai = bundesstrasse4laneTunnel.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", bundesstrasse4laneTunnel.name));
			});

			// Autobahn four lanes, two for each direction, devided by a concrete lane in the middle
			NetInfo autobahn = clonePrefab("Road", "Medium Road", "Autobahn", "The German Autobahn with four lanes, two for each direction. Speed 200!!! No Zoning!");
			autobahn.m_Atlas = thumbnails;
			autobahn.m_Thumbnail = "GR2";
			later(() => {
				autobahn.m_color.r = 0.4f;
				autobahn.m_color.g = 0.4f;
				autobahn.m_color.b = 0.4f;
				autobahn.m_createPavement = false;
				autobahn.m_createGravel = true;
				autobahn.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn.m_lanes;

				for(int i = 0; i<autobahn.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();
							

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(-2,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(2,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(2.5f,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(-2.5f,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

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
				autobahnElevated.m_color.r = 0.4f;
				autobahnElevated.m_color.g = 0.4f;
				autobahnElevated.m_color.b = 0.4f;
				autobahnElevated.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahnElevated.m_lanes;

				for(int i = 0; i<autobahnElevated.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahnElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(-2,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(2,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(0.8f,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(-0.8f,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref autobahnElevated.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_elevatedInfo = autobahnElevated;
				RoadBridgeAI bai = autobahnElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnElevated.name));
			});

			NetInfo autobahnBridge = clonePrefab("Road", "Medium Road Bridge", "Autobahn (Bridge)", "");
			later(() => {
				autobahnBridge.m_color.r = 0.4f;
				autobahnBridge.m_color.g = 0.4f;
				autobahnBridge.m_color.b = 0.4f;
				autobahnBridge.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahnBridge.m_lanes;

				for(int i = 0; i<autobahnBridge.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahnBridge.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(-2,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(2,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(0.8f,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(-0.8f,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref autobahnBridge.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_bridgeInfo = autobahnBridge;
				RoadBridgeAI bai = autobahnBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				bai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnBridge.name));
			});

			NetInfo autobahnSlope = clonePrefab("Road", "Medium Road Slope", "Autobahn (Slope)", "");
			later(() => {
				autobahnSlope.m_color.r = 0.4f;
				autobahnSlope.m_color.g = 0.4f;
				autobahnSlope.m_color.b = 0.4f;
				autobahnSlope.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahnSlope.m_lanes;

				for(int i = 0; i<autobahnSlope.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahnSlope.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
											//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(-2,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(2,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();


					} else if(l.m_laneType == NetInfo.LaneType.None) {
						// Middle of the street...

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
											//										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											//											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("50 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0) {
														newProp.m_position = new Vector3(2.4f,0,5);
													} else if (newProp.m_angle == 180) {
														newProp.m_position = new Vector3(-2.4f,0,-5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}
				}
				removeNull(ref autobahnSlope.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_slopeInfo = autobahnSlope;
				RoadTunnelAI tai = autobahnSlope.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnSlope.name));
			});

			NetInfo autobahnTunnel = clonePrefab("Road", "Medium Road Tunnel", "Autobahn (Tunnel)", "");
			later(() => {
				autobahnTunnel.m_color.r = 0.4f;
				autobahnTunnel.m_color.g = 0.4f;
				autobahnTunnel.m_color.b = 0.4f;
				autobahnTunnel.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahnTunnel.m_lanes;

				for(int i = 0; i<autobahnTunnel.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahnTunnel.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 4f;
					}

				}
				removeNull(ref autobahnTunnel.m_lanes);
				ai = autobahn.m_netAI as RoadAI;
				ai.m_tunnelInfo = autobahnTunnel;
				RoadTunnelAI tai = autobahnTunnel.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahnTunnel.name));
			});

			// Autobahn three-lane, one-way
			NetInfo autobahn3lane1way = clonePrefab("Road", "Highway", "Autobahn Three Lanes One-way", "The German Autobahn with three lanes but only one-way. So basically the same as the highway, but with Speed 200!!!");
			autobahn3lane1way.m_Atlas = thumbnails;
			autobahn3lane1way.m_Thumbnail = "GR3";
			later(() => {
				autobahn3lane1way.m_color.r = 0.4f;
				autobahn3lane1way.m_color.g = 0.4f;
				autobahn3lane1way.m_color.b = 0.4f;
				autobahn3lane1way.m_createPavement = false;
				autobahn3lane1way.m_createGravel = true;
				autobahn3lane1way.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn3lane1way.m_lanes;

				for(int i = 0; i < autobahn3lane1way.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn3lane1way.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Three Lanes One-way props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.None) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							// if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
							if (newLane.m_laneType == l.m_laneType && newLane.m_position == l.m_position && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
								
								// laneProps.name = string.Format("Autobahn Three Lanes One-way props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								laneProps.name = string.Format("Autobahn Three Lanes One-way props {0} {1}", newLane.m_position, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("100 Speed Limit")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Highway")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();
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
				autobahn3lane1wayElevated.m_color.r = 0.4f;
				autobahn3lane1wayElevated.m_color.g = 0.4f;
				autobahn3lane1wayElevated.m_color.b = 0.4f;
				autobahn3lane1wayElevated.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn3lane1wayElevated.m_lanes;

				for(int i = 0; i < autobahn3lane1wayElevated.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn3lane1wayElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Three Lanes One-way Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.None) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							// if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
							if (newLane.m_laneType == l.m_laneType && newLane.m_position== l.m_position && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								// laneProps.name = string.Format("Autobahn Three Lanes One-way Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								laneProps.name = string.Format("Autobahn Three Lanes One-way Elevated props {0} {1}", newLane.m_position, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("100 Speed Limit")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Highway")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();
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
				autobahn3lane1wayBridge.m_color.r = 0.4f;
				autobahn3lane1wayBridge.m_color.g = 0.4f;
				autobahn3lane1wayBridge.m_color.b = 0.4f;
				autobahn3lane1wayBridge.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn3lane1wayBridge.m_lanes;

				for(int i = 0; i < autobahn3lane1wayBridge.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn3lane1wayBridge.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Three Lanes One-way Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.None) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							// if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
							if (newLane.m_laneType == l.m_laneType && newLane.m_position == l.m_position && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								// laneProps.name = string.Format("Autobahn Three Lanes One-way Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								laneProps.name = string.Format("Autobahn Three Lanes One-way Bridge props {0} {1}", newLane.m_position, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("100 Speed Limit")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Highway")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();
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

			NetInfo autobahn3lane1waySlope = clonePrefab("Road", "Highway Slope", "Autobahn Three Lanes One-way (Slope)", "");
			later(() => {
				autobahn3lane1waySlope.m_color.r = 0.4f;
				autobahn3lane1waySlope.m_color.g = 0.4f;
				autobahn3lane1waySlope.m_color.b = 0.4f;
				autobahn3lane1waySlope.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn3lane1waySlope.m_lanes;

				for(int i = 0; i < autobahn3lane1waySlope.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn3lane1waySlope.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 4f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Three Lanes One-way Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.None) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							// if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {
							if (newLane.m_laneType == l.m_laneType && newLane.m_position == l.m_position && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								// laneProps.name = string.Format("Autobahn Three Lanes One-way Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								laneProps.name = string.Format("Autobahn Three Lanes One-way Slope props {0} {1}", newLane.m_position, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("100 Speed Limit")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Bus Stop Large")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Highway")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Motorway Sign")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													if (newProp.m_angle == 0 && newLane.m_position > 0f) {
														newProp.m_position = new Vector3(-2.4f,0,5);
													} else if (newProp.m_angle == 0 && newLane.m_position < 0f) {
														newProp.m_position = new Vector3(2.4f,0,5);
													}
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();
					}
				}
				removeNull(ref autobahn3lane1waySlope.m_lanes);
				ai = autobahn3lane1way.m_netAI as RoadAI;
				ai.m_slopeInfo = autobahn3lane1waySlope;
				RoadTunnelAI tai = autobahn3lane1waySlope.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn3lane1waySlope.name));
			});

			NetInfo autobahn3lane1wayTunnel = clonePrefab("Road","Highway Tunnel", "Autobahn Three Lanes One-way (Tunnel)", "");
			later(() => {
				autobahn3lane1wayTunnel.m_color.r = 0.4f;
				autobahn3lane1wayTunnel.m_color.g = 0.4f;
				autobahn3lane1wayTunnel.m_color.b = 0.4f;
				autobahn3lane1wayTunnel.m_averageVehicleLaneSpeed = 4f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = autobahn3lane1wayTunnel.m_lanes;

				for(int i = 0; i<autobahn3lane1wayTunnel.m_lanes.Length; i++) {
					NetInfo.Lane l = autobahn3lane1wayTunnel.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 4f;
					}

				}
				removeNull(ref autobahn3lane1wayTunnel.m_lanes);
				ai = autobahn3lane1way.m_netAI as RoadAI;
				ai.m_tunnelInfo = autobahn3lane1wayTunnel;
				RoadTunnelAI tai = autobahn3lane1wayTunnel.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", autobahn3lane1wayTunnel.name));
			});

			// Autobahn ramp, two-lane, one-way
			NetInfo auobahnramp = clonePrefab("Road", "Oneway Road", "Autobahn Ramp", "A two lane Autobahn ramp with speed 80.");
			auobahnramp.m_Atlas = thumbnails;
			auobahnramp.m_Thumbnail = "GR4";
			later(() => {
				auobahnramp.m_color.r = 0.4f;
				auobahnramp.m_color.g = 0.4f;
				auobahnramp.m_color.b = 0.4f;
				auobahnramp.m_createPavement = false;
				auobahnramp.m_createGravel = true;
				auobahnramp.m_averageVehicleLaneSpeed = 1.6f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = auobahnramp.m_lanes;

				for(int i = 0; i < auobahnramp.m_lanes.Length; i++) {
					NetInfo.Lane l = auobahnramp.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 1.6f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref auobahnramp.m_lanes);
				ai = auobahnramp.m_netAI as RoadAI;
				ai.m_highwayRules = true;
				ai.m_enableZoning = false;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", auobahnramp.name));
			});
			NetInfo auobahnrampElevated = clonePrefab("Road", "Oneway Road Elevated", "auobahnramp (Elevated)", "");
			later(() => {
				auobahnrampElevated.m_color.r = 0.4f;
				auobahnrampElevated.m_color.g = 0.4f;
				auobahnrampElevated.m_color.b = 0.4f;
				auobahnrampElevated.m_averageVehicleLaneSpeed = 1.6f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = auobahnrampElevated.m_lanes;

				for(int i = 0; i < auobahnrampElevated.m_lanes.Length; i++) {
					NetInfo.Lane l = auobahnrampElevated.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 1.6f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Elevated props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref auobahnrampElevated.m_lanes);
				ai = auobahnramp.m_netAI as RoadAI;
				ai.m_elevatedInfo = auobahnrampElevated;
				RoadBridgeAI bai = auobahnrampElevated.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", auobahnrampElevated.name));
			});
			NetInfo auobahnrampBridge = clonePrefab("Road", "Oneway Road Bridge", "auobahnramp (Bridge)", "");
			later(() => {
				auobahnrampBridge.m_color.r = 0.4f;
				auobahnrampBridge.m_color.g = 0.4f;
				auobahnrampBridge.m_color.b = 0.4f;
				auobahnrampBridge.m_averageVehicleLaneSpeed = 1.6f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = auobahnrampBridge.m_lanes;

				for(int i = 0; i < auobahnrampBridge.m_lanes.Length; i++) {
					NetInfo.Lane l = auobahnrampBridge.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 1.6f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Bridge props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref auobahnrampBridge.m_lanes);
				ai = auobahnramp.m_netAI as RoadAI;
				ai.m_bridgeInfo = auobahnrampBridge;
				RoadBridgeAI bai = auobahnrampBridge.m_netAI as RoadBridgeAI;
				bai.m_highwayRules = true;
				ai.m_trafficLights = false;
				Debug.Log(string.Format("German Roads: Initialized {0}", auobahnrampBridge.name));
			});

			NetInfo auobahnrampSlope = clonePrefab("Road", "Oneway Road Slope", "Autobahn Ramp (Slope)", "");
			later(() => {
				auobahnrampSlope.m_color.r = 0.4f;
				auobahnrampSlope.m_color.g = 0.4f;
				auobahnrampSlope.m_color.b = 0.4f;
				auobahnrampSlope.m_averageVehicleLaneSpeed = 1.6f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = auobahnrampSlope.m_lanes;

				for(int i = 0; i < auobahnrampSlope.m_lanes.Length; i++) {
					NetInfo.Lane l = auobahnrampSlope.m_lanes[i];
					l.m_allowStop = false;
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_speedLimit = 1.6f;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {
									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("Manhole")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}
								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					} else if(l.m_laneType == NetInfo.LaneType.Pedestrian) {
						l.m_laneType = NetInfo.LaneType.None;

						var proplist = new FastList<NetLaneProps.Prop>();
						NetLaneProps laneProps = new NetLaneProps();

						for (int k = 0; k < copyLanes.Length; k++) {

							NetInfo.Lane newLane = copyLanes[k];
							if (newLane.m_laneType == l.m_laneType && newLane.m_direction == l.m_direction && newLane.m_similarLaneIndex == l.m_similarLaneIndex) {

								laneProps.name = string.Format("Autobahn Ramp Slope props {0} {1}", newLane.m_direction, newLane.m_similarLaneIndex);
								if (laneProps.name == l.m_laneProps.name) {
									laneProps.name = "";
								} else {

									foreach(var l_props in newLane.m_laneProps.m_props) {
										if (l_props.m_prop.name.Equals ("New Street Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Info Terminal")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Parking Meter")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Electricity Box")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Fire Hydrant")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Avenue Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Highway Light")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("40 Speed Limit")) {
											try
											{
												NetLaneProps.Prop newProp = new NetLaneProps.Prop();
												shallowCopy<NetLaneProps.Prop>(l_props, ref newProp);
												newProp.m_prop = findProp("Motorway Sign");
												if (newProp.m_prop != null) {
													newProp.m_finalProp = newProp.m_prop;
													proplist.Add (newProp);
												}
												newProp = null;
											} catch { }
										} else if (l_props.m_prop.name.Equals ("Street Name Sign")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("Random Street Prop NoParking")) {
											continue;
										} else if (l_props.m_prop.name.Equals ("New Street Light Small Road")) {
											continue;
										} else {
											proplist.Add (l_props);
										}
									}

								}

							}

						}

						if (laneProps.name != "") {
							laneProps.m_props = proplist.ToArray();
							l.m_laneProps = laneProps;
						}

						proplist.Clear();
						proplist.Release();

					}

				}
				removeNull(ref auobahnrampSlope.m_lanes);
				ai = auobahnramp.m_netAI as RoadAI;
				ai.m_slopeInfo = auobahnrampSlope;
				RoadTunnelAI tai = auobahnrampSlope.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", auobahnrampSlope.name));
			});

			NetInfo auobahnrampTunnel = clonePrefab("Road", "Oneway Road Tunnel", "Autobahn Ramp (Tunnel)", "");
			later(() => {
				auobahnrampTunnel.m_color.r = 0.4f;
				auobahnrampTunnel.m_color.g = 0.4f;
				auobahnrampTunnel.m_color.b = 0.4f;
				auobahnrampTunnel.m_averageVehicleLaneSpeed = 1.6f;

				// Copy all lanes into a new array
				NetInfo.Lane[] copyLanes = auobahnrampTunnel.m_lanes;

				for(int i = 0; i<auobahnrampTunnel.m_lanes.Length; i++) {
					NetInfo.Lane l = auobahnrampTunnel.m_lanes[i];
					if(l.m_laneType == NetInfo.LaneType.Vehicle) {
						l.m_allowStop = false;
						l.m_speedLimit = 1.6f;
					}

				}
				removeNull(ref auobahnrampTunnel.m_lanes);
				ai = auobahnramp.m_netAI as RoadAI;
				ai.m_tunnelInfo = auobahnrampTunnel;
				RoadTunnelAI tai = auobahnrampTunnel.m_netAI as RoadTunnelAI;
				tai.m_highwayRules = true;
				ai.m_trafficLights = false;
				tai.m_trafficLights = ai.m_trafficLights;
				Debug.Log(string.Format("German Roads: Initialized {0}", auobahnrampTunnel.name));
			});
		}

		private void removeNull<T>(ref T[] array) {
			int count = 0;
			for(int i = 0; i < array.Length; i++) {
				if(array[i] != null) count++;
			}
			T[] nu = new T[count];
			count = 0;
			for(int i = 0; i < array.Length; i++) {
				if(array[i] != null) {
					nu[count] = array[i];
					count++;
				}
			}
			array = nu;
		}

		private void sortGRPanel() {
			UIButton[] uiButton = GameObject.FindObjectsOfType<UIButton>();
			Boolean bChange = false;

			for (int iruns = 0; iruns < 50; iruns++) {
				
				bChange = false;

				Debug.Log("German Roads: Sorting Panel.");

				for (int i = 0; i < uiButton.Length; i++) {

						switch (uiButton [i].name) {
						case "Bundesstrasse":
							if (uiButton [i].zOrder != 0) {
								uiButton [i].zOrder = 0;
								bChange = true;
							}
							break;
						case "Bundesstrasse Four Lanes":
							if (uiButton [i].zOrder != 1) {
								uiButton [i].zOrder = 1;
								bChange = true;
							}
							break;
						case "Autobahn":
							if (uiButton [i].zOrder != 2) {
								uiButton [i].zOrder = 2;
								bChange = true;
							}
							break;
						case "Autobahn Three Lanes One-way":
							if (uiButton [i].zOrder != 3) {
								uiButton [i].zOrder = 3;
								bChange = true;
							}
							break;
						case "Autobahn Ramp":
							if (uiButton [i].zOrder != 4) {
								uiButton [i].zOrder = 4;
								bChange = true;
							}
							break;
						}
				}

				if (bChange == false) {
					break;
				}
			}
		}

		private void loadThumbnailAtlas() {
			if(thumbnails != null) return;

			thumbnails = new UITextureAtlas();
			thumbnails.padding = 0;
			thumbnails.name = "German Roads Thumbnails";

			Shader shader = Shader.Find("UI/Default UI Shader");
			if(shader != null) thumbnails.material = new Material(shader);

			string path = getModPath() + "/GR_atlas.png";
			Texture2D tex = new Texture2D(1, 1);
			tex.LoadImage(System.IO.File.ReadAllBytes(path));
			thumbnails.material.mainTexture = tex;

			Texture2D tx = new Texture2D(109, 100);

			string[] ts = new string[] { "", "Disabled", "Focused", "Hovered", "Pressed" };
			for(int i = 0; i < 10; ++i) {
				for(int j = 0; j < ts.Length; ++j) {
					UITextureAtlas.SpriteInfo sprite = new UITextureAtlas.SpriteInfo();
					sprite.name = string.Format("GR{0}{1}", i, ts[j]);
					sprite.region = new Rect( (j*109f)/545f, (i*100f)/500f, 109f/545f, 100f/500f );
					sprite.texture = tx;
					thumbnails.AddSprite(sprite);
				}
			}
		}

		private static string getModPath() {
			string workshopPath = ".";
			foreach(PublishedFileId mod in Steam.workshop.GetSubscribedItems()) {
				if(mod.AsUInt64 == GermanRoadsMod.workshop_id) {
					workshopPath = Steam.workshop.GetSubscribedItemPath(mod);
					break;
				}
			}
			string localPath = DataLocation.modsPath + "/GermanRoads";
			if(System.IO.Directory.Exists(localPath)) {
				return localPath;
			}
			return workshopPath;
		}

//		private void cloneArray<T>(ref T[] source) where T: new() {
//			T[] new_array = new T[source.Length];
//			for(int i = 0; i < new_array.Length; i++) {
//				T original = source[i];
//				T copy = new T();
//				foreach(FieldInfo fi in typeof(T).GetAllFields()) {
//					fi.SetValue(copy, fi.GetValue(original));
//				}
//				new_array[i] = copy;
//			}
//			source = new_array;
//		}

		private void shallowCopy<T>(T source, ref T clone) where T: new() {
			foreach(FieldInfo f in typeof(T).GetAllFields()) {
				f.SetValue(clone, f.GetValue(source));
			}
		}

		private void later(Action a) {
			Singleton<LoadingManager>.instance.QueueLoadingAction(inCoroutine(a));
		}

		private IEnumerator inCoroutine(Action a) {
			a.Invoke();
			yield break;
		}

		private PropInfo findProp(string name) {
			PropCollection[] propArray = UnityEngine.Object.FindObjectsOfType<PropCollection>();
			for (int i = 0; i < propArray.Length; i++)
			{
				PropCollection prpCollection = propArray [i];
				PropInfo[] prpPrefabs = prpCollection.m_prefabs;
				for (int j = 0; j < prpPrefabs.Length; j++)
				{
					PropInfo prpInfo = prpPrefabs [j];
					if(prpInfo.name == name) {
						return prpInfo;
					}
				}
			}
			return null;
		}

		private NetInfo clonePrefab(string collectionName, string sourceName, string name, string desc)
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
		private NetInfo clonePrefab(NetInfo prefab, string collectionName, string name, string desc)
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