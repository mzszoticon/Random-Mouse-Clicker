using System;
using System.Drawing;

namespace Random_Mouse_Clicker
{
    class RandomPointGenerator : PointGeneratorBase
    {
        private readonly Rectangle _bounds;
        private int _count;

        public RandomPointGenerator(Random random, Rectangle bounds, int count) : base(random)
        {
            _bounds = bounds;
            _count = count;
        }

        protected override bool HasNextPointInternal()
        {
            return _count != 0;
        }

        protected override bool IsLimited => _count == -1;

        protected override Point GetNextPointInternal()
        {
            if (_count > 0)
                _count--;
            return GetNextPointInRectangle(_bounds);
        }
    }
}
