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
          
            if (Physics.SphereCast(a + b, 1f, Vector3.down, out RaycastHit result, 400f, camp.whatIsGround))
            {
                if (result.collider.name.Contains("Clone"))
                {
                    Plugin.Log.LogDebug($"Object is colliding with {result.collider.name} at {result.point}!");
                    result = default;
                }
                else if (WorldUtility.WorldHeightToBiome(result.point.y) == TextureData.TerrainType.Water)
                {
                    Plugin.Log.LogDebug($"Hit water at {result.point}!");
                    result = default;
                }
                // TODO: Might be a good idea to use Physics.CheckBox instead!
                else if (Physics.CheckSphere(result.point, radius, ~camp.whatIsGround.value))
                {
                    Plugin.Log.LogDebug($"Object is colliding with something at {result.point}!");
                    result = default;
                }
            }
            else
            {
                Plugin.Log.LogDebug($"No meshes were hit (not even the ground or water)!");
            }

            return result;
        }
    }
}