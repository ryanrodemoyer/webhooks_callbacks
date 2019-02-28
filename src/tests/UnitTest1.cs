using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using TypeMock.ArrangeActAssert;
using website.Controllers;

namespace tests
{
    //[TestClass]
    //public class CallbacksControllerTests
    //{
    //    [TestMethod]
    //    public void TestValuesController()
    //    {
    //        string data = "Hello World!!";
    //        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

    //        try
    //        {
    //            var context = new ControllerContext();
    //            context.HttpContext = new DefaultHttpContext();
    //            context.HttpContext.Request.Headers["x-payload-sig"] = "20317";
    //            context.HttpContext.Request.Body = stream;

    //            var controller = new CallbacksController();
    //            controller.ControllerContext = context;

    //            var result = controller.Weather();

    //            Console.WriteLine(result.GetType());

    //            Console.WriteLine((result as ObjectResult)?.ToFormattedJson());
    //        }
    //        finally
    //        {
    //            stream.Dispose();
    //        }
    //    }  

    //    [TestMethod]
    //    public void TestValuesController02()
    //    {
    //        string data = "Hello World!!";
    //        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

    //        try
    //        {
    //            var context = new ControllerContext();
    //            context.HttpContext = new DefaultHttpContext();
    //            //context.HttpContext.Request.Headers["x-payload-sig"] = "20317";
    //            context.HttpContext.Request.Body = stream;

    //            var controller = new CallbacksController();
    //            controller.ControllerContext = context;

    //            var result = controller.Weather();

    //            Console.WriteLine(result.GetType());

    //            Console.WriteLine((result as ObjectResult)?.ToFormattedJson());
    //        }
    //        finally
    //        {
    //            stream.Dispose();
    //        }
    //    }       
    //}

    public static class FormatExtensions
    {
        public static string ToFormattedJson(this ObjectResult o)
        {
            return JToken.Parse(o.Value.ToString()).ToString(Formatting.Indented);
        }
    }

   
}
