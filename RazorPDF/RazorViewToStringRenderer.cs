using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
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
            string sideLoadedAsm = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources",
                                   (IntPtr.Size == 8) ? "64 bit" : "32 bit",
                                                      $"libwkhtmltox{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" : ".so")}");
            return NativeLibrary.Load(sideLoadedAsm);
        }
        else return IntPtr.Zero;
    }
}

internal class RazorViewToStringRenderer
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorViewToStringRenderer(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary<TModel>(metadataProvider: new EmptyModelMetadataProvider(), modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                output,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext);

            return output.ToString();
        }
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = _razorViewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(Environment.NewLine, new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}