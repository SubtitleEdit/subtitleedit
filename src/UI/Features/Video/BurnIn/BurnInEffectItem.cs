using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInEffectItem(string name, BurnInEffectType type)
{

    public string Name { get; set; } = name;
    public BurnInEffectType Type { get; set; } = type;

    public override string ToString()
    {
        return Name;
    }

    public string ApplyEffect(string subtitleText, int screenWidth, int screenHeight, int fontSize, int durationMilliseconds)
    {
        if (Type == BurnInEffectType.FadeInOut)
        {
            return $"{{\\fad(250,250}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.SlowFontSizeChange)
        {
            return $"{{\\t(\\fs{((int)(fontSize * 1.2))})}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.IncreaseFontKerning)
        {
            return $"{{\\t(\\fsp4)}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.FixRightToLeft)
        {
            return Utilities.FixRtlViaUnicodeChars(subtitleText);
        }
        else if (Type == BurnInEffectType.SrollUp)
        {
            return $"{{\\move({screenWidth / 2},{screenHeight - 60},{screenWidth / 2},{screenHeight - 80})}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.ScrollDown)
        {
            return $"{{\\move({screenWidth / 2},{screenHeight - 80},{screenWidth / 2},{screenHeight - 60})}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.RotateIn)
        {
            return $"{{\\frx-30\\t(0,{durationMilliseconds / 2},\\frx0)}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.TiltBounce)
        {
            var step = 250;
            return $"{{\\frz-10\\t(0,{step},\\frz10)\\t({step},{step * 2},\\frz-6)\\t({step * 2},{step * 3},\\frz4)\\t({step * 3},{step * 4},\\frz-2)\\t({step * 4},{step * 5},\\frz1)\\t({step * 5},{durationMilliseconds},\\frz0)}}{subtitleText}";
        }
        else if (Type == BurnInEffectType.FontSizeBounceIn)
        {
            var step = 250;

            // Build the font-size animation sequence
            var fs0 = (int)(fontSize * 0.5);   // start smaller
            var fs1 = (int)(fontSize * 1.2);   // overshoot
            var fs2 = (int)(fontSize * 0.9);   // undershoot
            var fs3 = (int)(fontSize * 1.05);  // slight overshoot
            var fs4 = fontSize;                // settle to final size

            return $"{{\\fs{fs0}" +
                   $"\\t(0,{step},\\fs{fs1})" +
                   $"\\t({step},{step * 2},\\fs{fs2})" +
                   $"\\t({step * 2},{step * 3},\\fs{fs3})" +
                   $"\\t({step * 3},{durationMilliseconds},\\fs{fs4})" +
                   $"}}{subtitleText}";
        }


        return subtitleText; // No effect applied
    }

    public static BurnInEffectItem[] List()
    {
        return
        [
            new BurnInEffectItem("Fix right-to-left", BurnInEffectType.FixRightToLeft),
            new BurnInEffectItem("Fade in/out", BurnInEffectType.FadeInOut),
            new BurnInEffectItem("Font size change", BurnInEffectType.SlowFontSizeChange),
            new BurnInEffectItem("Increase font kerning", BurnInEffectType.IncreaseFontKerning),
            new BurnInEffectItem("Scroll up", BurnInEffectType.SrollUp),
            new BurnInEffectItem("Scroll down", BurnInEffectType.ScrollDown),
            new BurnInEffectItem("Rotate in", BurnInEffectType.RotateIn),
            new BurnInEffectItem("Tilt bounce", BurnInEffectType.TiltBounce),
            new BurnInEffectItem("Font size bounce in", BurnInEffectType.FontSizeBounceIn)
        ];
    }
}