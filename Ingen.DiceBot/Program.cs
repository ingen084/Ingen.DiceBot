using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ingen.DiceBot
{
	class Program
	{
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private DiscordSocketClient Client { get; set; }
		async Task MainAsync()
		{
			var random = new Random();
			var diceRegex = new Regex("(?<pre>[^0-9]*)(?<count>[0-9]{1,2})(d|ｄ)(?<size>[0-9]{0,3})(?<argument>[-+*/%ー－＋＊×÷／…^＾]([0-9]{1,4}|[0-9]{0,4}\\.[0-9]{1,3}))*(!|！)(?<suf>.*)", RegexOptions.Compiled | RegexOptions.ECMAScript);

			using (Client = new DiscordSocketClient())
			{
				Client.Log += Log;

				Console.WriteLine("Hello World!");

				Client.MessageReceived += async messageParam =>
				{
					try
					{
						if (!(messageParam is SocketUserMessage message)) return;

						var matches = diceRegex.Matches(message.Content);
						if (matches.Count <= 0) return;

						var results = new List<string>();
						string diceProcessText = null;
						//Console.WriteLine($"MatchCount: {matches.Count}");
						foreach (Match match in matches)
						{
							var preStr = match.Groups["pre"].Value;
							var diceCount = int.Parse(match.Groups["count"].Value);
							var diceSize = int.Parse(string.IsNullOrWhiteSpace(match.Groups["size"].Value) ? "6" : match.Groups["size"].Value);

							var sufStr = match.Groups["suf"].Value;

							StringBuilder diceProcessTextBuilder = new StringBuilder();
							decimal? totalAmount = 0;
							for (var count = 0; count < diceCount; count++)
							{
								int amount = random.Next(diceSize) + 1;
								totalAmount += amount;
								diceProcessTextBuilder.Append((count != 0 ? "+" : "") + amount);
							}

							diceProcessText = $"({diceProcessTextBuilder.ToString()})";
							foreach (Capture cap in match.Groups["argument"].Captures)
								if (!string.IsNullOrWhiteSpace(cap.Value))
								{
									switch (cap.Value[0])
									{
										case '+':
										case '＋':
											totalAmount += decimal.Parse(cap.Value.Substring(1));
											break;
										case '-':
										case 'ー':
										case '－':
											totalAmount -= decimal.Parse(cap.Value.Substring(1));
											break;
										case '*':
										case '＊':
										case '×':
											totalAmount *= decimal.Parse(cap.Value.Substring(1));
											break;
										case '/':
										case '÷':
										case '／':
											var d1 = decimal.Parse(cap.Value.Substring(1));
											if (d1 <= 0)
											{
												totalAmount = null;
												break;
											}
											totalAmount /= d1;
											break;
										case '%':
										case '％':
											var d2 = decimal.Parse(cap.Value.Substring(1));
											if (d2 <= 0)
											{
												totalAmount = null;
												break;
											}
											totalAmount %= d2;
											break;
										case '^':
										case '＾':
											if (totalAmount is decimal)
												totalAmount = (decimal)Math.Pow(decimal.ToDouble(totalAmount.Value), double.Parse(cap.Value.Substring(1)));
											break;
									}
									diceProcessText += cap.Value;
									if (totalAmount == null)
										break;
								}
							diceProcessText = $"{totalAmount?.ToString() ?? "未定義" } = {diceProcessText}";

							//Console.WriteLine("過程: " + diceProcessText);
							//Console.WriteLine("結果: " + (totalAmount?.ToString() ?? "未定義"));
							results.Add($"{preStr}`{(totalAmount?.ToString() ?? "未定義")}`{sufStr}");
							//Console.WriteLine($"出力: {preStr}`{(totalAmount?.ToString() ?? "未定義")}`{sufStr}\n");
						}

						await message.AddReactionAsync(new Emoji("🎲"));
						await message.Channel.SendMessageAsync(
							text: message.Author.Mention,
							embed: new EmbedBuilder().WithTitle("Dice Roll")
													 .WithColor(255, 0, 0)
													 .WithDescription($"{string.Concat(results)}{(matches.Count == 1 ? "\n" + diceProcessText : "")}").Build());
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				};

				if (!File.Exists("token"))
				{
					using var t = File.Create("token");
					return;
				}

				await Client.LoginAsync(TokenType.Bot, File.ReadAllText("token"));
				await Client.StartAsync();
				await Client.SetGameAsync("Let's dice roll!!!!!!");

				Console.ReadLine();

				await Client.LogoutAsync();
				await Client.StopAsync();

				await Task.Delay(500);
			}
		}
	}
}
