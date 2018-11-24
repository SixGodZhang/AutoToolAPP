using FineUIMvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ATMvc.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        public ActionResult Hello()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHello_Click()
        {
            Alert.Show("你好 FineUI！", MessageBoxIcon.Warning);

            return UIHelper.Result();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLogin_Click(string tbxUserName, string tbxPassword)
        {
            if (tbxUserName == "admin" && tbxPassword == "admin")
            {
                ShowNotify("成功登录！", MessageBoxIcon.Success);
            }
            else
            {
                ShowNotify("用户名或密码错误！", MessageBoxIcon.Error);
            }

            return UIHelper.Result();
        }


        // GET: Themes
        public ActionResult Themes()
        {
            return View();
        }

        private string _cookieMenuStyle = "tree";
        private string _cookieMenuMode = "normal";
        private string _cookieLang = "zh_CN";
        private string _cookieSearchText = "";

        private void LoadData()
        {
            HttpCookie cookie = null;

            //基础版设置
            _cookieMenuStyle = "plaintree";
            _cookieMenuMode = "normal";

            // 从Cookie中读取 - 语言
            cookie = Request.Cookies["Language"];
            if (cookie != null)
            {
                _cookieLang = cookie.Value;
            }

            // 从Cookie中读取 - 搜索文本
            cookie = Request.Cookies["SearchText"];
            if (cookie != null)
            {
                _cookieSearchText = HttpUtility.UrlDecode(cookie.Value);
            }

            LoadTreeMenuData();

            ViewBag.CookieMenuStyle = _cookieMenuStyle;
            ViewBag.CookieIsBase = Constants.IS_BASE;
            ViewBag.CookieMenuMode = _cookieMenuMode;
            ViewBag.CookieLang = _cookieLang;
            ViewBag.CookieSearchText = _cookieSearchText;
            ViewBag.ProductVersion = GlobalConfig.ProductVersion;
        }

        /// <summary>
        /// 加载树形菜单信息
        /// </summary>
        private void LoadTreeMenuData()
        {
            string xmlPath = Server.MapPath("~/res/menu.xml");

            string xmlContent = String.Empty;
            using (StreamReader sr = new StreamReader(xmlPath))
            {
                xmlContent = sr.ReadToEnd();
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlContent);

            //XML---->Tree
            IList<TreeNode> nodes = new List<TreeNode>();
            ResolveXmlNodeList(nodes, xdoc.DocumentElement.ChildNodes);

            // 视图数据
            ViewBag.TreeMenuNodes = nodes.ToArray();
        }

        private int ResolveXmlNodeList(IList<TreeNode> nodes, XmlNodeList xmlNodes)
        {
            // nodes 中渲染到页面上的节点个数
            int nodeVisibleCount = 0;

            foreach (XmlNode xmlNode in xmlNodes)
            {
                if (xmlNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                TreeNode node = new TreeNode();

                // 是否叶子节点
                bool isLeaf = xmlNode.ChildNodes.Count == 0;

                bool currentNodeIsVisible = true;

                string nodeText = "";
                bool nodeIsCorp = false;

                XmlAttribute textAttr = xmlNode.Attributes["Text"];
                if (textAttr != null)
                {
                    nodeText = textAttr.Value;
                }

                int childVisibleCount = 0;
                if (isLeaf)
                {
                    // 存在搜索文本
                    if (!String.IsNullOrEmpty(_cookieSearchText))
                    {
                        if (!nodeText.Contains(_cookieSearchText))
                        {
                            currentNodeIsVisible = false;
                        }
                    }
                }
                else
                {
                    // 递归
                    childVisibleCount = ResolveXmlNodeList(node.Nodes, xmlNode.ChildNodes);

                    if (childVisibleCount == 0)
                    {
                        currentNodeIsVisible = false;
                    }
                    else
                    {
                        // 存在搜索文本
                        if (!String.IsNullOrEmpty(_cookieSearchText))
                        {
                            // 展开节点
                            node.Expanded = true;
                        }
                    }
                }

                if (currentNodeIsVisible)
                {
                    foreach (XmlAttribute attribute in xmlNode.Attributes)
                    {
                        string name = attribute.Name;
                        string value = attribute.Value;

                        if (name == "Text")
                        {
                            // Text需要特殊处理
                            if (isLeaf)
                            {
                                // 设置节点的提示信息
                                node.ToolTip = nodeText;
                            }

                            // 存在 IsCorp=True 属性，则改变 Text 的值
                            if (nodeIsCorp)
                            {
                                node.IconFont = IconFont._Enterprise;
                                //nodeText += "&nbsp;<span class=\"iscorp\">Corp.</span>";
                            }

                            node.Text = nodeText;
                        }
                        else
                        {
                            node.SetPropertyValue(name, value);
                        }
                    }

                    nodes.Add(node);
                    nodeVisibleCount++;
                }

            }

            return nodeVisibleCount;
        }
    }
}