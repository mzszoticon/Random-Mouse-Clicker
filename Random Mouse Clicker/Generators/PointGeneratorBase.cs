using System;
using System.Drawing;

namespace Random_Mouse_Clicker
{
    abstract class PointGeneratorBase : INextPointGenerator
    {
        private readonly Random _random;

        protected PointGeneratorBase(Random random)
        {
            _random = random;
        }

        public bool HasNextPoint
        {
            get { return !IsLimited || HasNextPointInternal(); }
        }

        protected abstract bool HasNextPointInternal();

        protected abstract bool IsLimited { get; }

        public Point GetNextPoint()
        {
            if (!HasNextPoint)
                throw new InvalidOperationException("Sequence contains no more points!");

            return GetNextPointInternal();
        }

        protected abstract Point GetNextPointInternal();

        protected Point GetNextPointInRectangle(Rectangle rectangle)
        {
            return new Point(
                _random.Next(rectangle.Left, rectangle.Right),
                _random.Next(rectangle.Top, rectangle.Bottom)
            );
        }
    }
}
