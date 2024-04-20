using System.Collections.Generic;

namespace Utility
{
    public static class GroupIdManager
    {
        private static int nextId = 0;
        private static HashSet<int> activeIds = new HashSet<int>();

        public static int GetUniqueId()
        {
            int id = nextId;
            activeIds.Add(id);
            nextId++;
            return id;
        }

        public static void ReleaseId(int id)
        {
            if (activeIds.Contains(id))
            {
                activeIds.Remove(id);
            }
        }
    }
}
