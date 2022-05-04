using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mongosa.Entities
{
    public abstract class Migration
    {
        protected readonly IServiceProvider services;
        public string TimeStamp { get; }

        protected Migration(IServiceScopeFactory scopeFactory, string timeStamp)
        {
            var factory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            var scope = factory.CreateScope();
            services = scope.ServiceProvider;

            TimeStamp = timeStamp;
        }

        public abstract void Up();
        public abstract void Down();
    }
}