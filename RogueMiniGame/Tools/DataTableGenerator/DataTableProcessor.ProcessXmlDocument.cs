
using System;
using System.Collections.Generic;
using System.Xml;
using GameFramework;
using UnityEngine;
using System.IO;

public partial class DataTableProcessor
{
    #region 扩展处理xml

    /// <summary>
    /// 构造方法（用于处理 .xml 数据表文件）
    /// </summary>
    /// <param name="dataTableName">数据表名</param>
    /// <param name="dataTableFileName">数据表文件全路径</param>
    public DataTableProcessor(string dataTableName, string dataTableFileName)
    {
        if (string.IsNullOrEmpty(dataTableFileName))
        {
            throw new GameFrameworkException("Data table file name is invalid.");
        }

        if (!dataTableFileName.EndsWith(".xml"))
        {
            throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not a xml.",
                dataTableFileName));
        }

        if (!File.Exists(dataTableFileName))
        {
            throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not exist.",
                dataTableFileName));
        }

        // 解析 .xml 文件
        try
        {
            XmlDocument xmlDocument = new XmlDocument();

            // 加载 .xml 文件
            xmlDocument.Load(dataTableFileName);

            // 获取 root 结点
            XmlNode xmlRoot = xmlDocument.SelectSingleNode("root");
            if (xmlRoot == null)
            {
                Debug.LogError($"Can NOT find Node 'root' in data table [{dataTableName}.xml]!");
                return;
            }

            // 获取 header 结点
            XmlNode xmlHeader = xmlRoot.SelectSingleNode("header");
            if (xmlHeader == null)
            {
                Debug.LogError($"Can NOT find Node 'header' in data table [{dataTableName}.xml]!");
                return;
            }

            // 获取数据表名
            string rawDataTableName = xmlHeader.Attributes.GetNamedItem("table").Value;

            // 检查 .xml 文件名与 .xml 中声明的数据表名是否一致
            // if (!rawDataTableName.Equals(dataTableName))
            if (!rawDataTableName.Equals(Path.GetFileNameWithoutExtension(dataTableName)))
            {
                Debug.LogError(
                    $"Inconsistent data table name: dataTableFileName = [{dataTableName}.xml], dataTableName = [{rawDataTableName}]");
                return;
            }

            // 获取数据表描述
            DataTableDescription = xmlHeader.Attributes.GetNamedItem("desc").Value;

            // 字段个数
            int fieldCount = xmlHeader.ChildNodes.Count;
            List<Field> fields = new List<Field>(fieldCount);

            // 获取字段列表
            for (int i = 0; i < fieldCount; i++)
            {
                XmlNode xmlField = xmlHeader.ChildNodes[i];
                if (xmlField != null &&
                    xmlField.Name.Equals("field") &&
                    xmlField.Attributes != null)
                {
                    fields.Add(new Field()
                    {
                        Order = Convert.ToInt32(xmlField.Attributes.GetNamedItem("order").Value),
                        Name = xmlField.Attributes.GetNamedItem("name").Value,
                        Type = xmlField.Attributes.GetNamedItem("type").Value,
                        Description = xmlField.Attributes.GetNamedItem("desc").Value
                    });
                }
            }

            // 确保字段列表按 order 属性升序排列
            fields.Sort((field1, field2) => field1.Order.CompareTo(field2.Order));

            // 字段名数组
            m_NameRow = new string[fieldCount];
            // 字段类型数组
            m_TypeRow = new string[fieldCount];
            // 字段备注数组
            m_CommentRow = new string[fieldCount];

            // 收集字段名、字段类型、字段备注
            for (int i = 0; i < fieldCount; i++)
            {
                if (fields[i].Name.Equals("id"))
                {
                    // 设置 ID 字段所在的列数
                    m_IdColumn = i;
                }

                m_NameRow[i] = fields[i].Name;
                m_TypeRow[i] = fields[i].Type;
                m_CommentRow[i] = fields[i].Description;
            }

            // root 结点的子结点个数
            int itemCount = xmlRoot.ChildNodes.Count;
            // 实际有效数据行数
            int actualItemCount = 0;

            // 数据内容数组
            m_RawValues = new string[itemCount - 1][];

            // 收集数据内容
            for (int i = 0; i < itemCount; i++)
            {
                var itemNode = xmlRoot.ChildNodes[i];
                if (itemNode.Name.Equals("item"))
                {
                    string[] itemValues = new string[fieldCount];

                    for (int j = 0; j < fieldCount; j++)
                    {
                        itemValues[j] = itemNode.Attributes.GetNamedItem(fields[j].Name).Value;
                    }

                    m_RawValues[actualItemCount++] = itemValues;
                }
            }

            // 转换字段名的格式（确保首字母大写）
            m_NameRow = ParseNameRow(m_NameRow);

            m_DataProcessor = new DataProcessor[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                if (i == IdColumn)
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                }
                else
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(m_TypeRow[i]);
                }
            }
        }
        catch
        {
            Debug.LogError($"Error happening when processing data table [{dataTableName}.xml]!");
            throw;
        }

        m_CodeTemplate = null;
        m_CodeGenerator = null;
    }

    /// <summary>
    /// 数据表字段类
    /// </summary>
    private struct Field
    {
        /// <summary>
        /// 字段排序值
        /// </summary>
        public int Order;

        /// <summary>
        /// 字段名
        /// </summary>
        public string Name;

        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Description;

        public override string ToString()
        {
            return Order + "," + Name + "," + Type + "," + Description;
        }
    }

    /// <summary>
    /// 转换字段名的格式（确保首字母大写）
    /// </summary>
    /// <param name="rawNames">原始的字段名格式（eg: id）</param>
    /// <returns>转换后的字段名数组（eg: Id）</returns>
    private string[] ParseNameRow(string[] rawNames)
    {
        string[] nameRow = new string[rawNames.Length];

        for (int i = 0; i < rawNames.Length; i++)
        {
            if (rawNames[i].Length > 0)
            {
                // 字段名首字段转换为大写
                if (rawNames[i].Length == 1)
                {
                    nameRow[i] = rawNames[i].ToUpper();
                }
                else
                {
                    nameRow[i] = rawNames[i].Substring(0, 1).ToUpper() + rawNames[i].Substring(1);
                }
            }
        }

        return nameRow;
    }

    #endregion
}

