using System;
using System.Threading;

namespace DuckyProfileSwitcher
{
    class LinkedTimeoutCancellationToken : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationTokenRegistration registration;

        public LinkedTimeoutCancellationToken(CancellationToken cancellationToken, int timeoutMs)
        {
            cancellationTokenSource = new CancellationTokenSource(timeoutMs);
            registration = cancellationToken.Register(() =>
            {
                cancellationTokenSource.Cancel();
            });
        }

        public CancellationToken Token => cancellationTokenSource.Token;

        public void Dispose()
        {
            registration.Dispose();
            cancellationTokenSource.Dispose();
        }
    }
}
