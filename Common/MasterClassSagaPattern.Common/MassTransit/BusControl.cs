using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Common
{
    public class BusControl : IHostedService
    {
        private readonly IBusControl busControl;

        public BusControl(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await busControl.StopAsync(cancellationToken);
        }
    }
}
