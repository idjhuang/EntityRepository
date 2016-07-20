using System.Web.Http;
using Test;

namespace TestWeb.Controllers
{
    public class TestController : ApiController
    {
        public string Get(string id)
        {
            switch (id)
            {
                case "Load":
                    TestCases.LoadObjects();
                    break;
                case "Insert":
                    TestCases.InsertObjects();
                    break;
                case "Extension":
                    TestCases.InsertAndLoadExtensionObjects();
                    break;
            }
            return "Done!";
        }
    }
}
