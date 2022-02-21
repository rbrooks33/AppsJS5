
using Microsoft.AspNetCore.Mvc;

namespace AppsJSNuget
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