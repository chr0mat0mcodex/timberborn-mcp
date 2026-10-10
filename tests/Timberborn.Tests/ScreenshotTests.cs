using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class ScreenshotTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private static ScreenshotRequest Request()=>ScreenshotRequest.Parse(new NameValueCollection { ["session"]=Session });
    // Synthetic JPEG marker fixture, not a personal image or a decoder test.
    private static NativeScreenshot Frame() {
        byte[] bytes=[255,216,255,192,0,8,8,2,208,5,0,3,255,217];
        return new("game_view_with_ui","image/jpeg",1280,720,1920,1080,bytes.Length,DateTimeOffset.UnixEpoch,
            new("unity_world_y_up","world_units",new(1,10,3),10,new(0,0,1),new(0,1,0),new(0,0,0),new(1,10,23),20,20,
            "perpendicular_to_forward_through_camera_target",30,17,45,65,false,16f/9,new(0,0,1,1)),Convert.ToBase64String(bytes));
    }
    [Fact] public void Bounds_and_aspect_are_enforced_without_upscaling() {
        Assert.Equal((1280,720),Request().Fit(3840,2160));
        Assert.Equal((405,720),Request().Fit(1080,1920));
        Assert.Equal((320,200),Request().Fit(320,200));
        Assert.Throws<BridgeRejectionException>(()=>Request().Fit(10000,10000));
        Assert.Throws<ArgumentException>(()=>ScreenshotRequest.Parse(new NameValueCollection { ["session"]=Session,["maxWidth"]="1601" }));
        Assert.Throws<ArgumentException>(()=>ScreenshotRequest.Parse(new NameValueCollection { ["session"]=Session,["extra"]="1" }));
        Assert.Throws<ArgumentException>(()=>ScreenshotRequest.Parse(new NameValueCollection { { "session",Session },{ "session",Session } }));
    }
    [Fact] public void Camera_payload_and_JPEG_dimensions_are_validated() {
        var f=Frame(); Assert.Equal(f.ByteCount,NativeClient.ValidateScreenshot(f,Request()).Length);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateScreenshot(f with { Width=1200 },Request()));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateScreenshot(f with { Camera=f.Camera with { Height=99 } },Request()));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateScreenshot(f with { Camera=f.Camera with { HorizontalFovDegrees=float.NaN } },Request()));
        var b=Convert.FromBase64String(f.ImageBase64);b[10]=1;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateScreenshot(f with { ImageBase64=Convert.ToBase64String(b) },Request()));
    }
    [Fact] public void Pixels_appear_only_in_one_MCP_image_block() {
        var f=Frame(); var json=JsonSerializer.SerializeToNode(new NativeResult<NativeScreenshot>(1,"ok",f,new("native",false,Session,DateTimeOffset.UnixEpoch),null),NativeJson.Options)!.AsObject();
        var result=ScreenshotTools.ToToolResult(json);
        Assert.Equal(2,result.Content.Count);
        var image=Assert.IsType<ImageContentBlock>(result.Content[1]); Assert.Equal("image/jpeg",image.MimeType);
        Assert.Equal(Convert.FromBase64String(f.ImageBase64),image.DecodedData.ToArray());
        Assert.DoesNotContain("imageBase64",Assert.IsType<TextContentBlock>(result.Content[0]).Text);
        Assert.DoesNotContain("imageBase64",result.StructuredContent!.Value.GetRawText());
        Assert.NotNull(json["data"]!["imageBase64"]); // Presentation does not mutate backend result.
        Assert.True(ScreenshotTools.Reader().Annotations!.ReadOnlyHint);
    }
    [Fact] public async Task Async_frame_does_not_block_queue_and_unload_cancels_pending_response() {
        using var queue=new MainThreadQueue();
        var frame=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var request=BridgeRequest.Parse("/agent-api/v1/screenshot",new NameValueCollection { ["session"]=Session });
        var first=queue.Enqueue(request,CancellationToken.None);queue.PumpAsync(_=>frame.Task);
        var second=queue.Enqueue(BridgeRequest.Parse("/agent-api/v1/simulation",new()),CancellationToken.None);
        queue.PumpAsync(_=>Task.FromResult("simulation")); Assert.Equal("simulation",await second);
        Assert.False(first.IsCompleted);queue.Dispose();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>first);
        frame.SetResult("late image discarded");
    }
    [Fact] public async Task Async_frame_preserves_explicit_rejection() {
        using var queue=new MainThreadQueue();var first=queue.Enqueue(BridgeRequest.Parse("/agent-api/v1/screenshot",new NameValueCollection { ["session"]=Session }),CancellationToken.None);
        queue.PumpAsync(_=>Task.FromException<string>(new BridgeRejectionException("screenshot_unavailable")));
        var error=await Assert.ThrowsAsync<BridgeRejectionException>(()=>first);Assert.Equal("screenshot_unavailable",error.Code);
    }
}
