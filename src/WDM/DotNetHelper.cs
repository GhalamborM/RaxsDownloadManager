// Ref: https://stackoverflow.com/questions/951856/is-there-an-easy-way-to-check-the-net-framework-version
// Original answer by @mwijnands [ https://stackoverflow.com/users/503050/mwijnands ]
// I've added .net framework 4.7.1, 4.7.2 and 4.8 as well.

using System;

namespace WDM
{
    public static class DotNetFrameworkHelper
    {
        public static bool Is46Installed()
        {
            // API changes in 4.6: https://github.com/Microsoft/dotnet/blob/master/releases/net46/dotnet46-api-changes.md
            return Type.GetType("System.AppContext, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }

        public static bool Is461Installed()
        {
            // API changes in 4.6.1: https://github.com/Microsoft/dotnet/blob/master/releases/net461/dotnet461-api-changes.md
            return Type.GetType("System.Data.SqlClient.SqlColumnEncryptionCngProvider, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }

        public static bool Is462Installed()
        {
            // API changes in 4.6.2: https://github.com/Microsoft/dotnet/blob/master/releases/net462/dotnet462-api-changes.md
            return Type.GetType("System.Security.Cryptography.AesCng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }

        public static bool Is47Installed()
        {
            // API changes in 4.7: https://github.com/Microsoft/dotnet/blob/master/releases/net47/dotnet47-api-changes.md
            return Type.GetType("System.Web.Caching.CacheInsertOptions, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false) != null;
        }

        public static bool Is471Installed()
        {
            // API changes in 4.7.1: https://github.com/microsoft/dotnet/blob/master/releases/net471/dotnet471-api-changes.md
            return Type.GetType("System.StringNormalizationExtensions, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }
        public static bool Is472Installed()
        {
            // API changes in 4.7.2: https://github.com/microsoft/dotnet/blob/master/releases/net472/dotnet472-api-changes.md
            return Type.GetType("System.Data.SqlClient.SqlAuthenticationParameters, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }
        public static bool Is48Installed()
        {
            // API changes in 4.8: https://github.com/microsoft/dotnet/blob/master/releases/net48/dotnet48-api-changes.md
            return Type.GetType("System.Net.Configuration.SettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false) != null;
        }
    }
}
