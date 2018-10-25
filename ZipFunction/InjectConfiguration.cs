﻿using System;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;

using ZipBlobStorage.Repository;
using ZipBlobStorage.Services;

namespace ZipFunction
{
    public class InjectConfiguration : IExtensionConfigProvider
    {
        private IServiceProvider _serviceProvider;

        public void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            _serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<InjectAttribute>()
                .BindToInput<dynamic>(i => _serviceProvider.GetRequiredService(i.Type));
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IFtpRepository, FtpRepository>();
            services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();
            services.AddSingleton<IZipService, ZipService>();
        }
    }
}