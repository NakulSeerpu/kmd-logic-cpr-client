﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kmd.Logic.Cpr.Client.Models;
using Kmd.Logic.Identity.Authorization;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Kmd.Logic.Cpr.Client.Sample
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            InitLogger();

            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddUserSecrets(typeof(Program).Assembly)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build()
                    .Get<AppConfiguration>();

                await Run(config).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Log.Fatal(ex, "Caught a fatal unhandled exception");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void InitLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static async Task Run(AppConfiguration configuration)
        {
            var validator = new ConfigurationValidator(configuration);
            if (!validator.Validate())
            {
                return;
            }

            using (var httpClient = new HttpClient())
            using (var tokenProviderFactory = new LogicTokenProviderFactory(configuration.TokenProvider))
            {
                var cprClient = new CprClient(httpClient, tokenProviderFactory, configuration.Cpr);

                var configs = await cprClient.GetAllCprConfigurationsAsync().ConfigureAwait(false);
                if (configs == null || configs.Count == 0)
                {
                    Log.Error("There are no CPR configurations defined for this subscription");
                    return;
                }

                CprProviderConfigurationModel cprProvider;
                if (configuration.Cpr.CprConfigurationId == Guid.Empty)
                {
                    if (configs.Count > 1)
                    {
                        Log.Error("There is more than one CPR configuration defined for this subscription");
                        return;
                    }

                    cprProvider = configs[0];
                    configuration.Cpr.CprConfigurationId = cprProvider.Id.Value;
                }
                else
                {
                    cprProvider = configs.FirstOrDefault(x => x.Id == configuration.Cpr.CprConfigurationId);

                    if (cprProvider == null)
                    {
                        Log.Error("Invalid CPR configuration id {Id}", configuration.Cpr.CprConfigurationId);
                        return;
                    }
                }

                Log.Information("Fetching {Cpr} using configuration {Name}", configuration.CprNumber, cprProvider.Name);

                var citizen = await cprClient.GetCitizenByCprAsync(configuration.CprNumber).ConfigureAwait(false);

                Log.Information("Citizen data: {@Citizen}", citizen);

                var citizenList = await cprClient.GetAllCprEventsAsync(DateTime.Today.AddMonths(-2), DateTime.Today, 1, 10).ConfigureAwait(false);

                if (citizenList == null)
                {
                    Log.Error("Error in retriving citizen list");
                    return;
                }

                var success = await cprClient.SubscribeByCprAsync(configuration.CprNumber).ConfigureAwait(false);

                if (!success)
                {
                    Log.Error("Invalid CPR Number {@CprNumber}", configuration.CprNumber);
                    return;
                }

                Log.Information("Subscribed successfully for CprNumber {CprNumber}", configuration.CprNumber);

                success = await cprClient.SubscribeByIdAsync(citizen.Id.Value).ConfigureAwait(false);

                if (!success)
                {
                    Log.Error("Invalid CPR PersonId {personId}", citizen.Id.Value);
                    return;
                }

                Log.Information("Subscribed successfully for personId {personId}", citizen.Id.Value);

                success = await cprClient.UnsubscribeByCprAsync(configuration.CprNumber).ConfigureAwait(false);

                if (success)
                {
                    Log.Information("Unsubscribed successfully for CprNumber {CprNumber}", configuration.CprNumber);
                }

                success = await cprClient.UnsubscribeByIdAsync(citizen.Id.Value).ConfigureAwait(false);

                if (success)
                {
                    Log.Information("Unsubscribed successfully for personId {personId}", citizen.Id.Value);
                }

                int pageNo = 1;
                int pageSize = 100;
                var subscribedCitizenList = await cprClient.GetSubscribedCprEventsAsync(DateTime.Today.AddMonths(-2), DateTime.Today, pageNo, pageSize).ConfigureAwait(false);

                if (!(subscribedCitizenList is SubscribedCitizenEvents))
                {
                    Log.Error("Error in Subscribed retriving citizen list");
                    return;
                }

                while ((subscribedCitizenList as SubscribedCitizenEvents).ActualCount > 0)
                {
                    subscribedCitizenList = await cprClient.GetSubscribedCprEventsAsync(DateTime.Today.AddMonths(-2), DateTime.Today, ++pageNo, pageSize).ConfigureAwait(false);

                    if (!(subscribedCitizenList is SubscribedCitizenEvents))
                    {
                        Log.Error("Error in Subscribed retriving citizen list");
                        return;
                    }
                }
            }
        }
    }
}