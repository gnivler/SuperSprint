using System;
using System.IO;
using System.Reflection;
using BepInEx;
using Harmony;
using UnityEngine;
using static SuperSprint.Logger;
using static SuperSprint.CustomKeybindings;

namespace SuperSprint
{
    [BepInPlugin("com.gnivler.SuperSprint.Outward", "SuperSprint", "1.32")]
    public class SuperSprint : BaseUnityPlugin
    {
        internal static Settings modSettings = new Settings();

        public class Settings
        {
            public float mediumSpeed = 1;
            public float fastSpeed = 1;
            public bool autoRun = true;
            public bool enableDebug = false;
        }

        public void Awake()
        {
            var harmony = HarmonyInstance.Create("com.gnivler.SuperSprint.Outward");
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

            Clear();
        }
    }

    public static class Patches
    {
        private static bool autoRun;

        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateMovement", MethodType.Normal)]
        public class AutoRun
        {
            public static void Postfix(LocalCharacterControl __instance, bool ___m_autoRun)
            {
                if (!SuperSprint.modSettings.autoRun ||
                    __instance.Character.Faction != Character.Factions.Player) return;
                autoRun = ___m_autoRun;
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "MovementSpeed", MethodType.Getter)]
        public class MovementSpeed
        {
            public static void Postfix(CharacterStats __instance, ref float __result)
            {
                var character = Traverse
                    .Create(__instance)
                    .Field("m_character")
                    .GetValue<Character>();
                if (character.Faction != Character.Factions.Player) return;
                if (SuperSprint.modSettings.autoRun && autoRun)
                {
                    __result = SuperSprint.modSettings.mediumSpeed;
                }

                var playerID = character.OwnerPlayerSys.PlayerID;
                if (m_playerInputManager[playerID].GetButton("SuperSprint Medium"))
                {
                    __result = SuperSprint.modSettings.mediumSpeed;
                }

                if (m_playerInputManager[playerID].GetButton("SuperSprint Fast"))
                {
                    __result = SuperSprint.modSettings.fastSpeed;
                }
            }
        }
    }
}