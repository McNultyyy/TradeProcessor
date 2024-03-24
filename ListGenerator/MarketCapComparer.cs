using System.Net.Http.Json;
using TradeProcessor.Domain;

namespace ListGenerator
{
	public class MarketCapComparer : IComparer<Symbol>
	{
		public static readonly MarketCapComparer Instance = new();

		private Dictionary<string, decimal> _coinMarketCaps = new();

		private MarketCapComparer()
		{
			PopulateList();
		}

		private void PopulateList()
		{
			var client = new HttpClient();
			var response = client.GetFromJsonAsync<Rootobject>(
				"https://www.binance.com/bapi/asset/v2/public/asset-service/product/get-products?includeEtf=true")
				.Result;

			_coinMarketCaps = response.data
				.Where(x => x.q == "USDT")
				.ToDictionary(
				x => x.s.Replace("USDT", ""),
				x => decimal.Parse(x.c) * x.cs);
		}

		public int Compare(Symbol x, Symbol y)
		{
			if (!_coinMarketCaps.ContainsKey(x.Base))
				return -1;
			if (!_coinMarketCaps.ContainsKey(y.Base))
				return 1;

			return _coinMarketCaps[x.Base] > _coinMarketCaps[y.Base]
				? 1
				: -1;
		}
	}


	public class Rootobject
	{
		public string code { get; set; }
		public object message { get; set; }
		public object messageDetail { get; set; }
		public Datum[] data { get; set; }
	}

	public class Datum
	{
		public string s { get; set; }
		public string st { get; set; }
		public string b { get; set; }
		public string q { get; set; }
		public string ba { get; set; }
		public string qa { get; set; }
		public string i { get; set; }
		public string ts { get; set; }
		public string an { get; set; }
		public string qn { get; set; }
		public string o { get; set; }
		public string h { get; set; }
		public string l { get; set; }
		public string c { get; set; }
		public string v { get; set; }
		public string qv { get; set; }
		public decimal y { get; set; }
		public float _as { get; set; }
		public string pm { get; set; }
		public string pn { get; set; }
		public decimal cs { get; set; }
		public string[] tags { get; set; }
		public bool pom { get; set; }
		public object pomt { get; set; }
		public bool lc { get; set; }
		public bool g { get; set; }
		public bool sd { get; set; }
		public bool r { get; set; }
		public bool hd { get; set; }
		public bool rb { get; set; }
		public bool ks { get; set; }
		public bool etf { get; set; }
	}

}
