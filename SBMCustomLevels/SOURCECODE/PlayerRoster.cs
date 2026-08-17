using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Catobyte.Networking;
using SBM.Shared.Networking;
using UnityEngine;

namespace SBM.Shared
{
		public static class PlayerRoster
	{
								public static event Action<PlayerRoster.Event, PlayerRoster.Profile> OnRosterEvent
		{
			[CompilerGenerated]
			add
			{
				Action<PlayerRoster.Event, PlayerRoster.Profile> action = PlayerRoster.OnRosterEvent;
				Action<PlayerRoster.Event, PlayerRoster.Profile> action2;
				do
				{
					action2 = action;
					Action<PlayerRoster.Event, PlayerRoster.Profile> action3 = (Action<PlayerRoster.Event, PlayerRoster.Profile>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<PlayerRoster.Event, PlayerRoster.Profile>>(ref PlayerRoster.OnRosterEvent, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<PlayerRoster.Event, PlayerRoster.Profile> action = PlayerRoster.OnRosterEvent;
				Action<PlayerRoster.Event, PlayerRoster.Profile> action2;
				do
				{
					action2 = action;
					Action<PlayerRoster.Event, PlayerRoster.Profile> action3 = (Action<PlayerRoster.Event, PlayerRoster.Profile>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<PlayerRoster.Event, PlayerRoster.Profile>>(ref PlayerRoster.OnRosterEvent, action3, action2);
				}
				while (action != action2);
			}
		}

						public static ReadOnlyCollection<PlayerRoster.Profile> Profiles
		{
			get
			{
				return PlayerRoster.profiles.AsReadOnly();
			}
		}

						public static int LocalPlayerCount
		{
			get
			{
				return PlayerRoster.profiles.Count((PlayerRoster.Profile p) => p.IsLocal);
			}
		}

						public static int RemotePlayerCount
		{
			get
			{
				return PlayerRoster.profiles.Count((PlayerRoster.Profile p) => !p.IsLocal);
			}
		}

						public static int PlayerSlotsRemaining
		{
			get
			{
				return 4 - PlayerRoster.profiles.Count;
			}
		}

						public static bool IsFull
		{
			get
			{
				return PlayerRoster.PlayerSlotsRemaining == 0;
			}
		}

								public static bool TeamsRequireReassignment
		{
			[CompilerGenerated]
			get
			{
				return PlayerRoster.< TeamsRequireReassignment > k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				PlayerRoster.< TeamsRequireReassignment > k__BackingField = value;
			}
		}

				public static PlayerRoster.Profile GetProfile(int playerNumber)
		{
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.PlayerNumber == playerNumber)
				{
					return profile;
				}
			}
			return null;
		}

				public static PlayerRoster.Profile GetProfile(NetworkUserId userId, int localPlayerIndex)
		{
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.NetworkUserId == userId && profile.LocalPlayerIndex == localPlayerIndex)
				{
					return profile;
				}
			}
			return null;
		}

				public static PlayerRoster.Profile GetByInputDeviceIndex(int inputDeviceIndex, NetworkUserId networkUserId = default(NetworkUserId))
		{
			if (networkUserId == default(NetworkUserId) && NetworkSystem.IsInSession)
			{
				networkUserId = NetworkSystem.LocalUserId;
			}
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.InputDeviceIndex == inputDeviceIndex && (profile.IsLocal || profile.NetworkUserId == networkUserId))
				{
					return profile;
				}
			}
			return null;
		}

				public static PlayerRoster.Profile GetRemotePlayer(int offset)
		{
			PlayerRoster.tmpProfiles.Clear();
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.IsRemote)
				{
					PlayerRoster.tmpProfiles.Add(profile);
				}
			}
			PlayerRoster.tmpProfiles.Sort(PlayerRoster.ProfileComparer.Instance);
			if (offset >= 0 && offset < PlayerRoster.tmpProfiles.Count)
			{
				return PlayerRoster.tmpProfiles[offset];
			}
			return null;
		}

				public static int GetPlayerNumber(int playerIndex)
		{
			return PlayerRoster.profiles[playerIndex].PlayerNumber;
		}

				public static int GetPlayerNumber(NetworkUserId userId, int localPlayerIndex = 0)
		{
			int num = 0;
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.NetworkUserId == userId)
				{
					if (num == localPlayerIndex)
					{
						return profile.PlayerNumber;
					}
					num++;
				}
			}
			return -1;
		}

				public static bool PlayerIsRegistered(int playerNumber)
		{
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				if (PlayerRoster.profiles[i].PlayerNumber == playerNumber)
				{
					return true;
				}
			}
			return false;
		}

				public static int GetNextAvailablePlayerNumber()
		{
			for (int i = 1; i <= 4; i++)
			{
				if (!PlayerRoster.PlayerIsRegistered(i))
				{
					return i;
				}
			}
			return -1;
		}

				public static void Register(PlayerRoster.Profile profile)
		{
			if (PlayerRoster.PlayerIsRegistered(profile.PlayerNumber))
			{
				throw new Exception("PlayerRoster: Attempted to register new player with number = " + profile.PlayerNumber.ToString() + ", but number is already registered!");
			}
			PlayerRoster.profiles.Add(profile);
			PlayerRoster.TeamsRequireReassignment = true;
			Debug.Log(string.Concat(new string[]
			{
				"PlayerRoster: registered player ",
				profile.PlayerNumber.ToString(),
				" (username = ",
				profile.Username,
				")"
			}));
			Action<PlayerRoster.Event, PlayerRoster.Profile> onRosterEvent = PlayerRoster.OnRosterEvent;
			if (onRosterEvent != null)
			{
				onRosterEvent(PlayerRoster.Event.Registered, profile);
			}
			if (profile.IsLocal)
			{
				PlayerRoster.RefreshLocalPlayerIndexes();
			}
		}

				public static void RegisterLocalPlayer(int playerNumber, int inputDeviceIndex)
		{
			PlayerRoster.Register(new PlayerRoster.Profile(playerNumber, PlayerRoster.LocalPlayerCount, inputDeviceIndex, Team.Red, default(NetworkUserId), true, ""));
		}

				public static void RegisterLocalPlayerOnline(int playerNumber, int inputDeviceIndex, NetworkUserId networkUserId)
		{
			PlayerRoster.Register(new PlayerRoster.Profile(playerNumber, PlayerRoster.LocalPlayerCount, inputDeviceIndex, Team.Red, networkUserId, true, NetworkSystem.GetUsername(networkUserId)));
		}

				public static void RegisterRemotePlayer(int playerNumber, int localPlayerIndex, int inputDeviceIndex, NetworkUserId networkUserId, string userName)
		{
			PlayerRoster.Register(new PlayerRoster.Profile(playerNumber, localPlayerIndex, inputDeviceIndex, Team.Red, networkUserId, false, userName));
		}

				public static void Deregister(PlayerRoster.Profile profile)
		{
			if (!PlayerRoster.profiles.Remove(profile))
			{
				return;
			}
			if (profile == null)
			{
				Debug.LogError("=== PlayerRoster: trying to deregister player with null profile");
				return;
			}
			Debug.Log("PlayerRoster: deregistered player " + profile.PlayerNumber.ToString());
			if (profile.IsLocal)
			{
				PlayerRoster.RefreshLocalPlayerIndexes();
			}
			if (PlayerRoster.GetTeamSize(Team.Red) == 0 || PlayerRoster.GetTeamSize(Team.Blue) == 0)
			{
				PlayerRoster.TeamsRequireReassignment = true;
			}
			Action<PlayerRoster.Event, PlayerRoster.Profile> onRosterEvent = PlayerRoster.OnRosterEvent;
			if (onRosterEvent == null)
			{
				return;
			}
			onRosterEvent(PlayerRoster.Event.Deregistered, profile);
		}

				public static void Deregister(int playerNumber)
		{
			PlayerRoster.Deregister(PlayerRoster.GetProfile(playerNumber));
		}

				public static void Deregister(NetworkUserId networkUserId)
		{
			for (int i = PlayerRoster.profiles.Count - 1; i >= 0; i--)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.NetworkUserId == networkUserId)
				{
					PlayerRoster.Deregister(profile.PlayerNumber);
				}
			}
		}

				public static void RefreshProfiles()
		{
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (!NetworkSystem.IsInSession && profile.IsRemote)
				{
					PlayerRoster.Deregister(profile.PlayerNumber);
				}
			}
			PlayerRoster.RefreshLocalPlayerIndexes();
		}

				private static void RefreshLocalPlayerIndexes()
		{
			List<PlayerRoster.Profile> list = (from p in PlayerRoster.profiles
											   where p.IsLocal
											   orderby p.PlayerNumber
											   select p).ToList<PlayerRoster.Profile>();
			for (int i = 0; i < list.Count; i++)
			{
				PlayerRoster.Profile profile = list[i];
				int num = i;
				profile.Overwrite(num, profile.InputDeviceIndex, profile.Team, profile.NetworkUserId, profile.IsLocal, profile.BaseUsername);
			}
		}

				public static void Clear()
		{
			for (int i = PlayerRoster.profiles.Count - 1; i >= 0; i--)
			{
				PlayerRoster.Deregister(PlayerRoster.profiles[i].PlayerNumber);
			}
			PlayerRoster.profiles.Clear();
		}

				public static int GetTeamSize(Team team)
		{
			int num = 0;
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				if (PlayerRoster.profiles[i].Team == team)
				{
					num++;
				}
			}
			return num;
		}

				public static PlayerRoster.Profile GetTeamMember(Team team, int indexInTeam)
		{
			PlayerRoster.tmp.Clear();
			for (int i = 0; i < PlayerRoster.profiles.Count; i++)
			{
				PlayerRoster.Profile profile = PlayerRoster.profiles[i];
				if (profile.Team == team)
				{
					PlayerRoster.tmp.Add(profile.PlayerNumber);
				}
			}
			PlayerRoster.tmp.Sort();
			return PlayerRoster.GetProfile(PlayerRoster.tmp[indexInTeam]);
		}

				public static void AssignTeams()
		{
			List<PlayerRoster.Profile> list = PlayerRoster.profiles.OrderBy((PlayerRoster.Profile p) => p.PlayerNumber).ToList<PlayerRoster.Profile>();
			switch (list.Count)
			{
				case 1:
					list[0].Team = Team.Red;
					break;
				case 2:
					list[0].Team = Team.Red;
					list[1].Team = Team.Blue;
					break;
				case 3:
					list[0].Team = Team.Red;
					list[1].Team = Team.Blue;
					list[2].Team = Team.Blue;
					break;
				case 4:
					list[0].Team = Team.Red;
					list[1].Team = Team.Red;
					list[2].Team = Team.Blue;
					list[3].Team = Team.Blue;
					break;
			}
			Debug.Log("PlayerRoster: Re-balanced teams.");
			PlayerRoster.TeamsRequireReassignment = false;
		}

						static PlayerRoster()
		{
		}

				public const int MinPlayers = 1;

				public const int MaxPlayers = 4;

				private static List<PlayerRoster.Profile> profiles = new List<PlayerRoster.Profile>();

				private static List<int> tmp = new List<int>();

				private static List<PlayerRoster.Profile> tmpProfiles = new List<PlayerRoster.Profile>();

				[CompilerGenerated]
		private static Action<PlayerRoster.Event, PlayerRoster.Profile> OnRosterEvent;

				[CompilerGenerated]
		private static bool <TeamsRequireReassignment>k__BackingField;

				public class Profile
		{
									public int PlayerNumber
			{
				get
				{
					return this.data.PlayerNumber;
				}
			}

									public int InputDeviceIndex
			{
				get
				{
					return this.data.InputDeviceIndex;
				}
			}

									public NetworkUserId NetworkUserId
			{
				get
				{
					return this.data.NetworkUserId;
				}
			}

									public bool IsLocal
			{
				get
				{
					return this.data.IsLocal;
				}
			}

									public bool IsRemote
			{
				get
				{
					return !this.IsLocal;
				}
			}

									public int LocalPlayerIndex
			{
				get
				{
					return this.data.LocalPlayerIndex;
				}
			}

												public string Username
			{
				[CompilerGenerated]
				get
				{
					return this.< Username > k__BackingField;
				}
				[CompilerGenerated]
				private set
				{
					this.< Username > k__BackingField = value;
				}
			}

									public string BaseUsername
			{
				get
				{
					return this.data.BaseUsername;
				}
			}

												public Team Team
			{
				get
				{
					return this.data.Team;
				}
				set
				{
					Team team = this.data.Team;
					this.data.Team = value;
					if (team != this.data.Team)
					{
						Action<PlayerRoster.Event, PlayerRoster.Profile> onRosterEvent = PlayerRoster.OnRosterEvent;
						if (onRosterEvent == null)
						{
							return;
						}
						onRosterEvent(PlayerRoster.Event.ProfileModified, this);
					}
				}
			}

						public Profile(int playerNumber, int localPlayerIndex, int inputDeviceIndex, Team team, NetworkUserId networkUserId, bool isLocal, string baseUsername)
			{
				this.data = new PlayerRoster.Profile.Data
				{
					PlayerNumber = playerNumber,
					LocalPlayerIndex = localPlayerIndex,
					InputDeviceIndex = inputDeviceIndex,
					Team = team,
					NetworkUserId = networkUserId,
					IsLocal = isLocal,
					BaseUsername = baseUsername
				};
				this.Username = this.data.GetFullUsername();
			}

						public void Overwrite(int localPlayerIndex, int inputDeviceIndex, Team team, NetworkUserId networkUserId, bool isLocal, string baseUsername)
			{
				bool flag = false;
				bool flag2 = false;
				int localPlayerIndex2 = this.data.LocalPlayerIndex;
				this.data.LocalPlayerIndex = localPlayerIndex;
				if (this.data.LocalPlayerIndex != localPlayerIndex2)
				{
					flag = true;
					flag2 = true;
				}
				int inputDeviceIndex2 = this.data.InputDeviceIndex;
				this.data.InputDeviceIndex = inputDeviceIndex;
				if (this.data.InputDeviceIndex != inputDeviceIndex2)
				{
					flag = true;
				}
				Team team2 = this.data.Team;
				this.data.Team = team;
				if (this.data.Team != team2)
				{
					flag = true;
				}
				NetworkUserId networkUserId2 = this.data.NetworkUserId;
				this.data.NetworkUserId = networkUserId;
				if (this.data.NetworkUserId != networkUserId2)
				{
					flag = true;
					flag2 = true;
				}
				bool isLocal2 = this.data.IsLocal;
				this.data.IsLocal = isLocal;
				if (this.data.IsLocal != isLocal2)
				{
					flag = true;
					flag2 = true;
				}
				string baseUsername2 = this.data.BaseUsername;
				this.data.BaseUsername = baseUsername;
				if (this.data.BaseUsername != baseUsername2)
				{
					flag = true;
					flag2 = true;
				}
				if (flag2)
				{
					this.Username = this.data.GetFullUsername();
				}
				if (flag && PlayerRoster.profiles.Contains(this))
				{
					Action<PlayerRoster.Event, PlayerRoster.Profile> onRosterEvent = PlayerRoster.OnRosterEvent;
					if (onRosterEvent == null)
					{
						return;
					}
					onRosterEvent(PlayerRoster.Event.ProfileModified, this);
				}
			}

						public PlayerRoster.Profile.Data GetData()
			{
				return this.data;
			}

						private PlayerRoster.Profile.Data data;

						[CompilerGenerated]
			private string <Username>k__BackingField;

						public struct Data
			{
								public string GetFullUsername()
				{
					string text = this.BaseUsername;
					if (this.LocalPlayerIndex > 0)
					{
						text = text + " " + (this.LocalPlayerIndex + 1).ToString();
					}
					return text;
				}

								public int PlayerNumber;

								public int LocalPlayerIndex;

								public int InputDeviceIndex;

								public Team Team;

								public NetworkUserId NetworkUserId;

								public bool IsLocal;

								public string BaseUsername;
			}
		}

				public enum Event
		{
						Registered,
						Deregistered,
						ProfileModified
		}

				private class ProfileComparer : IComparer<PlayerRoster.Profile>
		{
						int IComparer<PlayerRoster.Profile>.Compare(PlayerRoster.Profile x, PlayerRoster.Profile y)
			{
				if (x == null)
				{
					return -1;
				}
				if (y == null)
				{
					return 1;
				}
				if (x.PlayerNumber == y.PlayerNumber)
				{
					return 0;
				}
				if (x.PlayerNumber <= y.PlayerNumber)
				{
					return -1;
				}
				return 1;
			}

						public ProfileComparer()
			{
			}

									static ProfileComparer()
			{
			}

						public static readonly PlayerRoster.ProfileComparer Instance = new PlayerRoster.ProfileComparer();
		}

				[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
									static <>c()
		{
		}

				public <>c()
		{
		}

				internal bool <get_LocalPlayerCount>b__11_0(PlayerRoster.Profile p)
		{
			return p.IsLocal;
		}

				internal bool <get_RemotePlayerCount>b__13_0(PlayerRoster.Profile p)
		{
			return !p.IsLocal;
		}

				internal bool <RefreshLocalPlayerIndexes>b__38_0(PlayerRoster.Profile p)
		{
			return p.IsLocal;
		}

				internal int <RefreshLocalPlayerIndexes>b__38_1(PlayerRoster.Profile p)
		{
			return p.PlayerNumber;
		}

				internal int <AssignTeams>b__42_0(PlayerRoster.Profile p)
		{
			return p.PlayerNumber;
		}

				public static readonly PlayerRoster.<>c<>9 = new PlayerRoster.<>c();

				public static Func<PlayerRoster.Profile, bool> <>9__11_0;

						public static Func<PlayerRoster.Profile, bool> <>9__13_0;

						public static Func<PlayerRoster.Profile, bool> <>9__38_0;

						public static Func<PlayerRoster.Profile, int> <>9__38_1;

						public static Func<PlayerRoster.Profile, int> <>9__42_0;
		}
}
}
