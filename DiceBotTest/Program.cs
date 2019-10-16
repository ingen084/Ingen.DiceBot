using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DiceBotTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var random = new Random();
			var diceRegex = new Regex("(?<pre>[^0-9]*)(?<count>[0-9]{1,2})(d|ｄ)(?<size>[0-9]{0,3})(?<argument>[-+*/%][0-9]{1,4}){0,1}(?<suf>.*)", RegexOptions.Compiled | RegexOptions.ECMAScript);
			string str = null;
			while ((str = Console.ReadLine()) != "quit")
			{
				var matches = diceRegex.Matches(str);
				Console.WriteLine($"MatchCount: {matches.Count}");
				foreach (Match match in matches)
				{
					var preStr = match.Groups["pre"].Value;
					var diceCount = int.Parse(match.Groups["count"].Value);
					var diceSize = int.Parse(string.IsNullOrWhiteSpace(match.Groups["size"].Value) ? "6" : match.Groups["size"].Value);

					var argument = match.Groups["argument"].Value;

					var sufStr = match.Groups["suf"].Value;

					StringBuilder diceProcessTextBuilder = new StringBuilder();
					long? totalAmount = 0;
					for (var count = 0; count < diceCount; count++)
					{
						int amount = random.Next(diceSize) + 1;
						totalAmount += amount;
						diceProcessTextBuilder.Append((count == 0 ? "+" : "") + amount);
					}

					string diceProcessText = $"({diceProcessTextBuilder.ToString()})";
					if (!string.IsNullOrWhiteSpace(argument))
					{
						switch (argument[0])
						{
							case '+':
								totalAmount += int.Parse(argument.Substring(1));
								break;
							case '-':
								totalAmount -= int.Parse(argument.Substring(1));
								break;
							case '*':
								totalAmount *= int.Parse(argument.Substring(1));
								break;
							case '/':
								var d1 = int.Parse(argument.Substring(1));
								if (d1 <= 0)
								{
									totalAmount = null;
									break;
								}
								totalAmount /= d1;
								break;
							case '%':
								var d2 = int.Parse(argument.Substring(1));
								if (d2 <= 0)
								{
									totalAmount = null;
									break;
								}
								totalAmount %= d2;
								break;
						}
						diceProcessText += argument;
					}
					diceProcessText = $"{totalAmount?.ToString() ?? "未定義" } = {diceProcessText}";

					Console.WriteLine("過程: " + diceProcessText);
					Console.WriteLine("結果: " + (totalAmount?.ToString() ?? "未定義"));
					Console.WriteLine($"出力: {preStr}`{(totalAmount?.ToString() ?? "未定義")}`{sufStr}\n");
				}
			}
		}
	}
}
