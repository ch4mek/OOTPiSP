using System;
using System.Drawing;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Базовый класс для эллиптических фигур.
    /// Объединяет: CircleShape, EllipseShape.
    ///
    /// Общие характеристики наследников:
    ///   - Draw()                → g.FillEllipse + g.DrawEllipse
    ///   - SideCount = 1         → единая линия обводки
    ///   - Круг = частный случай эллипса (SemiMajor == SemiMinor, Rotation == 0)
    /// </summary>
    public abstract class EllipseBasedShape : ShapeBase
    {
    }
}
