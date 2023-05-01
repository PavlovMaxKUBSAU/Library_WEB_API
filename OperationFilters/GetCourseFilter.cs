using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.OperationFilters
{
    public class GetCourseFilter : IOperationFilter //генератор новой схемы (для нового формата тела ответа)
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId != "GetCourse")
            {
                return;
            }

            operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add
            (
                "new format type",
                new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(Controllers.CoursesController), new SchemaRepository())//??????????
                }
            );
        }
    }
}
