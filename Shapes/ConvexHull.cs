using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Статический класс для вычисления выпуклой оболочки (Convex Hull)
    /// Использует алгоритм Грэхема (Graham Scan)
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Вычисляет выпуклую оболочку для списка точек
        /// используя алгоритм Грэхема (Graham Scan)
        /// </summary>
        /// <param name="points">Список исходных точек</param>
        /// <returns>Список точек, образующих выпуклую оболочку (по часовой стрелке)</returns>
        public static List<PointF> Compute(List<PointF> points)
        {
            if (points == null || points.Count < 3)
                return points?.ToList() ?? new List<PointF>();

            // Находим точку с минимальным Y (и X при равных Y) - стартовая точка
            var startPoint = points
                .OrderBy(p => p.Y)
                .ThenBy(p => p.X)
                .First();

            // Сортируем остальные точки по полярному углу относительно начальной точки
            var sorted = points
                .Where(p => p != startPoint)
                .OrderBy(p => Math.Atan2(p.Y - startPoint.Y, p.X - startPoint.X))
                .ToList();

            // Строим выпуклую оболочку с помощью стека
            var hull = new Stack<PointF>();
            hull.Push(startPoint);

            foreach (var p in sorted)
            {
                while (hull.Count > 1 && CrossProduct(GetSecondFromTop(hull), hull.Peek(), p) <= 0)
                {
                    hull.Pop();
                }
                hull.Push(p);
            }

            return hull.ToList();
        }

        /// <summary>
        /// Вычисляет выпуклую оболочку для массива точек
        /// </summary>
        /// <param name="points">Массив исходных точек</param>
        /// <returns>Массив точек, образующих выпуклую оболочку</returns>
        public static PointF[] Compute(PointF[] points)
        {
            if (points == null || points.Length < 3)
                return points ?? Array.Empty<PointF>();

            return Compute(points.ToList()).ToArray();
        }

        /// <summary>
        /// Получить второй элемент сверху стека без удаления
        /// </summary>
        private static T GetSecondFromTop<T>(Stack<T> stack)
        {
            if (stack.Count < 2)
                throw new ArgumentException("Стек должен содержать минимум 2 элемента");

            T top = stack.Pop();
            T second = stack.Peek();
            stack.Push(top);
            return second;
        }

        /// <summary>
        /// Вычисление векторного произведения (cross product)
        /// OA x OB > 0 => B слева от OA
        /// OA x OB < 0 => B справа от OA
        /// OA x OB = 0 => точки коллинеарны
        /// </summary>
        private static float CrossProduct(PointF O, PointF A, PointF B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }
    }
}
