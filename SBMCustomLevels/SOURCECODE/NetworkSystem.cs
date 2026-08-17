using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Catobyte.Networking;
using Catobyte.Networking.Physics;
using Catobyte.Networking.Physics.Authority;
using SBM.Shared.Networking.Data;
using SBM.Shared.Save;
using SBM.Shared.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBM.Shared.Networking
{
		public class NetworkSystem : MonoBehaviour
	{
								public static event Action OnSessionStarted
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkSystem.OnSessionStarted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionStarted, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkSystem.OnSessionStarted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionStarted, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnSessionInviteAccepted
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkSystem.OnSessionInviteAccepted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionInviteAccepted, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkSystem.OnSessionInviteAccepted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionInviteAccepted, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnSessionJoined
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkSystem.OnSessionJoined;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionJoined, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkSystem.OnSessionJoined;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionJoined, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnSessionHosted
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkSystem.OnSessionHosted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionHosted, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkSystem.OnSessionHosted;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionHosted, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action OnSessionEnded
		{
			[CompilerGenerated]
			add
			{
				Action action = NetworkSystem.OnSessionEnded;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionEnded, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = NetworkSystem.OnSessionEnded;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref NetworkSystem.OnSessionEnded, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo> OnUserConnectionReady
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserConnectionReady;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserConnectionReady, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserConnectionReady;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserConnectionReady, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo> OnUserTimeoutStart
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserTimeoutStart;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserTimeoutStart, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserTimeoutStart;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserTimeoutStart, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo> OnUserTimeoutEnd
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserTimeoutEnd;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserTimeoutEnd, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnUserTimeoutEnd;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnUserTimeoutEnd, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo> OnHostChanged
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnHostChanged;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnHostChanged, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo> action = NetworkSystem.OnHostChanged;
				Action<NetworkUserId, NetworkUserInfo> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo> action3 = (Action<NetworkUserId, NetworkUserInfo>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo>>(ref NetworkSystem.OnHostChanged, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo, DisconnectReason> OnUserDisconnected
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action = NetworkSystem.OnUserDisconnected;
				Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action3 = (Action<NetworkUserId, NetworkUserInfo, DisconnectReason>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, DisconnectReason>>(ref NetworkSystem.OnUserDisconnected, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action = NetworkSystem.OnUserDisconnected;
				Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, DisconnectReason> action3 = (Action<NetworkUserId, NetworkUserInfo, DisconnectReason>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, DisconnectReason>>(ref NetworkSystem.OnUserDisconnected, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo, Badge> OnUserBadgeUnlocked
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo, Badge> action = NetworkSystem.OnUserBadgeUnlocked;
				Action<NetworkUserId, NetworkUserInfo, Badge> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, Badge> action3 = (Action<NetworkUserId, NetworkUserInfo, Badge>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, Badge>>(ref NetworkSystem.OnUserBadgeUnlocked, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo, Badge> action = NetworkSystem.OnUserBadgeUnlocked;
				Action<NetworkUserId, NetworkUserInfo, Badge> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, Badge> action3 = (Action<NetworkUserId, NetworkUserInfo, Badge>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, Badge>>(ref NetworkSystem.OnUserBadgeUnlocked, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> OnUserUIChanged
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action = NetworkSystem.OnUserUIChanged;
				Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action3 = (Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex>>(ref NetworkSystem.OnUserUIChanged, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action = NetworkSystem.OnUserUIChanged;
				Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> action3 = (Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex>>(ref NetworkSystem.OnUserUIChanged, action3, action2);
				}
				while (action != action2);
			}
		}

								public static event Action<NetworkUserId, NetworkUserInfo, SceneState> OnRemoteUserSceneStateConflict
		{
			[CompilerGenerated]
			add
			{
				Action<NetworkUserId, NetworkUserInfo, SceneState> action = NetworkSystem.OnRemoteUserSceneStateConflict;
				Action<NetworkUserId, NetworkUserInfo, SceneState> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, SceneState> action3 = (Action<NetworkUserId, NetworkUserInfo, SceneState>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, SceneState>>(ref NetworkSystem.OnRemoteUserSceneStateConflict, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<NetworkUserId, NetworkUserInfo, SceneState> action = NetworkSystem.OnRemoteUserSceneStateConflict;
				Action<NetworkUserId, NetworkUserInfo, SceneState> action2;
				do
				{
					action2 = action;
					Action<NetworkUserId, NetworkUserInfo, SceneState> action3 = (Action<NetworkUserId, NetworkUserInfo, SceneState>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<NetworkUserId, NetworkUserInfo, SceneState>>(ref NetworkSystem.OnRemoteUserSceneStateConflict, action3, action2);
				}
				while (action != action2);
			}
		}

						public static bool ServiceIsConnected
		{
			get
			{
				return Network.Service.IsConnected;
			}
		}

						public static string ServiceName
		{
			get
			{
				return Network.Service.ServiceName;
			}
		}

						public static string LocalUsername
		{
			get
			{
				return Network.Service.Username;
			}
		}

						public static string RemoteCoopPartnerUsername
		{
			get
			{
				return Network.Service.GetUsernameById(NetworkSystem.GetRemoteUser(0).Id);
			}
		}

						public static NetworkUserId ServiceUserId
		{
			get
			{
				return Network.Service.UserId;
			}
		}

						public static bool ServiceOverlayIsActive
		{
			get
			{
				return Network.Service.OverlayIsActive;
			}
		}

						public static bool IsInSession
		{
			get
			{
				return Network.Session.Exists && Network.Session.Initialized;
			}
		}

						public static bool IsReadyForCoop
		{
			get
			{
				NetworkUser remoteUser = NetworkSystem.GetRemoteUser(0);
				return remoteUser != null && remoteUser.IsReady;
			}
		}

						public static bool IsHost
		{
			get
			{
				return Network.Session.IsHost;
			}
		}

								public static bool PhysicsSteppingPaused
		{
			get
			{
				return NetworkPhysics.SteppingPaused;
			}
			set
			{
				NetworkPhysics.SteppingPaused = value;
			}
		}

								public static bool NetworkPhysicsLocalDebugMode
		{
			get
			{
				return NetworkPhysics.LocalDebugMode;
			}
			set
			{
				NetworkPhysics.LocalDebugMode = value;
			}
		}

								public static bool TimeoutEnabled
		{
			get
			{
				return Network.Session.TimeoutEnabled;
			}
			set
			{
				Network.Session.TimeoutEnabled = value;
			}
		}

						public static int LocalUserCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < NetworkSystem.instance.users.Count; i++)
				{
					if (NetworkSystem.instance.users[i].IsLocal)
					{
						num++;
					}
				}
				return num;
			}
		}

						public static int RemoteUserCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < NetworkSystem.instance.users.Count; i++)
				{
					if (!NetworkSystem.instance.users[i].IsLocal)
					{
						num++;
					}
				}
				return num;
			}
		}

						public static bool AnyRemoteUsersAreTimingOut
		{
			get
			{
				for (int i = 0; i < NetworkSystem.RemoteUserCount; i++)
				{
					if (NetworkSystem.GetRemoteUser(i).IsTimingOut)
					{
						return true;
					}
				}
				return false;
			}
		}

						public static int UserCount
		{
			get
			{
				return NetworkSystem.instance.users.Count;
			}
		}

						public static bool IsConnectedToHost
		{
			get
			{
				return NetworkSystem.IsInSession && NetworkSystem.instance.hostUser != null && NetworkSystem.instance.hostUser.IsReady;
			}
		}

						public static NetworkSystem.HostInfo Host
		{
			get
			{
				return NetworkSystem.instance.hostInfo;
			}
		}

						public static bool SceneIsSyncedWithRemoteUsers
		{
			get
			{
				foreach (NetworkUser networkUser in NetworkSystem.instance.users)
				{
					if (!networkUser.IsLocal && !networkUser.SceneIsSynced)
					{
						return false;
					}
				}
				return true;
			}
		}

				[Conditional("DEVELOPMENT_BUILD")]
		private static void Log(string msg)
		{
			Debug.Log("NetworkSystem: " + msg);
		}

				private void Awake()
		{
			if (NetworkSystem.instance != null)
			{
				Object.DestroyImmediate(base.gameObject);
				return;
			}
			NetworkSystem.instance = this;
			this.users = new List<NetworkUser>();
			this.hostInfo = new NetworkSystem.HostInfo();
			NetworkSystem.Channels.Initialize();
			this.psPlayerRosterData.Init(new Action(this.SendPlayerRosterData));
			this.psSceneData.Init(new Action(this.SendSceneData));
			this.psBadgeData.Init(new Action(this.SendBadgeData));
			this.psPlayersData.Init(new Action(this.SendPlayersData));
			this.psUIData.Init(new Action(this.SendUIData));
			this.psGameData.Init(new Action(this.SendGameData));
			SceneSystem.OnSceneEvent += this.OnSceneEvent;
			Network.Session.OnStart += this.OnNetworkSession_Start;
			Network.Service.OnSessionInvitationAccepted += this.OnNetworkSession_InviteAccepted;
			Network.Session.OnEnd += this.OnNetworkSession_End;
			Network.Session.OnMemberJoined += this.OnNetworkSession_MemberJoined;
			Network.Session.OnMemberLeft += this.OnNetworkSession_MemberLeft;
			Network.Session.OnHostChanged += this.OnNetworkSession_HostChanged;
			Network.Session.OnTick += this.OnNetworkSession_Tick;
			Network.Session.OnUpdate += this.SendPackets;
			NetworkPhysics.OnPreStep += this.OnNetworkPhysics_PreStep;
			NetworkUser.OnConnectionReady += this.OnNetworkUser_ConnectionReady;
			NetworkUser.OnBadgeUnlocked += this.OnNetworkUser_BadgeUnlocked;
			NetworkUser.OnUIDataChanged += this.OnNetworkUser_UIChanged;
			PlayerRoster.OnRosterEvent += this.OnPlayerRosterEvent;
		}

				private void OnDestroy()
		{
			NetworkSystem.instance = null;
			SceneSystem.OnSceneEvent -= this.OnSceneEvent;
			Network.Session.OnStart -= this.OnNetworkSession_Start;
			Network.Service.OnSessionInvitationAccepted -= this.OnNetworkSession_InviteAccepted;
			Network.Session.OnEnd -= this.OnNetworkSession_End;
			Network.Session.OnMemberJoined -= this.OnNetworkSession_MemberJoined;
			Network.Session.OnMemberLeft -= this.OnNetworkSession_MemberLeft;
			Network.Session.OnHostChanged -= this.OnNetworkSession_HostChanged;
			Network.Session.OnTick -= this.OnNetworkSession_Tick;
			Network.Session.OnUpdate -= this.SendPackets;
			NetworkPhysics.OnPreStep -= this.OnNetworkPhysics_PreStep;
			NetworkUser.OnConnectionReady -= this.OnNetworkUser_ConnectionReady;
			NetworkUser.OnBadgeUnlocked -= this.OnNetworkUser_BadgeUnlocked;
			NetworkUser.OnUIDataChanged -= this.OnNetworkUser_UIChanged;
			PlayerRoster.OnRosterEvent -= this.OnPlayerRosterEvent;
		}

				private void OnSceneEvent(SceneEvent sceneEvent, Scene scene)
		{
			if (sceneEvent != SceneEvent.UnloadComplete)
			{
				if (sceneEvent == SceneEvent.LoadComplete)
				{
					GameManager gameManager = Object.FindObjectOfType<GameManager>();
					if (gameManager != null)
					{
						gameManager.OnRoundStateSet += this.OnGameManager_RoundStateSet;
					}
					if (Network.Session.Exists)
					{
						if (SceneSystem.InMenuScene)
						{
							NetworkPhysics.IsEnabled = false;
							return;
						}
						NetworkPhysics.IsEnabled = true;
						return;
					}
				}
			}
			else
			{
				NetworkPhysics.IsEnabled = false;
			}
		}

				private void OnPlayerRosterEvent(PlayerRoster.Event rEvent, PlayerRoster.Profile profile)
		{
			if (!NetworkSystem.IsInSession)
			{
				return;
			}
			if (NetworkSystem.IsHost)
			{
				if (rEvent == PlayerRoster.Event.Registered || rEvent == PlayerRoster.Event.Deregistered)
				{
					Network.Session.SetKeyValue("RPS", PlayerRoster.PlayerSlotsRemaining);
					return;
				}
			}
			else
			{
				if (this.localUser == null)
				{
					Debug.LogError(string.Format("=== OnPlayerRosterEvent {0} localUser is null", rEvent));
					return;
				}
				if (this.localUser.Data == null)
				{
					Debug.LogError(string.Format("=== OnPlayerRosterEvent {0} localUser.Data is null", rEvent));
					return;
				}
				if (this.localUser.Data.PlayerRoster == null)
				{
					Debug.LogError(string.Format("=== OnPlayerRosterEvent {0} localUser.Data.PlayerRoster is null", rEvent));
					return;
				}
				this.localUser.Data.PlayerRoster.RefreshChangeRequests();
			}
		}

				private void OnNetworkSession_InviteAccepted()
		{
			Action onSessionInviteAccepted = NetworkSystem.OnSessionInviteAccepted;
			if (onSessionInviteAccepted == null)
			{
				return;
			}
			onSessionInviteAccepted();
		}

				private void OnNetworkSession_Start()
		{
			Action onSessionStarted = NetworkSystem.OnSessionStarted;
			if (onSessionStarted != null)
			{
				onSessionStarted();
			}
			if (NetworkSystem.IsHost)
			{
				foreach (PlayerRoster.Profile profile in PlayerRoster.Profiles)
				{
					if (profile.IsLocal)
					{
						profile.Overwrite(profile.LocalPlayerIndex, profile.InputDeviceIndex, profile.Team, Network.Service.UserId, profile.IsLocal, Network.Service.Username);
					}
				}
				Action onSessionHosted = NetworkSystem.OnSessionHosted;
				if (onSessionHosted == null)
				{
					return;
				}
				onSessionHosted();
				return;
			}
			else
			{
				Action onSessionJoined = NetworkSystem.OnSessionJoined;
				if (onSessionJoined == null)
				{
					return;
				}
				onSessionJoined();
				return;
			}
		}

				private void OnNetworkSession_End()
		{
			this.users.Clear();
			Action onSessionEnded = NetworkSystem.OnSessionEnded;
			if (onSessionEnded == null)
			{
				return;
			}
			onSessionEnded();
		}

				private void OnNetworkSession_Tick()
		{
			if (GameManager.Exists)
			{
				bool flag = false;
				foreach (NetworkUser networkUser in this.users)
				{
					if (!Network.Service.UserIsLocal(networkUser.Id))
					{
						if (!networkUser.SceneIsSynced)
						{
							flag = true;
							break;
						}
						GameData game = networkUser.Data.Game;
						if (game != null)
						{
							ushort resetCount = GameManager.Instance.ResetCount;
							ushort resetCount2 = game.Manager.ResetCount;
							if (resetCount != resetCount2)
							{
								flag = true;
								break;
							}
						}
					}
				}
				NetworkPhysics.SteppingPaused = flag;
			}
			else
			{
				NetworkPhysics.SteppingPaused = false;
			}
			for (int i = 0; i < this.users.Count; i++)
			{
				NetworkUser networkUser2 = this.users[i];
				bool isTimingOut = networkUser2.IsTimingOut;
				networkUser2.IsTimingOut = Network.Session.MemberIsTimingOut(networkUser2.Id);
				if (!isTimingOut && networkUser2.IsTimingOut)
				{
					Action<NetworkUserId, NetworkUserInfo> onUserTimeoutStart = NetworkSystem.OnUserTimeoutStart;
					if (onUserTimeoutStart != null)
					{
						onUserTimeoutStart(networkUser2.Id, this.GetUserInfo(networkUser2));
					}
				}
				if (isTimingOut && !networkUser2.IsTimingOut)
				{
					Action<NetworkUserId, NetworkUserInfo> onUserTimeoutEnd = NetworkSystem.OnUserTimeoutEnd;
					if (onUserTimeoutEnd != null)
					{
						onUserTimeoutEnd(networkUser2.Id, this.GetUserInfo(networkUser2));
					}
				}
			}
		}

				private void OnNetworkSession_MemberJoined(NetworkUserId memberId)
		{
			bool flag = Network.Service.UserIsLocal(memberId);
			NetworkUser networkUser = new NetworkUser(memberId, flag);
			this.users.Add(networkUser);
			if (flag)
			{
				this.localUser = networkUser;
				this.localUser.Data = new NetworkUserData(false);
			}
			else
			{
				Network.Session.SubscribeToReceive<PlayerRosterData>(memberId, NetworkSystem.Channels.PlayerRoster, new OnDataReceived(this.OnReceivedPlayerRosterData), true);
				Network.Session.SubscribeToReceive<SceneData>(memberId, NetworkSystem.Channels.Scene, new OnDataReceived(this.OnReceivedSceneData), true);
				Network.Session.SubscribeToReceive<BadgeData>(memberId, NetworkSystem.Channels.Badge, new OnDataReceived(this.OnReceivedBadgeData), true);
				Network.Session.SubscribeToReceive<PlayersData>(memberId, NetworkSystem.Channels.Players, new OnDataReceived(this.OnReceivedPlayersData), false);
				Network.Session.SubscribeToReceive<GameData>(memberId, NetworkSystem.Channels.Game, new OnDataReceived(this.OnReceivedGameData), true);
				Network.Session.SubscribeToReceive<UIData>(memberId, NetworkSystem.Channels.UI, new OnDataReceived(this.OnReceivedUIData), true);
			}
			if (memberId == Network.Session.HostId)
			{
				this.hostUser = networkUser;
			}
			if (NetworkSystem.IsHost && NetworkSystem.Host.Id == memberId)
			{
				Action<NetworkUserId, NetworkUserInfo> onUserConnectionReady = NetworkSystem.OnUserConnectionReady;
				if (onUserConnectionReady == null)
				{
					return;
				}
				onUserConnectionReady(memberId, this.GetUserInfo(networkUser));
			}
		}

				private void OnNetworkSession_MemberLeft(NetworkUserId memberId, DisconnectReason reason)
		{
			Action<NetworkUserId, NetworkUserInfo, DisconnectReason> onUserDisconnected = NetworkSystem.OnUserDisconnected;
			if (onUserDisconnected != null)
			{
				onUserDisconnected(memberId, this.GetUserInfo(memberId), reason);
			}
			NetworkUser userById = this.GetUserById(memberId);
			this.users.Remove(userById);
			PlayerRoster.Deregister(memberId);
			if (userById == this.localUser)
			{
				this.localUser = null;
			}
			if (memberId == Network.Session.HostId)
			{
				this.hostUser = null;
			}
		}

				private void OnNetworkSession_HostChanged(NetworkUserId newHostId)
		{
			this.hostUser = NetworkSystem.GetUser(newHostId);
			Action<NetworkUserId, NetworkUserInfo> onHostChanged = NetworkSystem.OnHostChanged;
			if (onHostChanged == null)
			{
				return;
			}
			onHostChanged(newHostId, this.GetUserInfo(this.hostUser));
		}

				private void SendPackets()
		{
			if (this.localUser == null)
			{
				return;
			}
			this.psPlayerRosterData.Update(1);
			this.psSceneData.Update(1);
			this.psBadgeData.Update(1);
			this.psPlayersData.Update(1);
			this.psGameData.Update(1);
			if (NetworkSystem.IsHost && SceneSystem.InMenuScene)
			{
				this.psUIData.Update(1);
			}
		}

				private void OnNetworkPhysics_PreStep()
		{
			for (int i = 0; i < Player.Count; i++)
			{
				Player byIndex = Player.GetByIndex(i);
				NetworkAgent networkAgent = (byIndex.Alive ? byIndex.Agent : byIndex.Ragdoll.Agent);
				if (networkAgent == null)
				{
					return;
				}
				AgentType type = networkAgent.Type;
				if (type != AgentType.Local)
				{
					if (type == AgentType.Remote)
					{
						if (byIndex.PhysicsInputPrev.ExecutionTickNumber == byIndex.PhysicsInput.ExecutionTickNumber)
						{
							byIndex.PhysicsInput = byIndex.PhysicsInputPrev;
						}
					}
				}
				else
				{
					byIndex.PhysicsInput.ExecutionTickNumber = (Network.Session.Exists ? Network.Session.TickNumber : 0U);
					byIndex.RecordPrePhysicsInput();
				}
				byIndex.PrePhysics();
			}
		}

				private void OnNetworkUser_ConnectionReady(NetworkUser user)
		{
			Network.Service.GetUsernameById(user.Id);
			user.Id == NetworkSystem.Host.Id;
			if (NetworkSystem.IsHost && PlayerRoster.IsFull)
			{
				NetworkSystem.KickUser(user.Id, "PlayerRoster.IsFull");
				return;
			}
			Action<NetworkUserId, NetworkUserInfo> onUserConnectionReady = NetworkSystem.OnUserConnectionReady;
			if (onUserConnectionReady == null)
			{
				return;
			}
			onUserConnectionReady(user.Id, this.GetUserInfo(user));
		}

				private void OnNetworkUser_BadgeUnlocked(NetworkUser user, Badge badge)
		{
			Action<NetworkUserId, NetworkUserInfo, Badge> onUserBadgeUnlocked = NetworkSystem.OnUserBadgeUnlocked;
			if (onUserBadgeUnlocked == null)
			{
				return;
			}
			onUserBadgeUnlocked(user.Id, this.GetUserInfo(user), badge);
		}

				private void OnNetworkUser_UIChanged(NetworkUser user, UIState state, UISelectionIndex selectionIndex)
		{
			Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> onUserUIChanged = NetworkSystem.OnUserUIChanged;
			if (onUserUIChanged == null)
			{
				return;
			}
			onUserUIChanged(user.Id, this.GetUserInfo(user), state, selectionIndex);
		}

				private void OnGameManager_RoundStateSet(RoundState roundState)
		{
			if (roundState == RoundState.Reset)
			{
				NetworkPhysics.SetPhysicsSessionId(GameManager.Instance.ResetCount);
			}
		}

				private void SendPlayerRosterData()
		{
			PlayerRosterData playerRoster = this.localUser.Data.PlayerRoster;
			playerRoster.WriteCurrentRoster();
			Network.Session.SendToAll(NetworkSystem.Channels.PlayerRoster, playerRoster);
		}

				private void SendSceneData()
		{
			SceneData scene = this.localUser.Data.Scene;
			scene.State = SceneSystem.GetState();
			Network.Session.SendToAll(NetworkSystem.Channels.Scene, scene);
		}

				private void SendBadgeData()
		{
			BadgeData badge = this.localUser.Data.Badge;
			badge.SetBadges(SaveSystem.Story.Badges);
			Network.Session.SendToAll(NetworkSystem.Channels.Badge, badge);
		}

				private void SendPlayersData()
		{
			PlayersData players = this.localUser.Data.Players;
			players.Clear();
			players.SceneIndex = (byte)SceneSystem.CurrentScene.buildIndex;
			players.PhysicsSessionId = NetworkPhysics.PhysicsSessionId;
			for (int i = 0; i < Player.Count; i++)
			{
				Player byIndex = Player.GetByIndex(i);
				if (!(byIndex == null) && PlayerRoster.GetProfile(byIndex.Number) != null && !byIndex.IsNetworkControlled)
				{
					players.Write(byIndex);
					for (int j = 0; j < Player.Count; j++)
					{
						Player byIndex2 = Player.GetByIndex(j);
						if (!(byIndex2 == byIndex) && byIndex.Agent.HasAuthorityOfAgent(byIndex2.Agent))
						{
							players.Write(byIndex2);
						}
					}
				}
			}
			Network.Session.SendToAll(NetworkSystem.Channels.Players, players);
		}

				private void SendGameData()
		{
			GameData game = this.localUser.Data.Game;
			game.Mode = GameMode.Current;
			game.Manager.Clear();
			if (GameManager.Exists)
			{
				GameManager gameManager = GameManager.Instance;
				GameData.GameManagerData manager = this.localUser.Data.Game.Manager;
				manager.SceneIndex = (byte)SceneSystem.CurrentScene.buildIndex;
				manager.ResetCount = gameManager.ResetCount;
				manager.RoundState = gameManager.RoundState;
				manager.RoundTimer = gameManager.TimerSeconds;
				manager.WorldTime = gameManager.WorldTime;
				manager.PauseCount = gameManager.PauseCount;
				manager.ResumeCount = gameManager.ResumeCount;
				for (int i = 0; i < 4; i++)
				{
					manager.PlayerLives[i] = (byte)gameManager.GetPlayerLives(i + 1);
				}
				manager.ScoreRedTeam = (byte)gameManager.GetTeamScore(Team.Red);
				manager.ScoreBlueTeam = (byte)gameManager.GetTeamScore(Team.Blue);
				manager.WinningPlayerNumber = (byte)((gameManager.Winner != null) ? gameManager.Winner.Number : 0);
				GameData.GameManagerData gameManagerData = manager;
				Team? team = gameManager.WinningTeam;
				Team team2 = Team.Red;
				gameManagerData.RedTeamIsWinner = (team.GetValueOrDefault() == team2) & (team != null);
				GameData.GameManagerData gameManagerData2 = manager;
				team = gameManager.WinningTeam;
				team2 = Team.Blue;
				gameManagerData2.BlueTeamIsWinner = (team.GetValueOrDefault() == team2) & (team != null);
			}
			Network.Session.SendToAll(NetworkSystem.Channels.Game, game);
		}

				private void SendUIData()
		{
			UIData ui = this.localUser.Data.UI;
			ui.State = UIState.GetGlobalState();
			ui.SelectionIndex = (int)((byte)UISelectionIndex.GetGlobalIndex().value);
			Network.Session.SendToAll(NetworkSystem.Channels.UI, this.localUser.Data.UI);
		}

				private void OnReceivedPlayerRosterData(NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
		{
			NetworkSystem.<> c__DisplayClass116_0 CS$<> 8__locals1;
			CS$<> 8__locals1.senderId = senderId;
			NetworkUser userById = this.GetUserById(CS$<> 8__locals1.senderId);
			if (userById == null)
			{
				return;
			}
			PlayerRosterData playerRosterData = userById.ReceiveData<PlayerRosterData>(data);
			if (userById == this.hostUser)
			{
				ReadOnlyCollection<PlayerRosterData.ProfileData> rosterEntries = playerRosterData.RosterEntries;
				for (int i = PlayerRoster.Profiles.Count - 1; i >= 0; i--)
				{
					int playerNumber = PlayerRoster.Profiles[i].PlayerNumber;
					if (!playerRosterData.ContainsProfileFor(playerNumber))
					{
						PlayerRoster.Deregister(playerNumber);
					}
				}
				for (int j = 0; j < rosterEntries.Count; j++)
				{
					int playerNumber2 = rosterEntries[j].PlayerNumber;
					int localPlayerIndex = rosterEntries[j].LocalPlayerIndex;
					int inputDeviceIndex = rosterEntries[j].InputDeviceIndex;
					Team team = rosterEntries[j].Team;
					NetworkUserId networkUserId = rosterEntries[j].NetworkUserId;
					bool flag = Network.Service.UserIsLocal(networkUserId);
					string usernameById = Network.Service.GetUsernameById(networkUserId);
					PlayerRoster.Profile profile = PlayerRoster.GetProfile(playerNumber2);
					if (profile == null)
					{
						profile = new PlayerRoster.Profile(playerNumber2, localPlayerIndex, inputDeviceIndex, team, networkUserId, flag, usernameById);
						PlayerRoster.Register(profile);
					}
					else
					{
						profile.Overwrite(localPlayerIndex, inputDeviceIndex, team, networkUserId, flag, usernameById);
					}
				}
			}
			if (NetworkSystem.IsHost)
			{
				ReadOnlyCollection<PlayerRosterData.ChangeRequest> changeRequests = playerRosterData.ChangeRequests;
				for (int k = 0; k < changeRequests.Count; k++)
				{
					NetworkSystem.< OnReceivedPlayerRosterData > g__ProcessChangeRequest | 116_0(changeRequests[k], ref CS$<> 8__locals1);
				}
			}
		}

				private void OnReceivedSceneData(NetworkUserId id, TickNumber tickNumber, NetworkData data)
		{
			NetworkUser userById = this.GetUserById(id);
			if (userById != null)
			{
				SceneData sceneData = userById.ReceiveData<SceneData>(data);
				SceneState state = SceneSystem.GetState();
				SceneState state2 = sceneData.State;
				if (state != state2)
				{
					Action<NetworkUserId, NetworkUserInfo, SceneState> onRemoteUserSceneStateConflict = NetworkSystem.OnRemoteUserSceneStateConflict;
					if (onRemoteUserSceneStateConflict == null)
					{
						return;
					}
					onRemoteUserSceneStateConflict(id, this.GetUserInfo(id), sceneData.State);
				}
			}
		}

				private void OnReceivedPlayersData(NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
		{
			NetworkUser userById = this.GetUserById(senderId);
			if (userById == null)
			{
				return;
			}
			PlayersData playersData = userById.ReceiveData<PlayersData>(data);
			int localPlayerCount = userById.LocalPlayerCount;
			if (localPlayerCount <= 0)
			{
				return;
			}
			if ((int)playersData.SceneIndex != SceneSystem.CurrentScene.buildIndex)
			{
				return;
			}
			if (playersData.PhysicsSessionId != NetworkPhysics.PhysicsSessionId)
			{
				return;
			}
			for (int i = 0; i < playersData.Count; i++)
			{
				PlayerData playerData = playersData[i];
				Player byNumber = Player.GetByNumber((int)playerData.Number);
				if (!(byNumber == null))
				{
					bool flag = false;
					for (int j = 0; j < localPlayerCount; j++)
					{
						int playerNumber = PlayerRoster.GetPlayerNumber(senderId, j);
						if (playerNumber >= 1 && playerNumber <= 4 && PlayerRoster.PlayerIsRegistered(playerNumber))
						{
							Player byNumber2 = Player.GetByNumber(playerNumber);
							if (byNumber == byNumber2 || byNumber2.Agent.HasAuthorityOfAgent(byNumber.Agent))
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						if (byNumber.Alive && byNumber.DeathCount < (int)playerData.DeathCount)
						{
							byNumber.KillViaNetwork(playerData.LastDeathType);
						}
						if (!byNumber.Alive && playerData.Alive && byNumber.DeathCount == (int)playerData.DeathCount)
						{
							byNumber.SpawnPoint = playerData.SpawnPoint;
							byNumber.Respawn();
						}
						if (byNumber.DeathCount == (int)playerData.DeathCount)
						{
							if (byNumber.IsNetworkControlled)
							{
								byNumber.ApplyPhysicsInput(playerData.LastPhysicsInput);
							}
							if (playerData.LegIsBrokenL)
							{
								byNumber.Legs.BreakLeftLeg();
							}
							if (playerData.LegIsBrokenR)
							{
								byNumber.Legs.BreakRightLeg();
							}
							if (byNumber.IsNetworkControlled)
							{
								byNumber.Wetness = (float)playerData.Wetness / 255f;
								byNumber.CurrentBurnFactor = (float)playerData.Burntness / 255f;
							}
						}
					}
				}
			}
		}

				private void OnReceivedGameData(NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
		{
			NetworkUser userById = this.GetUserById(senderId);
			if (userById == null)
			{
				return;
			}
			GameData gameData = userById.ReceiveData<GameData>(data);
			bool flag = senderId == NetworkSystem.Host.Id;
			if (flag)
			{
				GameMode.Current = gameData.Mode;
			}
			if (GameManager.Exists)
			{
				GameManager.Instance.ProcessNetworkGameManagerData(gameData.Manager, flag);
			}
		}

				private void OnReceivedBadgeData(NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
		{
			NetworkUser userById = this.GetUserById(senderId);
			if (userById == null)
			{
				return;
			}
			userById.ReceiveData<BadgeData>(data);
		}

				private void OnReceivedUIData(NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
		{
			NetworkUser userById = this.GetUserById(senderId);
			if (userById == null)
			{
				return;
			}
			userById.ReceiveData<UIData>(data);
		}

				private NetworkUser GetUserById(NetworkUserId id)
		{
			for (int i = 0; i < this.users.Count; i++)
			{
				NetworkUser networkUser = this.users[i];
				if (networkUser.Id == id)
				{
					return networkUser;
				}
			}
			return null;
		}

				private NetworkUserInfo GetUserInfo(NetworkUser user)
		{
			return new NetworkUserInfo(this.users.IndexOf(user), user.IsLocal, this.hostInfo.Id == user.Id);
		}

				private NetworkUserInfo GetUserInfo(NetworkUserId id)
		{
			NetworkUser userById = this.GetUserById(id);
			return this.GetUserInfo(userById);
		}

				private static NetworkUser GetUser(NetworkUserId userId)
		{
			for (int i = 0; i < NetworkSystem.instance.users.Count; i++)
			{
				NetworkUser networkUser = NetworkSystem.instance.users[i];
				if (networkUser.Id == userId)
				{
					return networkUser;
				}
			}
			return null;
		}

				private static NetworkUser GetRemoteUser(int index)
		{
			int num = 0;
			for (int i = 0; i < NetworkSystem.instance.users.Count; i++)
			{
				NetworkUser networkUser = NetworkSystem.instance.users[i];
				if (!networkUser.IsLocal)
				{
					if (index == num)
					{
						return networkUser;
					}
					num++;
				}
			}
			return null;
		}

						public static NetworkUserId LocalUserId
		{
			get
			{
				return Network.Service.UserId;
			}
		}

				public static NetworkUserId GetRemoteUserId(int index)
		{
			return NetworkSystem.GetRemoteUser(index).Id;
		}

				public static int GetRemoteUserIndex(NetworkUserId remoteUserId)
		{
			for (int i = 0; i < NetworkSystem.RemoteUserCount; i++)
			{
				if (NetworkSystem.GetRemoteUser(i).Id == remoteUserId)
				{
					return i;
				}
			}
			return -1;
		}

				public static string GetUsername(NetworkUserId userId)
		{
			return Network.Service.GetUsernameById(userId);
		}

				public static bool UserIsLocal(NetworkUserId userId)
		{
			return Network.Service.UserIsLocal(userId);
		}

				public static bool UserIsRemote(NetworkUserId userId)
		{
			return !NetworkSystem.UserIsLocal(userId);
		}

				public static bool UserIsTimingOut(NetworkUserId userId)
		{
			return Network.Session.MemberIsTimingOut(userId);
		}

				public static float GetUserTimeoutRemaining(NetworkUserId userId)
		{
			return Network.Session.GetMemberTimeoutRemaining(userId);
		}

				public static float GetUserPing(NetworkUserId userId)
		{
			return Network.Session.GetMemberPing(userId);
		}

				public static Badges GetUserBadges(NetworkUserId userId)
		{
			return NetworkSystem.GetUser(userId).Badges;
		}

				public static bool GetUserUIState(NetworkUserId userId, out UIState uiState, out UISelectionIndex selectionIndex)
		{
			NetworkUser user = NetworkSystem.GetUser(userId);
			if (user != null)
			{
				user.GetUIState(out uiState, out selectionIndex);
				return true;
			}
			Debug.LogError(string.Format("Error: NetworkSystem::GetUserUIState: GetUser() returns null, userId:{0}", userId));
			uiState = null;
			selectionIndex = -1;
			return false;
		}

				[return: TupleElementNames(new string[] { "isSuccess", "showError" })]
		public static async Task<ValueTuple<bool, bool>> HostSessionAsync(int maxPlayers, SessionAccess access, Network.NetworkType networkType)
		{
			return await Network.Session.HostAsync(maxPlayers, access, networkType);
		}

				public static async void HostSession(int maxPlayers, SessionAccess access, Network.NetworkType networkType)
		{
			await NetworkSystem.HostSessionAsync(maxPlayers, access, networkType);
		}

				public static async Task<bool> EndSessionAsync()
		{
			return await Network.Session.EndAsync();
		}

				public static async void EndSession()
		{
			await NetworkSystem.EndSessionAsync();
		}

				public static async Task<NetworkSessionId[]> FetchCompatibleSessionsAsync(Region region)
		{
			int count = PlayerRoster.Profiles.Count;
			NetworkSessionId[] array = await Network.Service.FetchSessionList(region, true, new SessionQueryFilter[]
			{
				new SessionQueryFilter("RPS", Compare.GreaterThan, count - 1)
			});
			if (array != null && array.Length != 0)
			{
				array = array.Except(new NetworkSessionId[] { Network.Session.Id }).ToArray<NetworkSessionId>();
			}
			if (array != null)
			{
				int num = array.Length;
			}
			return array;
		}

				public static void SetSessionAccess(SessionAccess access)
		{
			Network.Session.SetAccessType(access);
		}

				public static void InviteViaServiceOverlay(Action<bool> onComplete)
		{
			Network.Session.InviteViaServiceOverlay(onComplete);
		}

				public static void InviteDevUser(int devUserIndex)
		{
			CatobyteDevUser? catobyteDevUser = null;
			switch (devUserIndex)
			{
				case 1:
					catobyteDevUser = new CatobyteDevUser?(CatobyteDevUser.Dev1);
					break;
				case 2:
					catobyteDevUser = new CatobyteDevUser?(CatobyteDevUser.Dev2);
					break;
				case 3:
					catobyteDevUser = new CatobyteDevUser?(CatobyteDevUser.Dev3);
					break;
				case 4:
					catobyteDevUser = new CatobyteDevUser?(CatobyteDevUser.Dev4);
					break;
			}
			if (catobyteDevUser != null)
			{
				Network.Session.Invite(DevUsers.GetDevUserId(catobyteDevUser.Value));
			}
		}

				public static void AcceptPreLaunchInvitation()
		{
			Network.Service.AcceptPreLaunchInvitation();
		}

				public static void KickUser(NetworkUserId userId, string reason)
		{
			if (!NetworkSystem.IsHost)
			{
				NetworkSystem.GetUsername(userId);
				return;
			}
			Network.Session.Kick(userId, reason);
		}

				public static void RequestPlayerRegistration(int playerNumber, int localPlayerIndex, int inputDeviceIndex)
		{
			if (NetworkSystem.IsHost)
			{
				return;
			}
			if (PlayerRoster.PlayerIsRegistered(playerNumber))
			{
				return;
			}
			NetworkSystem.instance.localUser.Data.PlayerRoster.RequestRegisterPlayer(playerNumber, localPlayerIndex, inputDeviceIndex, Network.Service.Username);
		}

				public static void RequestPlayerDeregistration(int playerNumber)
		{
			if (NetworkSystem.IsHost)
			{
				return;
			}
			if (!PlayerRoster.PlayerIsRegistered(playerNumber))
			{
				return;
			}
			NetworkSystem.instance.localUser.Data.PlayerRoster.RequestDeregisterPlayer(playerNumber);
		}

				public static void RequestPlayerTeamChange(int playerNumber, Team changeToTeam)
		{
			if (NetworkSystem.IsHost)
			{
				return;
			}
			PlayerRoster.Profile profile = PlayerRoster.GetProfile(playerNumber);
			if (profile != null && profile.Team == changeToTeam)
			{
				return;
			}
			NetworkSystem.instance.localUser.Data.PlayerRoster.RequestPlayerTeamChange(playerNumber, changeToTeam);
		}

				public static void CancelPlayerRosterChangeRequests()
		{
			if (NetworkSystem.IsHost)
			{
				return;
			}
			NetworkSystem.instance.localUser.Data.PlayerRoster.CancelAllChangeRequests();
		}

				public static void SetGameMode(GameModeType gameMode)
		{
			Network.GameMode gameMode2;
			if (gameMode != GameModeType.CoopStory)
			{
				if (gameMode - GameModeType.Deathmatch > 2)
				{
					< PrivateImplementationDetails >.ThrowSwitchExpressionException(gameMode);
				}
				else
				{
					gameMode2 = Network.GameMode.Party;
				}
			}
			else
			{
				gameMode2 = Network.GameMode.Coop;
			}
			Network.Service.GameMode = gameMode2;
		}

				public static void SetNetworkMode_LocalWireless()
		{
			Network.Service.CurrentNetworkMode = Network.NetworkType.LocalWireless;
		}

				public static void SetNetworkMode_Online()
		{
			Network.Service.CurrentNetworkMode = Network.NetworkType.Online;
		}

				public static void SetNetworkMode_Local()
		{
			Network.Service.CurrentNetworkMode = Network.NetworkType.Local;
		}

				public static bool Debug_InviteUser(string username)
		{
			List<NetworkUserId> list = new List<NetworkUserId>();
			Network.Service.GetFriends(list, true);
			foreach (NetworkUserId networkUserId in list)
			{
				if (Network.Service.GetUsernameById(networkUserId) == username)
				{
					Network.Session.Invite(networkUserId);
					return true;
				}
			}
			return false;
		}

				public static void Debug_InviteDevUser(int number)
		{
			NetworkUserId devUserId = DevUsers.GetDevUserId((CatobyteDevUser)number);
			if (devUserId == NetworkSystem.LocalUserId)
			{
				return;
			}
			Network.Session.Invite(devUserId);
		}

				public static void Debug_AcceptLastInvitation()
		{
			Network.Service.AcceptLastInvitation();
		}

				[ContextMenu("Add debug remote user")]
		public void Debug_AddDebugRemoteUser()
		{
			NetworkUser networkUser = new NetworkUser(0UL, false);
			NetworkUserId id = networkUser.Id;
			this.users.Add(networkUser);
			Network.Session.SubscribeToReceive<PlayerRosterData>(id, NetworkSystem.Channels.PlayerRoster, new OnDataReceived(this.OnReceivedPlayerRosterData), true);
			Network.Session.SubscribeToReceive<SceneData>(id, NetworkSystem.Channels.Scene, new OnDataReceived(this.OnReceivedSceneData), true);
			Network.Session.SubscribeToReceive<BadgeData>(id, NetworkSystem.Channels.Badge, new OnDataReceived(this.OnReceivedBadgeData), true);
			Network.Session.SubscribeToReceive<PlayersData>(id, NetworkSystem.Channels.Players, new OnDataReceived(this.OnReceivedPlayersData), false);
			Network.Session.SubscribeToReceive<GameData>(id, NetworkSystem.Channels.Game, new OnDataReceived(this.OnReceivedGameData), true);
			Network.Session.SubscribeToReceive<UIData>(id, NetworkSystem.Channels.UI, new OnDataReceived(this.OnReceivedUIData), true);
		}

				public NetworkSystem()
		{
		}

				[CompilerGenerated]
		internal static void <OnReceivedPlayerRosterData>g__ProcessChangeRequest|116_0(PlayerRosterData.ChangeRequest request, ref NetworkSystem.<>c__DisplayClass116_0 A_1)
		{
			int playerNumber = request.PlayerNumber;
		PlayerRoster.Profile profile = PlayerRoster.GetProfile(playerNumber);
			if (profile != null && A_1.senderId != profile.NetworkUserId)
			{
				return;
			}
			switch (request.ChangeType)
			{
			case PlayerRosterData.ChangeRequest.ChangeRequestType.Register:
				if (profile != null)
				{
					return;
				}
				PlayerRoster.RegisterRemotePlayer(playerNumber, request.LocalPlayerIndex, request.InputDeviceIndex, A_1.senderId, request.Username.ToString());
				return;
			case PlayerRosterData.ChangeRequest.ChangeRequestType.Deregister:
				if (profile == null)
				{
					return;
				}
				PlayerRoster.Deregister(playerNumber);
				return;
			case PlayerRosterData.ChangeRequest.ChangeRequestType.ChangeTeam:
			{
				if (profile == null)
				{
					return;
				}
				if (request.ChangeToTeam == profile.Team)
				{
					return;
				}
				bool flag = false;
				if (PlayerRoster.GetTeamSize(Team.Red) == 1 && PlayerRoster.GetTeamSize(Team.Blue) == 1)
				{
					for (int i = 0; i<PlayerRoster.Profiles.Count; i++)
					{
						PlayerRoster.Profile profile2 = PlayerRoster.Profiles[i];
		Team team = profile2.Team;
						if (team != Team.Red)
						{
							if (team == Team.Blue)
							{
								profile2.Team = Team.Red;
							}
}
						else
{
	profile2.Team = Team.Blue;
}
					}
					flag = true;
				}
				else if (PlayerRoster.GetTeamSize(profile.Team) - 1 > 0)
{
	flag = true;
}
if (flag)
{
	profile.Team = request.ChangeToTeam;
}
return;
			}
			default:
				return;
			}
		}

				[Header("Packet send intervals")]
[SerializeField]
private NetworkSystem.PacketSender psPlayerRosterData;

[SerializeField]
private NetworkSystem.PacketSender psSceneData;

[SerializeField]
private NetworkSystem.PacketSender psBadgeData;

[SerializeField]
private NetworkSystem.PacketSender psPlayersData;

[SerializeField]
private NetworkSystem.PacketSender psUIData;

[SerializeField]
private NetworkSystem.PacketSender psGameData;

[Header("Network users")]
[SerializeField]
private List<NetworkUser> users;

private NetworkUser localUser;

private NetworkUser hostUser;

private NetworkSystem.HostInfo hostInfo;

private static NetworkSystem instance;

[CompilerGenerated]
private static Action OnSessionStarted;

[CompilerGenerated]
private static Action OnSessionInviteAccepted;

[CompilerGenerated]
private static Action OnSessionJoined;

[CompilerGenerated]
private static Action OnSessionHosted;

[CompilerGenerated]
private static Action OnSessionEnded;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo> OnUserConnectionReady;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo> OnUserTimeoutStart;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo> OnUserTimeoutEnd;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo> OnHostChanged;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo, DisconnectReason> OnUserDisconnected;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo, Badge> OnUserBadgeUnlocked;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo, UIState, UISelectionIndex> OnUserUIChanged;

[CompilerGenerated]
private static Action<NetworkUserId, NetworkUserInfo, SceneState> OnRemoteUserSceneStateConflict;

private const string SessionKeyRemainingPlayerSlots = "RPS";

public class HostInfo
{
			public NetworkUserId Id
	{
		get
		{
			return Network.Session.HostId;
		}
	}

			public GameModeType GameMode
	{
		get
		{
			if (NetworkSystem.instance.hostUser != null)
			{
				return NetworkSystem.instance.hostUser.Data.Game.Mode;
			}
			return GameModeType.None;
		}
	}

		public HostInfo()
	{
	}
}

private static class Channels
{
		public static void Initialize()
	{
		NetworkSystem.Channels.PlayerRoster = NetworkChannel.RegisterChannel(SendType.Reliable);
		NetworkSystem.Channels.Scene = NetworkChannel.RegisterChannel(SendType.Reliable);
		NetworkSystem.Channels.Players = NetworkChannel.RegisterChannel(SendType.Unreliable);
		NetworkSystem.Channels.Badge = NetworkChannel.RegisterChannel(SendType.Reliable);
		NetworkSystem.Channels.Game = NetworkChannel.RegisterChannel(SendType.Reliable);
		NetworkSystem.Channels.UI = NetworkChannel.RegisterChannel(SendType.Reliable);
	}

		public static NetworkChannel PlayerRoster;

		public static NetworkChannel Scene;

		public static NetworkChannel Players;

		public static NetworkChannel Badge;

		public static NetworkChannel Game;

		public static NetworkChannel UI;
}

[Serializable]
private class PacketSender
{
				private event Action onSend
	{
		[CompilerGenerated]
		add
		{
			Action action = this.onSend;
			Action action2;
			do
			{
				action2 = action;
				Action action3 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange<Action>(ref this.onSend, action3, action2);
			}
			while (action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = this.onSend;
			Action action2;
			do
			{
				action2 = action;
				Action action3 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange<Action>(ref this.onSend, action3, action2);
			}
			while (action != action2);
		}
	}

		public void Init(Action onSend)
	{
		this.onSend = onSend;
		this.framesSinceSent = this.startOffset;
	}

		public void Update(int framesElapsed = 1)
	{
		this.framesSinceSent += framesElapsed;
		if (this.framesSinceSent >= this.sendInterval)
		{
			Action action = this.onSend;
			if (action != null)
			{
				action();
			}
			this.framesSinceSent = 0;
		}
	}

		public PacketSender()
	{
	}

		[SerializeField]
	private int sendInterval;

		[SerializeField]
	private int startOffset;

		private int framesSinceSent;

		[CompilerGenerated]
	private Action onSend;
}

[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <>c__DisplayClass116_0
{
						public NetworkUserId senderId;
		}

				[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <EndSession>d__142: IAsyncStateMachine
		{
		void IAsyncStateMachine.MoveNext()
			{
		int num = this.<> 1__state;
		try
		{
			TaskAwaiter<bool> awaiter;
			if (num != 0)
			{
				awaiter = NetworkSystem.EndSessionAsync().GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					this.<> 1__state = 0;
					this.<> u__1 = awaiter;
					this.<> t__builder.AwaitUnsafeOnCompleted < TaskAwaiter<bool>, NetworkSystem.< EndSession > d__142 > (ref awaiter, ref this);
					return;
				}
			}
			else
			{
				awaiter = this.<> u__1;
				this.<> u__1 = default(TaskAwaiter<bool>);
				this.<> 1__state = -1;
			}
			awaiter.GetResult();
		}
		catch (Exception ex)
		{
			this.<> 1__state = -2;
			this.<> t__builder.SetException(ex);
			return;
		}
		this.<> 1__state = -2;
		this.<> t__builder.SetResult();
	}

		[DebuggerHidden]
	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
		this.<> t__builder.SetStateMachine(stateMachine);
	}

		public int <>1__state;

						public AsyncVoidMethodBuilder<> t__builder;

private TaskAwaiter<bool> <> u__1;
		}

				[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <EndSessionAsync>d__141: IAsyncStateMachine
		{
		void IAsyncStateMachine.MoveNext()
			{
		int num = this.<> 1__state;
		bool result;
		try
		{
			TaskAwaiter<bool> awaiter;
			if (num != 0)
			{
				awaiter = Network.Session.EndAsync().GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					this.<> 1__state = 0;
					this.<> u__1 = awaiter;
					this.<> t__builder.AwaitUnsafeOnCompleted < TaskAwaiter<bool>, NetworkSystem.< EndSessionAsync > d__141 > (ref awaiter, ref this);
					return;
				}
			}
			else
			{
				awaiter = this.<> u__1;
				this.<> u__1 = default(TaskAwaiter<bool>);
				this.<> 1__state = -1;
			}
			result = awaiter.GetResult();
		}
		catch (Exception ex)
		{
			this.<> 1__state = -2;
			this.<> t__builder.SetException(ex);
			return;
		}
		this.<> 1__state = -2;
		this.<> t__builder.SetResult(result);
	}

		[DebuggerHidden]
	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
		this.<> t__builder.SetStateMachine(stateMachine);
	}

		public int <>1__state;

		public AsyncTaskMethodBuilder<bool> <>t__builder;

		private TaskAwaiter<bool> <>u__1;
}

[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <FetchCompatibleSessionsAsync>d__143: IAsyncStateMachine
		{
		void IAsyncStateMachine.MoveNext()
			{
		int num = this.<> 1__state;
		NetworkSessionId[] array2;
		try
		{
			TaskAwaiter<NetworkSessionId[]> awaiter;
			if (num != 0)
			{
				int count = PlayerRoster.Profiles.Count;
				awaiter = Network.Service.FetchSessionList(this.region, true, new SessionQueryFilter[]
				{
							new SessionQueryFilter("RPS", Compare.GreaterThan, count - 1)
				}).GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					this.<> 1__state = 0;
					this.<> u__1 = awaiter;
					this.<> t__builder.AwaitUnsafeOnCompleted < TaskAwaiter<NetworkSessionId[]>, NetworkSystem.< FetchCompatibleSessionsAsync > d__143 > (ref awaiter, ref this);
					return;
				}
			}
			else
			{
				awaiter = this.<> u__1;
				this.<> u__1 = default(TaskAwaiter<NetworkSessionId[]>);
				this.<> 1__state = -1;
			}
			NetworkSessionId[] array = awaiter.GetResult();
			if (array != null && array.Length != 0)
			{
				array = array.Except(new NetworkSessionId[] { Network.Session.Id }).ToArray<NetworkSessionId>();
			}
			if (array != null)
			{
				int num2 = array.Length;
			}
			array2 = array;
		}
		catch (Exception ex)
		{
			this.<> 1__state = -2;
			this.<> t__builder.SetException(ex);
			return;
		}
		this.<> 1__state = -2;
		this.<> t__builder.SetResult(array2);
	}

		[DebuggerHidden]
	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
		this.<> t__builder.SetStateMachine(stateMachine);
	}

		public int <>1__state;

		public AsyncTaskMethodBuilder<NetworkSessionId[]> <>t__builder;

						public Region region;

private TaskAwaiter<NetworkSessionId[]> <> u__1;
		}

				[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <HostSession>d__140: IAsyncStateMachine
		{
		void IAsyncStateMachine.MoveNext()
			{
		int num = this.<> 1__state;
		try
		{
			TaskAwaiter<ValueTuple<bool, bool>> awaiter;
			if (num != 0)
			{
				awaiter = NetworkSystem.HostSessionAsync(this.maxPlayers, this.access, this.networkType).GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					this.<> 1__state = 0;
					this.<> u__1 = awaiter;
					this.<> t__builder.AwaitUnsafeOnCompleted < TaskAwaiter<ValueTuple<bool, bool>>, NetworkSystem.< HostSession > d__140 > (ref awaiter, ref this);
					return;
				}
			}
			else
			{
				awaiter = this.<> u__1;
				this.<> u__1 = default(TaskAwaiter<ValueTuple<bool, bool>>);
				this.<> 1__state = -1;
			}
			awaiter.GetResult();
		}
		catch (Exception ex)
		{
			this.<> 1__state = -2;
			this.<> t__builder.SetException(ex);
			return;
		}
		this.<> 1__state = -2;
		this.<> t__builder.SetResult();
	}

		[DebuggerHidden]
	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
		this.<> t__builder.SetStateMachine(stateMachine);
	}

		public int <>1__state;

						public AsyncVoidMethodBuilder<> t__builder;

public int maxPlayers;

public SessionAccess access;

public Network.NetworkType networkType;

[TupleElementNames(new string[] { "isSuccess", "showError" })]
private TaskAwaiter<ValueTuple<bool, bool>> <> u__1;
		}

				[CompilerGenerated]
[StructLayout(LayoutKind.Auto)]
private struct <HostSessionAsync>d__139: IAsyncStateMachine
		{
		void IAsyncStateMachine.MoveNext()
			{
		int num = this.<> 1__state;
		ValueTuple<bool, bool> result;
		try
		{
			TaskAwaiter<ValueTuple<bool, bool>> awaiter;
			if (num != 0)
			{
				awaiter = Network.Session.HostAsync(this.maxPlayers, this.access, this.networkType).GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					this.<> 1__state = 0;
					this.<> u__1 = awaiter;
					this.<> t__builder.AwaitUnsafeOnCompleted < TaskAwaiter<ValueTuple<bool, bool>>, NetworkSystem.< HostSessionAsync > d__139 > (ref awaiter, ref this);
					return;
				}
			}
			else
			{
				awaiter = this.<> u__1;
				this.<> u__1 = default(TaskAwaiter<ValueTuple<bool, bool>>);
				this.<> 1__state = -1;
			}
			result = awaiter.GetResult();
		}
		catch (Exception ex)
		{
			this.<> 1__state = -2;
			this.<> t__builder.SetException(ex);
			return;
		}
		this.<> 1__state = -2;
		this.<> t__builder.SetResult(result);
	}

		[DebuggerHidden]
	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
		this.<> t__builder.SetStateMachine(stateMachine);
	}

		public int <>1__state;

		[TupleElementNames(new string[] { "isSuccess", "showError" })]
	public AsyncTaskMethodBuilder<ValueTuple<bool, bool>> <>t__builder;

						public int maxPlayers;

public SessionAccess access;

public Network.NetworkType networkType;

[TupleElementNames(new string[] { "isSuccess", "showError" })]
private TaskAwaiter<ValueTuple<bool, bool>> <> u__1;
		}
	}
}
