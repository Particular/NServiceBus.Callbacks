namespace NServiceBus.Core.Analyzer.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class AwaitOrCaptureTasksAnalyzerTests : DiagnosticVerifier
    {
        // IPipelineContext
        [TestCase("IPipelineContext", "obj.Send(new object(), new SendOptions());")]

        // Callbacks extensions
        [TestCase("IMessageSession", "obj.Request<object>(new object());")]
        [TestCase("IMessageSession", "obj.Request<object>(new object(), CancellationToken.None);")]
        [TestCase("IMessageSession", "obj.Request<object>(new object(), new SendOptions());")]
        [TestCase("IMessageSession", "obj.Request<object>(new object(), new SendOptions(), CancellationToken.None);")]
        
        public async Task DiagnosticIsReported(string type, string call)
        {
            var source =
$@"using NServiceBus;
using System.Threading;
public class Foo
{{
    public void Bar({type} obj)
    {{
        {call}
    }}
}}";

            var expected = new DiagnosticResult
            {
                Id = "NSB0001",
                Message = "Expression calling an NServiceBus method creates a Task that is not awaited or assigned to a variable.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 9) },
            };

            await Verify(source, expected);
        }

        [Test]
        public async Task DiagnosticsIsReportedForAsyncMethods()
        {
            var source =
@"using NServiceBus;
public class Foo
{
    public async Task Bar(IMessageSession session)
    {
        session.Request<object>(new object());
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "NSB0001",
                Message = "Expression calling an NServiceBus method creates a Task that is not awaited or assigned to a variable.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 9) },
            };

            await Verify(source, expected);
        }

        [TestCase("")]
        [TestCase(
            @"using NServiceBus;
public class Foo
{
    public async Task Bar(IMessageSession session)
    {
        await session.Request<object>(new object());
    }
}")]
        [TestCase(
            @"using NServiceBus;
public class Foo
{
    public Task Bar(IMessageSession session) =>
        session.Request<object>(new object());
}")]
        [TestCase(
            @"using NServiceBus;
public class Foo
{
    public Task Bar(IMessageSession session)
    {
        session.Request<object>(new object()).GetAwaiter().GetResult();
    }
}")]
        [TestCase(
            @"using NServiceBus;
public class Foo
{
    public Task Bar(IMessageSession session)
    {
        session.Request<object>(new object()).Wait();
    }
}")]
        [TestCase(
            @"using NServiceBus;
public class Foo
{
    public Task Bar(IMessageSession session)
    {
        session.Request<object>(new object()).ConfigureAwait(false);
    }
}")]
        public async Task NoDiagnosticIsReported(string source) => await Verify(source);

        protected override DiagnosticAnalyzer GetAnalyzer() => new AwaitOrCaptureTasksAnalyzer();
    }
}