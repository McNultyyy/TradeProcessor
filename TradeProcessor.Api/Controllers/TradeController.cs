using System.ComponentModel;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using TradeProcessor.Api.Contracts;
using TradeProcessor.Api.Contracts.FvgChaser;
using TradeProcessor.Domain;

namespace TradeProcessor.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TradeController : ControllerBase
	{
		private readonly Domain.Services.FvgChaser _fvgChaser;

		public TradeController(Domain.Services.FvgChaser fvgChaser)
		{
			_fvgChaser = fvgChaser;
		}

		[HttpPost(Name = "FvgChaser")]
		public IActionResult FvgChase(FvgChaserRequest request)
		{
			BackgroundJob
				.Enqueue(() =>

					_fvgChaser.DoWork(
						Symbol.Create(request.Symbol).Value,
						request.Interval,
						request.RiskPerTrade,
						request.Stoploss,
						request.TakeProfit,
						request.Bias
						)

					/*
					Execute(
						request.Symbol,
						request.Interval,
						request.RiskPerTrade,
						request.Stoploss,
						request.TakeProfit,
						request.Bias,
						_fvgChaser
					)*/
					);

			return Ok();
		}
		/*
		// todo: workout why this doesn't work :/
		[DisplayName("{6} {0} {1}")] // Used by Hangfire console for JobName
		public static void Execute(
			string symbol,
			string interval,
			decimal riskPerTrade,
			string stoploss,
			string? takeProfit,
			BiasType bias,
			Domain.Services.FvgChaser service)
		{
			service.DoWork(symbol, interval, riskPerTrade, stoploss, takeProfit, bias);
		}
		*/
	}
}
