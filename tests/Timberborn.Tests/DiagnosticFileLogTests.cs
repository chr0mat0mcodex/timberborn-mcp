using System.Text.Json;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class DiagnosticFileLogTests : IDisposable
{
    private readonly string directory=Path.Combine(Path.GetTempPath(),"synthetic-diagnostics-"+Guid.NewGuid().ToString("N"));
    private DiagnosticFileLog Logger(Action? warning=null)=>new(directory,"mcp",value=>JsonSerializer.Serialize(value),warning);
    private string OnlyFile()=>Assert.Single(Directory.GetFiles(directory,"*.log"));
    public void Dispose(){if(Directory.Exists(directory))Directory.Delete(directory,true);}

    [Fact]
    public void RecordsIncludeDetailsAndCorrelationsButRedactKnownSecrets()
    {
        var logger=Logger();var id="11111111-1111-1111-1111-111111111111";
        logger.Write("call_started","inspect_simulation_run","running",callId:id,session:id,correlations:[new("run",id,"request")],
            reasoning:"Synthetische Prüfung Bearer synthetic-secret",summary:"limit=1",saveName:"Testkolonie--Testsave");
        logger.Write("call_finished","Bearer synthetic-secret\nprivate-path","ok",new string('a',64),id,id);
        var lines=File.ReadAllLines(OnlyFile());
        Assert.Collection(lines,line=>Assert.Contains("call_started",line),line=>Assert.Contains("call_finished",line));
        using var first=JsonDocument.Parse(lines[0]);using var second=JsonDocument.Parse(lines[1]);
        Assert.Equal(first.RootElement.GetProperty("instanceId").GetString(),second.RootElement.GetProperty("instanceId").GetString());
        Assert.Equal(id,first.RootElement.GetProperty("callId").GetString());
        Assert.Equal(2,second.RootElement.GetProperty("sequence").GetInt64());
        Assert.Equal("unknown",second.RootElement.GetProperty("operation").GetString());
        Assert.Equal("unknown",second.RootElement.GetProperty("code").GetString());
        Assert.Single(first.RootElement.GetProperty("correlations").EnumerateArray());
        Assert.DoesNotContain("synthetic-secret",string.Join("",lines));
        Assert.Contains("[redacted]",first.RootElement.GetProperty("reasoning").GetString());
        Assert.Equal("limit=1",first.RootElement.GetProperty("summary").GetString());
    }

    [Fact]
    public void ReloadingSameSaveAndRestartingServerNeverReusesFiles()
    {
        var logger=Logger();var first=Guid.NewGuid().ToString("D");var second=Guid.NewGuid().ToString("D");
        logger.Write("server_starting");
        logger.Write("call_finished",session:first,saveName:"Testsave");
        logger.Write("call_finished",session:second,saveName:"Testsave");
        Logger().Write("call_finished",session:second,saveName:"Testsave");
        Assert.Single(Directory.GetFiles(directory,"mcp_prozess_*.log"));
        var saves=Directory.GetFiles(directory,"mcp_Testsave_*.log");
        Assert.Collection(saves,_=>{},_=>{},_=>{});
        Assert.All(saves,file=>Assert.Matches(@"mcp_Testsave_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\.\d{7}Z_[a-f0-9]{32}\.log$",file));
        logger.Write("call_finished",session:first);
        Assert.Contains(saves,file=>File.ReadAllLines(file).Length==2);
    }

    [Theory]
    [InlineData("../Test:Save\\name?", "_Test_Save_name_")]
    [InlineData("...", "unbekannter-spielstand")]
    [InlineData("Grüne Kolonie", "Grüne Kolonie")]
    public void SaveNameCannotEscapeDirectory(string name,string expected)=>Assert.Equal(expected,DiagnosticFileLog.FileNamePart(name));

    [Fact]
    public void FileLargerThanFormerLimitRemainsAppendOnly()
    {
        var logger=Logger();logger.Write("server_starting");string file=OnlyFile();
        string existing=File.ReadAllText(file)+new string(' ',2*1024*1024)+"\n";
        File.WriteAllText(file,existing);
        logger.Write("server_stopping");
        Assert.Equal(file,OnlyFile());Assert.StartsWith(existing,File.ReadAllText(file));
        Assert.Equal(0,logger.FailedWrites);
    }

    [Fact]
    public void LockedFileDoesNotRetryAndReportsMissingRecordsAfterRecovery()
    {
        int warnings=0;var logger=Logger(()=>warnings++);logger.Write("server_starting");string file=OnlyFile();
        using(var held=new FileStream(file,FileMode.Open,FileAccess.ReadWrite,FileShare.None)) {
            logger.Write("call_started");logger.Write("call_finished");
            Assert.Equal(2,logger.FailedWrites);Assert.Equal(1,warnings);
        }
        logger.Write("server_stopping");
        using var record=JsonDocument.Parse(File.ReadAllLines(file).Last());
        Assert.Equal(4,record.RootElement.GetProperty("sequence").GetInt64());
        Assert.Equal(2,record.RootElement.GetProperty("failedWrites").GetInt64());
    }

    [Fact]
    public void InvalidDirectoryAndEvenWarningFailureNeverEscape()
    {
        Directory.CreateDirectory(directory);string blocked=Path.Combine(directory,"blocked");File.WriteAllText(blocked,"synthetic");
        var logger=new DiagnosticFileLog(blocked,"bridge",value=>JsonSerializer.Serialize(value),()=>throw new InvalidOperationException("synthetic"));
        logger.Write("bridge_ready");logger.Write("request_finished");Assert.Equal(2,logger.FailedWrites);
        Assert.Equal("synthetic",File.ReadAllText(blocked));
    }

    [Fact]
    public void ConcurrentCallsProduceCompleteSeparateRecords()
    {
        var logger=Logger();Parallel.For(0,12,_=>logger.Write("call_finished","inspect_colony","ok"));
        var sequences=File.ReadAllLines(OnlyFile()).Select(line=> {using var record=JsonDocument.Parse(line);return record.RootElement.GetProperty("sequence").GetInt64();}).ToArray();
        Assert.Equal(Enumerable.Range(1,12).Select(i=>(long)i),sequences);Assert.Equal(0,logger.FailedWrites);
    }
}
