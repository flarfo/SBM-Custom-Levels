using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Catobyte.Networking;
using SBM.Shared.Networking;
using HarmonyLib;
using UnityEngine;

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
                Network.Session.SubscribeToReceive<CustomSceneData>(memberId,CustomChannel, new OnDataReceived(OnReceivedCustomLevelData), true);
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
                    Debug.Log("World not found! Potential mismatch between client and server?");
                    return;
                }

                LevelManager.instance.BeginLoadLevel(false, false, receivedWorld.levels[sceneData.level], sceneData.level); // add isNetworkClient ? 
                // send callback confirming load, THEN activate network ?
            }
        }

        #endregion

        #region Send

        // level sent by OnSceneStateDoesNotMatchRemoteUser (i think ?) which is called by OnRemoteUserSceneStateConflict

        // GameManagerStory needs to start round!
        // NetworkSystem.SceneIsSyncedWithRemoteUsers is FALSE! GameManager yields null until this is TRUE!

        public static void SendCustomLevelData(ulong world, int level)
        {
            // if (custom level to be loaded)

            CustomSceneData sceneData = new CustomSceneData();
            sceneData.world = world;
            sceneData.level = (ushort)level;

            Network.Session.SendToAll(CustomChannel, sceneData);
        }

        #endregion

        #region Patches
        
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
