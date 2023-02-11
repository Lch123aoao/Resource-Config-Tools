using System.IO;
using System.Text;
using System.Xml;

namespace tablegen2.logic
{
    public static class TableExcelExportXml
    {
        public static void exportExcelFile(TableExcelData data, string filePath)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("root");
            root.SetAttribute("platform", "");
            doc.AppendChild(root);

            string[] configNameAll = filePath.Split('\\');
            string mConfigName = string.Empty;
            foreach (string vConfigName in configNameAll)
            {
                if (vConfigName.Contains(".xml"))
                {
                    string[] str = vConfigName.Split('.');
                    mConfigName = str[0];
                }
            }

            if (mConfigName == string.Empty)
                return;

            var header = doc.CreateElement("header");
            header.SetAttribute("table", mConfigName);
            header.SetAttribute("desc", "这是文本");

            int index = 0;
            foreach (var header1 in data.Headers)
            {
                var item = doc.CreateElement("field");
                string order = index.ToString();
                string name = header1.FieldName;
                string type = header1.FieldType;
                string desc = header1.FieldDesc;
                item.SetAttribute("order", order);
                item.SetAttribute("name", name);
                item.SetAttribute("type", type);
                item.SetAttribute("desc", desc);
                header.AppendChild(item);
                index++;
            }

            root.AppendChild(header);

            foreach (var row in data.Rows)
            {
                var item = doc.CreateElement("item");
                for (int i = 0; i < data.Headers.Count; i++)
                {
                    var hdr = data.Headers[i];
                    var val = row.StrList[i];
                    item.SetAttribute(hdr.FieldName, val);
                }
                root.AppendChild(item);
            }

            // 保存
            using (FileStream fs = File.Create(filePath))
            {
                var writer = new XmlTextWriter(fs, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
                writer.Close();
            }
        }
    }
}
