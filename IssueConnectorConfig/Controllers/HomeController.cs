using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IssueConnectorConfig.Models;
using IssueConnectorLib.Services;
using IssueConnectorConfig.Services.Database;

namespace IssueConnectorConfig.Controllers
{
    public class HomeController : Controller
    {
		private IConfigDatabaseService _dbService;

		public HomeController(IConfigDatabaseService dbService)
		{
			_dbService = dbService;
		}

		public async Task<IActionResult> Index()
        {
			ViewData["ClientUsers"] = await _dbService.GetAllUserMappingsForConfig();
			ViewData["ProjectMappings"] = await _dbService.GetAllProjectMappings();

			return View();
        }

		[HttpPost("submituser")]
		public async Task<int> AddUserMap(string jirauser, string tcuser, string eauser)
		{
			return await _dbService.AddUserMapping(jirauser, tcuser, eauser);
		}

		[HttpPost("addprojectmap")]
		public async Task<ActionResult> Config(string jiraProjectKey, string trimbleConnectPID, string easyAccessPID)
		{
			var result = await _dbService.SaveNewProjectMap(jiraProjectKey, trimbleConnectPID, easyAccessPID);

			if (result < 1)
			{
				throw new Exception("There was an error saving to db.");
			}
			else
			{
				ViewData["ProjectMappings"] = await _dbService.GetAllProjectMappings();

				return View("Index");
			}
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
    }
}
