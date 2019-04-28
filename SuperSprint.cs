using System;
using System.IO;
using System.Reflection;
using BepInEx;
using Harmony;
using UnityEngine;
using static CustomKeybindings;
using static BlockingDamageAdjuster.Logger;
using static SuperSprint.SuperSprint;

namespace SuperSprint
{
    [BepInPlugin("com.gnivler.SuperSprint", "SuperSprint", "1.1")]
    public class SuperSprint : BaseUnityPlugin
    {
        internal static Settings modSettings = new Settings();

        public class Settings
        {
            public float mediumSpeed = 1;
            public float fastSpeed = 1;
            public bool autoRun = false;
            public bool enableDebug = false;
        }

        public void Awake()
        {
            var harmony = HarmonyInstance.Create("com.gnivler.SuperSprint");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            AddAction(
                "SuperSprint Medium",
                KeybindingsCategory.Actions,
                ControlType.Both, 4);
            AddAction(
                "SuperSprint Fast",
                KeybindingsCategory.Actions,
                ControlType.Both, 4);

            try
            {
                using (StreamReader reader = new StreamReader(
                    Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName +
                    "\\SuperSprint.json"))
                {
                    var json = reader.ReadToEnd();
                    modSettings = JsonUtility.FromJson<Settings>(json);
                }
            }
            catch (Exception e)
            {
                Error(e);
            }

            LogDebug($"{DateTime.Now.ToShortTimeString()} SuperSprint Starting up");
        }
    }

    public static class Patches
    {
        private static bool autoRun;

        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateMovement", MethodType.Normal)]
        public class AutoRun
        {
            public static void Postfix(bool ___m_autoRun)
            {
                if (!modSettings.autoRun) return;
                autoRun = ___m_autoRun;
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "MovementSpeed", MethodType.Getter)]
        public class MovementSpeed
        {
            public static void Postfix(CharacterStats __instance, ref float __result)
            {
                if (__instance.m_character.Faction != Character.Factions.Player) return;
                if (modSettings.autoRun && autoRun)
                {
                    __result = modSettings.mediumSpeed;
                }

                var playerID = __instance.m_character.OwnerPlayerSys.PlayerID;
                if (m_playerInputManager[playerID].GetButton("SuperSprint Medium"))
                {
                    __result = modSettings.mediumSpeed;
                }

                if (m_playerInputManager[playerID].GetButton("SuperSprint Fast"))
                {
                    __result = modSettings.fastSpeed;
                }
            }
        }
    }
}