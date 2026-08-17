using System;
using Catobyte.Networking;
using SBM.Shared;
using SBM.Shared.Networking;
using UnityEngine;

namespace SBM.UI.Components
{
		public class UIPlayerRoster : MonoBehaviour
	{
				public void SetupSinglePlayer()
		{
			PlayerRoster.Clear();
			PlayerRoster.RegisterLocalPlayer(1, 0);
		}

				public void SetupCoopPlayers()
		{
			PlayerRoster.Clear();
			PlayerRoster.RegisterLocalPlayer(1, 0);
			PlayerRoster.RegisterLocalPlayer(2, 1);
		}

				public void SetSwitchAppletSinglePlayer()
		{
		}

				public void SetSwitchAppletCoopPlayers()
		{
		}

				public void ConfigureCoopPlayersForNetworkPlay()
		{
			if (NetworkSystem.IsHost)
			{
				PlayerRoster.Profile profile = PlayerRoster.GetProfile(1);
				PlayerRoster.Profile profile2 = PlayerRoster.GetProfile(2);
				NetworkUserId localUserId = NetworkSystem.LocalUserId;
				string localUsername = NetworkSystem.LocalUsername;
				NetworkUserId remoteUserId = NetworkSystem.GetRemoteUserId(0);
				string username = NetworkSystem.GetUsername(remoteUserId);
				profile.Overwrite(0, 0, Team.Red, localUserId, true, localUsername);
				profile2.Overwrite(0, 0, Team.Red, remoteUserId, false, username);
			}
		}

				public void ClearRoster()
		{
			PlayerRoster.Clear();
		}

				public void RegisterNextAvailablePlayerToInputDevice(int inputDeviceIndex)
		{
			if (NetworkSystem.IsInSession && !NetworkSystem.IsHost)
			{
				return;
			}
			if (PlayerRoster.GetByInputDeviceIndex(inputDeviceIndex, default(NetworkUserId)) != null)
			{
				return;
			}
			int nextAvailablePlayerNumber = PlayerRoster.GetNextAvailablePlayerNumber();
			if (nextAvailablePlayerNumber > 0)
			{
				PlayerRoster.RegisterLocalPlayer(nextAvailablePlayerNumber, inputDeviceIndex);
			}
		}

				public void DeregisterPlayerWithInputDevice(int inputDeviceIndex)
		{
			if (NetworkSystem.IsInSession && !NetworkSystem.IsHost)
			{
				return;
			}
			PlayerRoster.Profile byInputDeviceIndex = PlayerRoster.GetByInputDeviceIndex(inputDeviceIndex, default(NetworkUserId));
			if (byInputDeviceIndex != null)
			{
				PlayerRoster.Deregister(byInputDeviceIndex);
			}
		}

				public void RegisterNextAvailablePlayerToRemoteNetworkUser(int remoteUserIndex)
		{
			if (!NetworkSystem.IsInSession || !NetworkSystem.IsHost)
			{
				return;
			}
			int nextAvailablePlayerNumber = PlayerRoster.GetNextAvailablePlayerNumber();
			NetworkUserId remoteUserId = NetworkSystem.GetRemoteUserId(remoteUserIndex);
			PlayerRoster.RegisterRemotePlayer(nextAvailablePlayerNumber, 0, 0, remoteUserId, NetworkSystem.GetUsername(remoteUserId));
		}

				public void DeregisterPlayerWithRemoteNetworkUserIndex(int remoteUserIndex)
		{
			if (!NetworkSystem.IsInSession || !NetworkSystem.IsHost)
			{
				return;
			}
			PlayerRoster.Deregister(NetworkSystem.GetRemoteUserId(remoteUserIndex));
		}

				public void ReassignTeamsIfRequired()
		{
			if (PlayerRoster.TeamsRequireReassignment)
			{
				PlayerRoster.AssignTeams();
			}
		}

				public UIPlayerRoster()
		{
		}
	}
}
