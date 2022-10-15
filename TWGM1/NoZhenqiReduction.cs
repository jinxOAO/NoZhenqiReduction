using System;
using TaiwuModdingLib.Core.Plugin;
using HarmonyLib;
using GameData.Domains.Mod;
using GameData.Domains.Combat;
using GameData.Domains.Character;
using GameData.Common;
using GameData.Utilities;
using GameData.Domains;

namespace NoZhenqiReduction
{
    [PluginConfig("NoZhenqiReduction","GniMaerd","1.0.0")]
    public class NoZhenqiReduction : TaiwuRemakePlugin
    {
        public static int allyMode = 2;
        public static int enemyMode = 0;
        Harmony harmony;
        public override void Dispose()
        {
            if (harmony != null)
            { 
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(NoZhenqiReduction));
        }

        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(base.ModIdStr, "AllyMode", ref allyMode);
            AdaptableLog.Info("Ally zhenqi mode = " + allyMode.ToString());

            DomainManager.Mod.GetSetting(base.ModIdStr, "EnemyMode", ref enemyMode);
            AdaptableLog.Info("Enemy zhenqi mode = " + enemyMode.ToString());

        }

        //[HarmonyPostfix, HarmonyPatch(typeof(CombatCharacter), "ChangeNeiliAllocation")]
        //public unsafe static void ZhenqiSetPostPatch(ref CombatCharacter __instance, DataContext context, byte type, int addValue, bool applySpecialEffect)
        //{
        //    if (!__instance.IsAlly)
        //        return;
        //    Traverse tr = Traverse.Create(__instance);
        //    NeiliAllocation nl = tr.Field("_neiliAllocation").GetValue<NeiliAllocation>();
        //    short* ptr = nl.Items;
        //    int zq1 = *ptr;
        //    int zq2 = *(ptr + 1);
        //    int zq3 = *(ptr + 2);
        //    int zq4 = *(ptr + 4);
        //    int zq5 = *(ptr + 16);
        //    int ipsize = IntPtr.Size;

        //    AdaptableLog.Info(zq1.ToString() + " // " + zq2.ToString() + " // " + zq3.ToString() + " // " + zq4.ToString() + " // " + zq5.ToString() + " IntPtrSize = " + ipsize.ToString());
        //    if (addValue < 0 && __instance.IsAlly)
        //    {
        //        __instance.ChangeNeiliAllocation(context, type, -addValue, applySpecialEffect);
        //    }
        //}

        [HarmonyPrefix, HarmonyPatch(typeof(CombatCharacterStateBase), "TimeUpdate")]
        public unsafe static bool ZhenqiReduceCompesation(ref CombatCharacter combatChar)
        {
            // 判断角色是友军还是敌人决定真气自动回正策略
            int mode = enemyMode;
            if (combatChar.IsAlly)
            {
                mode = allyMode;
            }

            if (mode > 0)
            {
                mode = mode - 2; // 变为-1 0 1代表仅衰减、禁止变化、仅恢复

                NeiliAllocation cur = combatChar.GetNeiliAllocation();
                NeiliAllocation ori = combatChar.GetOriginNeiliAllocation();
                short* curPtr = cur.Items;
                short* oriPtr = ori.Items;
                // 计算当前真气与原始真气差
                int diff0 = *curPtr - *oriPtr;
                int diff1 = *(curPtr + 1) - *(oriPtr + 1);
                int diff2 = *(curPtr + 2) - *(oriPtr + 2);
                int diff3 = *(curPtr + 3) - *(oriPtr + 3);

                if (diff0 * mode <= 0)
                    combatChar.NeiliAllocationAutoRecoverProgress[0] = 0;
                if (diff1 * mode <= 0)
                    combatChar.NeiliAllocationAutoRecoverProgress[1] = 0;
                if (diff2 * mode <= 0)
                    combatChar.NeiliAllocationAutoRecoverProgress[2] = 0;
                if (diff3 * mode <= 0)
                    combatChar.NeiliAllocationAutoRecoverProgress[3] = 0;
            }
            return true;
        }

    }
}
