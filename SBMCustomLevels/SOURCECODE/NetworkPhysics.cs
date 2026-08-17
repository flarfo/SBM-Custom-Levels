using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Catobyte.Networking.Physics.Authority;
using Catobyte.Networking.Physics.Data;
using Catobyte.Networking.Physics.Interpolation;
using Catobyte.Networking.Physics.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Catobyte.Networking.Physics
{
		public class NetworkPhysics : MonoBehaviour
	{
								public static event Action OnReset
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkPhysics.OnReset;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnReset, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkPhysics.OnReset;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnReset, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnPreStep
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkPhysics.OnPreStep;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnPreStep, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkPhysics.OnPreStep;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnPreStep, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnPostStep
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkPhysics.OnPostStep;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnPostStep, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkPhysics.OnPostStep;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkPhysics.OnPostStep, action3, action2);
				}
				while (action != action2);
			}
		}

								internal static NetworkPhysics Instance
		{
			[CompilerGenerated]
			get
			{
				return NetworkPhysics.< Instance > k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				NetworkPhysics.< Instance > k__BackingField = value;
			}
		}

								public static bool IsEnabled
		{
			get
			{
				return NetworkPhysics.Instance.networkPhysicsEnabled;
			}
			set
			{
				NetworkPhysics.Instance.networkPhysicsEnabled = value;
				Physics.autoSimulation = !NetworkPhysics.Instance.networkPhysicsEnabled;
			}
		}

								public static bool SteppingPaused
		{
			get
			{
				return NetworkPhysics.Instance.steppingPaused;
			}
			set
			{
				NetworkPhysics.Instance.steppingPaused = value;
			}
		}

						public static PhysicsSessionId PhysicsSessionId
		{
			get
			{
				return NetworkPhysics.Instance.physicsSessionId;
			}
		}

						public static Interpolation Interpolation
		{
			get
			{
				return NetworkPhysics.Instance.interpolation;
			}
		}

								public static bool LocalDebugMode
		{
			get
			{
				return NetworkPhysics.Instance.localDebugMode;
			}
			set
			{
				NetworkPhysics.Instance.localDebugMode = value;
				NetworkPhysics.Instance.debugAutoUpdate = value;
				NetworkPhysics.Instance.drawAuthVolumes = value;
				NetworkPhysics.Instance.drawAuthChains = value;
			}
		}

								public static bool DrawAuthVolumes
		{
			get
			{
				return NetworkPhysics.Instance != null && NetworkPhysics.Instance.drawAuthVolumes;
			}
			set
			{
				if (NetworkPhysics.Instance != null)
				{
					NetworkPhysics.Instance.drawAuthVolumes = value;
				}
			}
		}

								public static bool SimulatePhysics
		{
			get
			{
				return NetworkPhysics.Instance != null && NetworkPhysics.Instance.simulatePhysics;
			}
			set
			{
				if (NetworkPhysics.Instance != null)
				{
					NetworkPhysics.Instance.simulatePhysics = value;
				}
			}
		}

								public static bool DebugAutoUpdate
		{
			get
			{
				return NetworkPhysics.Instance != null && NetworkPhysics.Instance.debugAutoUpdate;
			}
			set
			{
				if (NetworkPhysics.Instance != null)
				{
					NetworkPhysics.Instance.debugAutoUpdate = value;
				}
			}
		}

								public static bool InterpolationEnabled
		{
			get
			{
				return NetworkPhysics.Instance != null && NetworkPhysics.Instance.interpolationEnabled;
			}
			set
			{
				if (NetworkPhysics.Instance != null)
				{
					NetworkPhysics.Instance.interpolationEnabled = value;
				}
			}
		}

								public static bool ApplyJointStates
		{
			[CompilerGenerated]
			get
			{
				return NetworkPhysics.< ApplyJointStates > k__BackingField;
			}
			[CompilerGenerated]
			set
			{
				NetworkPhysics.< ApplyJointStates > k__BackingField = value;
			}
		} = true;

								public static bool ApplyVelocity
		{
			[CompilerGenerated]
			get
			{
				return NetworkPhysics.< ApplyVelocity > k__BackingField;
			}
			[CompilerGenerated]
			set
			{
				NetworkPhysics.< ApplyVelocity > k__BackingField = value;
			}
		} = true;

								public static bool ApplyAngularVelocity
		{
			[CompilerGenerated]
			get
			{
				return NetworkPhysics.< ApplyAngularVelocity > k__BackingField;
			}
			[CompilerGenerated]
			set
			{
				NetworkPhysics.< ApplyAngularVelocity > k__BackingField = value;
			}
		} = true;

								public static bool IgnoreSnapToTargetStateDistance
		{
			[CompilerGenerated]
			get
			{
				return NetworkPhysics.< IgnoreSnapToTargetStateDistance > k__BackingField;
			}
			[CompilerGenerated]
			set
			{
				NetworkPhysics.< IgnoreSnapToTargetStateDistance > k__BackingField = value;
			}
		} = false;

						public static InterpolationSettings InterpolationSettings
		{
			get
			{
				if (!(NetworkPhysics.Instance != null))
				{
					return null;
				}
				return NetworkPhysics.Instance.interpolationSettings;
			}
		}

				public static InterpolationSettings GetOrCreateDebugInterpolationSettings()
		{
			if (NetworkPhysics.Instance == null)
			{
				return null;
			}
			if (NetworkPhysics.Instance.interpolationSettings == null)
			{
				return null;
			}
			if (!NetworkPhysics.Instance.debugInterpolationSettingsCloned)
			{
				InterpolationSettings interpolationSettings = Object.Instantiate<InterpolationSettings>(NetworkPhysics.Instance.interpolationSettings);
				interpolationSettings.name = NetworkPhysics.Instance.interpolationSettings.name + " (Debug Clone)";
				interpolationSettings.hideFlags = HideFlags.HideAndDontSave;
				NetworkPhysics.Instance.interpolationSettings = interpolationSettings;
				Interpolation interpolation = NetworkPhysics.Instance.interpolation;
				if (interpolation != null)
				{
					interpolation.SetSettings(interpolationSettings);
				}
				NetworkPhysics.Instance.debugInterpolationSettingsCloned = true;
			}
			return NetworkPhysics.Instance.interpolationSettings;
		}

				public static void ResetInterpolationSettingsToDefaults()
		{
			if (NetworkPhysics.Instance == null)
			{
				return;
			}
			if (!NetworkPhysics.Instance.defaultInterpolationSettingsCaptured)
			{
				return;
			}
			InterpolationSettings orCreateDebugInterpolationSettings = NetworkPhysics.GetOrCreateDebugInterpolationSettings();
			if (orCreateDebugInterpolationSettings == null)
			{
				return;
			}
			orCreateDebugInterpolationSettings.Set(NetworkPhysics.Instance.defaultPosRate, NetworkPhysics.Instance.defaultRotRate, NetworkPhysics.Instance.defaultVelocityRate, NetworkPhysics.Instance.defaultAngVelocityRate, NetworkPhysics.Instance.defaultSnapToTargetStateDistance);
		}

				private void Log(string msg)
		{
			if (this.debugLogMethodCalls)
			{
				Debug.Log(msg);
			}
		}

				private void OnDrawGizmos()
		{
			for (int i = 0; i < NetworkAgent.All.Count; i++)
			{
				NetworkAgent networkAgent = NetworkAgent.All[i];
				if (this.drawAuthVolumes)
				{
					networkAgent.DrawAuthVolumeGizmos();
				}
				if (this.drawAuthChains)
				{
					networkAgent.DrawAuthChainGizmos();
				}
				if (this.drawAuthLabels)
				{
					networkAgent.DrawAuthLabels();
				}
			}
		}

				private void OnRenderObject()
		{
			if (!this.drawAuthVolumes)
			{
				return;
			}
			for (int i = 0; i < NetworkAgent.All.Count; i++)
			{
				NetworkAgent.All[i].DrawAuthVolumesRuntime();
			}
		}

				private void Awake()
		{
			if (NetworkPhysics.Instance != null)
			{
				Object.DestroyImmediate(this);
				return;
			}
			NetworkPhysics.Instance = this;
			if (this.interpolationSettings != null)
			{
				this.defaultPosRate = this.interpolationSettings.PosRate;
				this.defaultRotRate = this.interpolationSettings.RotRate;
				this.defaultVelocityRate = this.interpolationSettings.VelocityRate;
				this.defaultAngVelocityRate = this.interpolationSettings.AngVelocityRate;
				this.defaultSnapToTargetStateDistance = this.interpolationSettings.SnapToTargetStateDistance;
				this.defaultInterpolationSettingsCaptured = true;
			}
			this.physicsChannel = NetworkChannel.RegisterChannel(SendType.Unreliable);
			this.sendQueue = new List<AuthDec>();
			this.txPhysicsData = ScriptableObject.CreateInstance<PhysicsData>();
			this.lastRxPhysicsData = ScriptableObject.CreateInstance<PhysicsData>();
			this.timeOfLastRemoteDataByAgentId = new Dictionary<ushort, DateTime>();
			this.interpolation = new Interpolation(this.interpolationSettings);
			this.sceneIndex = SceneManager.GetActiveScene().buildIndex;
			SceneManager.sceneLoaded += this.OnSceneLoad;
			Network.Session.OnTick += this.OnNetworkSessionTick;
			Network.Session.OnUpdate += this.OnNetworkSessionUpdate;
			Network.Session.OnEnd += this.OnNetworkSessionEnd;
			Network.Session.OnMemberJoined += this.OnMemberJoined;
		}

				private void OnDestroy()
		{
			Network.Session.OnTick -= this.OnNetworkSessionTick;
			Network.Session.OnUpdate -= this.OnNetworkSessionUpdate;
			Network.Session.OnEnd -= this.OnNetworkSessionEnd;
			Network.Session.OnMemberJoined -= this.OnMemberJoined;
			if (this.debugInterpolationSettingsCloned && this.interpolationSettings != null)
			{
				Object.Destroy(this.interpolationSettings);
			}
		}

				private void Update()
		{
			if (Physics.simulationMode != SimulationMode.Script)
			{
				Physics.simulationMode = SimulationMode.Script;
			}
		}

				private void FixedUpdate()
		{
			if (NRegistry.RefreshRequired)
			{
				NRegistry.Refresh();
			}
			this.StepLocalPhysics();
		}

				private void StepLocalPhysics()
		{
			int buildIndex = SceneManager.GetActiveScene().buildIndex;
			if ((buildIndex == 0 || buildIndex == 63 || buildIndex == 64 || buildIndex == 65 || buildIndex == 66 || !Network.Session.Exists) && !this.steppingPaused && this.simulatePhysics && !Mathf.Approximately(Time.timeScale, 0f))
			{
				this.Log("\tSimulating...");
				Physics.defaultPhysicsScene.Simulate(Time.fixedDeltaTime);
				this.interpolation.InterpolateStates(Time.fixedDeltaTime);
			}
		}

				private void OnSceneLoad(Scene scene, LoadSceneMode mode)
		{
			this.sceneIndex = scene.buildIndex;
			this.ResetPhysics();
		}

				private void OnMemberJoined(NetworkUserId memberId)
		{
			if (Network.Service.UserIsLocal(memberId))
			{
				return;
			}
			Network.Session.SubscribeToReceive<PhysicsData>(memberId, this.physicsChannel, new OnDataReceived(this.OnPhysicsDataReceived), false);
		}

				private void OnNetworkSessionTick()
		{
			this.Log("--- NetworkPhysics Tick: " + (Network.Session.Exists ? Network.Session.TickNumber.ToString() : "N/A") + " ---");
			if (!NetworkPhysics.IsEnabled || this.steppingPaused || Mathf.Approximately(Time.timeScale, 0f))
			{
				return;
			}
			if (Network.Session.AnyClientsAreTimingOut)
			{
				this.StorePhysics();
				return;
			}
			this.ResumePhysics();
			this.Log("\tPrePhysics()");
			Action onPreStep = NetworkPhysics.OnPreStep;
			if (onPreStep != null)
			{
				onPreStep();
			}
			NObject.OnPrePhysics();
			if (this.simulatePhysics)
			{
				this.Log("\tSimulating...");
				Physics.defaultPhysicsScene.Simulate(Time.fixedDeltaTime);
			}
			this.Log("\tPostPhysics()");
			NObject.OnPostPhysics();
			NetworkAgent.OnPostPhysics();
			Action onPostStep = NetworkPhysics.OnPostStep;
			if (onPostStep != null)
			{
				onPostStep();
			}
			this.Log("\tUpdateInterpolationTargets()");
			this.UpdateInterpolationTargets();
			this.interpolation.InterpolateStates(Time.fixedDeltaTime);
			this.SendPackets();
		}

				private void OnNetworkSessionUpdate()
		{
		}

				private void OnNetworkSessionEnd()
		{
			NetworkPhysics.IsEnabled = false;
		}

				private void SendPackets()
		{
			this.Log("\tSendPhysicsData()");
			this.SendPhysicsData();
		}

				private void OnPhysicsDataReceived(NetworkUserId sender, TickNumber tickNumber, NetworkData networkData)
		{
			PhysicsData physicsData = (PhysicsData)networkData;
			if (!this.receivingEnabled)
			{
				return;
			}
			((ICopyable<PhysicsData>)this.lastRxPhysicsData).CopyFrom(physicsData);
			if ((int)physicsData.SceneIndex != this.sceneIndex)
			{
				return;
			}
			if (physicsData.PhysicsSessionId != NetworkPhysics.PhysicsSessionId)
			{
				return;
			}
			NetworkAgent byId = NetworkAgent.GetById(physicsData.AgentId);
			if (byId != null)
			{
				if (byId.Type == AgentType.Local)
				{
					return;
				}
				bool flag = false;
				if (tickNumber > byId.RemoteTickNumber)
				{
					DateTime now = DateTime.Now;
					if (this.timeOfLastRemoteDataByAgentId.ContainsKey(byId.Id))
					{
						if ((now - this.timeOfLastRemoteDataByAgentId[byId.Id]).TotalMilliseconds > 500.0)
						{
							flag = true;
						}
						this.timeOfLastRemoteDataByAgentId[byId.Id] = now;
					}
					else
					{
						this.timeOfLastRemoteDataByAgentId.Add(byId.Id, now);
					}
				}
				byId.ReceiveRemoteAuthDecs(physicsData.AuthDecs, tickNumber, physicsData.AuthChainIndex, physicsData.DeclaredAuthChainIndices, flag);
			}
		}

				private void UpdateInterpolationTargets()
		{
			this.interpolation.ClearTargetStates();
			foreach (NetworkAgent networkAgent in NetworkAgent.GetRemoteAgents())
			{
				for (int i = 0; i < networkAgent.Authorities.Count; i++)
				{
					NObject nobject = networkAgent.Authorities[i];
					NObjectState state = networkAgent.AuthDecs.GetState(nobject);
					if (state != null)
					{
						if (this.interpolationEnabled && !state.Snap)
						{
							this.interpolation.PushTargetState(nobject, state);
						}
						else
						{
							nobject.PushState(state);
						}
					}
				}
			}
			if (!this.interpolationEnabled)
			{
				NObject.ApplyPushedStates();
			}
		}

				private void SendPhysicsData()
		{
			if (!this.sendingEnabled)
			{
				return;
			}
			foreach (NetworkAgent networkAgent in NetworkAgent.GetLocalAgents())
			{
				if (networkAgent.AuthDecs.Count == 0)
				{
					this.txPhysicsData.SceneIndex = (byte)this.sceneIndex;
					this.txPhysicsData.PhysicsSessionId = NetworkPhysics.PhysicsSessionId;
					this.txPhysicsData.AgentId = networkAgent.Id;
					this.txPhysicsData.AuthChainIndex = 0;
					this.txPhysicsData.DeclaredAuthChainIndices.Clear();
					this.txPhysicsData.AuthDecs.Clear();
					Network.Session.SendToAll(this.physicsChannel, this.txPhysicsData);
				}
				else
				{
					this.authChainIndicesToSend.Clear();
					for (int i = 0; i < networkAgent.AuthDecs.Count; i++)
					{
						byte b = (byte)networkAgent.AuthDecs[i].AuthChainIndex;
						if (!this.authChainIndicesToSend.Contains(b))
						{
							this.authChainIndicesToSend.Add(b);
						}
					}
					this.sendQueue.Clear();
					for (int j = 0; j < networkAgent.AuthDecs.Count; j++)
					{
						this.sendQueue.Add(networkAgent.AuthDecs[j]);
					}
					while (this.sendQueue.Count > 0)
					{
						int authChainIndex = this.sendQueue[0].AuthChainIndex;
						this.txPhysicsData.SceneIndex = (byte)this.sceneIndex;
						this.txPhysicsData.PhysicsSessionId = NetworkPhysics.PhysicsSessionId;
						this.txPhysicsData.AgentId = networkAgent.Id;
						this.txPhysicsData.AuthChainIndex = (byte)authChainIndex;
						this.txPhysicsData.AuthDecs.Clear();
						this.txPhysicsData.DeclaredAuthChainIndices.Clear();
						this.txPhysicsData.DeclaredAuthChainIndices.AddRange(this.authChainIndicesToSend);
						int k = 0;
						while (k < this.sendQueue.Count)
						{
							if (this.sendQueue[k].AuthChainIndex == authChainIndex)
							{
								this.txPhysicsData.AuthDecs.AddCloneOf(this.sendQueue[k]);
								int num = this.sendQueue.Count - 1;
								this.sendQueue[k] = this.sendQueue[num];
								this.sendQueue.RemoveAt(num);
							}
							else
							{
								k++;
							}
						}
						Network.Session.SendToAll(this.physicsChannel, this.txPhysicsData);
					}
				}
			}
		}

				private void ResetPhysics()
		{
			this.timeOfLastRemoteDataByAgentId.Clear();
			this.interpolation.ClearTargetStates();
			Action onReset = NetworkPhysics.OnReset;
			if (onReset == null)
			{
				return;
			}
			onReset();
		}

				private void StorePhysics()
		{
			if (this.isPhysicsPaused)
			{
				return;
			}
			this.isPhysicsPaused = true;
			NObject.OnPause();
		}

				private void ResumePhysics()
		{
			if (!this.isPhysicsPaused)
			{
				return;
			}
			this.isPhysicsPaused = false;
			NObject.OnResume();
		}

				public static void SetPhysicsSessionId(PhysicsSessionId id)
		{
			NetworkPhysics.Instance.physicsSessionId = id;
			NetworkPhysics.Instance.ResetPhysics();
		}

				public NetworkPhysics()
		{
		}

						static NetworkPhysics()
		{
		}

				public const float TIME_DIFF_SNAP_THRESHOLD = 500f;

				[SerializeField]
		private bool networkPhysicsEnabled;

				[SerializeField]
		private bool steppingPaused;

				[SerializeField]
		private bool simulatePhysics = true;

				[SerializeField]
		private PhysicsSessionId physicsSessionId;

				[Header("Data Sending")]
		[SerializeField]
		private bool sendingEnabled = true;

				[SerializeField]
		private List<AuthDec> sendQueue;

				[SerializeField]
		private PhysicsData txPhysicsData;

				private readonly List<byte> authChainIndicesToSend = new List<byte>();

				[Header("Data Receiving")]
		[SerializeField]
		private bool receivingEnabled = true;

				[SerializeField]
		private PhysicsData lastRxPhysicsData;

				[Header("Interpolation")]
		[SerializeField]
		private bool interpolationEnabled = true;

				[SerializeField]
		private InterpolationSettings interpolationSettings;

				[SerializeField]
		private Interpolation interpolation;

				[Header("Debug")]
		[SerializeField]
		private bool debugAutoUpdate;

				[SerializeField]
		private bool drawAuthChains;

				[SerializeField]
		private bool drawAuthVolumes;

				[SerializeField]
		private bool drawAuthLabels;

				[SerializeField]
		private bool debugLogMethodCalls;

				private bool debugAutoUpdatePrev;

				private int sceneIndex;

				private bool localDebugMode;

				private bool debugInterpolationSettingsCloned;

				private bool defaultInterpolationSettingsCaptured;

				private float defaultPosRate;

				private float defaultRotRate;

				private float defaultVelocityRate;

				private float defaultAngVelocityRate;

				private float defaultSnapToTargetStateDistance;

				private NetworkChannel physicsChannel;

				[CompilerGenerated]
		private static Action OnReset;

				[CompilerGenerated]
		private static Action OnPreStep;

				[CompilerGenerated]
		private static Action OnPostStep;

				private bool isPhysicsPaused;

				private Dictionary<ushort, DateTime> timeOfLastRemoteDataByAgentId;

				[CompilerGenerated]
		private static NetworkPhysics<Instance> k__BackingField;

				[CompilerGenerated]
		private static bool <ApplyJointStates>k__BackingField;

				[CompilerGenerated]
		private static bool <ApplyVelocity>k__BackingField;

				[CompilerGenerated]
		private static bool <ApplyAngularVelocity>k__BackingField;

				[CompilerGenerated]
		private static bool <IgnoreSnapToTargetStateDistance>k__BackingField;
	}
}