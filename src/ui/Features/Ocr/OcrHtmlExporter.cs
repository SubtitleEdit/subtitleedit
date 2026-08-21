using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// Writes every subtitle image as a png plus a standalone "index.html" that shows each image
/// next to its OCR text - the SE4 "Save all images with HTML index" feature. The page is one
/// self-contained file (no external css/js/fonts) so it works straight off the file system,
/// and it follows the reader's light/dark preference with a manual override on top.
/// </summary>
public static class OcrHtmlExporter
{
    public const string ImagesSubFolder = "images";

    public sealed class Result
    {
        public int ImageCount { get; init; }
        public string HtmlFileName { get; init; } = string.Empty;
    }

    public static Result Export(
        IReadOnlyList<OcrSubtitleItem> items,
        string folder,
        string? sourceFileName,
        Action<int, int>? progress,
        CancellationToken cancellationToken)
    {
        var imagesFolder = Path.Combine(folder, ImagesSubFolder);
        Directory.CreateDirectory(imagesFolder);

        var rows = new StringBuilder();
        var imageCount = 0;
        var emptyCount = 0;

        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = items[i];
            var imageFileName = string.Format(CultureInfo.InvariantCulture, "{0:0000}.png", i + 1);

            // GetSkBitmap() hands back the item's cached bitmap - never dispose it here.
            var bitmap = item.GetSkBitmap();
            if (bitmap == null)
            {
                continue;
            }

            File.WriteAllBytes(Path.Combine(imagesFolder, imageFileName), bitmap.ToPngArray());
            imageCount++;

            var text = item.Text ?? string.Empty;
            var isEmpty = string.IsNullOrWhiteSpace(text);
            if (isEmpty)
            {
                emptyCount++;
            }

            AppendRow(rows, i + 1, item, $"{ImagesSubFolder}/{imageFileName}", bitmap.Width, bitmap.Height, text, isEmpty);

            progress?.Invoke(i + 1, items.Count);
        }

        var html = BuildHtml(rows.ToString(), imageCount, emptyCount, sourceFileName);
        var htmlFileName = Path.Combine(folder, "index.html");
        File.WriteAllText(htmlFileName, html, new UTF8Encoding(false));

        return new Result { ImageCount = imageCount, HtmlFileName = htmlFileName };
    }

    private static void AppendRow(
        StringBuilder sb,
        int number,
        OcrSubtitleItem item,
        string imagePath,
        int width,
        int height,
        string text,
        bool isEmpty)
    {
        var language = Se.Language.Ocr;
        var start = new TimeCode(item.StartTime).ToString();
        var end = new TimeCode(item.EndTime).ToString();
        var duration = item.Duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

        // data-text drives the client-side filter - plain lower-case text, no markup.
        var searchText = WebUtility.HtmlEncode(HtmlUtil.RemoveHtmlTags(text, true)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .ToLowerInvariant());

        sb.Append("<article class=\"item\" data-text=\"").Append(searchText)
            .Append("\" data-empty=\"").Append(isEmpty ? '1' : '0').AppendLine("\">");
        sb.Append("<div class=\"bar\"><span class=\"num\">#").Append(number.ToString(CultureInfo.InvariantCulture))
            .Append("</span><span class=\"time\">").Append(WebUtility.HtmlEncode(start))
            .Append("<span class=\"arrow\">&#8594;</span>").Append(WebUtility.HtmlEncode(end))
            .Append("</span><span class=\"chip\">").Append(duration).Append("&#8201;s</span>")
            .Append("<span class=\"chip dim\">").Append(width.ToString(CultureInfo.InvariantCulture))
            .Append("&#215;").Append(height.ToString(CultureInfo.InvariantCulture)).AppendLine("</span></div>");
        sb.Append("<div class=\"body\"><div class=\"shot\"><img src=\"").Append(imagePath)
            .Append("\" alt=\"#").Append(number.ToString(CultureInfo.InvariantCulture))
            .AppendLine("\" loading=\"lazy\" /></div>");

        if (isEmpty)
        {
            sb.Append("<div class=\"text empty\">").Append(WebUtility.HtmlEncode(language.HtmlExportNoText)).AppendLine("</div>");
        }
        else
        {
            sb.Append("<div class=\"text\">").Append(FormatText(text)).AppendLine("</div>");
        }

        sb.AppendLine("</div></article>");
    }

    private static readonly Regex SimpleTagRegex = new(@"&lt;(/?)(i|b|u)&gt;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FontColorRegex = new(@"&lt;font\s+color=(?:&quot;|&#39;)?(#?[a-zA-Z0-9]+)(?:&quot;|&#39;)?\s*&gt;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FontEndRegex = new(@"&lt;/font&gt;", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Html-encodes the OCR text, then puts back the handful of tags subtitles actually use so
    /// italics/bold/color still render. Anything else stays visible as literal text.
    /// </summary>
    private static string FormatText(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);
        encoded = SimpleTagRegex.Replace(encoded, "<$1$2>");
        encoded = FontColorRegex.Replace(encoded, m => "<span style=\"color:" + m.Groups[1].Value + "\">");
        encoded = FontEndRegex.Replace(encoded, "</span>");
        return encoded
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />")
            .Replace("\r", "<br />");
    }

    private static string BuildHtml(string rows, int imageCount, int emptyCount, string? sourceFileName)
    {
        var language = Se.Language.Ocr;
        var general = Se.Language.General;

        var heading = string.IsNullOrEmpty(sourceFileName)
            ? language.HtmlExportTitle
            : Path.GetFileName(sourceFileName);
        var subHeading = string.Format(
            CultureInfo.CurrentCulture,
            language.HtmlExportXImagesYWithoutText,
            imageCount,
            emptyCount);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.Append("<title>").Append(WebUtility.HtmlEncode(heading)).Append(" - ")
            .Append(WebUtility.HtmlEncode(language.HtmlExportTitle)).AppendLine("</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(Css);
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body class=\"shot-dark\">");

        sb.AppendLine("<header class=\"top\">");
        sb.AppendLine("<div class=\"top-inner\">");
        sb.Append("<div class=\"title\"><h1>").Append(WebUtility.HtmlEncode(heading)).Append("</h1><p>")
            .Append(WebUtility.HtmlEncode(subHeading)).AppendLine("</p></div>");
        sb.AppendLine("<div class=\"tools\">");
        sb.Append("<input id=\"q\" type=\"search\" spellcheck=\"false\" placeholder=\"")
            .Append(WebUtility.HtmlEncode(language.HtmlExportSearch)).Append("\" aria-label=\"")
            .Append(WebUtility.HtmlEncode(language.HtmlExportSearch)).AppendLine("\" />");
        sb.Append("<label class=\"check\"><input id=\"onlyEmpty\" type=\"checkbox\" /><span>")
            .Append(WebUtility.HtmlEncode(language.HtmlExportOnlyWithoutText)).AppendLine("</span></label>");
        sb.Append("<div class=\"seg\" id=\"shotSeg\" role=\"group\" aria-label=\"")
            .Append(WebUtility.HtmlEncode(language.HtmlExportImageBackground)).AppendLine("\">");
        sb.Append("<button type=\"button\" data-shot=\"dark\" aria-pressed=\"true\">").Append(WebUtility.HtmlEncode(general.Dark)).AppendLine("</button>");
        sb.Append("<button type=\"button\" data-shot=\"light\" aria-pressed=\"false\">").Append(WebUtility.HtmlEncode(general.Light)).AppendLine("</button>");
        sb.Append("<button type=\"button\" data-shot=\"check\" aria-pressed=\"false\">").Append(WebUtility.HtmlEncode(language.HtmlExportCheckerboard)).AppendLine("</button>");
        sb.AppendLine("</div>");
        sb.Append("<div class=\"seg\" id=\"themeSeg\" role=\"group\" aria-label=\"")
            .Append(WebUtility.HtmlEncode(language.HtmlExportTheme)).AppendLine("\">");
        sb.Append("<button type=\"button\" data-theme=\"auto\" aria-pressed=\"true\">").Append(WebUtility.HtmlEncode(general.Auto)).AppendLine("</button>");
        sb.Append("<button type=\"button\" data-theme=\"light\" aria-pressed=\"false\">").Append(WebUtility.HtmlEncode(general.Light)).AppendLine("</button>");
        sb.Append("<button type=\"button\" data-theme=\"dark\" aria-pressed=\"false\">").Append(WebUtility.HtmlEncode(general.Dark)).AppendLine("</button>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</header>");

        sb.AppendLine("<main id=\"list\">");
        sb.Append(rows);
        sb.Append("<p id=\"noMatches\" class=\"no-matches\" hidden>").Append(WebUtility.HtmlEncode(language.HtmlExportNoMatches)).AppendLine("</p>");
        sb.AppendLine("</main>");

        sb.Append("<footer>").Append(WebUtility.HtmlEncode(language.HtmlExportGeneratedBySubtitleEdit)).AppendLine("</footer>");

        sb.AppendLine("<dialog id=\"lightbox\"><img id=\"lightboxImg\" alt=\"\" /></dialog>");

        sb.AppendLine("<script>");
        sb.AppendLine(Script);
        sb.AppendLine("</script>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private const string Css = @"
*,*::before,*::after{box-sizing:border-box}
:root{
  color-scheme:light dark;
  --bg:#f4f6f9; --surface:#fff; --surface-2:#eef1f6; --border:#dde2ea;
  --text:#141922; --muted:#5b6575; --accent:#2f6fd0;
  --warn-bg:#fdf3e2; --warn-border:#efd9ad; --warn-text:#8a5a08;
  --shadow:0 1px 2px rgba(16,24,40,.06),0 10px 26px rgba(16,24,40,.07);
}
@media (prefers-color-scheme:dark){
  :root:not([data-theme=""light""]){
    --bg:#0e1116; --surface:#171b22; --surface-2:#1d222b; --border:#2a3140;
    --text:#e7ebf2; --muted:#9aa4b6; --accent:#79aaf5;
    --warn-bg:#2b2213; --warn-border:#54421f; --warn-text:#e9c374;
    --shadow:0 1px 2px rgba(0,0,0,.5),0 10px 26px rgba(0,0,0,.4);
  }
}
:root[data-theme=""dark""]{
  --bg:#0e1116; --surface:#171b22; --surface-2:#1d222b; --border:#2a3140;
  --text:#e7ebf2; --muted:#9aa4b6; --accent:#79aaf5;
  --warn-bg:#2b2213; --warn-border:#54421f; --warn-text:#e9c374;
  --shadow:0 1px 2px rgba(0,0,0,.5),0 10px 26px rgba(0,0,0,.4);
}
html{scroll-behavior:smooth}
body{
  margin:0; background:var(--bg); color:var(--text);
  font:15px/1.5 -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;
  -webkit-font-smoothing:antialiased;
}
.top{position:sticky; top:0; z-index:5; background:color-mix(in srgb,var(--bg) 86%,transparent);
  backdrop-filter:blur(12px); border-bottom:1px solid var(--border)}
.top-inner{max-width:1180px; margin:0 auto; padding:14px 20px; display:flex; flex-wrap:wrap;
  gap:12px 18px; align-items:center; justify-content:space-between}
.title h1{margin:0; font-size:17px; font-weight:650; letter-spacing:-.01em; word-break:break-all}
.title p{margin:2px 0 0; font-size:12.5px; color:var(--muted)}
.tools{display:flex; flex-wrap:wrap; gap:10px; align-items:center}
#q{width:210px; padding:7px 11px; border-radius:9px; border:1px solid var(--border);
  background:var(--surface); color:var(--text); font-size:13.5px; outline:none}
#q:focus{border-color:var(--accent); box-shadow:0 0 0 3px color-mix(in srgb,var(--accent) 25%,transparent)}
.check{display:flex; gap:7px; align-items:center; font-size:13px; color:var(--muted); cursor:pointer; user-select:none}
.check input{accent-color:var(--accent); margin:0}
.seg{display:inline-flex; background:var(--surface-2); border:1px solid var(--border);
  border-radius:9px; padding:2px; gap:2px}
.seg button{appearance:none; border:0; background:transparent; color:var(--muted); cursor:pointer;
  font:inherit; font-size:12.5px; padding:5px 10px; border-radius:7px; transition:background .12s,color .12s}
.seg button:hover{color:var(--text)}
.seg button[aria-pressed=""true""]{background:var(--surface); color:var(--text); box-shadow:0 1px 2px rgba(0,0,0,.14)}
main{max-width:1180px; margin:0 auto; padding:22px 20px 60px}
.item{background:var(--surface); border:1px solid var(--border); border-radius:14px;
  box-shadow:var(--shadow); overflow:hidden; margin-bottom:16px}
.bar{display:flex; flex-wrap:wrap; align-items:center; gap:10px; padding:9px 14px;
  background:var(--surface-2); border-bottom:1px solid var(--border);
  font:12.5px/1 ui-monospace,SFMono-Regular,Menlo,Consolas,monospace; color:var(--muted)}
.num{color:var(--accent); font-weight:700}
.time{color:var(--text)}
.arrow{margin:0 7px; color:var(--muted)}
.chip{margin-left:auto; padding:3px 8px; border-radius:999px; background:var(--bg); border:1px solid var(--border)}
.chip.dim{margin-left:0; opacity:.75}
.body{display:grid; grid-template-columns:minmax(0,1fr); align-items:stretch}
@media (min-width:900px){ .body{grid-template-columns:minmax(0,1.05fr) minmax(0,1fr)} }
.shot{display:flex; align-items:center; justify-content:center; padding:16px; min-height:96px}
body.shot-dark .shot{background:#0a0b0d}
body.shot-light .shot{background:#f0f0f2}
body.shot-check .shot{
  background-color:#9a9a9a;
  background-image:linear-gradient(45deg,#6f6f6f 25%,transparent 25%,transparent 75%,#6f6f6f 75%),
                   linear-gradient(45deg,#6f6f6f 25%,transparent 25%,transparent 75%,#6f6f6f 75%);
  background-size:18px 18px; background-position:0 0,9px 9px;
}
.shot img{max-width:100%; height:auto; display:block; cursor:zoom-in}
.text{padding:16px 18px; font-size:19px; line-height:1.45; word-wrap:break-word;
  border-top:1px solid var(--border)}
@media (min-width:900px){ .text{border-top:0; border-left:1px solid var(--border)} }
.text.empty{display:flex; align-items:center; font-size:14px; font-style:italic;
  color:var(--warn-text); background:var(--warn-bg)}
@media (min-width:900px){ .text.empty{border-left-color:var(--warn-border)} }
.no-matches{text-align:center; color:var(--muted); padding:48px 0; font-size:14px}
footer{text-align:center; color:var(--muted); font-size:12px; padding:0 20px 34px}
#lightbox{border:0; padding:0; background:transparent; max-width:96vw; max-height:96vh}
#lightbox::backdrop{background:rgba(0,0,0,.78)}
#lightbox img{max-width:96vw; max-height:96vh; display:block; cursor:zoom-out;
  background:#0a0b0d; border-radius:8px}
";

    private const string Script = @"
(function(){
  var root=document.documentElement, body=document.body;
  function select(seg,value,attr){
    seg.querySelectorAll('button').forEach(function(b){
      b.setAttribute('aria-pressed', b.getAttribute(attr)===value ? 'true' : 'false');
    });
  }
  var themeSeg=document.getElementById('themeSeg');
  function applyTheme(t){
    if(t==='light'||t==='dark'){ root.setAttribute('data-theme',t); } else { t='auto'; root.removeAttribute('data-theme'); }
    select(themeSeg,t,'data-theme');
    try{ localStorage.setItem('se-ocr-html-theme',t); }catch(e){}
  }
  themeSeg.addEventListener('click',function(e){
    var b=e.target.closest('button'); if(b){ applyTheme(b.getAttribute('data-theme')); }
  });

  var shotSeg=document.getElementById('shotSeg');
  function applyShot(s){
    if(s!=='light'&&s!=='check'){ s='dark'; }
    body.classList.remove('shot-dark','shot-light','shot-check');
    body.classList.add('shot-'+s);
    select(shotSeg,s,'data-shot');
    try{ localStorage.setItem('se-ocr-html-shot',s); }catch(e){}
  }
  shotSeg.addEventListener('click',function(e){
    var b=e.target.closest('button'); if(b){ applyShot(b.getAttribute('data-shot')); }
  });

  try{
    applyTheme(localStorage.getItem('se-ocr-html-theme')||'auto');
    applyShot(localStorage.getItem('se-ocr-html-shot')||'dark');
  }catch(e){ applyTheme('auto'); applyShot('dark'); }

  var items=Array.prototype.slice.call(document.querySelectorAll('.item'));
  var q=document.getElementById('q');
  var onlyEmpty=document.getElementById('onlyEmpty');
  var noMatches=document.getElementById('noMatches');
  function filter(){
    var needle=q.value.trim().toLowerCase(), empties=onlyEmpty.checked, shown=0;
    for(var i=0;i<items.length;i++){
      var it=items[i];
      var ok=(!empties||it.getAttribute('data-empty')==='1') &&
             (needle===''||it.getAttribute('data-text').indexOf(needle)>=0);
      it.hidden=!ok;
      if(ok){ shown++; }
    }
    noMatches.hidden=shown>0;
  }
  q.addEventListener('input',filter);
  onlyEmpty.addEventListener('change',filter);

  var lightbox=document.getElementById('lightbox'), lightboxImg=document.getElementById('lightboxImg');
  document.getElementById('list').addEventListener('click',function(e){
    var img=e.target.closest('.shot img');
    if(img&&lightbox.showModal){ lightboxImg.src=img.getAttribute('src'); lightbox.showModal(); }
  });
  lightbox.addEventListener('click',function(){ lightbox.close(); });

  document.addEventListener('keydown',function(e){
    if(e.key==='/'&&document.activeElement!==q){ e.preventDefault(); q.focus(); q.select(); }
  });
})();
";
}
