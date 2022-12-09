using BeeEeeLibs.HttpServerBase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerBase
{
    public class EndPointBuilder
    {
        public static RegisterDelegate Post(string pattern, RequestDelegate callback)
        {
            return builder => builder.MapPost(pattern, EndPointExtensions.WrapConsole(callback));
        }

        public static RegisterDelegate Get(string pattern, RequestDelegate callback)
        {
            return builder => builder.MapGet(pattern, EndPointExtensions.WrapConsole(callback));
        }

        public static RegisterDelegate Put(string pattern, RequestDelegate callback)
        {
            return builder => builder.MapPut(pattern, EndPointExtensions.WrapConsole(callback));
        }

        public static RegisterDelegate Delete(string pattern, RequestDelegate callback)
        {
            return builder => builder.MapDelete(pattern, EndPointExtensions.WrapConsole(callback));
        }
    }
}
