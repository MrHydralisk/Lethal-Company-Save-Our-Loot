using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace SaveOurLoot
{
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony gObject = new Harmony("LethalCompany.MrHydralisk.SaveOurLoot");
            gObject.Patch(AccessTools.Method(typeof(RoundManager), "DespawnPropsAtEndOfRound", (Type[])null, (Type[])null), transpiler: new HarmonyMethod(patchType, "RM_DespawnPropsAtEndOfRound_Transpiler", (Type[])null));
        }

        public static IEnumerable<CodeInstruction> RM_DespawnPropsAtEndOfRound_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int startIndex = -1;
            int endIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_0) //Ldc_I4_0 Stloc_9
                {
                    startIndex = i;
                    for (int j = startIndex + 1; j < codes.Count; j++)
                    {
                        if (codes[j].Is(OpCodes.Ldstr, "TemporaryEffect"))
                        {
                            endIndex = j;
                            break;
                        }
                    }
                    if (endIndex > -1)
                    {
                        break;
                    }
                }
            }
            if (startIndex > -1 && endIndex > -1)
            {
                Label labelSkip = il.DefineLabel();
                codes[endIndex].labels.Add(labelSkip);
                List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "CustomDespawnProps")));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, labelSkip));
                codes.InsertRange(startIndex + 1, instructionsToInsert);
            }
            return codes.AsEnumerable();
        }

        public static bool CustomDespawnProps(RoundManager rManager, GrabbableObject[] gObjects, bool despawnAllItems = false)
        {
            if (despawnAllItems)
            {
                return false;
            }
            System.Random RNG = new System.Random(StartOfRound.Instance.randomMapSeed + 369);
            List<GrabbableObject> gObjectsAll = gObjects.ToList();
            List<GrabbableObject> gObjectsInside = new List<GrabbableObject>();
            try
            {
                VehicleController[] array2 = UnityEngine.Object.FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
                for (int i = 0; i < array2.Length; i++)
                {
                    VehicleController vehicleController = array2[i];
                    if (!vehicleController.magnetedToShip)
                    {
                        if (vehicleController.NetworkObject != null)
                        {
                            Debug.Log("Despawn vehicle");
                            vehicleController.NetworkObject.Despawn(destroy: false);
                        }
                    }
                    else
                    {
                        vehicleController.CollectItemsInTruck();
                    }
                }
            }
            catch (Exception arg)
            {
                Debug.LogError($"Error despawning vehicle: {arg}");
            }
            BeltBagItem[] array3 = UnityEngine.Object.FindObjectsByType<BeltBagItem>(FindObjectsSortMode.None);
            for (int i = 0; i < array3.Length; i++)
            {
                BeltBagItem beltBagItem = array3[i];
                if ((bool)beltBagItem.insideAnotherBeltBag && (beltBagItem.insideAnotherBeltBag.isInShipRoom || beltBagItem.insideAnotherBeltBag.isHeld))
                {
                    beltBagItem.isInElevator = true;
                    beltBagItem.isInShipRoom = true;
                }
                if (beltBagItem.isInShipRoom || beltBagItem.isHeld)
                {
                    for (int j = 0; j < beltBagItem.objectsInBag.Count; j++)
                    {
                        beltBagItem.objectsInBag[j].isInElevator = true;
                        beltBagItem.objectsInBag[j].isInShipRoom = true;
                    }
                }
            }



            foreach (GrabbableObject gObject in gObjects)
            {
                if (gObject == null)
                {
                    continue;
                }
                if (!(gObject.isInShipRoom || gObject.isHeld) || gObject.deactivated)
                {
                    Plugin.MLogS.LogInfo($"{gObject.name} Lost Outside");
                    DespawnItem(gObject);
                }
                else
                {
                    gObjectsInside.Add(gObject);
                }
            }

            var result = gObjectsInside.ToLookup((GrabbableObject go) => go.itemProperties.isScrap);
            List<GrabbableObject> gObjectsScrap = result[true].ToList();
            List<GrabbableObject> gObjectsEquipment = result[false].ToList();

            if ((Config.hoardingBugInfestationEnabled?.Value ?? false) && (RNG.NextDouble() >= (1f - (Config.hoardingBugInfestationChance?.Value ?? 1f))))
            {
                List<string> hoardingBugInfestationLostItems = new List<string>();
                if (Config.hoardingBugInfestationValueLossEnabled?.Value ?? false)
                {
                    gObjectsScrap = gObjectsScrap.OrderByDescending((GrabbableObject go) => go.scrapValue).ToList();
                    float lossScrap = gObjectsScrap.Sum((GrabbableObject go) => go.scrapValue) * (Config.hoardingBugInfestationValueLossPercent?.Value ?? 0.1f);
                    int stolenScrap = 0;
                    foreach (GrabbableObject gObject in gObjectsScrap)
                    {
                        stolenScrap += gObject.scrapValue;
                        Plugin.MLogS.LogInfo($"{gObject.name} Lost Value Mafia {gObject.scrapValue}");
                        hoardingBugInfestationLostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                        DespawnItem(gObject);
                        if (stolenScrap >= lossScrap)
                        {
                            Plugin.MLogS.LogInfo($"{stolenScrap} Scrap Value Lost Mafia");
                            break;
                        }
                    }
                }
                else
                {
                    int lostSCount = 0;
                    foreach (GrabbableObject gObject in gObjectsScrap)
                    {
                        if (RNG.NextDouble() >= (1f - (Config.hoardingBugInfestationLossEachChance?.Value ?? 0.1f)))
                        {
                            Plugin.MLogS.LogInfo($"{gObject.name} Lost Mafia");
                            hoardingBugInfestationLostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                            DespawnItem(gObject);
                            lostSCount++;
                            if (lostSCount >= (Config.hoardingBugInfestationLossMax?.Value ?? int.MaxValue))
                            {
                                Plugin.MLogS.LogInfo($"Lost Mafia total {lostSCount}");
                                break;
                            }
                        }
                    }
                }
                if (Config.hoardingBugInfestationEquipmentLossEnabled?.Value ?? false)
                {
                    int lostECount = 0;
                    foreach (GrabbableObject gObject in gObjectsEquipment)
                    {
                        if (RNG.NextDouble() >= (1f - (Config.hoardingBugInfestationEquipmentLossChance?.Value ?? 0.05f)))
                        {
                            Plugin.MLogS.LogInfo($"{gObject.name} Equipment Lost Mafia");
                            hoardingBugInfestationLostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                            DespawnItem(gObject);
                            lostECount++;
                            if (lostECount >= (Config.hoardingBugInfestationEquipmentLossMax?.Value ?? int.MaxValue))
                            {
                                Plugin.MLogS.LogInfo($"Equipment Lost Mafia total {lostECount}");
                                break;
                            }
                        }
                    }
                }
                if (hoardingBugInfestationLostItems.Count() > 0)
                {
                    string msg = $"Lost to Bug Mafia ({hoardingBugInfestationLostItems.Count()}/{gObjectsInside.Count()}): ";
                    msg += string.Join("; ", hoardingBugInfestationLostItems.GroupBy(s => s).Select(s => new { name = s.Key, count = s.Count() }).Select(item => item.count > 1 ? $"{item.name} x{item.count}" : item.name));
                    HUDManager.Instance.StartCoroutine(DisplayAlert("Bug Mafia", "Space ain't no picnic, see? We kept you safe out there, so a little somethin' for our troubles, understand?", msg));
                }
            }
            if (StartOfRound.Instance.allPlayersDead)
            {
                if (RNG.NextDouble() >= (1f - (Config.saveAllChance?.Value ?? 0.25f)))
                {
                    Plugin.MLogS.LogInfo("All Saved");
                    HUDManager.Instance.StartCoroutine(DisplayAlert(bodyAlertText: "You got lucky. All items was saved.", messageText: "You got lucky. All items was saved."));
                }
                else
                {
                    gObjectsScrap.RemoveAll((GrabbableObject go) => !go.IsSpawned);
                    List<string> LostItems = new List<string>();
                    if (Config.valueSaveEnabled?.Value ?? false)
                    {
                        gObjectsScrap = gObjectsScrap.OrderByDescending((GrabbableObject go) => go.scrapValue).ToList();
                        int totalScrap = gObjectsScrap.Sum((GrabbableObject go) => go.scrapValue);
                        float saveScrap = totalScrap * (Config.valueSavePercent?.Value ?? 0.25f);
                        foreach (GrabbableObject gObject in gObjectsScrap)
                        {
                            totalScrap -= gObject.scrapValue;
                            Plugin.MLogS.LogInfo($"{gObject.name} Lost Value {gObject.scrapValue}");
                            LostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                            DespawnItem(gObject);
                            if (totalScrap < saveScrap)
                            {
                                Plugin.MLogS.LogInfo($"{totalScrap} Scrap Value Saved");
                                break;
                            }
                        }
                    }
                    else
                    {
                        int lostSCount = 0;
                        foreach (GrabbableObject gObject in gObjectsScrap)
                        {
                            if (RNG.NextDouble() >= (1f - (Config.saveEachChance?.Value ?? 0.5f)))
                            {
                                Plugin.MLogS.LogInfo($"{gObject.name} Saved");
                            }
                            else
                            {
                                Plugin.MLogS.LogInfo($"{gObject.name} Lost");
                                LostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                                DespawnItem(gObject);
                                lostSCount++;
                                if (lostSCount >= (Config.scrapLossMax?.Value ?? int.MaxValue))
                                {
                                    Plugin.MLogS.LogInfo($"Lost total {lostSCount}");
                                    break;
                                }
                            }
                        }
                    }
                    if (Config.equipmentLossEnabled?.Value ?? false)
                    {
                        gObjectsEquipment.RemoveAll((GrabbableObject go) => !go.IsSpawned);
                        int lostECount = 0;
                        foreach (GrabbableObject gObject in gObjectsEquipment)
                        {
                            if (RNG.NextDouble() >= (1f - (Config.equipmentLossChance?.Value ?? 0.1f)))
                            {
                                Plugin.MLogS.LogInfo($"{gObject.name} Equipment Lost");
                                LostItems.Add(gObject.itemProperties?.itemName ?? gObject.name);
                                DespawnItem(gObject);
                                lostECount++;
                                if (lostECount >= (Config.equipmentLossMax?.Value ?? int.MaxValue))
                                {
                                    Plugin.MLogS.LogInfo($"Equipment Lost total {lostECount}");
                                    break;
                                }
                            }
                        }
                    }
                    if (LostItems.Count() > 0)
                    {
                        string msg = $"Lost items ({LostItems.Count()}/{gObjectsInside.Count()}): ";
                        msg += string.Join("; ", LostItems.GroupBy(s => s).Select(s => new { name = s.Key, count = s.Count() }).Select(item => item.count > 1 ? $"{item.name} x{item.count}" : item.name));
                        HUDManager.Instance.StartCoroutine(DisplayAlert(bodyAlertText: $"Some of your loot was lost.", messageText: msg));
                    }
                }
            }
            return true;

            void DespawnItem(GrabbableObject gObject)
            {
                if (gObject.isHeld && gObject.playerHeldBy != null)
                {
                    gObject.playerHeldBy.DropAllHeldItems();
                }
                gObject.gameObject.GetComponent<NetworkObject>().Despawn(true);
                if (rManager.spawnedSyncedObjects.Contains(gObject.gameObject))
                {
                    rManager.spawnedSyncedObjects.Remove(gObject.gameObject);
                }
            }

            IEnumerator DisplayAlert(string headerAlertText = "Save Our Loot", string bodyAlertText = "", string messageText = "")
            {
                int index = 0;
                while (index < 20)
                {
                    if (StartOfRound.Instance.inShipPhase)
                    {
                        break;
                    }
                    index++;
                    yield return new WaitForSeconds(5f);
                }
                yield return new WaitForSeconds(2f);
                if (!(string.IsNullOrEmpty(headerAlertText) && string.IsNullOrEmpty(bodyAlertText)))
                {
                    HUDManager.Instance.DisplayTip(headerAlertText, bodyAlertText);
                }
                if (!string.IsNullOrEmpty(messageText))
                {
                    HUDManager.Instance.AddTextToChatOnServer(messageText);
                }
            }
        }
    }
}
