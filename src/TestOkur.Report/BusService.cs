﻿namespace TestOkur.Report
{
	using System.Threading;
	using System.Threading.Tasks;
	using MassTransit;
	using Microsoft.Extensions.Hosting;

	public class BusService : IHostedService
	{
		private readonly IBusControl _busControl;

		public BusService(IBusControl busControl)
		{
			_busControl = busControl;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _busControl.StartAsync(cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _busControl.StopAsync(cancellationToken);
		}
	}
}