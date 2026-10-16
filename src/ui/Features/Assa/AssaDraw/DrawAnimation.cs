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
/// Where a layer (or a group's layers) is at a moment of its animation: moved by Dx/Dy, scaled and
/// turned around the anchor, faded to Opacity. Rotation is degrees clockwise on screen.
/// </summary>
public readonly record struct DrawAnimationState(float Dx, float Dy, float Scale, float Rotation, float Opacity)
{
    public static readonly DrawAnimationState Rest = new(0, 0, 1, 0, 1);
}

/// <summary>
/// The in and out effect of a line. It is written as ASSA override tags (\move, \fad, \t with \fscx,
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

    public bool IsNone => In == DrawAnimationEffect.None && Out == DrawAnimationEffect.None;

    public DrawAnimation Clone() => new() { In = In, InMs = InMs, Out = Out, OutMs = OutMs };

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
    /// \fad and the scale/rotation tags (everything but the position and \org). <paramref name="baseFrz"/>
    /// is the line's own \frz, which a spin turns from and back to; <paramref name="outSlideDegrees"/>
    /// the turn of a double slide's slide out (<see cref="GetOutSlideRotation"/>).
    /// </summary>
    public string GetEffectTags(int lineMs, float baseFrz = 0, double outSlideDegrees = 0)
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
                sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{lineMs},{N(EaseIn)},\\frz{N(baseFrz - 360)}\\fscx0\\fscy0)");
                break;
            default:
                if (IsDoubleSlide)
                {
                    // Seven decimals: the far \org turns a thousandth of a degree into ~17 px
                    var degrees = (baseFrz + outSlideDegrees).ToString("0.#######", CultureInfo.InvariantCulture);
                    sb.Append(CultureInfo.InvariantCulture, $"\\t({outStart},{lineMs},{N(EaseIn)},\\frz{degrees})");
                }

                break;
        }

        return sb.ToString();
    }

    public bool HasRotation => In == DrawAnimationEffect.Spin || Out == DrawAnimationEffect.Spin;

    private static readonly Regex MarkerRegex = new(
        @"\{assadraw-anim c=(-?[\d.]+),(-?[\d.]+) in=(\w+),(\d+) out=(\w+),(\d+)\}",
        RegexOptions.Compiled);

    /// <summary>
    /// The comment block that keeps the animation and its anchor in the line.
    /// </summary>
    public string FormatMarker(float anchorX, float anchorY) =>
        $"{{assadraw-anim c={N(anchorX)},{N(anchorY)} in={In},{InMs.ToString(CultureInfo.InvariantCulture)} out={Out},{OutMs.ToString(CultureInfo.InvariantCulture)}}}";

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
        other != null && In == other.In && InMs == other.InMs && Out == other.Out && OutMs == other.OutMs;

    public override bool Equals(object? obj) => Equals(obj as DrawAnimation);

    public override int GetHashCode() => HashCode.Combine(In, InMs, Out, OutMs);

    // No "-0" for a text with no angle of its own
    private static string N(double value) => Math.Abs(value) < 0.005 ? "0" : value.ToString("0.##", CultureInfo.InvariantCulture);

    private static float F(string value) => float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}
