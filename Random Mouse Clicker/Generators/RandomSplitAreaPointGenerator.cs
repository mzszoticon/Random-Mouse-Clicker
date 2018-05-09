using System;
using System.Drawing;

namespace Random_Mouse_Clicker
{
    class RandomSplitAreaPointGenerator : SplitAreaPointGeneratorBase
    {
        private readonly Random _random;

        public RandomSplitAreaPointGenerator(Random random, Rectangle bounds, int rows, int columns, int count) : base(random, bounds, rows, columns, count)
        {
            _random = random;
        }

        protected override Point GetNextPointInternal()
        {
            Area next = Areas[_random.Next(Areas.Count)];
            next.Click();
            if (!next.CanClick)
            {
                Areas.Remove(next);
            }

            return GetNextPointInRectangle(next.Bounds);
        }
    }
}