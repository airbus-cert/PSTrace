using System;
using System.EnterpriseServices.Internal;

namespace GACInstaller
{
    /// <summary>
    /// Just for install step
    /// </summary>
    public class GACInstaller
    {
        static void Main(string[] args)
        {
            new Publish().GacInstall(args[0]);
        }
    }
}
