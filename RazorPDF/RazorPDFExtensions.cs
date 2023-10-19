using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace RazorPDF;

public static class RazorPDFExtensions
{
    internal static IServiceProvider ServiceProvider { get; set; }

    public static IServiceCollection AddRazorPDF(this IServiceCollection services)
    {
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += Default_ResolvingUnmanagedDll;
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
        services.AddTransient<RazorViewToStringRenderer>();
        services.AddSingleton<ThreadSafeHTMLToPDFConverter>();

        return services;
    }

    public static WebApplication UseRazorPDF(this WebApplication webApplication)
    {
        ServiceProvider = webApplication.Services;
        return webApplication;
    }

    private static IntPtr Default_ResolvingUnmanagedDll(Assembly arg1, string arg2)
    {
        if (arg2.Contains("libwkhtmltox"))
        {
            string runtimeIdentifier;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                runtimeIdentifier = "win";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                runtimeIdentifier = "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                runtimeIdentifier = "osx";
            }
            else
            {
                return IntPtr.Zero; // Unsupported platform
            }

            string bitness = (IntPtr.Size == 8) ? "x64" : "x86";
            string fileExtension = (runtimeIdentifier == "win") ? ".dll" : (runtimeIdentifier == "linux" ? ".so" : ".dylib");

            string sideLoadedAsm = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                "runtimes",
                                                $"{runtimeIdentifier}-{bitness}",
                                                "native",
                                                $"libwkhtmltox{fileExtension}");

            if(!File.Exists(sideLoadedAsm))
                throw new FileNotFoundException($"Could not find {sideLoadedAsm}");

            return NativeLibrary.Load(sideLoadedAsm);
        }

        return IntPtr.Zero;
    }
}
