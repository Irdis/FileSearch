using System.Web.Mvc;
using In.FileSearch.Engine;

namespace In.FileSearch.Web.Controllers
{
    public class FileSearchController : Controller
    {
        private readonly IFileSearchManager _manager = FileSearchManagerFactory.Get();
        // GET: FileSearch
        public ActionResult Index()
        {
            var lst = _manager.GetExtensionList();
            return View(lst);
        }

        public JsonResult Run(string id, FileSearchOptions options)
        {
            _manager.Search(id, options);
            return Json(string.Empty);
        }

        public JsonResult Stop(string id)
        {
            _manager.Cancel(id);
            return Json(string.Empty);
        }
    }
}