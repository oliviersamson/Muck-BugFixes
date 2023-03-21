using HarmonyLib;

namespace UnityEngine
{
    public static class ExtendBoat
    {
        public static void CheckForMapUpdate(this Boat boat, int itemId)
        {
            if (boat.status == Boat.BoatStatus.Hidden)
            {
                if (itemId == boat.mapItem.id)
                {
                    AccessTools.Method(typeof(Boat), "SendMarkShip").Invoke(boat, null);
                }
            }
            else if (!(bool)AccessTools.Field(typeof(Boat), "gemsDiscovered").GetValue(boat))
            {
                if (itemId == boat.gemMap.id)
                {
                    AccessTools.Method(typeof(Boat), "SendMarkGems").Invoke(boat, null);
                }
            }
        }
    }
}
