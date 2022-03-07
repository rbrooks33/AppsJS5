using System;
using System.Collections.Generic;
using System.Text;

namespace brooksoft.appsjs
{
    public class Result
    {
        public Result()
        {
            SuccessMessages = new List<string>();
            FailMessages = new List<string>();
            Codes = new List<ResultCode>();
        }
        public bool Success { get; set; }
        public List<string> SuccessMessages { get; set; }
        public List<string> FailMessages { get; set; }
        public List<ResultCode> Codes { get; set; }
        public object Data { get; set; }
    }
    public class ResultCode
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

}
