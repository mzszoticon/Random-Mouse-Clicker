using System;
using System.Drawing;

namespace Random_Mouse_Clicker.Generators
{
    class SequentialSplitAreaPointGenerator : SplitAreaPointGeneratorBase
    {
        public SequentialSplitAreaPointGenerator(Random random, Rectangle bounds, int rows, int columns, int count) : base(random, bounds, rows, columns, count)
        {
        }

        protected override Point GetNextPointInternal()
        {
            Area next = Areas[0];

            next.Click();
            if (!next.CanClick)
                Areas.RemoveAt(0);

            return GetNextPointInRectangle(next.Bounds);
        }
    }
}
