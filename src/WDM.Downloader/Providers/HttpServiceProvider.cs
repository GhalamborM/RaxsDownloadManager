using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WDM.Extensions.Providers;

namespace WDM.Downloaders.Providers
{
    public class HttpServiceProvider : ServiceProvider
    {
        public override string Name => "Http/Https";

        public override Task PauseAsync()
        {
            throw new NotImplementedException();
        }

        public override Task ProcessAsync()
        {
            throw new NotImplementedException();
        }

        public override Task RestartAsync()
        {
            throw new NotImplementedException();
        }

        public override Task StartAsync()
        {
            throw new NotImplementedException();
        }
    }
}
