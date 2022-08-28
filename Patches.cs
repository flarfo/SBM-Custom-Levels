using HarmonyLib;
using UnityEngine;
using SceneSystem = SBM.Shared.SceneSystem;

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
                EditorManager.instance.InEditor = false;
            }
        }

        //create patch for LevelSystem.Worlds
    }
}
