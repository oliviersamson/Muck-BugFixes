using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace BugFixes.ResourceGeneratorPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(ResourceGenerator), "GenerateForest")]
        [HarmonyPrefix]
        static bool GenerateForestPreFix(ref ResourceGenerator __instance, int ___density, ConsistentRandom ___randomGen, ref int ___totalResources)
        {
            int nextGenOffset;
            if (__instance.forceSeedOffset != -1)
            {
                nextGenOffset = __instance.forceSeedOffset;
            }
            else
            {
                nextGenOffset = ResourceManager.GetNextGenOffset();
            }
            float[,] array = __instance.mapGenerator.GeneratePerlinNoiseMap(GameManager.GetSeed());
            float[,] array2 = __instance.mapGenerator.GeneratePerlinNoiseMap(__instance.noiseData, GameManager.GetSeed() + nextGenOffset, __instance.useFalloffMap);
            __instance.width = array.GetLength(0);
            __instance.height = array.GetLength(1);
            float topLeftX = (float)(__instance.width - 1) / -2f;
            float topLeftZ = (float)(__instance.height - 1) / 2f;
            __instance.resources = new List<GameObject>[__instance.drawChunks.nChunks];
            for (int i = 0; i < __instance.resources.Length; i++)
            {
                __instance.resources[i] = new List<GameObject>();
            }
            int num = ___density;
            //int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            while (num3 < __instance.minSpawnAmount && num4 < 100)
            {
                num4++;
                for (int j = 0; j < __instance.height; j += num)
                {
                    for (int k = 0; k < __instance.width; k += num)
                    {
                        if (array[k, j] >= __instance.minSpawnHeight && array[k, j] <= __instance.maxSpawnHeight)
                        {
                            float num5 = array2[k, j];
                            if (num5 >= __instance.spawnThreshold)
                            {
                                num5 = (num5 - __instance.spawnThreshold) / (1f - __instance.spawnThreshold);
                                float num6 = __instance.noiseDistribution.Evaluate((float)___randomGen.NextDouble());
                                //num2++;
                                if (num6 > 1f - num5)
                                {
                                    int repeatCounter = 0;
                                    while (repeatCounter < 10)
                                    {
                                        Vector3 vector = new Vector3(topLeftX + (float)k, 100f, topLeftZ - (float)j) * __instance.worldScale;
                                        vector += new Vector3((float)___randomGen.Next(-__instance.randPos, __instance.randPos), 0f, (float)___randomGen.Next(-__instance.randPos, __instance.randPos));
                                        RaycastHit raycastHit;
                                        if (Physics.Raycast(vector, Vector3.down, out raycastHit, 1200f))
                                        {
                                            if (raycastHit.collider.name.Contains("Clone"))
                                            {
                                                repeatCounter++;
                                                continue;
                                            }

                                            vector.y = raycastHit.point.y;
                                            //num3++;
                                            int num7 = __instance.drawChunks.FindChunk(k, j);
                                            if (num7 >= __instance.drawChunks.nChunks)
                                            {
                                                num7 = __instance.drawChunks.nChunks - 1;
                                            }

                                            //__instance.resources[num7].Add(__instance.SpawnTree(vector));
                                            GameObject gameObject = (GameObject)AccessTools.Method(typeof(ResourceGenerator), "SpawnTree", new Type[] { typeof(Vector3) }).Invoke(__instance, new object[] { vector });

                                            if (gameObject != null)
                                            {
                                                num3++;
                                                __instance.resources[num7].Add(gameObject);
                                                break;
                                            }
                                            else
                                            {
                                                repeatCounter++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            ___totalResources = num3;
            __instance.drawChunks.InitChunks(__instance.resources);
            __instance.drawChunks.totalTrees = ___totalResources;

            return false;
        }

        [HarmonyPatch(typeof(ResourceGenerator), "SpawnTree")]
        [HarmonyPostfix]
        static void SpawnTreePostfix(ref GameObject __result, ResourceGenerator __instance, Vector3 pos)
        {
            // Set pos a bit on top of object
            Vector3 castPos = __result.transform.position + (__result.transform.forward * 108.7f);

            // Create box parameters with same size and oriantation as object
            Vector3 halfExtents = new();

            if (new List<string> { "Tree", "Birch", "Fir", "Oak" }.Any(__result.name.Contains))
            {
                halfExtents = __instance.GetTreeExtents(__result.name.Replace("(Clone)", ""));
            }
            else if (__result.CompareTag("Count") && !new List<string> { "Pickup", "Flint" }.Any(__result.name.Contains))
            {                
                halfExtents = __result.GetComponent<MeshRenderer>().bounds.extents;
            }
            else if (__result.TryGetComponent(out MeshFilter meshFilter))
            {
                halfExtents = meshFilter.sharedMesh.bounds.extents;
            }
            else if (__result.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
            {
                halfExtents = skinnedMeshRenderer.localBounds.extents;
            }
            else
            {
                Plugin.Log.LogDebug("No mesh found for this object");
                return;
            }

            // Verify around spawning point for object(s) that might be in the way
            if (Physics.CheckBox(pos, halfExtents, __result.transform.rotation, ~512))
            {
                Plugin.Log.LogDebug($"{__result.name} is colliding with something at {pos}! Trying again...");

                // Destroy game object
                GameObject.Destroy(__result);

                // Set return value to null
                __result = null;
            }
        }
    }
}
