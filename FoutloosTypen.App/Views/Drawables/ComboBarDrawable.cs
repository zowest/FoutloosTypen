using Microsoft.Maui.Graphics;
using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views.Drawables;

public class ComboBarDrawable : IDrawable
{
    private readonly EndlessModeViewModel _vm;

    public ComboBarDrawable(EndlessModeViewModel vm)
    {
        _vm = vm;
    }

    public void Draw(ICanvas canvas, RectF r)
    {
        float radius = r.Height / 2;

        // background
        canvas.FillColor = Color.FromArgb("#E6E6E6");
        canvas.FillRoundedRectangle(r, radius);

        float p = (float)Math.Clamp(_vm.ComboProgress, 0, 1);
        if (p <= 0) return;

        var bar = new RectF(r.X, r.Y, r.Width * p, r.Height);

        canvas.FillColor =
            _vm.ComboProgress > 0.66 ? Color.FromArgb("#ffcb05") :
            _vm.ComboProgress > 0.33 ? Colors.Orange :
            Colors.Red;

        canvas.FillRoundedRectangle(bar, radius);
    }

}
