using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API
{
    public static class CustomConventions
    {

        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        //
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        //совпадения должны быть в 1-й части названий методов (до заглавной у подвергаемого обработке ответами (который в контроллере))
        public static string Details([ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)] int ss)
        {
            return "Infooo";
        }

        
    }
}
