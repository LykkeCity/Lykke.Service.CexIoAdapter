using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Tests
{
    public class LaunchSettingsFixture : IDisposable
    {
        private const string SettingsFilePath = @"Properties\launchSettings.json";

        public LaunchSettingsFixture()
        {
            if (File.Exists(SettingsFilePath))
            {
                using (var file = File.OpenText(SettingsFilePath))
                {
                    var reader = new JsonTextReader(file);
                    var jObject = JObject.Load(reader);

                    var variables = jObject
                        .GetValue("profiles")
                        .SelectMany(profiles => profiles.Children())
                        .SelectMany(profile => profile.Children<JProperty>())
                        .Where(prop => prop.Name == "environmentVariables")
                        .SelectMany(prop => prop.Value.Children<JProperty>())
                        .ToList();

                    foreach (var variable in variables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        }

        public void Dispose()
        {
        }
    }
}
