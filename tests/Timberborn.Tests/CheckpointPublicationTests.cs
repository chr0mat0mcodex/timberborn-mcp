using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class CheckpointPublicationTests
{
    private static BuildBatchJob Job()
    {
        var spec = new BuildBatchSpec("11111111-1111-4111-8111-111111111111",
            "22222222-2222-4222-8222-222222222222", "33333333-3333-4333-8333-333333333333",
            1,1,3,8,8,[0],[new("SmallWarehouse.Folktails")],"none",0,24,24,7,300);
        return new() { Spec=spec, Fingerprint=BuildBatchState.Fingerprint(spec) };
    }

    [Theory]
    [InlineData("transient",32,3)]
    [InlineData("transient",33,3)]
    [InlineData("transient",1175,3)]
    [InlineData("exhausted",1175,4)]
    [InlineData("destination_changed",1175,1)]
    [InlineData("temporary_changed",1175,1)]
    [InlineData("published",1175,1)]
    [InlineData("other_io",5,1)]
    public void Exchange_retries_only_known_errors_with_unchanged_files(string scenario,int error,int expectedAttempts)
    {
        string directory=Path.Combine(Path.GetTempPath(),"checkpoint-publication-"+Guid.NewGuid().ToString("N"));
        try {
            int attempts=0;
            // Simulated filesystem outcomes: no native exchange flakiness in fault injection.
            var store=new BuildBatchStore(directory,(temp,target)=> {
                attempts++;
                if (scenario=="published" || scenario=="transient" && attempts==3) {
                    File.WriteAllBytes(target,File.ReadAllBytes(temp));
                    File.Delete(temp);
                    if(scenario=="transient")return;
                }
                if(scenario=="destination_changed")File.WriteAllText(target,"synthetic other writer");
                if(scenario=="temporary_changed")File.WriteAllText(temp,"synthetic changed temporary");
                throw new IOException("synthetic exchange failure",unchecked((int)0x80070000)|error);
            });
            using var lease=store.Acquire();
            var job=Job();store.Save(job);
            string path=Path.Combine(directory,job.Spec.Session+"-"+job.Spec.BatchId+".json");
            byte[] previous=File.ReadAllBytes(path);
            job.State="stopped";job.Reason="synthetic_stop";
            if(scenario is "transient" or "published") {
                store.Save(job);
                Assert.Equal("stopped",store.Load(job.Spec.Session,job.Spec.BatchId)!.State);
            } else {
                var failure=Assert.Throws<IOException>(()=>store.Save(job));
                Assert.Equal(unchecked((int)0x80070000)|error,failure.HResult);
                if(scenario=="destination_changed")Assert.Equal("synthetic other writer",File.ReadAllText(path));
                else Assert.Equal(previous,File.ReadAllBytes(path));
            }
            Assert.Equal(expectedAttempts,attempts);
            Assert.Empty(Directory.EnumerateFiles(directory,"*.tmp"));
        } finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }
}
