using UnityEngine;

namespace WILCommunityGame
{
    public interface ITimeTracker
    {
        void ClockUpdate(GameTimestamp timestamp);
    }
}
