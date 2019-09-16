using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CommandLine;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Certificates
{
    public class Program
    {
        internal class Options
        {
            [Option('c', "certificate", Required = false, HelpText = "Base64 encoded string of the certificate data.")]
            public string Certificate { get; set; }

            [Option('p', "password", Required = false, HelpText = "Password of the certificate.")]
            public string CertPassword { get; set; }
        }

        internal static string ThumbPrint;

        public static void Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Certificate))
                    {
                        o.CertPassword = o.CertPassword ?? throw new ArgumentNullException("No certificate password provided.");

                        var rawData = Convert.FromBase64String(o.Certificate);

                        //// install certificate
                        using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
                        {
                            var cert = new X509Certificate2(rawData, o.CertPassword, X509KeyStorageFlags.PersistKeySet);
                            ThumbPrint = cert.Thumbprint;
                            store.Add(cert);
                        }
                    }
                });

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configHost =>
                {
                    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadOnly);

                        var certs = store.Certificates.Find(X509FindType.FindByThumbprint, ThumbPrint, false);


                        var builder = new ConfigurationBuilder();
                        builder.AddJsonFile("appsettings.json");
                        var config = builder.Build();

                        configHost.AddAzureKeyVault(config["Vault"], config["ClientId"],
                            certs.OfType<X509Certificate2>().Single());
                    }
                })
                .UseStartup<Startup>();
    }
}
