using HarmonyLib;
using UnityEngine;
using SceneSystem = SBM.Shared.SceneSystem;
using SBM;

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
    }
}
