namespace Timberborn.Bridge.Core;

public static class ProjectPreviewPolicy
{
    // Deferred support is a limited preview scenario, never an overlap exemption.
    public static bool CanRegister(bool nativeValid, bool deferredSupport, bool collision) =>
        !collision && (nativeValid || deferredSupport);
}
