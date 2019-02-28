using NServiceBus.Callbacks.Testing;
using NUnit.Framework;
using Particular.Approvals;
using PublicApiGenerator;

[TestFixture]
public class APIApprovals
{
    [Test]
    public void Approve()
    {
        var publicApi = ApiGenerator.GeneratePublicApi(typeof(TestableCallbackAwareSession).Assembly);
        Approver.Verify(publicApi);
    }
}