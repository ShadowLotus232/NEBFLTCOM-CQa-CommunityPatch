using HarmonyLib;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Mirror;

using Conquest.Crew;
using Conquest.Units;

using System;
using System.Collections.Generic;

//  TARGET: Issue 1
//  PURPOSE: Allows loading of corrupted saves, just in case.

namespace CQaCP {
  [HarmonyPatch]
  class Patch_TeamNavyLoadAllOfficerAssignments {

    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
      //Debug.Log("[CQaCP] TeamNavy.LoadAllOfficerAssignments patch");
      //foreach(var mb in AccessTools.GetDeclaredMethods(typeof(TeamNavy))) { Debug.Log(mb); } //DEBUG
      return AccessTools.GetDeclaredMethods(typeof(TeamNavy)).Where(method => method.Name.ToLower().Contains("loadallofficerassignments")).Cast<MethodBase>().First();
    }

    [HarmonyPrefix]
    static bool RelocateOrExecute(TeamNavy __instance) {
      if (!NetworkServer.active)
      {
        Debug.LogWarning((object) "[CQaCP] [Server] function 'System.Void Conquest.Units.TeamNavy::LoadAllOfficerAssignments()' called when server was not active");
      }
      else
      {
        //ORIGINAL METHOD CODE
        //foreach (IGrouping<IOfficerAssignmentLocation, Officer> grouping in Enumerable.GroupBy<Officer, IOfficerAssignmentLocation>(this.AllOfficers, (Func<Officer, IOfficerAssignmentLocation>) (x => x.AssignedLocation)))
        //  grouping.Key.SetAllOfficers((IEnumerable<Officer>) grouping);

        var groupList = Enumerable.GroupBy<Officer, IOfficerAssignmentLocation>(__instance.AllOfficers, (Func<Officer, IOfficerAssignmentLocation>) (x => x.AssignedLocation));

        foreach (IGrouping<IOfficerAssignmentLocation, Officer> grouping in groupList) {
          if (grouping.Key == null) { Patch_SkirmishBattleAftermathSynchronizer_CoroutineCompileBattleData.ReassignOfficers(__instance, grouping); }
        }
        //regenerate groupings
        groupList = Enumerable.GroupBy<Officer, IOfficerAssignmentLocation>(__instance.AllOfficers, (Func<Officer, IOfficerAssignmentLocation>) (x => x.AssignedLocation));

        foreach (IGrouping<IOfficerAssignmentLocation, Officer> grouping in groupList) {
          if (grouping.Key == null) { continue; } //Executed officers may be included here, so still ignore nulls
          grouping.Key.SetAllOfficers((IEnumerable<Officer>) grouping);
        }
      }
      return false;
    }
  }
}