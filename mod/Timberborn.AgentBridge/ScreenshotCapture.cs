using System.Collections;
using System.Threading.Tasks;
using Timberborn.Bridge.Core;
using UnityEngine;
using Timberborn.CameraSystem;
namespace Timberborn.AgentBridge;

// Game-frame pixels only. No OS capture, camera movement, file output or image history.
public sealed class ScreenshotCapture : MonoBehaviour
{
    private TaskCompletionSource<object>? pending;
    private Coroutine? routine;
    private float deadline, nextCapture;
    public Task<object> Capture(ScreenshotRequest request, CameraService cameraService) {
        if(pending is not null || Time.realtimeSinceStartup<nextCapture) throw new BridgeRejectionException("screenshot_busy");
        if(cameraService.Transform.GetComponent<Camera>() is not { enabled: true } camera || camera.cameraType!=CameraType.Game) throw new BridgeRejectionException("screenshot_unavailable");
        request.Fit(Screen.width,Screen.height);
        pending=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var task=pending.Task;
        deadline=Time.realtimeSinceStartup+3; nextCapture=Time.realtimeSinceStartup+2;
        routine=StartCoroutine(CaptureFrame(request,cameraService)); return task;
    }
    private IEnumerator CaptureFrame(ScreenshotRequest request, CameraService cameraService) {
        yield return new WaitForEndOfFrame();
        Texture2D? source=null, scaled=null; RenderTexture? target=null;
        var previous=RenderTexture.active;
        try {
            if(pending is null) yield break;
            request.Fit(Screen.width,Screen.height);
            var camera=cameraService.Transform.GetComponent<Camera>();
            if(camera is null || !camera.enabled) throw new InvalidOperationException();
            var position=camera.transform.position; var forward=camera.transform.forward;
            var targetPoint=cameraService.Target;
            float planeDepth=Vector3.Dot(targetPoint-position,forward);
            if(planeDepth<=0) throw new InvalidOperationException();
            var corners=new Vector3[4];
            camera.CalculateFrustumCorners(new Rect(0,0,1,1),planeDepth,Camera.MonoOrStereoscopicEye.Mono,corners);
            float viewWidth=Vector3.Distance(corners[0],corners[3]), viewHeight=Vector3.Distance(corners[0],corners[1]);
            object V(Vector3 v) => new { x=v.x,y=v.y,z=v.z };
            var cameraMetadata=new { coordinateSystem="unity_world_y_up",units="world_units",position=V(position),height=position.y,
                forward=V(forward),up=V(camera.transform.up),eulerDegrees=V(camera.transform.eulerAngles),target=V(targetPoint),
                targetDistance=Vector3.Distance(position,targetPoint),viewPlaneDepth=planeDepth,
                viewPlane="perpendicular_to_forward_through_camera_target",viewWidth,viewHeight,
                verticalFovDegrees=camera.orthographic?(float?)null:Vector3.Angle(corners[0]+corners[3],corners[1]+corners[2]),
                horizontalFovDegrees=camera.orthographic?(float?)null:Vector3.Angle(corners[0]+corners[1],corners[2]+corners[3]),
                orthographic=camera.orthographic,aspect=camera.aspect,
                viewport=new { x=camera.rect.x,y=camera.rect.y,width=camera.rect.width,height=camera.rect.height } };
            var capturedAt=DateTimeOffset.UtcNow;
            source=ScreenCapture.CaptureScreenshotAsTexture(1);
            if(source is null) throw new InvalidOperationException();
            var size=request.Fit(source.width,source.height);
            target=RenderTexture.GetTemporary(size.Width,size.Height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(source,target); RenderTexture.active=target;
            scaled=new Texture2D(size.Width,size.Height,TextureFormat.RGB24,false);
            scaled.ReadPixels(new Rect(0,0,size.Width,size.Height),0,0,false);
            var bytes=ImageConversion.EncodeToJPG(scaled,75);
            if(bytes is null || bytes.Length==0 || bytes.Length>ScreenshotRequest.MaxBytes) throw new BridgeRejectionException("screenshot_too_large");
            pending.TrySetResult(new { scope="game_view_with_ui", mimeType="image/jpeg", width=size.Width,height=size.Height,
                sourceWidth=source.width,sourceHeight=source.height,byteCount=bytes.Length,capturedAtUtc=capturedAt,
                camera=cameraMetadata,imageBase64=Convert.ToBase64String(bytes) });
        } catch(BridgeRejectionException ex) { pending?.TrySetException(ex); }
        catch(Exception) { pending?.TrySetException(new BridgeRejectionException("screenshot_unavailable")); }
        finally {
            RenderTexture.active=previous;
            if(target is not null) RenderTexture.ReleaseTemporary(target);
            if(source is not null) Destroy(source);
            if(scaled is not null) Destroy(scaled);
            pending=null; routine=null;
        }
    }
    private void Update() { if(pending is not null && Time.realtimeSinceStartup>=deadline) Cancel(); }
    public void Cancel() {
        if(routine is not null) StopCoroutine(routine);
        pending?.TrySetException(new BridgeRejectionException("screenshot_unavailable"));
        pending=null; routine=null;
    }
    private void OnDestroy() => Cancel();
}
