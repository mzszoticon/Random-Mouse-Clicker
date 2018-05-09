using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Random_Mouse_Clicker.Utilities;

namespace Random_Mouse_Clicker
{
    abstract class SplitAreaPointGeneratorBase : PointGeneratorBase
    {
        protected class Area
        {
            private int _count;

            public Area(Rectangle bounds, int count)
            {
                _count = count;
                Bounds = bounds;
            }

            public Rectangle Bounds { get; }

            public bool CanClick => _count != 0;

            public void Click()
            {
                if (!CanClick)
                    throw new InvalidOperationException();

                if (_count > 0)
                    _count--;
            }
        }

        protected SplitAreaPointGeneratorBase(Random random, Rectangle bounds, int rows, int columns, int count) : base(random)
        {
            var rectangles = SplitGenerator.GetAreas(bounds, rows, columns);

            for (int i = 0; i < rectangles.GetLength(0); ++i)
            {
                for (int j = 0; j < rectangles.GetLength(1); ++j)
                {
                    Areas.Add(new Area(rectangles[i, j], count));
                }
            }

            IsLimited = count != -1;
        }

        protected override bool IsLimited { get; }

        protected List<Area> Areas { get; } = new List<Area>();

        protected override bool HasNextPointInternal()
        {
            return Areas.Count > 0;
        }
    }
}
