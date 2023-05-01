using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/v" + APIversion + "/testCont")]
    [ApiConventionType(typeof(CustomConventions))] //такой вариант Convention
    [ApiVersion(APIversion)]

    public class TestsController : ControllerBase
    {
        private const string APIversion = "1.2";

        // GET: TestsController
        [HttpGet]
        public IEnumerable<string> Index()
        {
            return new string[] { "v1", "v2" };
        }

        // GET: TestsController/Details/5
        [HttpGet("{id}", Name = "Get")]
        //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))] //такой вариант Convention
        //[ApiConventionMethod(typeof(CustomConventions), nameof(CustomConventions.Infominer))]
        public string Details(int id)
        {
            return "value";
        }
    }
}
