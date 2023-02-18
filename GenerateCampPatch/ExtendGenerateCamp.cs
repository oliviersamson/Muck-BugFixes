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

            if (Physics.SphereCast(a + b, radius, Vector3.down, out RaycastHit result, 400f, camp.whatIsGround))
            {
                if (result.collider.CompareTag("Camp"))
                {
                    result = default;
                }
                // Add new condition to also look for camp houses (somehow houses spawned by GenerateCamp don't have the Camp tag)
                else if (result.collider.name.Contains("House"))
                {
                    result = default;
                }
                if (WorldUtility.WorldHeightToBiome(result.point.y) == TextureData.TerrainType.Water)
                {
                    result = default;
                }
            }

            return result;
        }
    }
}