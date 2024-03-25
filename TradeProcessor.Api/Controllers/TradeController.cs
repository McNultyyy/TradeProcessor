using Hangfire.Console.Extensions;
using Microsoft.AspNetCore.Mvc;
using TradeProcessor.Api.Contracts.FvgChaser;
using TradeProcessor.Domain;

namespace TradeProcessor.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TradeController : ControllerBase
	{
		private readonly Domain.Services.FvgChaser _fvgChaser;
		private readonly IJobManager _jobManager;

		public TradeController(Domain.Services.FvgChaser fvgChaser, IJobManager jobManager)
		{
			_fvgChaser = fvgChaser;
			_jobManager = jobManager;
		}

		[HttpPost(Name = "FvgChaser")]
		public IActionResult FvgChaser(FvgChaserRequest request)
		{
#pragma warning disable CS4014
			_jobManager.Start<Domain.Services.FvgChaser>(x => _fvgChaser.DoWork(
				Symbol.Create(request.Symbol).Value,
				request.Interval,
				request.RiskPerTrade,
				request.Stoploss,
				request.SetStoploss,
				request.TakeProfit,
				request.Bias,
				request.NumberOfActiveOrders,
				request.NumberOfTrades,
				request.Gaps
			));
#pragma warning restore CS4014

			return Ok();
		}
	}
}
