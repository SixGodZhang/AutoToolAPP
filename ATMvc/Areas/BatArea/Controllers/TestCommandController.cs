using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATFramework.Log;
using ATFramework.Bat;

namespace FineUIMvc.EmptyProject.Areas.BatArea.Controllers
{
    public class TestCommandController : Controller
    {
        // GET: BatArea/TestCommand
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OnClickCopyCommandBtn(string type, string gridSourceKey, JArray gridFields, JObject typeParams)
        {
            //ShowNotify("服务器返回消息...");
            ViewBag.AllMsgs = AutoBindGrid(gridSourceKey, gridFields);

            var panelLeftRegion = UIHelper.Panel("panelCenterRegion");

            string newtitle = String.Format("命令执行时间 更新时间：{0}", DateTime.Now.ToLongTimeString());
            panelLeftRegion.Title(newtitle);
            panelLeftRegion.TitleToolTip(newtitle);

            return UIHelper.Result();
        }

        private List<string> AutoBindGrid(string gridSourceKey, JArray gridFields)
        {
            var grid1 = UIHelper.Grid("LogGrid");

            bool result = BatTool.CallBat(@"C:\Users\Administrator\Desktop\copy.bat");
            //CMDMSG resultMsg = ToolPackage.BatTool(GlobalConstant.BAT_PATH_DIC["copy"]);
            List<string> allMsgs = new List<string>() { "1", "2", "3", "4",result.ToString() };

            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("Value", typeof(string)));

            DataRow row = null;
            for (int i = 0; i < allMsgs.Count; i++)
            {
                row = table.NewRow();
                row[0] = i;
                row[1] = allMsgs[i];
                table.Rows.Add(row);
            }

            //ShowNotify(Environment.CurrentDirectory);
            grid1.DataSource(table, gridFields);
            grid1.Attribute("data-source-key", gridSourceKey);

            return allMsgs;
        }
    }
}