using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using Harmony;
using UnityEngine;

namespace SuperSprint
{
    [BepInPlugin("com.gnivler.HuntersEyeHack", "HuntersEyeHack", "1.0.0")]
    public class SuperSprint : BaseUnityPlugin
    {
        public void Awake()
        {
           
            var harmony = HarmonyInstance.Create("com.gnivler.HuntersEyeHack");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public static class Patches
    {
        private static bool autoRun = false;
    
        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateMovement", MethodType.Normal)]
        public class AutoRun
        {
            public static void Postfix(bool ___m_autoRun)
            {
                autoRun = ___m_autoRun;
            }
        }
    
        [HarmonyPatch(typeof(CharacterStats), "SprintStaminaCost", MethodType.Getter)]
        public class SprintStaminaCost
        {
            public static void Postfix(ref float __result) => __result = __result * 0.1f;
        }
    
        [HarmonyPatch(typeof(CharacterStats), "MovementSpeed", MethodType.Getter)]
        public class MovementSpeed
        {
            public static void Postfix(ref float __result)
            {
                if (autoRun)
                {
                    __result *= 2.5f;
                    if (Input.GetKey(KeyCode.X))
                    {
                        __result *= 4f;
                    }
                }
    
                if (Input.GetKey(KeyCode.X))
                {
                    __result *= 2.5f;
                }
            }
        }
    
        [HarmonyPatch(typeof(CraftingMenu), "TryCraft", MethodType.Normal)]
        public class InstantCraft
        {
            public static void Prefix(CraftingMenu __instance, ref float ___CraftingTime)
            {
                ___CraftingTime = 0f;
            }
        }
    
    [HarmonyPatch(typeof(TargetingSystem), "TrueRange", MethodType.Getter)]
    public class HuntersEyeNoItem
    {
        public static void Postfix(TargetingSystem __instance, Character ___m_character, ref float __result)
        {
            if (___m_character != null)
            {
                __result = ___m_character.Inventory.SkillKnowledge.IsItemLearned(8205160)
                    ? __instance.HunterEyeRange
                    : __instance.LongRange;
            }
        }
    }
}