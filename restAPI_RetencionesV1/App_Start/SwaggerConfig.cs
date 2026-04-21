using System.Web.Http;
using WebActivatorEx;
using restAPI_RetencionesV1;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace restAPI_RetencionesV1
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "restAPI_RetencionesV1");
                    })
                .EnableSwaggerUi(c =>
                    {
                    });
        }
    }
}
