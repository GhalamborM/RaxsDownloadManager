using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WDM.Extensions.Providers
{
    public abstract class ServiceProvider
    {
        public abstract string Name { get; }

        public abstract Task ProcessAsync();

        public abstract Task StartAsync();

        public abstract Task PauseAsync();

        public abstract Task RestartAsync();

    }
}
