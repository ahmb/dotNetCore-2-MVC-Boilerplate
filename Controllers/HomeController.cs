using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core2test.Models;
using Microsoft.Extensions.FileProviders;

namespace Core2test.Controllers
{
    public class HomeController : Controller
    {

        private readonly IFileProvider _fileProvider;
        
         public HomeController(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// Used to list all the files in the current file provider
        public IActionResult Files()
        {
            var contents = _fileProvider.GetDirectoryContents("");
            //The below is what was originally used to send the DirectoryContents
            // return View(contents);

            return View(_fileProvider);

            //attempt at reading and streaming the files below
            //  foreach(IFileInfo file in contents){
            //     using(var s = file.CreateReadStream()){
            //         //do something with the stream here
            //         //investigate whether using and foreach can be used in views
            //     }
            

        }
    }
}
