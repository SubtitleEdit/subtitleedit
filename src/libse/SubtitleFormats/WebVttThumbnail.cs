using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    public class WebVttThumbnail : SubtitleFormat
    {
        public override string Extension => "vtt";
        public override string Name => "WebVTT based thumbnails";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var s = new Subtitle();
            LoadSubtitle(s, lines, fileName);
            return s.Paragraphs.Count > 0;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var format = new WebVTT();
            var s = new Subtitle();
            format.LoadSubtitle(s, lines, fileName);

            if (s.Paragraphs.Count == 0)
            {
                return;
            }

            foreach (var p in s.Paragraphs)
            {
                if (p.Text.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (p.Text.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (p.Text.Contains(".png#xywh=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (p.Text.Contains(".jpg#xywh=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return;
            }

            subtitle.Paragraphs.AddRange(s.Paragraphs);
        }

        public SKBitmap GetBitmap(string fileName, Subtitle subtitle, int index)
        {
            if (index < 0 || index >= subtitle.Paragraphs.Count)
            {
                return new SKBitmap(1, 1, true);
            }

            var folder = Path.GetDirectoryName(fileName);
            var raw = subtitle.Paragraphs[index].Text?.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                return new SKBitmap(1, 1, true);
            }

            // ---------------------------------------------------------------------
            // Extract filename and check whether it's a sprite ("xywh=")
            // ---------------------------------------------------------------------
            var imageFileName = raw;
            var useSprite = false;
            int x = 0, y = 0, w = 0, h = 0;

            // Handles .png#xywh=
            var posPng = imageFileName.IndexOf(".png#xywh=", StringComparison.OrdinalIgnoreCase);
            if (posPng >= 0)
            {
                string spec = imageFileName.Substring(posPng + 10); // after ".png#xywh="
                imageFileName = imageFileName.Substring(0, posPng + 4); // keep ".png"
                ParseSpriteSpec(spec, out x, out y, out w, out h);
                useSprite = true;
            }

            // Handles .jpg#xywh=
            var posJpg = imageFileName.IndexOf(".jpg#xywh=", StringComparison.OrdinalIgnoreCase);
            if (posJpg >= 0)
            {
                string spec = imageFileName.Substring(posJpg + 10); // after ".jpg#xywh="
                imageFileName = imageFileName.Substring(0, posJpg + 4); // keep ".jpg"
                ParseSpriteSpec(spec, out x, out y, out w, out h);
                useSprite = true;
            }

            // Handles .jpeg#xywh=
            var posJpeg = imageFileName.IndexOf(".jpeg#xywh=", StringComparison.OrdinalIgnoreCase);
            if (posJpeg >= 0)
            {
                string spec = imageFileName.Substring(posJpg + 11); // after ".jepg#xywh="
                imageFileName = imageFileName.Substring(0, posJpeg + 5); // keep ".jpeg"
                ParseSpriteSpec(spec, out x, out y, out w, out h);
                useSprite = true;
            }

            // ---------------------------------------------------------------------
            // Resolve full path
            // ---------------------------------------------------------------------
            var fullImageFileName = imageFileName;
            if (!File.Exists(fullImageFileName) && folder != null)
            {
                fullImageFileName = Path.Combine(folder, imageFileName);
            }

            if (!File.Exists(fullImageFileName))
            {
                return new SKBitmap(1, 1, true);
            }

            // ---------------------------------------------------------------------
            // If not a sprite, return complete bitmap
            // ---------------------------------------------------------------------
            if (!useSprite)
            {
                using var stream = File.OpenRead(fullImageFileName);
                return SKBitmap.Decode(stream) ?? new SKBitmap(1, 1, true);
            }

            // ---------------------------------------------------------------------
            // Load sprite-sheet and crop to x,y,w,h
            // ---------------------------------------------------------------------
            using (var stream = File.OpenRead(fullImageFileName))
            using (var decoded = SKBitmap.Decode(stream))
            {
                if (decoded == null)
                {
                    return new SKBitmap(1, 1, true);
                }

                // Validate rect
                if (w <= 0 || h <= 0 ||
                    x < 0 || y < 0 ||
                    x + w > decoded.Width ||
                    y + h > decoded.Height)
                {
                    // Invalid coordinates, return blank
                    return new SKBitmap(1, 1, true);
                }

                // Crop region
                var result = new SKBitmap(w, h, decoded.ColorType, decoded.AlphaType);
                decoded.ExtractSubset(result, new SKRectI(x, y, x + w, y + h));
                return result;
            }
        }

        /// <summary>
        /// Parses "0,0,120,67" or "xywh=0,0,120,67" into integers.
        /// </summary>
        private static void ParseSpriteSpec(string spec, out int x, out int y, out int w, out int h)
        {
            x = y = w = h = 0;

            // Remove optional "xywh="
            var clean = spec;
            int idx = clean.IndexOf('=');
            if (idx >= 0)
            {
                clean = clean.Substring(idx + 1);
            }

            var parts = clean.Split(',');
            if (parts.Length == 4 &&
                int.TryParse(parts[0], out x) &&
                int.TryParse(parts[1], out y) &&
                int.TryParse(parts[2], out w) &&
                int.TryParse(parts[3], out h))
            {
                return;
            }

            // parsing failed → leave zeros (will trigger blank image)
        }
    }
}