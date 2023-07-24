using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine.UI;
using SceneSystem = SBM.Shared.SceneSystem;
using SplineMesh;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class Patches
    {
		// fix for PlayerRoster profiles not being added when Level Editor button was selected. Roster is only updated when 
		// the gamemode panel buttons are selected, so the game thinks there are no players to spawn in when a level is loaded.
		// This brings the UI back to the main menu after leaving the level editor, instead of to the world select.
        [HarmonyPatch(typeof(SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls), "LerpInRagdolls")]
        [HarmonyPrefix]
		static bool ReturnToMainMenuAfterLevelEditor()
        {
			if (LevelManager.lastLevelWasEditor)
            {
				GameObject.Find("Screen_StoryMode").SetActive(false);

				var thing = MenuManager.FindInactiveGameObject<SBM.UI.Utilities.Focus.UIFocusable>("Screen_MainMenu");
				thing.gameObject.SetActive(true);
				thing.gameObject.GetComponent<SBM.UI.Utilities.Transitioner.UITransitioner>().Transition_In_From_Center();

				LevelManager.lastLevelWasEditor = false;
				return false;
			}

			return true;
        }

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
				if (SBM.Shared.Audio.AudioSystem.instance)
                {
					SBM.Shared.Audio.AudioSystem.instance.musicSource.volume = 1f;
				}
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
				LevelManager.loading = false;
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

		// boulder spawners have .spawningEnabled set to false by default & the built-in method for enabling does not work for custom levels -- fixes
        [HarmonyPatch(typeof(SBM.Objects.World3.Boulder.BoulderSpawner), "OnGameManagerRoundStateSet")]
        [HarmonyPostfix]
		static void FixBoulderSpawning(SBM.Objects.World3.Boulder.BoulderSpawner __instance, SBM.Shared.RoundState state)
        {
			if (!LevelManager.InLevel)
            {
				return;
            }

			if (state == SBM.Shared.RoundState.Started)
            {
				__instance.spawningEnabled = true;
            }
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

			if (__instance.transform.root.TryGetComponent(out SBM.Objects.World5.WaterTank waterTank))
            {
				waterTank.tankSize = new Vector2(LevelManager.instance.curWaterWidth, LevelManager.instance.curWaterHeight);

				float num = waterTank.tankSize.x - waterTank.tankMargins.x;
				float num2 = waterTank.tankSize.y - waterTank.tankMargins.y;
				float num3 = 0.5f * waterTank.tankSize.x;
				float num4 = 0.5f * waterTank.tankSize.y;

				waterTank.tank.Size = new Vector3(num, num2, 1.15f);
				//waterTank.tank.transform.localPosition = new Vector3(num3, num4, 0f);
				waterTank.tank.GetComponent<MeshCollider>().sharedMesh = waterTank.tank.GetComponent<MeshFilter>().sharedMesh;

				__instance.width = num;
				__instance.height = num2 + waterTank.waterTopInset;

				//0.05f - (waterTank.tankSize.x - 3) * .5f
				//x-pos: at 4 width, should be -.45 -- at 2 width, should be .55 -- 3 width should be 0.05 -- 8.63 width, should be -2.75
				//y-pos: (0.5f * waterTank.tankMargins.y) - 0.5f * (waterTank.tankSize.y - 1)
				//derived from observing change in water position based on initial tank size,
				//then relating correct water position with the incorrect water position
				waterTank.water.transform.localPosition = new Vector3(0.05f - (waterTank.tankSize.x - 3) * .5f, (0.5f * waterTank.tankMargins.y) - 0.5f * (waterTank.tankSize.y - 1), 0f);
				waterTank.water.transform.localScale = new Vector3(1f, 1f, 1.05f);
			}
            else
            {
				__instance.height = LevelManager.instance.curWaterHeight;
				__instance.width = LevelManager.instance.curWaterWidth;
			}

			//__instance.GetComponent<MeshFilter>().mesh = EditorManager.defaultCube;
			__instance.OnValidate();

			LevelManager.instance.curWaterHeight = 0;
			LevelManager.instance.curWaterWidth = 0;

			return true;
		}

		// if last focused button in group exceeds number of custom levels (e.g. 10 levels in a default world, custom world only has 2 levels)
		// set focus to first level, so that a non-active button is not focused

        [HarmonyPatch(typeof(SBM.UI.MainMenu.StoryMode.UIStoryLevelButtons), "FocusLastPlayedLevelButtonOrDefault")]
        [HarmonyPrefix]
		static bool FocusFirstLevelIfCustom(SBM.UI.MainMenu.StoryMode.UIStoryLevelButtons __instance)
        {
			Debug.Log(__instance.worldIndex);
			if (__instance.worldIndex >= 5)
            {
				Debug.Log("Custom World! Focusing level 1.");

				var focusableGroup = __instance.GetComponent<SBM.UI.Utilities.Focus.UIFocusableGroup>();
				focusableGroup.OverrideIndexOfPrevGroupFocus(0);

				return false;
			}
            
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

		// normal SBM.Objects.GameModes.Story.GameManagerStory.Start() tries to access current level via LevelSystem.GetLevelBySceneName(),
		// however for custom levels no scene exists.
		[HarmonyPatch(typeof(SBM.Objects.GameModes.Story.GameManagerStory), "Start")]
        [HarmonyPrefix]
		static bool FixGameStartPatch(SBM.Objects.GameModes.Story.GameManagerStory __instance)
        {
			if (LevelManager.InLevel)
            {
				SBM.Shared.Cameras.TrackingCamera.ScheduleCenterOnTargets();

				SBM.Shared.WorldResetHandler.ScanForResettables();
				SBM.Shared.GameManager.Instance.ResetRound(false, 0f);

				return false;
			}

			return true;
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

		// load into next customlevel after completing level
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
					try
					{
						LevelManager.instance.BeginLoadLevel(false, false, nextLevel, -1);

						int levelNumber = ushort.MaxValue;

                        for (int i = 0; i < LevelManager.instance.currentWorld.levels.Count; i++)
                        {
							if (LevelManager.instance.currentWorld.levels[i] == nextLevel)
                            {
								levelNumber = i;
                            }
                        }

						MultiplayerManager.SendCustomLevelData(LevelManager.instance.currentWorld.WorldHash, levelNumber);
					}
                    catch
                    {
						SceneSystem.LoadScene("Menu");
					}
				}
                else // if no next level, return to menu
                {
					SceneSystem.LoadScene("Menu");
					MultiplayerManager.SendCustomLevelData(LevelManager.instance.currentWorld.WorldHash, ushort.MaxValue);
				}
				
				return false;
            }

			return true;	
        }
	}
}
