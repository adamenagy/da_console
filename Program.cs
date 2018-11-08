using System;
using System.Reflection;
using Autodesk.Forge;
using System.Threading.Tasks;
using v3 = Autodesk.Forge.DesignAutomation.v3;
using v3m = Autodesk.Forge.Model.DesignAutomation.v3;

// Using https://www.areilly.com/2017/04/21/command-line-argument-parsing-in-net-core-with-microsoft-extensions-commandlineutils/
using Microsoft.Extensions.CommandLineUtils;

namespace da_console
{
    class Program
    {
        static async Task<string> GetAccessToken(CommandOption clientIdParam, CommandOption clientSecretParam) {
            string clientId = null, clientSecret = null;
            try {
                clientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
                clientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");
            } catch { }

            if (clientIdParam.HasValue()) 
                clientId = clientIdParam.Value();

            if (clientSecretParam.HasValue()) 
                clientSecret = clientSecretParam.Value();

            if (clientId == null || clientSecret == null) {
                Console.WriteLine("FORGE_CLIENT_ID and/or FORGE_CLIENT_SECRET not defined either as environment variables or command options -ci/-cs.");
                return null;
            }

            TwoLeggedApi oauth = new TwoLeggedApi();
            dynamic reply = await oauth.AuthenticateAsync(
                clientId, 
                clientSecret, 
                oAuthConstants.CLIENT_CREDENTIALS, 
                new Scope[] { Scope.CodeAll });
            string accessToken = reply.access_token;

            Console.WriteLine(accessToken);

            return accessToken;
        }

        static async Task ListActivities(string accessToken) {
            v3.ActivitiesApi activitiesApi = new v3.ActivitiesApi();
            activitiesApi.Configuration.AccessToken = accessToken;
            v3m.PageString activities = await activitiesApi.ActivitiesGetItemsAsync();

            Console.WriteLine(" *** Activities ***");
            foreach (string activity in activities.Data)
            {
               Console.WriteLine(activity);
            }
        }

        static async Task ListAppBundles(string accessToken) {
            v3.AppBundlesApi appbundlesApi = new v3.AppBundlesApi();
            appbundlesApi.Configuration.AccessToken = accessToken;
            v3m.PageString appbundles = await appbundlesApi.AppBundlesGetItemsAsync();

            Console.WriteLine(" *** AppBundles ***");
            foreach (string appbundle in appbundles.Data)
            {
               Console.WriteLine(appbundle);
            }
        }

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "Design Automation API CLI";
            app.Description = "Command Line Interface for Design Automation API";
            app.HelpOption("-?|-h|--help");

            var clientIdOption = app.Option("-ci|--clientid <ForgeClientID>", 
                "Forge Client ID",
                CommandOptionType.SingleValue);

            var clientSecretOption = app.Option("-cs|--clientsecret <ForgeClientSecret>", 
                "Forge Client Secret",
                CommandOptionType.SingleValue);

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            });

            app.Command("listactivities", (command) => {
                command.Description = "This is the description for listactivities.";
                command.HelpOption("-?|-h|--help");
            
                command.OnExecute(async () => {
                    string accessToken = await GetAccessToken(clientIdOption, clientSecretOption);
                    await ListActivities(accessToken);
                    return 0;
                });
            });

            app.Command("listappbundles", (command) => {
                command.Description = "This is the description for listappbundles.";
                command.HelpOption("-?|-h|--help");
            
                command.OnExecute(async () => {
                    string accessToken = await GetAccessToken(clientIdOption, clientSecretOption);
                    await ListAppBundles(accessToken);
                    return 0;
                });
            });

            app.OnExecute(() => {
                Console.WriteLine("No command was selected");

                return 0;
            });

            app.Execute(args);    
        }
    }
}
