﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WireGuardCommand.Models;

namespace WireGuardCommand
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }
        private AppSettings? Settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            string pathSettings = @"./appsettings.json";
            if(File.Exists(pathSettings))
            {
                string json = File.ReadAllText(@"./appsettings.json");
                Settings = JsonSerializer.Deserialize<AppSettings>(json)!;
            }
            else
            {
                var settings = new AppSettings();
                settings.Reset();
                settings.Save();

                Settings = settings;
            }
            
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Debug.Assert(Settings is not null);

            services.AddSingleton<AppSettings>(Settings);
            services.AddTransient(typeof(MainWindow));
        }
    }
}
