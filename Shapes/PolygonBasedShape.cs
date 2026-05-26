using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Базовый класс для многоугольных фигур.
    /// Объединяет: RectangleShape, TriangleShape, HexagonShape, TrapezoidShape.
    ///
    /// Общие характеристики наследников:
    ///   - GetWorldPoints()      → массив вершин
    ///   - Draw()                → GraphicsPath.AddPolygon + DrawSidesWithMiterClip
    ///   - UpdateVirtualBounds() → CalculateBoundsWithBorderWidth
    ///   - ResizeSide()          → PropagateDisplacement + ApplyDeformedVertices
    ///   - Каждая сторона имеет индивидуальные толщину и цвет (SideCount >= 3)
    /// </summary>
    public abstract class PolygonBasedShape : ShapeBase
    {
    }
}
