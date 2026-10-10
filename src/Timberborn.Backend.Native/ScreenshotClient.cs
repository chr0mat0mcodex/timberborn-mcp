using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record CameraVector(float X,float Y,float Z);
public sealed record CameraViewport(float X,float Y,float Width,float Height);
public sealed record ScreenshotCamera(string CoordinateSystem,string Units,CameraVector Position,float Height,CameraVector Forward,
    CameraVector Up,CameraVector EulerDegrees,CameraVector Target,float TargetDistance,float ViewPlaneDepth,string ViewPlane,
    float ViewWidth,float ViewHeight,float? VerticalFovDegrees,float? HorizontalFovDegrees,bool Orthographic,float Aspect,CameraViewport Viewport);
public sealed record NativeScreenshot(string Scope,string MimeType,int Width,int Height,int SourceWidth,int SourceHeight,
    int ByteCount,DateTimeOffset CapturedAtUtc,ScreenshotCamera Camera,string ImageBase64);
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeScreenshot>> Screenshot(ScreenshotRequest r,CancellationToken ct) {
        var e=await Get<NativeScreenshot>($"screenshot?session={r.Session}&maxWidth={r.MaxWidth}&maxHeight={r.MaxHeight}",ct,
            maxResponseBytes:ScreenshotRequest.MaxResponseBytes);
        if(e.BridgeVersion is not ("0.36.0" or "0.37.0")) throw new InvalidDataException("Unsupported screenshot bridge");
        if(e.SessionId!=r.Session) throw new BridgeRejectionException("stale_session");
        ValidateScreenshot(e.Data,r);
        return e;
    }
    public static byte[] ValidateScreenshot(NativeScreenshot d,ScreenshotRequest r) {
        if(d.Scope!="game_view_with_ui" || d.MimeType!="image/jpeg" || d.CapturedAtUtc==default ||
            d.ByteCount is <4 or >ScreenshotRequest.MaxBytes || d.ImageBase64 is null ||
            d.ImageBase64.Length!=4*((d.ByteCount+2)/3) || (d.Width,d.Height)!=r.Fit(d.SourceWidth,d.SourceHeight))
            throw new InvalidDataException("Invalid screenshot metadata");
        var c=d.Camera;
        static bool Vector(CameraVector? v) => v is not null && float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
        if(c is null || c.CoordinateSystem!="unity_world_y_up" || c.Units!="world_units" ||
            c.ViewPlane!="perpendicular_to_forward_through_camera_target" || !Vector(c.Position) || !Vector(c.Forward) ||
            !Vector(c.Up) || !Vector(c.EulerDegrees) || !Vector(c.Target) || c.Height!=c.Position.Y ||
            new[]{c.TargetDistance,c.ViewPlaneDepth,c.ViewWidth,c.ViewHeight,c.Aspect}.Any(v=>!float.IsFinite(v)||v<=0) ||
            c.Viewport is not { Width: >0,Height: >0 } ||
            new[]{c.Viewport.X,c.Viewport.Y,c.Viewport.Width,c.Viewport.Height}.Any(v=>!float.IsFinite(v)) ||
            (c.VerticalFovDegrees.HasValue && !float.IsFinite(c.VerticalFovDegrees.Value)) ||
            (c.HorizontalFovDegrees.HasValue && !float.IsFinite(c.HorizontalFovDegrees.Value)) ||
            (!c.Orthographic && (c.VerticalFovDegrees is null or <=0 or >=180 || c.HorizontalFovDegrees is null or <=0 or >=180)))
            throw new InvalidDataException("Invalid screenshot camera");
        byte[] bytes;
        try { bytes=Convert.FromBase64String(d.ImageBase64); } catch(FormatException) { throw new InvalidDataException("Invalid screenshot encoding"); }
        if(bytes.Length!=d.ByteCount || bytes[0]!=255 || bytes[1]!=216 || bytes[^2]!=255 || bytes[^1]!=217)
            throw new InvalidDataException("Invalid screenshot JPEG");
        // Validate declared dimensions against JPEG SOF, not just MIME or a signature.
        bool found=false;
        for(int i=2;i+3<bytes.Length;) {
            if(bytes[i++]!=255) break;
            while(i<bytes.Length && bytes[i]==255)i++;
            if(i>=bytes.Length)break;
            int marker=bytes[i++]; if(marker==218 || marker==217)break;
            if(marker==1 || marker is >=208 and <=215)continue;
            if(i+1>=bytes.Length)break;
            int length=(bytes[i]<<8)|bytes[i+1]; if(length<2 || i+length>bytes.Length)break;
            if(marker is 192 or 194) {
                if(length<8 || ((bytes[i+3]<<8)|bytes[i+4])!=d.Height || ((bytes[i+5]<<8)|bytes[i+6])!=d.Width)
                    throw new InvalidDataException("Invalid JPEG dimensions");
                found=true;break;
            }
            i+=length;
        }
        if(!found)throw new InvalidDataException("Missing JPEG dimensions");
        return bytes;
    }
}
