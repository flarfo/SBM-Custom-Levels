using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SceneSystem = SBM.Shared.SceneSystem;
using System.Linq;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class Patches
    {
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
	}
}
