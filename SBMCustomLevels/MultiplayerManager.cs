using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Catobyte.Networking;
using SBM.Shared.Networking;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UITransitioner = SBM.UI.Utilities.Transitioner.UITransitioner;
using UIFocusable = SBM.UI.Utilities.Focus.UIFocusable;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    static class MultiplayerManager
    {
        private static string level;

        public static string LevelID
        {
            get
            {
                return level;
            }
            private set
            {
                level = value;
            }
        }

        private static GameObject onlineInviteButton;
        private static List<GameObject> connectedPlayerUIs = new List<GameObject>();

        private static NetworkChannel CustomChannel;

        #region Receive
        // add custom network channel for handling mod specific traffic
        [HarmonyPatch(typeof(NetworkSystem.Channels), "Initialize")]
        [HarmonyPrefix]
        static void AddCustomChannel()
        {
            CustomChannel = NetworkChannel.RegisterChannel();
        }

        // create new packet receiver for "OnReceivedCustomLevelData" so mod specific data can be sent between client and server.
        [HarmonyPatch(typeof(NetworkSystem), "OnNetworkSession_MemberJoined")]
        [HarmonyPrefix]
        static void UpdatePacketReceivers(NetworkSystem __instance, NetworkUserId memberId)
        {
            bool userIsLocal = Network.Service.UserIsLocal(memberId);

            if (userIsLocal)
            {
                return;
            }
            else
            {
                // clear existing player roster, since it is updated again in ConfigureCoopPlayersForNetworkPlay
                OverrideCoopPlayerProfileCount();
                // Debug.Log($"OnNetworkSession_MemberJoined {Network.Service.GetUsernameById(memberId)}");
                Network.Session.SubscribeToReceive<CustomSceneData>(memberId, CustomChannel, new OnDataReceived(OnReceivedCustomLevelData), true);
            }
        }

        private static string GetLevelByHash(string worldName, ulong levelHash)
        {
            foreach (World world in LevelLoader_Mod.worldsList)
            {
                if (world.Name == worldName)
                {
                    foreach (Level level in world.levels)
                    {
                        if (level.levelHash == levelHash)
                        {
                            return level.levelPath;
                        }
                    }
                }
            }

            return null;
        }

        private static void OnReceivedCustomLevelData(NetworkUserId id, TickNumber tickNumber, NetworkData data)
        {
            if (NetworkSystem.instance == null)
            {
                return;
            }

            NetworkUser userById = NetworkSystem.instance.GetUserById(id);

            if (userById != null)
            {
                CustomSceneData sceneData = userById.ReceiveData<CustomSceneData>(data);

                Debug.Log("CUSTOM WORLD: " + sceneData.world);
                Debug.Log("CUSTOM LEVEL: " + sceneData.level);
                Debug.Log("LEVEL TYPE: " + (LevelManager.LevelType)sceneData.levelType);

                LevelManager.LevelType levelType = (LevelManager.LevelType)sceneData.levelType;

                // escape condition if nonexistant level loads or other fault in transmitting level
                if (sceneData.level == ushort.MaxValue)
                {
                    if (LevelManager.InLevel)
                    {
                        SBM.Shared.SceneSystem.LoadScene("Menu");
                        return;
                    }
                }

                string receivedLevel = "";

                // if level type is party, determine and send level based on level itself, not world
                if (levelType == LevelManager.LevelType.Deathmatch)
                {
                    if (!Directory.Exists(LevelLoader_Mod.deathmatchPath)) 
                    {
                        if (LevelManager.InLevel)
                        {
                            SBM.Shared.SceneSystem.LoadScene("Menu");
                        }

                        Debug.Log("World not found! Potential mismatch between client and server?");
                        return;
                    }

                    receivedLevel = GetLevelByHash("Deathmatch", sceneData.world);
                }
                else if (levelType == LevelManager.LevelType.Basketball)
                {
                    if (!Directory.Exists(LevelLoader_Mod.basketballPath))
                    {
                        if (LevelManager.InLevel)
                        {
                            SBM.Shared.SceneSystem.LoadScene("Menu");
                        }

                        Debug.Log("World not found! Potential mismatch between client and server?");
                        return;
                    }

                    receivedLevel = GetLevelByHash("Basketball", sceneData.world);
                }
                else if (levelType == LevelManager.LevelType.CarrotGrab)
                {
                    if (!Directory.Exists(LevelLoader_Mod.carrotGrabPath))
                    {
                        if (LevelManager.InLevel)
                        {
                            SBM.Shared.SceneSystem.LoadScene("Menu");
                        }

                        Debug.Log("World not found! Potential mismatch between client and server?");
                        return;
                    }

                    receivedLevel = GetLevelByHash("Carrot Grab", sceneData.world);
                }
                else // if not party level type, determine id of level based on index in the sent world
                {
                    World receivedWorld = null;

                    foreach (World world in LevelLoader_Mod.worldsList)
                    {
                        if (world.WorldHash == sceneData.world)
                        {
                            receivedWorld = world;
                        }
                    }

                    if (receivedWorld == null)
                    {
                        if (LevelManager.InLevel)
                        {
                            SBM.Shared.SceneSystem.LoadScene("Menu");
                        }

                        Debug.Log("World not found! Potential mismatch between client and server?");
                        return;
                    }
                    else if (sceneData.level > receivedWorld.levels.Count - 1)
                    {
                        if (LevelManager.InLevel)
                        {
                            SBM.Shared.SceneSystem.LoadScene("Menu");
                        }

                        Debug.Log("Level not found! Potential mismatch between client and server?");
                        return;
                    }

                    LevelManager.instance.BeginLoadLevel(false, false, receivedWorld.levels[sceneData.level].levelPath, sceneData.level + 1, levelType, receivedWorld); // add isNetworkClient ?
                    return;
                }

                // if level type is party, determine and send level based on level itself, not world
                LevelManager.instance.BeginLoadLevel(false, false, receivedLevel, sceneData.level + 1, levelType);
            }
        }

        #endregion

        #region Send

        // level sent by OnSceneStateDoesNotMatchRemoteUser (i think ?) which is called by OnRemoteUserSceneStateConflict

        // GameManagerStory needs to start round!
        // NetworkSystem.SceneIsSyncedWithRemoteUsers is FALSE! GameManager yields null until this is TRUE!

        // if (custom level to be loaded)
        public static void SendCustomLevelData(ulong world, int level, LevelManager.LevelType levelType)
        {
            if (!Network.Session.Exists)
            {
                return;
            }

            CustomSceneData sceneData = new CustomSceneData();
            sceneData.world = world;
            sceneData.level = (ushort)level;
            sceneData.levelType = (ushort)levelType;

            Network.Session.SendToAll(CustomChannel, sceneData);
        }

        #endregion

        #region Patches

        // adjust steam maxplayers to always be 4 instead of 2
        [HarmonyPatch(typeof(NetworkSystem), "HostSession")]
        [HarmonyPrefix]
        static void IncreaseMaxPlayers(ref int maxPlayers, SessionAccess access)
        {
            maxPlayers = 4;
        }

        // called on both client and server, use to update UI when a new player (beyond 2nd) joins
        [HarmonyPatch(typeof(NetworkSystem), "OnNetworkUser_ConnectionReady")]
        [HarmonyPostfix]
        static void UpdateOnPlayerJoined(NetworkUser user)
        {
            if (NetworkSystem.IsHost && SBM.Shared.PlayerRoster.IsFull)
            {
                return;
            }

            string usernameById = Network.Service.GetUsernameById(user.Id);
            Debug.Log("Player Joined: " + usernameById);
        }


        [HarmonyPatch(typeof(NetworkSystem), "OnNetworkSession_MemberLeft")]
        [HarmonyPostfix]
        static void UpdateOnPlayerLeft(NetworkSystem __instance, NetworkUserId memberId)
        {
            if (!NetworkSystem.IsInSession)
            {
                return;
            }

            NetworkUser userById = __instance.GetUserById(memberId);
            
            for (int i = 0; i < __instance.users.Count; i++)
            {
                if (userById == __instance.localUser)
                {
                    continue;
                }

                if (userById == __instance.users[i] && i != 1) // dont delete original ui
                {
                    GameObject.Destroy(connectedPlayerUIs[i-1]);
                    connectedPlayerUIs.RemoveAt(i-1);
                }
            }
        }

        [HarmonyPatch(typeof(SBM.UI.MainMenu.StoryMode.UIStoryNetworkInvite), "BeginInvitation")]
        [HarmonyPrefix]
        static bool PreventHostSessionIfExists()
        {
            Debug.Log("Attempted to host session... returned: " + !NetworkSystem.IsInSession);

            return !NetworkSystem.IsInSession;
        }

        [HarmonyPatch(typeof(SBM.UI.Components.UIGameModeSetter), "SetGameMode_CoopStory")]
        [HarmonyPrefix]
        static void UpdateOnlineInviteButtonCoop()
        {
            onlineInviteButton.SetActive(true);
        }

        [HarmonyPatch(typeof(SBM.UI.Components.UIGameModeSetter), "SetGameMode_Story")]
        [HarmonyPrefix]
        static void UpdateOnlineInviteButtonStory()
        {
            onlineInviteButton.SetActive(false);
        }

        // non-specifically patch UIWorld5Model awake since it is at the same time as UIWorldSelector creation
        // allow the invite UI to properly invite more than 1 person
        [HarmonyPatch(typeof(SBM.UI.MainMenu.StoryMode.UIWorld5Model), "Awake")]
        [HarmonyPostfix]
        static void UpdateInviteUI()
        {
            // Networked Multiplayer Overrides
            Transform uiParent = GameObject.Find("Screen_StoryMode").transform;

            var inviteButton = uiParent.Find("UI_Bars/UI_Bar_Bottom/Networking/Network_Offline/Button_Invite").GetComponent<UIFocusable>();
            // re-parent invite button so that it is not deactivated on load and can thus be pressed again for more invitations
            inviteButton.transform.SetParent(inviteButton.transform.parent.parent);
            onlineInviteButton = inviteButton.gameObject;

            inviteButton.onSubmitSuccess = new UnityEngine.Events.UnityEvent();
            inviteButton.onSubmitSuccess.AddListener(delegate
            {
                if (!NetworkSystem.IsInSession)
                {
                    var networkPanel = uiParent.Find("Panel_NetworkInvite").GetComponent<UITransitioner>();
                    networkPanel.Transition_In_From_Top();

                    return;
                }

                if (NetworkSystem.IsHost)
                {
                    if (NetworkSystem.UserCount > 1)
                    {
                        var networkPanel = uiParent.Find("Panel_NetworkInvite").GetComponent<UITransitioner>();

                        networkPanel.Transition_Out_To_Top();
                        GameObject.Find("World Selector").GetComponent<UIFocusable>().Focus();
                        NetworkSystem.InviteViaServiceOverlay();
                    }
                }
            });

            var networkInvite = uiParent.Find("Panel_NetworkInvite").GetComponent<SBM.UI.MainMenu.StoryMode.UIStoryNetworkInvite>();
            var networkOnlineUI = uiParent.Find("UI_Bars/UI_Bar_Bottom/Networking/Network_Online").GetComponent<UITransitioner>();

            for (int i = 1; i < NetworkSystem.UserCount; i++)
            {
                if (i == 1)
                {
                    connectedPlayerUIs.Clear();
                    networkOnlineUI.Transition_Out_To_Top();
                    networkOnlineUI.anchoredPosWhenShown = new Vector3(125, 0);
                    networkOnlineUI.Transition_In_From_Bottom();
                    networkOnlineUI.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>().PlayerNumber = 1;
                    continue;
                }

                var networkGO = GameObject.Instantiate(networkOnlineUI, networkOnlineUI.transform.parent);
                connectedPlayerUIs.Add(networkGO.gameObject);
                networkGO.anchoredPosWhenShown = new Vector3(125 + i * 100, 0);
                networkGO.Transition_In_From_Bottom();
                var playerIcon = networkGO.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>();
                playerIcon.PlayerNumber = i + 1;
                playerIcon.Refresh();
            }

            networkInvite.onInviteSuccess = new UnityEngine.Events.UnityEvent();
            networkInvite.onInviteSuccess.AddListener(delegate
            {
                if (!NetworkSystem.IsHost)
                {
                    return;
                }

                networkInvite.GetComponent<UITransitioner>().Transition_Out_To_Top();

                if (NetworkSystem.UserCount <= 2)
                {
                    connectedPlayerUIs.Clear();
                    networkOnlineUI.anchoredPosWhenShown = new Vector3(125, 0);
                    networkOnlineUI.Transition_In_From_Bottom();
                    networkOnlineUI.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>().PlayerNumber = NetworkSystem.UserCount;
                }
                else
                {
                    // duplicate Network_Online UI
                    var networkGO = GameObject.Instantiate(networkOnlineUI, networkOnlineUI.transform.parent);
                    connectedPlayerUIs.Add(networkGO.gameObject);
                    networkGO.anchoredPosWhenShown = new Vector3(125 + NetworkSystem.UserCount * 100, 0);
                    networkGO.Transition_In_From_Bottom();
                    var playerIcon = networkGO.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>();
                    playerIcon.PlayerNumber = NetworkSystem.UserCount;
                    playerIcon.Refresh();
                }

                GameObject.FindObjectOfType<SBM.UI.MainMenu.StoryMode.UIWorldSelector>().GetComponent<UIFocusable>().Focus();
                GameObject.FindObjectOfType<SBM.UI.Components.UIPlayerRoster>().ConfigureCoopPlayersForNetworkPlay();
            });
        }

        // loop through ALL players instead of just first 2 for coop
        [HarmonyPatch(typeof(SBM.UI.Components.UIPlayerRoster), "ConfigureCoopPlayersForNetworkPlay")]
        [HarmonyPrefix]
        public static bool OverrideCoopPlayerProfileCount()
        {
            if (NetworkSystem.IsHost)
            {
                // Debug.Log("OverrideCoopPlayerProfileCount " + SBM.Shared.PlayerRoster.profiles.Count);
                // Debug.Log("Network Count " + NetworkSystem.UserCount);
                var localProfile = SBM.Shared.PlayerRoster.GetProfile(1);
                NetworkUserId localUserId = NetworkSystem.LocalUserId;
                string localUsername = NetworkSystem.LocalUsername;
                localProfile.Overwrite(0, 0, SBM.Shared.Team.Red, localUserId, true, localUsername);

                for (int i = 1; i < NetworkSystem.UserCount; i++)
                {
                    // register profile here ? (if network.usercount is accurate)
                    SBM.Shared.PlayerRoster.Deregister(i + 1);
                    SBM.Shared.PlayerRoster.RegisterRemotePlayer(i + 1, 0, 0, NetworkSystem.instance.users[i].Id);
                    var remoteProfile1 = SBM.Shared.PlayerRoster.GetProfile(i + 1);
                    
                    NetworkUserId remoteUserId1 = NetworkSystem.GetRemoteUserId(i - 1); // i - 1, since this should start at 1 (first remote user)
                    string remoteUsername1 = NetworkSystem.GetUsername(remoteUserId1);
                    Debug.Log(remoteUsername1);
                    Debug.Log(remoteUserId1);
                    remoteProfile1.Overwrite(0, 0, SBM.Shared.Team.Red, remoteUserId1, false, remoteUsername1);
                }
            }

            return false;
        }

        // ensure that gamemanager properly spawns all players, regardless of actual player spawn count
        [HarmonyPatch(typeof(SBM.Shared.GameManager), "RespawnAllPlayers")]
        [HarmonyPrefix]
        static bool OverridePlayerRespawn(SBM.Shared.GameManager __instance)
        {
            for (int i = 0; i < SBM.Shared.Player.Count; i++)
            {
                SBM.Shared.Player.GetByIndex(i).SetVisible(false);
            }

            // GameManager.ReorderSpawnPoints(List<Vector3> output) *NON-OVERRIDE*
            __instance.spawnPoints.Clear();
            for (int i = 0; i < SBM.Shared.PlayerSpawnPoint.Count; i++)
            {
                Vector3 byIndex = SBM.Shared.PlayerSpawnPoint.GetByIndex(i);
                __instance.spawnPoints.Add(byIndex);
            }
            var state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(__instance.ResetCount);
            Catobyte.Utilities.ExtensionMethods.Shuffle(__instance.spawnPoints);
            UnityEngine.Random.state = state;
            // end

            Debug.Log("Spawn Count: " + __instance.spawnPoints.Count);

            for (int j = 0; j < SBM.Shared.PlayerRoster.Profiles.Count; j++)
            {
                SBM.Shared.Player byNumber = SBM.Shared.Player.GetByNumber(SBM.Shared.PlayerRoster.GetPlayerNumber(j));

                if (byNumber != null)
                {
                    if (j >= __instance.spawnPoints.Count)
                    {
                        Vector3 lastSpawnPoint = __instance.spawnPoints.Last();

                        byNumber.SpawnPoint = new Vector3(lastSpawnPoint.x + j, lastSpawnPoint.y, lastSpawnPoint.z);
                        byNumber.Respawn();

                        continue;
                    }

                    byNumber.SpawnPoint = __instance.spawnPoints[j];
                    byNumber.Respawn();
                }
            }

            return false;
        }

        // change the SceneState, since CurrentSceneIndex is only set when a new scene is loaded based on that scene's build index.
        // scenes loaded through asset bundles (like the 'base level') are not registered in the build settings, and therefore have a
        // build index of -1, a value which will not be properly sent through the existing SceneData packets.
        [HarmonyPatch(typeof(SBM.Shared.SceneSystem), "GetState")]
        [HarmonyPrefix]
        static bool ModifyCustomSceneState(ref SBM.Shared.SceneState __result)
        {
            if (LevelManager.InLevel)
            {
                __result = new SBM.Shared.SceneState
                {
                    CurrentSceneIndex = 255,
                    TargetSceneIndex = SBM.Shared.SceneSystem.TargetSceneIndex
                };
            }
            else
            {
                __result = new SBM.Shared.SceneState
                {
                    CurrentSceneIndex = SBM.Shared.SceneSystem.CurrentSceneIndex,
                    TargetSceneIndex = SBM.Shared.SceneSystem.TargetSceneIndex
                };
            }

            return false;
        }

        // original method has a check for if playerScene matches CurrentSceneIndex, this will never be true since CurrentSceneIndex is -1 (loaded by assetbundle)
        // and thus is not actually in the scene build index at all.
        [HarmonyPatch(typeof(NetworkSystem), "OnReceivedPlayersData")]
        [HarmonyPrefix]
        static bool OverridePlayerSceneRequirement(NetworkSystem __instance, NetworkUserId senderId, TickNumber tickNumber, NetworkData data)
        {
            if (!LevelManager.InLevel)
            {
                return true;
            }

            NetworkUser userById = __instance.GetUserById(senderId);
            if (userById == null)
            {
                return false;
            }

            var playersData = userById.ReceiveData<SBM.Shared.Networking.Data.PlayersData>(data);

            SBM.Shared.Utilities.CloudDiagnosticsHelper.CloudDiagnostics.ExceptionPinpointer.InspectIfNull(playersData, "pDatas", 820, "Issue #100");

            int localPlayerCount = userById.LocalPlayerCount;
            if (localPlayerCount <= 0)
            {
                return false;
            }

            // determine if custom level, Scene buildIndex of -1 means it was loaded via assetbundle, meaning a custom level.
            // so method should continue as players are (almost certainly) in the same client scenes.

            if ((int)playersData.SceneIndex != SBM.Shared.SceneSystem.CurrentScene.buildIndex)
            {
                if (SBM.Shared.SceneSystem.CurrentScene.buildIndex != -1)
                {
                    return false;
                }
            }

            if (playersData.PhysicsSessionId != Catobyte.Networking.Physics.NetworkPhysics.PhysicsSessionId)
            {
                Debug.Log("[SBM Custom Levels] Physics Session Match: false");
                return false;
            }

            for (int i = 0; i < playersData.Count; i++)
            {
                SBM.Shared.Networking.Data.PlayerData playerData = playersData[i];
                var byNumber = SBM.Shared.Player.GetByNumber((int)playerData.Number);
                if (!(byNumber == null))
                {
                    bool flag = false;
                    for (int j = 0; j < localPlayerCount; j++)
                    {
                        int playerNumber = SBM.Shared.PlayerRoster.GetPlayerNumber(senderId, j);
                        if (playerNumber >= 1 && playerNumber <= 4 && SBM.Shared.PlayerRoster.PlayerIsRegistered(playerNumber))
                        {
                            var byNumber2 = SBM.Shared.Player.GetByNumber(playerNumber);
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
                            byNumber.Wetness = (float)playerData.Wetness / 255f;
                            byNumber.CurrentBurnFactor = (float)playerData.Burntness / 255f;
                        }
                    }
                }
            }

            return false;
        }

        // original method has a check for if playerScene matches CurrentSceneIndex, this will never be true since CurrentSceneIndex is -1 (loaded by assetbundle)
        // and thus is not actually in the scene build index at all.
        [HarmonyPatch(typeof(SBM.Shared.GameManager), "ProcessNetworkGameManagerData")]
        [HarmonyPrefix]
        static bool OverrideGameManagerSceneRequirement(SBM.Shared.GameManager __instance, SBM.Shared.Networking.Data.GameData.GameManagerData data, bool senderIsHost)
        {
            if (!LevelManager.InLevel)
            {
                return true;
            }

            if ((int)data.SceneIndex != SBM.Shared.SceneSystem.CurrentScene.buildIndex)
            {
                if (SBM.Shared.SceneSystem.CurrentScene.buildIndex != -1)
                {
                    return false;
                }
            }

            if (data.ResetCount > __instance.resetCount)
            {
                __instance.resetCount = data.ResetCount;
                __instance.ResetRound(false, 0f);
            }

            if (data.ResetCount == __instance.resetCount && data.RoundState > __instance.roundState)
            {
                SBM.Shared.RoundState roundState = __instance.roundState;
                if (roundState != SBM.Shared.RoundState.Reset)
                {
                    if (roundState == SBM.Shared.RoundState.Started)
                    {
                        __instance.EndRound(0f);
                    }
                }
                else
                {
                    __instance.StartRound(0f);
                }
            }
            if (senderIsHost)
            {
                byte[] playerLives = data.PlayerLives;
                for (int i = 0; i < playerLives.Length; i++)
                {
                    __instance.SetPlayerLives(i + 1, (int)playerLives[i]);
                }
                __instance.SetTeamScore(SBM.Shared.Team.Red, (int)data.ScoreRedTeam);
                __instance.SetTeamScore(SBM.Shared.Team.Blue, (int)data.ScoreBlueTeam);
                __instance.Winner = SBM.Shared.Player.GetByNumber((int)data.WinningPlayerNumber);
                if (data.RedTeamIsWinner)
                {
                    __instance.WinningTeam = new SBM.Shared.Team?(SBM.Shared.Team.Red);
                }
                else if (data.BlueTeamIsWinner)
                {
                    __instance.WinningTeam = new SBM.Shared.Team?(SBM.Shared.Team.Blue);
                }
                else
                {
                    __instance.WinningTeam = null;
                }
            }
            if (!NetworkSystem.IsHost && senderIsHost)
            {
                __instance.timer.Set(data.RoundTimer);
                __instance.worldTime = data.WorldTime;
            }
            bool flag = false;
            if (data.PauseCount > __instance.pauseCount)
            {
                __instance.pauseCount = data.PauseCount;
                flag = true;
            }
            if (data.ResumeCount > __instance.resumeCount)
            {
                __instance.resumeCount = data.ResumeCount;
                flag = true;
            }
            if (flag)
            {
                if (__instance.pauseCount > __instance.resumeCount)
                {
                    __instance.SetGamePaused(true, false);
                    return false;
                }
                if (__instance.pauseCount == __instance.resumeCount)
                {
                    __instance.SetGamePaused(false, false);
                }
            }

            return false;
        }

        // original method has a check for if playerScene matches CurrentSceneIndex, this will never be true since CurrentSceneIndex is -1 (loaded by assetbundle)
        // and thus is not actually in the scene build index at all.
        [HarmonyPatch(typeof(Catobyte.Networking.Physics.NetworkPhysics), "OnPhysicsDataReceived")]
        [HarmonyPrefix]
        static bool OverridePhysicsSceneRequirement(Catobyte.Networking.Physics.NetworkPhysics __instance, NetworkUserId sender, TickNumber tickNumber, NetworkData networkData)
        {
            if (!LevelManager.InLevel)
            {
                return true;
            }

            var physicsData = (Catobyte.Networking.Physics.Data.PhysicsData)networkData;
            if (!__instance.receivingEnabled)
            {
                return false;
            }

            ((ICopyable<Catobyte.Networking.Physics.Data.PhysicsData>)__instance.lastRxPhysicsData).CopyFrom(physicsData);
            if ((int)physicsData.SceneIndex != __instance.sceneIndex)
            {
                if (SBM.Shared.SceneSystem.CurrentScene.buildIndex != -1)
                {
                    return false;
                }
            }

            if (physicsData.PhysicsSessionId != Catobyte.Networking.Physics.NetworkPhysics.PhysicsSessionId)
            {
                return false;
            }

            var byId = Catobyte.Networking.Physics.Authority.NetworkAgent.GetById(physicsData.AgentId);
            if (byId != null)
            {
                if (byId.Type == Catobyte.Networking.Physics.Authority.AgentType.Local)
                {
                    return false;
                }
                if (physicsData.AuthDecs.Count == 0)
                {
                    byId.ClearRemoteAuthDecs();
                    return false;
                }
                for (int i = 0; i < physicsData.AuthDecs.Count; i++)
                {
                    byId.ReceiveRemoteAuthDec(physicsData.AuthDecs[i], tickNumber);
                }
            }

            return false;
        }

        #endregion

        [Serializable]
        public class CustomSceneData : NetworkData
        {
            // hashed version of the world based on world name, used to differentiate between worlds
            public ulong world;
            // integer identifier of level within a world 0-9 for the 10 levels (max) that exist in a world
            public ushort level;
            // integer identifier for type of level (using enum LevelManager.LevelType)
            public ushort levelType;

            public override void Serialize(ISerializer ser)
            {
                ser.Write(world);
                ser.Write(level);
                ser.Write(levelType);
            }

            public override void Deserialize(IDeserializer deser)
            {
                this.world = deser.Read<ulong>();
                this.level = deser.Read<ushort>();
                this.levelType = deser.Read<ushort>();
            }
        }
    }
}
