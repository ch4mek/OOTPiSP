using System;
using System.Collections.Generic;
using System.Drawing;
using Clipper2Lib;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Конвертер между форматами точек для работы с библиотекой Clipper2.
    /// Clipper2 работает с целочисленными координатами (Point64),
    /// поэтому требуется масштабирование для сохранения точности.
    /// </summary>
    public static class PolygonConverter
    {
        /// <summary>
        /// Масштаб для конвертации float координат в целочисленные.
        /// Умножаем на 1000 для сохранения 3 знаков после запятой.
        /// </summary>
        private const double SCALE = 1000.0;

        /// <summary>
        /// Конвертирует массив PointF в Path64 (формат Clipper2)
        /// </summary>
        /// <param name="points">Точки в формате PointF</param>
        /// <returns>Путь в формате Clipper2 (Point64)</returns>
        public static Path64 ToPath64(PointF[] points)
        {
            var path = new Path64();
            foreach (var pt in points)
            {
                long x = (long)Math.Round(pt.X * SCALE);
                long y = (long)Math.Round(pt.Y * SCALE);
                path.Add(new Point64(x, y));
            }
            return path;
        }

        /// <summary>
        /// Конвертирует список PointF в Path64 (формат Clipper2)
        /// </summary>
        /// <param name="points">Точки в формате PointF</param>
        /// <returns>Путь в формате Clipper2 (Point64)</returns>
        public static Path64 ToPath64(List<PointF> points)
        {
            var path = new Path64();
            foreach (var pt in points)
            {
                long x = (long)Math.Round(pt.X * SCALE);
                long y = (long)Math.Round(pt.Y * SCALE);
                path.Add(new Point64(x, y));
            }
            return path;
        }

        /// <summary>
        /// Конвертирует массив Point в Path64 (формат Clipper2)
        /// </summary>
        /// <param name="points">Точки в формате Point</param>
        /// <returns>Путь в формате Clipper2 (Point64)</returns>
        public static Path64 ToPath64(Point[] points)
        {
            var path = new Path64();
            foreach (var pt in points)
            {
                long x = (long)Math.Round(pt.X * SCALE);
                long y = (long)Math.Round(pt.Y * SCALE);
                path.Add(new Point64(x, y));
            }
            return path;
        }

        /// <summary>
        /// Конвертирует Path64 (формат Clipper2) в массив PointF
        /// </summary>
        /// <param name="path">Путь в формате Clipper2</param>
        /// <returns>Точки в формате PointF</returns>
        public static PointF[] ToPointFArray(Path64 path)
        {
            var points = new PointF[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                var pt = path[i];
                float x = (float)(pt.X / SCALE);
                float y = (float)(pt.Y / SCALE);
                points[i] = new PointF(x, y);
            }
            return points;
        }

        /// <summary>
        /// Конвертирует Paths64 (несколько путей) в список массивов PointF
        /// </summary>
        /// <param name="paths">Пути в формате Clipper2</param>
        /// <returns>Список массивов точек PointF</returns>
        public static List<PointF[]> ToPointFArrayList(Paths64 paths)
        {
            var result = new List<PointF[]>();
            foreach (var path in paths)
            {
                result.Add(ToPointFArray(path));
            }
            return result;
        }

        /// <summary>
        /// Выполняет объединение (union) нескольких полигонов с помощью Clipper2.
        /// Возвращает внешний контур объединённой области.
        /// Использует FillRule.Positive для корректного объединения вогнутых форм.
        /// </summary>
        /// <param name="polygons">Список полигонов для объединения</param>
        /// <returns>Массив точек объединённого полигона (внешний контур)</returns>
        public static PointF[] UnionPolygons(List<PointF[]> polygons)
        {
            if (polygons == null || polygons.Count == 0)
                return Array.Empty<PointF>();

            if (polygons.Count == 1)
                return polygons[0];

            // Конвертируем все полигоны в формат Clipper2
            var subjects = new Paths64();
            foreach (var polygon in polygons)
            {
                if (polygon != null && polygon.Length >= 3)
                {
                    subjects.Add(ToPath64(polygon));
                }
            }

            if (subjects.Count == 0)
                return Array.Empty<PointF>();

            // Выполняем объединение с FillRule.NonZero для корректной обработки всех форм
            var solution = Clipper.Union(subjects, FillRule.NonZero);

            if (solution == null || solution.Count == 0)
                return Array.Empty<PointF>();

            // Находим самый большой полигон (внешний контур)
            Path64? largestPath = null;
            double maxArea = 0;
            
            foreach (var path in solution)
            {
                // Вычисляем площадь (может быть отрицательной для внутренних контуров)
                double area = Clipper.Area(path);
                if (area < 0)
                    area = -area; // Берём модуль
                
                if (area > maxArea)
                {
                    maxArea = area;
                    largestPath = path;
                }
            }

            if (largestPath == null || largestPath.Count < 3)
                return Array.Empty<PointF>();

            return ToPointFArray(largestPath);
        }

        /// <summary>
        /// Выполняет объединение (union) нескольких полигонов с помощью Clipper2.
        /// Возвращает все контуры (внешние и внутренние/отверстия).
        /// Использует FillRule.Positive для корректного объединения вогнутых форм.
        /// </summary>
        /// <param name="polygons">Список полигонов для объединения</param>
        /// <returns>Список массивов точек (все контуры)</returns>
        public static List<PointF[]> UnionPolygonsAllContours(List<PointF[]> polygons)
        {
            var result = new List<PointF[]>();
            
            if (polygons == null || polygons.Count == 0)
                return result;

            if (polygons.Count == 1)
            {
                result.Add(polygons[0]);
                return result;
            }

            // Конвертируем все полигоны в формат Clipper2
            var subjects = new Paths64();
            foreach (var polygon in polygons)
            {
                if (polygon != null && polygon.Length >= 3)
                {
                    subjects.Add(ToPath64(polygon));
                }
            }

            if (subjects.Count == 0)
                return result;

            // Выполняем объединение с FillRule.NonZero для корректной обработки всех форм
            var solution = Clipper.Union(subjects, FillRule.NonZero);

            if (solution == null || solution.Count == 0)
                return result;

            // Конвертируем все контуры обратно
            foreach (var path in solution)
            {
                if (path.Count >= 3)
                {
                    result.Add(ToPointFArray(path));
                }
            }

            return result;
        }

        /// <summary>
        /// Аппроксимирует окружность как полигон с заданным количеством сегментов.
        /// Используется для корректного объединения окружностей с другими фигурами.
        /// </summary>
        /// <param name="center">Центр окружности</param>
        /// <param name="radius">Радиус окружности</param>
        /// <param name="segments">Количество сегментов (по умолчанию 64 для высокой точности)</param>
        /// <returns>Массив точек, аппроксимирующих окружность</returns>
        public static PointF[] CircleToPolygon(PointF center, float radius, int segments = 64)
        {
            if (segments < 8)
                segments = 8;
            
            var points = new PointF[segments];
            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                points[i] = new PointF(
                    center.X + radius * (float)Math.Cos(angle),
                    center.Y + radius * (float)Math.Sin(angle)
                );
            }
            return points;
        }

        /// <summary>
        /// Проверяет, является ли полигон валидным (минимум 3 точки, ненулевая площадь)
        /// </summary>
        /// <param name="polygon">Полигон для проверки</param>
        /// <returns>true, если полигон валидный</returns>
        public static bool IsValidPolygon(PointF[] polygon)
        {
            if (polygon == null || polygon.Length < 3)
                return false;

            var path = ToPath64(polygon);
            double area = Clipper.Area(path);
            return area != 0;
        }
    }
}
