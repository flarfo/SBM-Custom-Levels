﻿using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Emit;
using UnityEngine.UI;
using SceneSystem = SBM.Shared.SceneSystem;
using SplineMesh;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class Patches
    {
		private static IEnumerator WaitToEnableCollider(int frameCount, MeshCollider collider)
		{
			for (int i = 0; i < frameCount; i++)
			{
				yield return null;
			}

			collider.enabled = true;
		}

        [HarmonyPatch(typeof(SBM.Shared.Audio.AudioSystem), "OnSceneEvent")]
        [HarmonyPostfix]
		static void FixMenuVolume(SBM.Shared.SceneEvent sceneEvent, Scene scene)
        {
			if (sceneEvent == SBM.Shared.SceneEvent.LoadComplete && scene.name == "Menu")
            {
				SBM.Shared.Audio.AudioSystem.instance.musicMain.volume = 1f;
            }
        }

        [HarmonyPatch(typeof(SBM.UI.Game.StoryMode.UIStoryTimeDetails), "GetSavedTimeRecord")]
        [HarmonyPrefix]
		static bool StopGetRecord()
        {
			if (LevelManager.InLevel || EditorManager.InEditor)
            {
				return false;
            }

			return true;
        }

		[HarmonyPatch(typeof(SceneSystem), "SetActiveScene")]
        [HarmonyPrefix]
        static void ExitEditor(string name)
        {
            if (name == "Menu")
            {
				EditorManager.InEditor = false;
				LevelManager.InLevel = false;
			}
        }

		// meshcollider created in code by SplineMesh has no collision, by disabling and re-enabling the meshcollider, collision returns.
        [HarmonyPatch(typeof(SplineMeshTiling), "CreateMeshes")]
        [HarmonyPostfix]
		static void StupidMeshCollisionFix(SplineMeshTiling __instance)
        {
			MeshCollider generatedCollider = __instance.GetComponentInChildren<MeshCollider>();
			MeshCollider newCollider = __instance.gameObject.AddComponent<MeshCollider>();

			newCollider.enabled = false;

			newCollider.sharedMesh = generatedCollider.sharedMesh;
			newCollider.gameObject.AddComponent<SBM.Objects.World3.Minecart.MinecartRail>();

			LevelManager.instance.StartCoroutine(WaitToEnableCollider(1, newCollider));
		}

		[HarmonyPatch(typeof(SBM.Shared.Utilities.Water.Water), "OnValidate")]
        [HarmonyPrefix]
        static void UpdateWaterMesh(SBM.Shared.Utilities.Water.Water __instance)
        {
			MeshRenderer component = __instance.gameObject.GetComponent<MeshRenderer>();
			SBM.Shared.Utilities.Water.WaterMesh waterMesh = new SBM.Shared.Utilities.Water.WaterMesh();
			if (component.sharedMaterials.Length < 2)
			{
				Material sharedMaterial = component.sharedMaterial;
				component.sharedMaterials = new Material[] { sharedMaterial, sharedMaterial };
			}
			Vector2 one = Vector2.one;
			Vector2 one2 = Vector2.one;
			if (component.sharedMaterial == null)
			{
				return;
			}
			if (component.sharedMaterial.mainTexture != null)
			{
				Material sharedMaterial2 = component.sharedMaterial;
				one.x = (float)sharedMaterial2.mainTexture.width / 100f;
				one.y = (float)sharedMaterial2.mainTexture.height / 100f;
			}
			else
			{
				one.x = 512f;
				one.y = 512f;
			}
			__instance.xSegments = (int)Mathf.Ceil(__instance.width * 4f);
			__instance.xSegmentWidth = __instance.width / (float)__instance.xSegments;
			int num = __instance.xSegments + 1;
			int num2 = 2;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					Vector3 vector;
					Vector3 vector2;
					if (i == 0)
					{
						vector = new Vector3((j < num - 1) ? ((float)j * __instance.xSegmentWidth) : __instance.width, 0f, -0.495f);
						vector2 = new Vector2((vector.x + __instance.gameObject.transform.position.x) / one.x, 1f - __instance.height / one.y);
					}
					else
					{
						vector = new Vector3((j < num - 1) ? ((float)j * __instance.xSegmentWidth) : __instance.width, __instance.height, -0.495f);
						vector2 = new Vector2((vector.x + __instance.gameObject.transform.position.x) / one.x, 1f);
					}
					waterMesh.AddVertex(vector, vector2);
				}
			}
			waterMesh.GenerateTriangles(__instance.xSegments, 1, num, true);
			int[] currentTriangleList = waterMesh.GetCurrentTriangleList(0);
			Material material = component.sharedMaterials[1];
			if (component.sharedMaterial.mainTexture != null)
			{
				one2.x = (float)material.mainTexture.width / 100f;
				one2.y = (float)material.mainTexture.height / 100f;
			}
			else
			{
				one2.x = 512f;
				one2.y = 512f;
			}
			for (int k = 0; k < num; k++)
			{
				Vector3 vector = new Vector3((k < num - 1) ? ((float)k * __instance.xSegmentWidth) : __instance.width, __instance.height, -0.495f);
				Vector3 vector2 = new Vector2((vector.x + __instance.gameObject.transform.position.x) / one2.x, 1f - 1f / one2.y);
				waterMesh.AddVertex(vector, vector2);
			}
			for (int l = 0; l < num; l++)
			{
				Vector3 vector = new Vector3((l < num - 1) ? ((float)l * __instance.xSegmentWidth) : __instance.width, __instance.height, 0.5f);
				Vector3 vector2 = new Vector2((vector.x + __instance.gameObject.transform.position.x) / one2.x, 1f);
				waterMesh.AddVertex(vector, vector2);
			}
			waterMesh.GenerateTriangles(__instance.xSegments, 1, num, false);
			int[] currentTriangleList2 = waterMesh.GetCurrentTriangleList(currentTriangleList.Length);
			__instance.waterMesh = new Mesh
			{
				name = __instance.gameObject.gameObject.name + __instance.gameObject.gameObject.GetInstanceID().ToString() + "-Mesh"
			};
			__instance.MeshFilter.sharedMesh = __instance.waterMesh;
			waterMesh.Build(ref __instance.waterMesh);
			__instance.waterMesh.subMeshCount = 2;
			__instance.waterMesh.SetTriangles(currentTriangleList, 0);
			__instance.waterMesh.SetTriangles(currentTriangleList2, 1);
			__instance.UpdateColliders();
        }

		[HarmonyPatch(typeof(SBM.Shared.Utilities.Water.Water), "Awake")]
		[HarmonyPrefix]
		static bool InitializeWaterValues(SBM.Shared.Utilities.Water.Water __instance)
        {
			if (__instance.GetComponent<MeshFilter>().mesh.vertices.Length != 0) //if normal level water (mesh vertices already set) return. else custom water (no mesh vertices)
			{
				return true;
			}

			__instance.height = LevelManager.instance.curWaterHeight;
			__instance.width = LevelManager.instance.curWaterWidth;

			__instance.GetComponent<MeshFilter>().mesh = EditorManager.defaultCube;
			__instance.OnValidate();

			LevelManager.instance.curWaterHeight = 0;
			LevelManager.instance.curWaterWidth = 0;

			return true;
		}

		[HarmonyPatch(typeof(SBM.UI.Game.StoryMode.UIStoryLevelName), "Start")]
		[HarmonyPrefix]
		static bool ChangeUILevelName(SBM.UI.Game.StoryMode.UIStoryLevelName __instance)
        {
			Text component = __instance.gameObject.GetComponent<Text>();

			if (EditorManager.InEditor)
            {
				component.text = "Editor";

				return false;
			}
			else if (LevelManager.InLevel)
            {
				component.text = "Level " + LevelManager.instance.levelNumber;

				return false;
			}

			return true;
		}

		//there should be no carrot by default, stop checking for carrot object (which might not exist)
		[HarmonyPatch(typeof(SBM.UI.Game.StoryMode.UICarrotEvent), "Awake")]
        [HarmonyPrefix]
		static bool FixEditorUICarrotEvent() 
        {
			return !EditorManager.InEditor;
        }

		[HarmonyPatch(typeof(SBM.Objects.GameModes.Story.GameManagerStory), "Awake")]
        [HarmonyTranspiler]
		static IEnumerable<CodeInstruction> FixEditorGameManager(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			var codes = new List<CodeInstruction>(instructions);

			int insertionIndex = -1;
			Label callFindCarrotLabel = il.DefineLabel();

			for (int i = 0; i < codes.Count - 1; i++) // -1 since checking i + 1
			{
				if (codes[i].opcode == OpCodes.Call && codes[i - 1].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Ldarg_0) // find point of insertion (before find carrot call)
				{
					insertionIndex = i;
					codes[i].labels.Add(callFindCarrotLabel);
					break;
				}
			}

			var insertInstructions = new List<CodeInstruction>();

			insertInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
			insertInstructions.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EditorManager), nameof(EditorManager.inEditor)))); // load inEditor
			insertInstructions.Add(new CodeInstruction(OpCodes.Brfalse_S, callFindCarrotLabel)); // check if inEditor is false, if false jump to label (skip ret)
			insertInstructions.Add(new CodeInstruction(OpCodes.Ret)); // if true return, avoid checking for a carrot that might not exist

			if (insertionIndex != -1)
			{
				codes.InsertRange(insertionIndex, insertInstructions);
			}

			return codes;
		}

		// if inLevel or inEditor, stop original method. if in neither, pass through.
		[HarmonyPatch(typeof(SBM.Shared.Save.StorySaveData), "GetTimeRecord")]
        [HarmonyPrefix]
		static bool StopGetTimeRecord()
        {
			return !LevelManager.InLevel && !EditorManager.InEditor; 
        }

		// dont unlock badges for custom levels (which don't have normal badges) /// NullReferenceException
        [HarmonyPatch(typeof(SBM.Objects.GameModes.Story.GameManagerStory), "OnRoundEnd")]
        [HarmonyPrefix]
		static bool StopBadgeUnlock(SBM.Objects.GameModes.Story.GameManagerStory __instance)
        {
			if (LevelManager.InLevel)
            {
				if (__instance.cCompleteLevel != null)
				{
					__instance.StopCoroutine(__instance.cCompleteLevel);
				}

				SBM.Shared.GameManager.Instance.timer.Stop();
				SBM.Shared.Player.SetAllInputEnabled(false);

				foreach (var cameraTarget in SBM.Shared.Cameras.CameraTarget.All)
				{
					cameraTarget.TrackingEnabled = false;
				}

				var wormhole = SBM.Objects.Common.Wormhole.Wormhole.Instance;
				wormhole.CameraTarget.TrackingEnabled = true;
				wormhole.Close();
			}

			return !LevelManager.InLevel;
        }

		// if a custom level is loaded, there will be no standard "next level," rather next level will be defined by logic stored in the mod.
		// prevent the game from trying to get this next level based on a nonexistant current level.
		// since LevelSystem.CurrentStoryLevel is null, a prefix must prevent that code from being run, returning stage 1 instead.
		[HarmonyPatch(typeof(SBM.Shared.Level.LevelSystem), nameof(SBM.Shared.Level.LevelSystem.NextStoryLevel), MethodType.Getter)]
        [HarmonyPrefix]
		static bool StopGetNextLevel(ref SBM.Shared.Level.StoryLevel __result)
        {
			if (LevelManager.InLevel)
            {
				__result = new SBM.Shared.Level.StoryLevel(1, 1, 1, 1);
				return false;
            }

			return true;
        }

        [HarmonyPatch(typeof(SBM.UI.Game.StoryMode.UIStoryNextLevelQuery), "GoToNextLevel")]
        [HarmonyPrefix]
		static bool GoToNextCustomLevel(SBM.UI.Game.StoryMode.UIStoryNextLevelQuery __instance)
        {
			if (LevelManager.InLevel)
            {
				string nextLevel = LevelManager.instance.GetNextLevel();

				Debug.Log("NEXT LEVEL: " + nextLevel);

				if (nextLevel != string.Empty)
                {
					// LevelManager.FadeOutCustomScene(Color.clear, Color.black, __instance.screenFader);
					LevelManager.instance.BeginLoadLevel(false, false, nextLevel, -1);
				}
                else // if no next level, return to menu
                {
					SceneSystem.LoadScene("Menu");
                }
				
				return false;
            }

			return true;	
        }
	}
}
