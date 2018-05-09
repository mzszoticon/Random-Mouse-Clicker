using System.Drawing;

namespace Random_Mouse_Clicker.Utilities
{
    public static class SplitGenerator
    {
        public static Rectangle[,] GetAreas(Rectangle bounds, int rows, int columns)
        {
            int width = bounds.Width / columns;
            int height = bounds.Height / rows;

            var result = new Rectangle[rows, columns];


            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    result[i, j] = new Rectangle(bounds.Left + j * width, bounds.Top + i * height, width, height);
                }
            }

            return result;
        }
    }
}
