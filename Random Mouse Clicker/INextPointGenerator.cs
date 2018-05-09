using System.Drawing;

namespace Random_Mouse_Clicker
{
    interface INextPointGenerator
    {
        bool HasNextPoint { get; }

        Point GetNextPoint();
    }
}
