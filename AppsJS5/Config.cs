using System.Collections.Generic;

namespace brooksoft.appsjs
{
    public class Config
    {
        public static string AppsJSRoot { get; set; } = "c:\\Scripts\\Apps";
        public static List<string> SuccessMessages { get; set; } = new List<string>();
        public static List<string> FailMessages { get; set; } = new List<string>();
    }
}
