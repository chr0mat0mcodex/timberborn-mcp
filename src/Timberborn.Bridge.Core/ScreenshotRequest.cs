using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class ScreenshotRequest
{
    public const int MaxBytes = 768 * 1024;
    public const int MaxResponseBytes = 1100 * 1024;
    public string Session { get; }
    public int MaxWidth { get; }
    public int MaxHeight { get; }
    private ScreenshotRequest(string session, int width, int height) { Session=session; MaxWidth=width; MaxHeight=height; }
    public static ScreenshotRequest Parse(NameValueCollection q) {
        if (q.AllKeys.Any(k => k is not ("session" or "maxWidth" or "maxHeight")) ||
            q.GetValues("session") is not { Length: 1 } ids || !Guid.TryParseExact(ids[0], "D", out var id) || id == Guid.Empty)
            throw new ArgumentException("invalid_screenshot_request");
        int Read(string name, int fallback, int max) {
            var values=q.GetValues(name); if(values is null) return fallback;
            if(values.Length!=1 || !int.TryParse(values[0],NumberStyles.None,CultureInfo.InvariantCulture,out var n) || n<64 || n>max) throw new ArgumentException("invalid_screenshot_size");
            return n;
        }
        return new(id.ToString("D"),Read("maxWidth",1280,1600),Read("maxHeight",720,900));
    }
    public (int Width,int Height) Fit(int width,int height) {
        if(width<=0 || height<=0 || width>8192 || height>8192 || (long)width*height>16777216) throw new BridgeRejectionException("screenshot_unavailable");
        double scale=Math.Min(1,Math.Min((double)MaxWidth/width,(double)MaxHeight/height));
        return (Math.Max(1,(int)Math.Floor(width*scale)),Math.Max(1,(int)Math.Floor(height*scale)));
    }
}
