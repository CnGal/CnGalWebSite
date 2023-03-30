using HtmlAgilityPack;

namespace CnGalWebSite.Shared.Models.Articles
{
    public class ArticleCatalogModel
    {
        public string Text { get; set; }

        public string Href { get; set; }

        public bool IsActive { get; set; }

        public List<ArticleCatalogModel> Nodes { get; set; } = new List<ArticleCatalogModel>();

        private class Heading
        {
            public int Id { get; set; }
            public int Pid { get; set; } = -1;
            public string Text { get; set; }
            public string Href { get; set; }
            public int Level { get; set; }
        }

        /// <summary>
        /// 获取 Html 目录
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<ArticleCatalogModel> GetCatalog(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return new List<ArticleCatalogModel>();
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);

            //遍历标题block，添加到一个列表中
            var headings = new List<Heading>();

            foreach (var item in document.DocumentNode.ChildNodes)
            {
                if (item.Name[0] == 'h' && item.Name[1] >= '1' && item.Name[1] <= '6'&&string.IsNullOrWhiteSpace( item.InnerText)==false)
                {
                    //创建节点
                    var node = new Heading
                    {
                        Text = item.InnerText,
                        Href = item.Id,
                        Level = item.Name[1]
                    };

                    headings.Add(node);
                }
            }

            //根据不同block的位置、level关系，推出父子关系，使用 id 和 pid 关联
            for (var i = 0; i < headings.Count; i++)
            {
                var item = headings[i];
                item.Id = i;
                for (var j = i; j >= 0; j--)
                {
                    var preItem = headings[j];
                    if (item.Level == preItem.Level + 1)
                    {
                        item.Pid = j;
                        break;
                    }
                }
            }

            //最后用递归生成树结构
            List<ArticleCatalogModel> GetNodes(int pid = -1)
            {
                var nodes = headings.Where(a => a.Pid == pid).ToList();
                return nodes.Count == 0 ? null
                  : nodes.Select(a => new ArticleCatalogModel { Text = a.Text, Href = a.Href, Nodes = GetNodes(a.Id) }).ToList();
            }


            return GetNodes();

        }
    }


}
