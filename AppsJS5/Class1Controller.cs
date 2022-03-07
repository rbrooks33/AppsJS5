
using Microsoft.AspNetCore.Mvc;

namespace brooksoft.appsjs
{
    [Route("api/[controller]")]
    public class Class1Controller : Controller
    {
        [HttpGet]
        [Route("GetHiya")]
        public string GetHiya()
        {
            return "hiya";
        }
        public string GetHiya2()
        {
            return "there";
        }
    }

}