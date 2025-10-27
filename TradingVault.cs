using TradingVault.Interfaces;
using TradingVault.Interfaces.SignalTracker;
using TradingVault.Interfaces.SymbolBasedArchitecture;
using TradingVault.Services;

namespace TradingVault;

public class TradingVault(
    ITelegramService telegramService,
    IBinanceClient binanceClient,
    IKlineProcessor klineProcessor,
    ITradingAnalyzer tradingAnalyzer,
    IInputParser inputParser,
    // ISignalTrackerFactory signalTrackerFactory,
    ISignalTrackerFactorySB signalTrackerFactory,
    IOutputService outputService)
    : ITradingVault
{
    private bool _isTradeVaultRunning = true;

    public async Task Run()
    {
        var binanceTrackerCTS = new CancellationTokenSource();
        var token = binanceTrackerCTS.Token;

        Console.WriteLine("App Running");
        await telegramService.SendMessageAsync("App Running.");
        await telegramService.InitializeLastUpdateId();

        while (_isTradeVaultRunning)
        {
            var message = await telegramService.ListenForCommands();

            try
            {
                switch (message)
                {
                    case (null): break;

                    case "stop trackers":
                        signalTrackerFactory.StopAllTrackers();
                        ;
                        break;

                    case "trackers count":
                        await signalTrackerFactory.RequestActiveTrackersCount();
                        break;

                    default:
                        if (message.StartsWith("rsi below "))
                        {
                            var rsiRequest = inputParser.ParseRsiCommand(message);
                            if (rsiRequest != null)
                            {
                                var rsiRsults =
                                    await klineProcessor.CheckRsiBellow(rsiRequest.RsiValue, rsiRequest.Interval);
                                await outputService.HandleRsiBelow(rsiRsults, rsiRequest.Interval, rsiRequest.RsiValue);
                            }

                            break;
                        }

                        if (message.StartsWith("rsi above "))
                        {
                            var rsiRequest = inputParser.ParseRsiCommand(message);
                            if (rsiRequest != null)
                            {
                                var rsiRsults =
                                    await klineProcessor.CheckRsiAbove(rsiRequest.RsiValue, rsiRequest.Interval);
                                await outputService.HandleRsiAbove(rsiRsults, rsiRequest.Interval, rsiRequest.RsiValue);
                            }

                            break;
                        }

                        if (message.StartsWith("start trackers ")) // start tracker 5m 30
                        {
                            var trackerData = inputParser.ParseStartTrackerCommand(message); //TODO Refactor namings
                            if (trackerData != null)
                                await signalTrackerFactory.CreateTrackersForUsdcPairsAsync(trackerData.Interval,
                                    trackerData.RsiValue);
                        }

                        if (message.StartsWith("stop tracking "))
                        {
                            var interval = inputParser.ParseStopTrackingIntervalCommand(message);
                            if (interval != null)
                                await signalTrackerFactory.StopTrackersForInterval(message);
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                await telegramService.SendMessageAsync("Error: " + e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), token);
        }
    }
}