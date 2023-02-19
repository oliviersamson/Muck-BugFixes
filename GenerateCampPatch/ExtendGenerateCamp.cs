using BugFixes;
using HarmonyLib;
using System;

// Modify the UnityEngine namespace
namespace UnityEngine
{
    // Add extension class to extend the GenerateCamp class with another method
    public static class GenerateCamp_Method_Extensions
    {
        // Create another FindPos method for Generate camp which will use a custom radius
        public static RaycastHit FindPos(this GenerateCamp camp, ConsistentRandom rand, float radius)
        {
            Vector3 a = camp.transform.position + Vector3.up * 200f;

            // Get this.RandomSpherePos(rand) * this.campRadius by reflection
            Vector3 randomSpherePos = (Vector3)AccessTools.Method(typeof(GenerateCamp), "RandomSpherePos", new Type[] { typeof(ConsistentRandom) }).Invoke(camp, new object[] { rand });
            float campRadius = (float)AccessTools.Field(typeof(GenerateCamp), "campRadius").GetValue(camp);
            Vector3 b = randomSpherePos * campRadius;


            // TODO: Might be a good idea to use Physics.CheckBox instead!
            if (Physics.SphereCast(a + b, radius, Vector3.down, out RaycastHit result, 400f))
            {
                Plugin.Log.LogDebug($"SphereCast hit {result.collider.name} ({result.collider.tag}) at {result.point}");
                if (result.collider.CompareTag("Camp"))
                {
                    Plugin.Log.LogDebug($"Hit camp!");
                    result = default;
                }
                // Add new condition to also look for camp houses (somehow houses spawned by GenerateCamp don't have the Camp tag)
                else if (result.collider.name.Contains("House"))
                {
                    Plugin.Log.LogDebug($"Hit house!");
                    result = default;
                }
                // Add new condition to also look for boat
                else if (result.collider.name.Contains("Boat") || result.collider.name.Contains("Planks"))
                {
                    Plugin.Log.LogDebug($"Hit boat!");
                    result = default;
                }
                //Add new condition to also look for chests
                else if (result.collider.name.Contains("Chest"))
                {
                    Plugin.Log.LogDebug($"Hit chest!");
                    result = default;
                }
                //Add new condition to also look for caves
                else if (result.collider.name.Contains("Cave"))
                {
                    Plugin.Log.LogDebug($"Hit cave!");
                    result = default;
                }
                else if (WorldUtility.WorldHeightToBiome(result.point.y) == TextureData.TerrainType.Water)
                {
                    Plugin.Log.LogDebug($"Hit water!");
                    result = default;
                }
            }
            else
            {
                Plugin.Log.LogDebug($"Nothing was hit!");
            }

            return result;
        }
    }
}