using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using static Microsoft.Extensions.Hosting.Host;

namespace Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main()
            => CreateDefaultBuilder()
                .ConfigureWebHostDefaults(_ => _.UseStartup<Startup>())
                .Build()
                .Run();
    }
}
