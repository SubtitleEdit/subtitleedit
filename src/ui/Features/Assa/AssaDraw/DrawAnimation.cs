using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// An in or out effect. Left/Right/Top/Bottom slide from (in) or to (out) that edge of the frame.
/// </summary>
public enum DrawAnimationEffect
{
    None,
    Fade,
    Pop,
    Zoom,
    Spin,
    Left,
    Right,
    Top,
    Bottom,
}

/// <summary>
/// A repeating effect between the in and the out effect. ASSA has no loops, so it is written as
/// one \t per cycle for the length of the line.
/// </summary>
public enum DrawLoopEffect
{
    None,
    Spin,
    Pulse,
    Blink,
    Bounce,
    Shake,
    ShakeHorizontal,
    Swing,

    /// <summary>
    /// A quick half turn, then a pause (an hourglass loader).
    /// </summary>
    Flip,
}

/// <summary>
/// The \org an animation turns around: none, the anchor (spin, shake), the top edge (swing), or one
/// far away (a slide out after a slide in, a bounce) - a line has one \org.
/// </summary>
public enum DrawAnimationOrigin
{
    None,
    Anchor,
    Top,
    FarOut,
    FarBounce,
    FarAbove,
}

/// <summary>
/// Where a layer (or a group's layers) is at a moment of its animation: moved by Dx/Dy, scaled and
/// turned around the anchor, faded to Opacity. Rotation is degrees clockwise on screen.
/// </summary>
public readonly record struct DrawAnimationState(float Dx, float Dy, float Scale, float Rotation, float Opacity)
{
    public static readonly DrawAnimationState Rest = new(0, 0, 1, 0, 1);
}

/// <summary>
/// The in, loop and out effect of a line. It is written as ASSA override tags (\move, \fad, \t with \fscx,
/// \fscy and \frz) around an anchor - the center of the layer or of its group, so a group scales and
/// turns as one - and kept in an <c>{assadraw-anim ...}</c> comment block so the editor can open it again.
/// </summary>
public sealed class DrawAnimation : IEquatable<DrawAnimation>
{
    public const int DefaultDurationMs = 400;
    private const float PopOvershoot = 1.1f;
    private const float PopRiseShare = 0.7f;
    private const double EaseOut = 0.5;
    private const double EaseIn = 2;

    public DrawAnimationEffect In { get; set; }
    public int InMs { get; set; } = DefaultDurationMs;
    public DrawAnimationEffect Out { get; set; }
    public int OutMs { get; set; } = DefaultDurationMs;
    public DrawLoopEffect Loop { get; set; }

    /// <summary>
    /// Length of one cycle of the loop; fitted so whole cycles fill the time between in and out.
    /// </summary>
    public int LoopMs { get; set; } = DefaultLoopMs;

    public const int DefaultLoopMs = 1000;
    private const float PulseScale = 1.1f;
    private const float ShakeDegrees = 8;
    private const float SwingDegrees = 14;

    /// <summary>
    /// Share of a flip cycle spent turning; the rest is the pause.
    /// </summary>
    private const double FlipTurnShare = 0.35;

    public bool IsNone => In == DrawAnimationEffect.None && Out == DrawAnimationEffect.None && Loop == DrawLoopEffect.None;

    public DrawAnimation Clone() => new() { In = In, InMs = InMs, Out = Out, OutMs = OutMs, Loop = Loop, LoopMs = LoopMs };

    private DrawAnimationOrigin InOutOrigin =>
        IsDoubleSlide ? DrawAnimationOrigin.FarOut :
        In == DrawAnimationEffect.Spin || Out == DrawAnimationEffect.Spin ? DrawAnimationOrigin.Anchor :
        DrawAnimationOrigin.None;

    private DrawAnimationOrigin LoopOrigin => Loop switch
    {
        DrawLoopEffect.Spin or DrawLoopEffect.Shake or DrawLoopEffect.Flip => DrawAnimationOrigin.Anchor,
        DrawLoopEffect.Swing => DrawAnimationOrigin.Top,
        DrawLoopEffect.Bounce => DrawAnimationOrigin.FarBounce,
        DrawLoopEffect.ShakeHorizontal => DrawAnimationOrigin.FarAbove,
        _ => DrawAnimationOrigin.None,
    };

    /// <summary>
    /// The loop turns around another \org than the in/out effect (a swing with a spin in, a bounce
    /// with two slides) - a line has only one, so the loop is left out.
    /// </summary>
    public bool IsLoopBlocked => LoopOrigin != DrawAnimationOrigin.None && InOutOrigin != DrawAnimationOrigin.None && LoopOrigin != InOutOrigin;

    public DrawLoopEffect EffectiveLoop => IsLoopBlocked ? DrawLoopEffect.None : Loop;

    /// <summary>
    /// The \org the line needs.
    /// </summary>
    public DrawAnimationOrigin OriginKind => InOutOrigin != DrawAnimationOrigin.None ? InOutOrigin : IsLoopBlocked ? DrawAnimationOrigin.None : LoopOrigin;

    /// <summary>
    /// The loop's time (start, end) and whole cycles between the in and the out effect; 0 cycles for none.
    /// </summary>
    public (int Start, int End, int Cycles) GetLoopWindow(int lineMs)
    {
        var (inMs, outMs) = GetDurations(lineMs);
        var start = inMs;
        var end = lineMs - outMs;
        if (EffectiveLoop == DrawLoopEffect.None || end - start < 2)
        {
            return (start, end, 0);
        }

        return (start, end, Math.Clamp((int)Math.Round((end - start) / (double)Math.Max(10, LoopMs)), 1, 2000));
    }

    /// <summary>
    /// How high a bounce goes: a tenth of the height, at least 8 px.
    /// </summary>
    private static float GetBounceHeight((float Left, float Top, float Right, float Bottom) bounds) =>
        Math.Max(8, (bounds.Bottom - bounds.Top) * 0.1f);

    /// <summary>
    /// How far a horizontal shake sways to each side: a twentieth of the width, at least 4 px.
    /// </summary>
    private static float GetShakeDistance((float Left, float Top, float Right, float Bottom) bounds) =>
        Math.Max(4, (bounds.Right - bounds.Left) * 0.05f);

    /// <summary>
    /// The shake wave over one cycle: 0 to 1, back to 0, to -1 and back to 0.
    /// </summary>
    private static double ShakeWave(double u) => u < 0.25 ? u * 4 : u < 0.75 ? 1 - (u - 0.25) * 4 : (u - 0.75) * 4 - 1;

    public static bool IsSlide(DrawAnimationEffect effect) => effect >= DrawAnimationEffect.Left;

    /// <summary>
    /// A slide in and a slide out: ASSA has one \move per line, so the slide in uses it and the slide
    /// out turns the line a tiny bit around an \org far away - a straight move over the frame.
    /// </summary>
    public bool IsDoubleSlide => IsSlide(In) && IsSlide(Out);

    /// <summary>
    /// How far away the \org of a double slide is: far enough that the arc is straight to a fraction
    /// of a pixel over a frame width (2 px off at 2000 px), near enough for float renderers.
    /// </summary>
    private const double FarOriginDistance = 1_000_000;

    /// <summary>
    /// The in effect gets the start of the line and the out effect the end; on a short line they
    /// share it instead of overlapping.
    /// </summary>
    public (int InMs, int OutMs) GetDurations(int lineMs)
    {
        var inMs = In == DrawAnimationEffect.None ? 0 : Math.Max(1, InMs);
        var outMs = Out == DrawAnimationEffect.None ? 0 : Math.Max(1, OutMs);
        if (lineMs > 0 && inMs + outMs > lineMs)
        {
            var factor = lineMs / (double)(inMs + outMs);
            inMs = (int)(inMs * factor);
            outMs = (int)(outMs * factor);
        }

        return (inMs, outMs);
    }

    /// <summary>
    /// The state at <paramref name="timeMs"/> into a line of <paramref name="lineMs"/>, like libass shows it.
    /// <paramref name="bounds"/> (what is animated) and the frame size give the slide distance.
    /// </summary>
    public DrawAnimationState GetState(double timeMs, int lineMs, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight)
    {
        var (inMs, outMs) = GetDurations(lineMs);
        var outStart = lineMs - outMs;
        float dx = 0, dy = 0, scale = 1, rotation = 0, opacity = 1;

        if (inMs > 0 && timeMs < inMs)
        {
            var p = Math.Clamp(timeMs / inMs, 0, 1);
            var eased = (float)Math.Pow(p, EaseOut);
            switch (In)
            {
                case DrawAnimationEffect.Fade:
                    opacity = (float)p;
                    break;
                case DrawAnimationEffect.Zoom:
                    scale = eased;
                    break;
                case DrawAnimationEffect.Pop:
                    scale = p < PopRiseShare
                        ? PopOvershoot * (float)Math.Pow(p / PopRiseShare, EaseOut)
                        : PopOvershoot - (PopOvershoot - 1) * (float)((p - PopRiseShare) / (1 - PopRiseShare));
                    break;
                case DrawAnimationEffect.Spin:
                    scale = eased;
                    rotation = 360 * (1 - eased);
                    break;
                default:
                    var (sx, sy) = GetSlideOffset(In, bounds, frameWidth, frameHeight);
                    dx = sx * (float)(1 - p);
                    dy = sy * (float)(1 - p);
                    break;
            }
        }
        else if (outMs > 0 && timeMs > outStart)
        {
            var q = Math.Clamp((timeMs - outStart) / outMs, 0, 1);
            var eased = (float)Math.Pow(q, EaseIn);
            switch (Out)
            {
                case DrawAnimationEffect.Fade:
                    opacity = (float)(1 - q);
                    break;
                case DrawAnimationEffect.Zoom:
                    scale = 1 - eased;
                    break;
                case DrawAnimationEffect.Pop:
                    var rise = 1 - PopRiseShare;
                    scale = q < rise
                        ? 1 + (PopOvershoot - 1) * (float)(q / rise)
                        : PopOvershoot * (1 - (float)Math.Pow((q - rise) / PopRiseShare, EaseIn));
                    break;
                case DrawAnimationEffect.Spin:
                    scale = 1 - eased;
                    rotation = -360 * eased;
                    break;
                default:
                    // \move is linear; the \org turn of a double slide is eased like the other out effects
                    var (sx, sy) = GetSlideOffset(Out, bounds, frameWidth, frameHeight);
                    var progress = IsDoubleSlide ? eased : (float)q;
                    dx = sx * progress;
                    dy = sy * progress;
                    break;
            }
        }

        else
        {
            var (loopStart, loopEnd, cycles) = GetLoopWindow(lineMs);
            if (cycles > 0 && timeMs >= loopStart && timeMs <= loopEnd)
            {
                var period = (loopEnd - loopStart) / (double)cycles;
                var u = (timeMs - loopStart) / period;
                var cycle = Math.Min(Math.Floor(u), cycles - 1);
                u -= cycle;
                switch (EffectiveLoop)
                {
                    case DrawLoopEffect.Spin:
                        rotation = (float)(-360 * u);
                        break;
                    case DrawLoopEffect.Flip:
                        var turned = u < FlipTurnShare ? Math.Pow(u / FlipTurnShare, EaseOut) : 1;
                        rotation = (float)(-180 * (cycle + turned));
                        break;
                    case DrawLoopEffect.Pulse:
                        scale = 1 + (PulseScale - 1) * (float)(u < 0.5 ? u * 2 : 2 - u * 2);
                        break;
                    case DrawLoopEffect.Blink:
                        scale = u < 0.5 ? 1 : 0;
                        break;
                    case DrawLoopEffect.Bounce:
                        // Up easing out, down easing in, like the \t accelerations
                        var height = u < 0.5 ? Math.Pow(u * 2, EaseOut) : 1 - Math.Pow(u * 2 - 1, EaseIn);
                        dy = -GetBounceHeight(bounds) * (float)height;
                        break;
                    case DrawLoopEffect.Shake:
                        rotation = ShakeDegrees * (float)ShakeWave(u);
                        break;
                    case DrawLoopEffect.ShakeHorizontal:
                        dx = GetShakeDistance(bounds) * (float)ShakeWave(u);
                        break;
                    case DrawLoopEffect.Swing:
                        var quarter = u * 4 - Math.Floor(u * 4);
                        var swing = (u * 4) switch
                        {
                            < 1 => Math.Pow(quarter, EaseOut),
                            < 2 => 1 - Math.Pow(quarter, EaseIn),
                            < 3 => -Math.Pow(quarter, EaseOut),
                            _ => -(1 - Math.Pow(quarter, EaseIn)),
                        };
                        rotation = SwingDegrees * (float)swing;

                        // Around the top edge: the anchor (the middle) swings out to the side
                        var halfHeight = (bounds.Bottom - bounds.Top) / 2;
                        var radians = -rotation * Math.PI / 180;
                        dx = (float)(-halfHeight * Math.Sin(radians));
                        dy = (float)(halfHeight * (Math.Cos(radians) - 1));
                        break;
                }
            }
        }

        // \frz turns counter-clockwise; the state is clockwise on screen like the canvas
        return new DrawAnimationState(dx, dy, Math.Max(0, scale), -rotation, Math.Clamp(opacity, 0, 1));
    }

    /// <summary>
    /// How far a slide moves: just out of the frame on that side.
    /// </summary>
    public static (float Dx, float Dy) GetSlideOffset(DrawAnimationEffect effect, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight)
    {
        const float margin = 4;
        return effect switch
        {
            DrawAnimationEffect.Left => (-(bounds.Right + margin), 0),
            DrawAnimationEffect.Right => (frameWidth - bounds.Left + margin, 0),
            DrawAnimationEffect.Top => (0, -(bounds.Bottom + margin)),
            DrawAnimationEffect.Bottom => (0, frameHeight - bounds.Top + margin),
            _ => (0, 0),
        };
    }

    /// <summary>
    /// The position tag: \move for a slide (the slide in when there are two), else \pos.
    /// <paramref name="map"/> turns where something should show into the position to write (a line
    /// with its own \frz and an \org elsewhere, see <see cref="MapAroundOrigin"/>).
    /// </summary>
    public string GetPositionTag(float x, float y, int lineMs, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight,
        Func<double, double, (double X, double Y)>? map = null)
    {
        map ??= (px, py) => (px, py);
        var (inMs, outMs) = GetDurations(lineMs);
        var rest = map(x, y);
        if (IsSlide(In))
        {
            var (dx, dy) = GetSlideOffset(In, bounds, frameWidth, frameHeight);
            var start = map(x + dx, y + dy);
            return $"\\move({N(start.X)},{N(start.Y)},{N(rest.X)},{N(rest.Y)},0,{inMs})";
        }

        if (IsSlide(Out))
        {
            var (dx, dy) = GetSlideOffset(Out, bounds, frameWidth, frameHeight);
            var end = map(x + dx, y + dy);
            return $"\\move({N(rest.X)},{N(rest.Y)},{N(end.X)},{N(end.Y)},{lineMs - outMs},{lineMs})";
        }

        return $"\\pos({N(rest.X)},{N(rest.Y)})";
    }

    /// <summary>
    /// The far \org of a double slide and how far (degrees, \frz) the line turns around it: the
    /// anchor moves by the slide-out offset.
    /// </summary>
    public (double X, double Y, double Degrees) GetOutSlideRotation(float anchorX, float anchorY, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight)
    {
        var (dx, dy) = GetSlideOffset(Out, bounds, frameWidth, frameHeight);
        var distance = Math.Sqrt(dx * dx + dy * dy);

        // \frz turns counter-clockwise on screen, so with the \org on this side of the anchor (at a
        // right angle to the slide) a positive turn sets the anchor off in the slide direction
        var originX = anchorX + FarOriginDistance * dy / distance;
        var originY = anchorY - FarOriginDistance * dx / distance;
        return (originX, originY, Math.Asin(distance / FarOriginDistance) * 180 / Math.PI);
    }

    /// <summary>
    /// The \org the line turns around (<see cref="OriginKind"/>), or null for none.
    /// </summary>
    public (double X, double Y)? GetOrigin(float anchorX, float anchorY, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight)
    {
        switch (OriginKind)
        {
            case DrawAnimationOrigin.Anchor:
                return (anchorX, anchorY);
            case DrawAnimationOrigin.Top:
                return (anchorX, bounds.Top);
            case DrawAnimationOrigin.FarOut:
                var (x, y, _) = GetOutSlideRotation(anchorX, anchorY, bounds, frameWidth, frameHeight);
                return (x, y);
            case DrawAnimationOrigin.FarBounce:
                // To the left, so a positive (counter-clockwise) turn lifts the anchor
                return (anchorX - FarOriginDistance, anchorY);
            case DrawAnimationOrigin.FarAbove:
                // Above, so a positive (counter-clockwise) turn sways the anchor to the right
                return (anchorX, anchorY - FarOriginDistance);
            default:
                return null;
        }
    }

    /// <summary>
    /// Where to put a line with its own \frz (<paramref name="frz"/>) so it shows at (x, y) although
    /// it turns around (originX, originY) instead of its position.
    /// </summary>
    public static (double X, double Y) MapAroundOrigin(double x, double y, double originX, double originY, double frz)
    {
        // libass turns counter-clockwise on screen (y down) by \frz; undo that turn
        var radians = -frz * Math.PI / 180;
        var vx = x - originX;
        var vy = y - originY;
        return (originX + vx * Math.Cos(radians) + vy * Math.Sin(radians), originY - vx * Math.Sin(radians) + vy * Math.Cos(radians));
    }

    /// <summary>
    /// \fad and the scale/rotation tags (everything but the position and \org): the in effect, the
    /// loop (one \t per cycle) and the out effect. <paramref name="baseFrz"/> is the line's own \frz,
    /// which a spin turns from and back to. <paramref name="bounds"/> and the frame size give the
    /// distances turned around a far \org (a slide out after a slide in, a bounce).
    /// </summary>
    public string GetEffectTags(int lineMs, float baseFrz, (float Left, float Top, float Right, float Bottom) bounds, float frameWidth, float frameHeight)
    {
        var (inMs, outMs) = GetDurations(lineMs);
        var outStart = lineMs - outMs;
        var sb = new StringBuilder();

        var fadeIn = In == DrawAnimationEffect.Fade ? inMs : 0;
        var fadeOut = Out == DrawAnimationEffect.Fade ? outMs : 0;
        if (fadeIn > 0 || fadeOut > 0)
        {
            sb.Append(CultureInfo.InvariantCulture, $"\\fad({fadeIn},{fadeOut})");
        }

        switch (In)
        {
            case DrawAnimationEffect.Zoom:
                sb.Append(CultureInfo.InvariantCulture, $"\\fscx0\\fscy0\\t(0,{inMs},{N(EaseOut)},\\fscx100\\fscy100)");
                break;
            case DrawAnimationEffect.Pop:
                var rise = (int)Math.Round(inMs * PopRiseShare);
                sb.Append(CultureInfo.InvariantCulture, $"\\fscx0\\fscy0\\t(0,{rise},{N(EaseOut)},\\fscx{N(PopOvershoot * 100)}\\fscy{N(PopOvershoot * 100)})\\t({rise},{inMs},\\fscx100\\fscy100)");
                break;
            case DrawAnimationEffect.Spin:
                sb.Append(CultureInfo.InvariantCulture, $"\\frz{N(baseFrz + 360)}\\fscx0\\fscy0\\t(0,{inMs},{N(EaseOut)},\\frz{N(baseFrz)}\\fscx100\\fscy100)");
                break;
        }

        // A spin loop leaves the line whole turns further; a spin out goes on from there
        var restFrz = AppendLoopTags(sb, lineMs, baseFrz, bounds);

        switch (Out)
        {
            case DrawAnimationEffect.Zoom:
                sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{lineMs},{N(EaseIn)},\\fscx0\\fscy0)");
                break;
            case DrawAnimationEffect.Pop:
                var rise = outStart + (int)Math.Round(outMs * (1 - PopRiseShare));
                sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{rise},\\fscx{N(PopOvershoot * 100)}\\fscy{N(PopOvershoot * 100)})\\t({rise},{lineMs},{N(EaseIn)},\\fscx0\\fscy0)");
                break;
            case DrawAnimationEffect.Spin:
                sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{lineMs},{N(EaseIn)},\\frz{N(restFrz - 360)}\\fscx0\\fscy0)");
                break;
            default:
                if (IsDoubleSlide)
                {
                    var outSlideDegrees = GetOutSlideRotation(0, 0, bounds, frameWidth, frameHeight).Degrees;
                    var degrees = Precise(baseFrz + outSlideDegrees);
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{lineMs},{N(EaseIn)},\\frz{degrees})");
                }

                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// One \t per cycle of the loop; returns the \frz the loop leaves the line at.
    /// </summary>
    private float AppendLoopTags(StringBuilder sb, int lineMs, float baseFrz, (float Left, float Top, float Right, float Bottom) bounds)
    {
        var (start, end, cycles) = GetLoopWindow(lineMs);
        if (cycles == 0)
        {
            return baseFrz;
        }

        var period = (end - start) / (double)cycles;
        int T(double cycle) => (int)Math.Round(start + cycle * period);
        var pulse = N(PulseScale * 100);
        var bounceDegrees = Precise(baseFrz + Math.Asin(GetBounceHeight(bounds) / FarOriginDistance) * 180 / Math.PI);
        var swayDegrees = Math.Asin(GetShakeDistance(bounds) / FarOriginDistance) * 180 / Math.PI;
        var restFrz = baseFrz;
        for (var k = 0; k < cycles; k++)
        {
            switch (EffectiveLoop)
            {
                case DrawLoopEffect.Spin:
                    // Clockwise: \frz goes down a whole turn per cycle
                    restFrz = baseFrz - 360 * (k + 1);
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 1)},\\frz{N(restFrz)})");
                    break;
                case DrawLoopEffect.Flip:
                    // Clockwise half turn easing out (accel 0.5), then a pause
                    restFrz = baseFrz - 180 * (k + 1);
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + FlipTurnShare)},{N(EaseOut)},\\frz{N(restFrz)})");
                    break;
                case DrawLoopEffect.Pulse:
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 0.5)},\\fscx{pulse}\\fscy{pulse})\\t({T(k + 0.5)},{T(k + 1)},\\fscx100\\fscy100)");
                    break;
                case DrawLoopEffect.Blink:
                    // A \t with no length switches at once; the scale, so the colors' own alpha stays
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k + 0.5)},{T(k + 0.5)},\\fscx0\\fscy0)\\t({T(k + 1)},{T(k + 1)},\\fscx100\\fscy100)");
                    break;
                case DrawLoopEffect.Bounce:
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 0.5)},{N(EaseOut)},\\frz{bounceDegrees})\\t({T(k + 0.5)},{T(k + 1)},{N(EaseIn)},\\frz{N(baseFrz)})");
                    break;
                case DrawLoopEffect.Shake:
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 0.25)},\\frz{N(baseFrz + ShakeDegrees)})\\t({T(k + 0.25)},{T(k + 0.75)},\\frz{N(baseFrz - ShakeDegrees)})\\t({T(k + 0.75)},{T(k + 1)},\\frz{N(baseFrz)})");
                    break;
                case DrawLoopEffect.ShakeHorizontal:
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 0.25)},\\frz{Precise(baseFrz + swayDegrees)})\\t({T(k + 0.25)},{T(k + 0.75)},\\frz{Precise(baseFrz - swayDegrees)})\\t({T(k + 0.75)},{T(k + 1)},\\frz{Precise(baseFrz)})");
                    break;
                case DrawLoopEffect.Swing:
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k)},{T(k + 0.25)},{N(EaseOut)},\\frz{N(baseFrz + SwingDegrees)})\\t({T(k + 0.25)},{T(k + 0.5)},{N(EaseIn)},\\frz{N(baseFrz)})");
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({T(k + 0.5)},{T(k + 0.75)},{N(EaseOut)},\\frz{N(baseFrz - SwingDegrees)})\\t({T(k + 0.75)},{T(k + 1)},{N(EaseIn)},\\frz{N(baseFrz)})");
                    break;
            }
        }

        return restFrz;
    }

    // Seven decimals: a far \org turns a thousandth of a degree into ~17 px
    private static string Precise(double degrees) => degrees.ToString("0.#######", CultureInfo.InvariantCulture);

    private static readonly Regex MarkerRegex = new(
        @"\{assadraw-anim c=(-?[\d.]+),(-?[\d.]+) in=(\w+),(\d+) out=(\w+),(\d+)(?: loop=(\w+),(\d+))?\}",
        RegexOptions.Compiled);

    /// <summary>
    /// The comment block that keeps the animation and its anchor in the line.
    /// </summary>
    public string FormatMarker(float anchorX, float anchorY) =>
        $"{{assadraw-anim c={N(anchorX)},{N(anchorY)} in={In},{InMs.ToString(CultureInfo.InvariantCulture)} out={Out},{OutMs.ToString(CultureInfo.InvariantCulture)}" +
        (Loop == DrawLoopEffect.None ? string.Empty : $" loop={Loop},{LoopMs.ToString(CultureInfo.InvariantCulture)}") + "}";

    /// <summary>
    /// Reads the marker of an animated line; false for a line without one.
    /// </summary>
    public static bool TryParseMarker(string text, out DrawAnimation animation, out float anchorX, out float anchorY)
    {
        animation = new DrawAnimation();
        anchorX = anchorY = 0;
        var match = MarkerRegex.Match(text ?? string.Empty);
        if (!match.Success)
        {
            return false;
        }

        anchorX = F(match.Groups[1].Value);
        anchorY = F(match.Groups[2].Value);
        animation.In = Enum.TryParse<DrawAnimationEffect>(match.Groups[3].Value, out var inEffect) ? inEffect : DrawAnimationEffect.None;
        animation.InMs = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
        animation.Out = Enum.TryParse<DrawAnimationEffect>(match.Groups[5].Value, out var outEffect) ? outEffect : DrawAnimationEffect.None;
        animation.OutMs = int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
        if (match.Groups[7].Success)
        {
            animation.Loop = Enum.TryParse<DrawLoopEffect>(match.Groups[7].Value, out var loop) ? loop : DrawLoopEffect.None;
            animation.LoopMs = int.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
        }

        return true;
    }

    private static readonly Regex DrawingAnimationRegex = new(@"^\{\\an7[^}]*\}\{assadraw-anim [^}]*\}", RegexOptions.Compiled);
    private static readonly Regex TextAnimationRegex = new(@"\{assadraw-anim [^}]*\}(\{[^}]*\})?", RegexOptions.Compiled);
    private static readonly Regex MoveRegex = new(@"\\move\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex PosRegex = new(@"\\pos\([^)]*\)", RegexOptions.Compiled);

    /// <summary>
    /// The line without its animation: a drawing line loses its leading position/effect block (its
    /// coordinates are then relative to the anchor), a text line its effect block and position (its
    /// text area comment says where it goes; the written position allows for the \org).
    /// </summary>
    public static string RemoveFromLine(string text)
    {
        if (DrawingAnimationRegex.IsMatch(text))
        {
            return DrawingAnimationRegex.Replace(text, string.Empty, 1);
        }

        text = TextAnimationRegex.Replace(text, string.Empty, 1);
        text = MoveRegex.Replace(text, string.Empty, 1);
        return PosRegex.Replace(text, string.Empty, 1);
    }

    /// <summary>
    /// True for a drawing line written relative to its anchor (see <see cref="RemoveFromLine"/>).
    /// </summary>
    public static bool IsAnchoredDrawingLine(string text) => DrawingAnimationRegex.IsMatch(text ?? string.Empty);

    public bool Equals(DrawAnimation? other) =>
        other != null && In == other.In && InMs == other.InMs && Out == other.Out && OutMs == other.OutMs && Loop == other.Loop && LoopMs == other.LoopMs;

    public override bool Equals(object? obj) => Equals(obj as DrawAnimation);

    public override int GetHashCode() => HashCode.Combine(In, InMs, Out, OutMs, Loop, LoopMs);

    // No "-0" for a text with no angle of its own
    private static string N(double value) => Math.Abs(value) < 0.005 ? "0" : value.ToString("0.##", CultureInfo.InvariantCulture);

    private static float F(string value) => float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}
