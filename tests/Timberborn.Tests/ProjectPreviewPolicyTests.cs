using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProjectPreviewPolicyTests
{
    [Theory]
    [InlineData(true,false)]
    [InlineData(false,true)]
    public void Occupied_path_node_never_enters_preview_services_even_when_validator_or_support_allows_it(bool valid,bool deferred)
    {
        Assert.False(ProjectPreviewPolicy.CanRegister(valid,deferred,collision:true));
    }

    [Fact]
    public void Invalid_geometry_is_not_registered_but_scoped_support_and_valid_free_geometry_remain_available()
    {
        Assert.False(ProjectPreviewPolicy.CanRegister(false,false,false));
        Assert.True(ProjectPreviewPolicy.CanRegister(false,true,false));
        Assert.True(ProjectPreviewPolicy.CanRegister(true,false,false));
    }

    [Fact]
    public void Preview_runtime_failure_is_not_a_safe_predispatch_rejection()
    {
        Assert.True(BridgeRejectionException.IsCode("project_preview_failed"));
        Assert.False(LargeProjectPlanningPolicy.IsPreDispatchCode("project_preview_failed"));
        Assert.False(LargeProjectStartPolicy.IsPreDispatchCode("project_preview_failed"));
    }
}
