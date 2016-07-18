using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using ObjectRepositoryContract;
using Test;
using TestWeb.Models;

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
