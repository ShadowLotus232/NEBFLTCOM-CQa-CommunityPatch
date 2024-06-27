using UnityEngine;
using Modding;
using HarmonyLib;

namespace CQaCP {
  public class ModEntryPoint : IModEntryPoint {

    public static string version;

    public void PreLoad() {
      ModEntryPoint.version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public void PostLoad() {
      Harmony harmony = new Harmony("nebulous.CQa-CommunityPatch");
      harmony.PatchAll();

      Debug.Log($"[CQaCP] Conquest alpha Community Patch {ModEntryPoint.version} loaded!");
    }
  }
}
