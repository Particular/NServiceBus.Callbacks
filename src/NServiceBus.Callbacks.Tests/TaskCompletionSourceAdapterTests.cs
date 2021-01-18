namespace NServiceBus.Callbacks.Tests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class TaskCompletionSourceAdapterTests
    {
        [Test]
        public async Task ShouldSetResultNonBlocking()
        {
            var tcs = new TaskCompletionSource<bool>();
            var adapter = new TaskCompletionSourceAdapter<bool>(tcs);

            var @continue = false;
            // simulate code handling the callback response after await endpoint.Request<Response>(...);
            var result = tcs.Task.ContinueWith(_ =>
            {
                while (!@continue)
                {
                    // spin
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            adapter.TrySetResult(true);
            // TrySetResult should continue immediately
            @continue = true;

            await result;
        }

        [Test]
        public async Task ShouldSetCanceledNonBlocking()
        {
            var tcs = new TaskCompletionSource<bool>();
            var adapter = new TaskCompletionSourceAdapter<bool>(tcs);

            var @continue = false;
            // simulate code handling the callback response after await endpoint.Request<Response>(...);
            var result = tcs.Task.ContinueWith(_ =>
            {
                while (!@continue)
                {
                    // spin
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            adapter.TrySetCanceled();
            // TrySetCanceled should continue immediately
            @continue = true;

            await result;
        }
    }
}