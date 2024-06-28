using HarmonyLib;
using System.Reflection;
using System.Linq;
using System.Collections;
using UnityEngine;
using Conquest;

using Conquest.Crew;
using Conquest.Persistence.Model;
using Conquest.Units;
using System;
using System.Collections.Generic;
using Utility;

//  TARGET: Issue 1
//  PURPOSE: Limited - Officers with null assignments will be sent to other friendly locations. No capture mechanics.

namespace CQaCP {
  [HarmonyPatch]
  class Patch_SkirmishBattleAftermathSynchronizer_CoroutineCompileBattleData {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
      //Debug.Log("[CQaCP] SkirmishBattleAftermathSynchronizer.CoroutineCompileBattleData patch");
      //foreach(var mb in AccessTools.GetDeclaredMethods(typeof(SkirmishBattleAftermathSynchronizer))) { Debug.Log(mb); } //DEBUG
      return AccessTools.GetDeclaredMethods(typeof(SkirmishBattleAftermathSynchronizer)).Where(method => method.Name.ToLower().Contains("coroutinecompilebattledata")).Cast<MethodBase>().First();
    }

    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(SkirmishBattleAftermathSynchronizer), "CoroutineCompileBattleData")]
    static bool SummaryExecution() {
      Debug.LogError("prefix call");
      return true;
    }*/

    [HarmonyPostfix]
    //[HarmonyPatch(typeof(SkirmishBattleAftermathSynchronizer), "CoroutineCompileBattleData")]
    static IEnumerator CoroutineWrapper(IEnumerator __result,
      SkirmishReturnToConquestData data,
      IReadOnlyList<ConquestTeam> teams)
    {
      // Run original enumerator code
      while (__result.MoveNext()) //NOTE: if prefix skips coroutine, __result == null
        yield return __result.Current;

      //postfix
      foreach (var team in teams) {
        TeamNavy navy = team.Navy;
        if (navy == null) { continue; } //Team.None lmao

        foreach (IGrouping<IOfficerAssignmentLocation, Officer> grouping in Enumerable.GroupBy<Officer, IOfficerAssignmentLocation>(navy.AllOfficers, (Func<Officer, IOfficerAssignmentLocation>) (x => x.AssignedLocation))) {
          if (grouping.Key != null) { continue; }
          ReassignOfficers(navy, grouping);
        }
      }
    }


    internal static void ReassignOfficers(TeamNavy navy, IGrouping<IOfficerAssignmentLocation, Officer> grouping) {
      if (navy == null) { return; }

      bool nowhereToGo = navy.AllStations.Count() == 0; //NOTE: not including planets, I don't know if that works. I know officers on stations work though

      foreach (var orphanedOfficer in grouping.ToList<Officer>())
      {
        if (orphanedOfficer.Network_fate == OfficerRecord.OfficerFate.Killed) { continue; } //shouldn't happen but just in case

        if (nowhereToGo) {
          //unlikely but technically possible
          orphanedOfficer.Network_assignedRole = OfficerBillet.Generic;
          orphanedOfficer.Network_fate = OfficerRecord.OfficerFate.Killed;
          Debug.Log($"[CQaCP] {orphanedOfficer.FirstName} {orphanedOfficer.LastName} had an invalid location and nowhere to go. They have been executed.");
        } else {
          //send officer to random friendly station
          navy.AllStations.SelectRandom().OfficerPool.AssignOfficerTo(orphanedOfficer, OfficerBillet.Generic);
          Debug.Log($"[CQaCP] {orphanedOfficer.FirstName} {orphanedOfficer.LastName} had an invalid location. They have been sent to {orphanedOfficer.AssignedLocation.LongName}");
        }
      }
    }
  }
}
