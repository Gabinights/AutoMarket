using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace AutoMarket.Services.Implementations
{
    /// <summary>
    /// Service for rendering Razor views to strings.
    /// </summary>
    public class ViewRenderService
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(
            ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderToStringAsync(string viewName, object model, HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (!viewResult.Success)
            {
                viewResult = _viewEngine.GetView(viewName, viewName, false);
            }

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Could not find view '{viewName}'. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");
            }

            var view = viewResult.View;
            using var writer = new StringWriter();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                new TempDataDictionary(httpContext, _tempDataProvider),
                writer,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);
            return writer.ToString();
        }
    }
}

