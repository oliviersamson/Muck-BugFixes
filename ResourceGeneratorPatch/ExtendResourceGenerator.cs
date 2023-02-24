using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class ExtendResourceGenerator
    {
        private static readonly Dictionary<string, Vector3> treesExtents= new Dictionary<string, Vector3>()
        {
            // Arbitrary values, TBD
            { "Tree", new Vector3(0.5f, 5f, 0.5f) },
            { "Birch", new Vector3(0.4f, 5f, 0.4f) },
            { "Fir", new Vector3(0.8f, 5f, 0.8f) },
            { "Oak", new Vector3(1f, 5f, 1f) },
            { "DarkOak", new Vector3(1.2f, 5f, 1.2f) }
        };

        public static Vector3 GetTreeExtents(this ResourceGenerator resourceGenerator, string tree)
        {
            return treesExtents[tree];
        }
    }
}
