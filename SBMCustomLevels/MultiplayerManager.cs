using System;
using System.Collections.Generic;
using System.Linq;
using Catobyte.Networking;
using SBM.Shared.Networking;
using HarmonyLib;
using UnityEngine;
using UITransitioner = SBM.UI.Utilities.Transitioner.UITransitioner;
using UIFocusable = SBM.UI.Utilities.Focus.UIFocusable;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    static class MultiplayerManager
    {
        private static string level;
        public static int playerCount = 1;
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
        static void UpdatePacketReceivers(NetworkUserId memberId)
        {
            bool userIsLocal = Network.Service.UserIsLocal(memberId);

            if (userIsLocal)
            {
                return;
            }
            else
            {
                Network.Session.SubscribeToReceive<CustomSceneData>(memberId, CustomChannel, new OnDataReceived(OnReceivedCustomLevelData), true);
            }
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

                // escape condition if nonexistant level loads or other fault in transmitting level
                if (sceneData.level == ushort.MaxValue)
                {
                    if (LevelManager.InLevel)
                    {
                        SBM.Shared.SceneSystem.LoadScene("Menu");
                        return;
                    }
                }

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

                LevelManager.instance.BeginLoadLevel(false, false, receivedWorld.levels[sceneData.level], sceneData.level + 1, LevelManager.LevelType.Story, receivedWorld); // add isNetworkClient ? 
                // send callback confirming load, THEN activate network ?
            }
        }

        #endregion

        #region Send

        // level sent by OnSceneStateDoesNotMatchRemoteUser (i think ?) which is called by OnRemoteUserSceneStateConflict

        // GameManagerStory needs to start round!
        // NetworkSystem.SceneIsSyncedWithRemoteUsers is FALSE! GameManager yields null until this is TRUE!

        // if (custom level to be loaded)
        public static void SendCustomLevelData(ulong world, int level)
        {
            if (!Network.Session.Exists)
            {
                return;
            }

            CustomSceneData sceneData = new CustomSceneData();
            sceneData.world = world;
            sceneData.level = (ushort)level;

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

            playerCount -= 1;

            if (playerCount < 1)
            {
                playerCount = 1;
            }

            NetworkUser userById = __instance.GetUserById(memberId);
            
            for (int i = 0; i < __instance.users.Count; i++)
            {
                if (userById == __instance.localUser)
                {
                    continue;
                }

                if (userById == __instance.users[i])
                {
                    GameObject.Destroy(connectedPlayerUIs[i]);
                    connectedPlayerUIs.RemoveAt(i);
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

        // non-specifically patch UIWorld5Model awake since it is at the same time as UIWorldSelector creation
        // allow the invite UI to properly invite more than 1 person
        [HarmonyPatch(typeof(SBM.UI.MainMenu.StoryMode.UIWorld5Model), "Awake")]
        [HarmonyPostfix]
        static void UpdateInviteUI()
        {
            // Networked Multiplayer Overrides
            Transform uiParent = GameObject.Find("Screen_StoryMode").transform;

            var inviteButton = uiParent.Find("UI_Bars/UI_Bar_Bottom/Networking/Network_Offline/Button_Invite").GetComponent<UIFocusable>();

            inviteButton.onSubmitSuccess = new UnityEngine.Events.UnityEvent();
            inviteButton.onSubmitSuccess.AddListener(delegate
            {
                var networkPanel = uiParent.Find("Panel_NetworkInvite").GetComponent<UITransitioner>();
                networkPanel.Transition_In_From_Top();

                if (!NetworkSystem.IsInSession)
                {
                    return;
                }

                if (NetworkSystem.IsHost)
                {
                    Debug.Log("PLAYER COUNT: " + playerCount);

                    if (playerCount > 1)
                    {
                        networkPanel.Transition_Out_To_Top();
                        GameObject.Find("World Selector").GetComponent<UIFocusable>().Focus();
                        NetworkSystem.InviteViaServiceOverlay();
                    }
                }
            });

            var networkInvite = uiParent.Find("Panel_NetworkInvite").GetComponent<SBM.UI.MainMenu.StoryMode.UIStoryNetworkInvite>();

            networkInvite.onInviteSuccess = new UnityEngine.Events.UnityEvent();
            networkInvite.onInviteSuccess.AddListener(delegate
            {
                playerCount += 1;

                networkInvite.GetComponent<UITransitioner>().Transition_Out_To_Top();

                var networkOnlineUI = uiParent.Find("UI_Bars/UI_Bar_Bottom/Networking/Network_Online").GetComponent<UITransitioner>();

                if (playerCount <= 2)
                {
                    networkOnlineUI.Transition_In_From_Bottom();
                    networkOnlineUI.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>().PlayerNumber = playerCount;
                }
                else
                {
                    // duplicate Network_Online UI
                    var networkGO = GameObject.Instantiate(networkOnlineUI);
                    networkGO.transform.position = new Vector3(networkOnlineUI.transform.position.x + (playerCount * 5), networkOnlineUI.transform.position.y);
                    networkGO.transform.Find("RemotePlayerIcon").GetComponent<SBM.UI.Components.PlayerIcon.UIRemotePlayerIcon>().PlayerNumber = playerCount;
                }

                GameObject.FindObjectOfType<SBM.UI.MainMenu.StoryMode.UIWorldSelector>().GetComponent<UIFocusable>().Focus();
                GameObject.FindObjectOfType<SBM.UI.Components.UIPlayerRoster>().ConfigureCoopPlayersForNetworkPlay();
            });
        }

        // loop through ALL players instead of just first 2 for coop
        [HarmonyPatch(typeof(SBM.UI.Components.UIPlayerRoster), "ConfigureCoopPlayersForNetworkPlay")]
        [HarmonyPrefix]
        static bool OverrideCoopPlayerProfileCount()
        {
            if (NetworkSystem.IsHost)
            {
                var localProfile = SBM.Shared.PlayerRoster.GetProfile(1);
                NetworkUserId localUserId = NetworkSystem.LocalUserId;
                string localUsername = NetworkSystem.LocalUsername;

                localProfile.Overwrite(0, 0, SBM.Shared.Team.Red, localUserId, true, localUsername);

                for (int i = 2; i < SBM.Shared.PlayerRoster.profiles.Count; i++)
                {
                   var remoteProfile = SBM.Shared.PlayerRoster.GetProfile(i);

                    NetworkUserId remoteUserId = NetworkSystem.GetRemoteUserId(i - 2); // i - 2, since this should start at 0 (first remote user)
                    string remoteUsername = NetworkSystem.GetUsername(remoteUserId);

                    // maybe change 0 to i - 2?
                    remoteProfile.Overwrite(0, 0, SBM.Shared.Team.Red, remoteUserId, false, remoteUsername);
                }
            }

            return false;
        }

        // debug
        /*[HarmonyPatch(typeof(SBM.UI.Components.UIPlayerRoster), "SetupCoopPlayers")]
        [HarmonyPostfix]
        static void LogLocalPlayerRoster()
        {
            SBM.Shared.PlayerRoster.RegisterLocalPlayer(3, 2);
            Debug.Log("[Registered Player] Count: " + SBM.Shared.PlayerRoster.LocalPlayerCount + ", Max: " + SBM.Shared.PlayerRoster.MaxPlayers);
        }*/

        // ensure that gamemanager properly spawns all players, regardless of actual player spawn count
        [HarmonyPatch(typeof(SBM.Shared.GameManager), "RespawnAllPlayers")]
        [HarmonyPrefix]
        static bool OverridePlayerRespawn(SBM.Shared.GameManager __instance)
        {
            Debug.Log("Test 1");

            for (int i = 0; i < SBM.Shared.Player.Count; i++)
            {
                SBM.Shared.Player.GetByIndex(i).SetVisible(false);
            }

            Debug.Log("Test 2");

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

                Debug.Log($"Test {3 + j}");

                if (byNumber != null)
                {
                    if (j >= __instance.spawnPoints.Count)
                    {
                        Debug.Log($"Nested Test 3, {j + 1}");
                        Vector3 lastSpawnPoint = __instance.spawnPoints.Last();

                        byNumber.SpawnPoint = new Vector3(lastSpawnPoint.x + j, lastSpawnPoint.y, lastSpawnPoint.z);
                        Debug.Log($"Nested Test 3, {j + 2}");
                        byNumber.Respawn();
                        Debug.Log($"Nested Test 3, {j + 3}");

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

            //Debug.Log("Current Scene Index: " + SBM.Shared.SceneSystem.CurrentScene.buildIndex);
            //Debug.Log("Player Scene Index: " + (int)playersData.SceneIndex);

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

            public override void Serialize(ISerializer ser)
            {
                ser.Write(world);
                ser.Write(level);
            }

            public override void Deserialize(IDeserializer deser)
            {
                this.world = deser.Read<ulong>();
                this.level = deser.Read<ushort>();
            }
        }
    }
}
