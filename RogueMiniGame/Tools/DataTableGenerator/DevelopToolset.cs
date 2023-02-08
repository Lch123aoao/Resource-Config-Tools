using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 表解析工具
/// </summary>
public class DevelopToolset : EditorWindow
{
    private DataTableTool dataTableTool = new DataTableTool();
    
    [MenuItem("Game Framework/导表工具", false, 50)]
    private static void Open()
    {
        DevelopToolset window = GetWindow<DevelopToolset>(true, "Develop Tools", true);
        window.minSize = new Vector2(500f, 300f);
        window.maxSize = new Vector2(800f, 600f);
    }
    
    private void OnEnable()
    {
        // 初始化数据表工具
        InitDataTableTool();
    }
    
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Space(5f);
            // 绘制数据表工具 GUI
            DrawDataTableTool();
            GUILayout.Space(5f);
        }
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDataTableTool()
    {
        dataTableTool.Draw();
    }

    private static void UpdateProgress(int progress, int progressMax, string desc, string title = "未命名")
    {
        float value = progress / (float) progressMax;
        EditorUtility.DisplayProgressBar($"{title} Processing...[{progress}-{progressMax}]", desc, value);
    }

    #region 表操作
    public class DataTableTool
    {
        public static string ToolPathName = "Editor";
        /// <summary>
        /// Player Prefs 保存路径
        /// </summary>
        private class DataTablePlayerPrefsKey
        {
            private static readonly string DataTablePrefix = $"{ToolPathName}.DevelopToolset.DataTableTool.";
            public static string InputXMLDirectoryPath => DataTablePrefix + "InputXMLDirectoryPath" + Application.dataPath;
        }

        private class DataTableSelectItem
        {
            public bool Selected;
            public string DataTableName;
        }
        
        /// <summary>
        /// 空的字符串数组
        /// </summary>
        private static readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// xml文件名字符串数组
        /// </summary>
        private string[] m_XMLDocNames = EmptyStringArray;

        /// <summary>
        /// xml文档文件夹目录路径
        /// </summary>
        private string m_InputXmlDirectoryPath = string.Empty;

        /// <summary>
        /// 代码模板路径
        /// </summary>
        private string m_InputTemplateFilePath = string.Empty;

        /// <summary>
        /// 输出代码目录路径
        /// </summary>
        private string m_OutputCodeDirectoryPath = string.Empty;

        /// <summary>
        /// 输出二进制数据表路径
        /// </summary>
        private string m_OutputBinaryDirectoryPath = string.Empty;

        /// <summary>
        /// 成功生成的数据表数量
        /// </summary>
        private int m_GeneratedDataTableCount;
        
        private List<DataTableSelectItem> m_ToGenerateDataTables = null;

        //暂时修改 ==== > 
        public void Init()
        {
            m_InputXmlDirectoryPath =
                PlayerPrefs.GetString(DataTablePlayerPrefsKey.InputXMLDirectoryPath);

            m_InputTemplateFilePath = Path.Combine(Application.dataPath,
                $"{ToolPathName}/DataTableGenerator/DataTableTemplate/DataTableCodeTemplate.txt");
            m_OutputCodeDirectoryPath = Path.Combine(Application.dataPath,
                $"Scripts/Table/DataTable");
            m_OutputBinaryDirectoryPath = Path.Combine(Application.dataPath,
                $"Resources/DataTable");

            // Search Datatable Path
            SearchAllXMLDocuments(m_InputXmlDirectoryPath, ref m_XMLDocNames);

            InitTodoDataTables(m_XMLDocNames, ref m_ToGenerateDataTables);
        }


        /// <summary>
        /// Search Datatables
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="docsName">Buffer 文件名</param>
        /// <returns></returns>
        private bool SearchAllXMLDocuments(string folderPath, ref string[] docsName)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;

            var folderInfo = new DirectoryInfo(folderPath);
            if (folderInfo.Exists)
            {
                var xmlDocs = folderInfo.GetFiles("*.xml");
                if (xmlDocs.Length > 0)
                {
                    docsName = new string[xmlDocs.Length];
                    Array.Copy(xmlDocs.Select(p => Path.GetFileNameWithoutExtension(p.Name)).ToArray(), 0, docsName, 0, xmlDocs.Length);
                }
                else
                {
                    docsName = EmptyStringArray;
                }

                return docsName.Length > 0;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 浏览 XML 目录路径
        /// </summary>
        private void BrowseXmlDirectory()
        {
            string directory = EditorUtility.OpenFolderPanel("Select XML File Directory",
                m_InputXmlDirectoryPath, string.Empty);
            if (!string.IsNullOrEmpty(directory))
            {
                // 将 XML 配置数据表文件目录路径的值保存到本地 PlayerPrefs 中
                PlayerPrefs.SetString(DataTablePlayerPrefsKey.InputXMLDirectoryPath, directory);

                m_InputXmlDirectoryPath = directory;
                
                Init();
            }
        }

        
        private void InitTodoDataTables(string[] xMLDocNames, ref List<DataTableSelectItem> toGenerateDataTables)
        {
            if ((xMLDocNames?.Length ?? -1) > 0)
            {
                toGenerateDataTables = xMLDocNames.Select(a => new DataTableSelectItem()
                    { DataTableName = a, Selected = false }).ToList();
            }
            else toGenerateDataTables = new List<DataTableSelectItem>(0);
        }
        
        #region OnGUI
        
        public void Draw()
        {
            EditorGUILayout.LabelField("Data Table", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(5f);

                DrawBrowseXmlFileDirectory();

                GUILayout.Space(5f);

                DrawGenerateDataTables();

                GUILayout.Space(5f);
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBrowseXmlFileDirectory()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("XML Directory", GUILayout.Width(120f));

                m_InputXmlDirectoryPath = EditorGUILayout.TextField(m_InputXmlDirectoryPath);

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    BrowseXmlDirectory();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private Vector2 mScrollRectPosition = Vector2.zero;
        
        /// <summary>
        /// 跳过导入ScenePoint表
        /// </summary>
        private bool IsJumpScenePoint = true;
        
        private void DrawGenerateDataTables()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("全选"))
                {
                    m_ToGenerateDataTables.ForEach(item => item.Selected = true);
                }
                
                if (GUILayout.Button("反选"))
                {
                    m_ToGenerateDataTables.ForEach(item => item.Selected = !item.Selected);
                }
                
                if (GUILayout.Button("清空"))
                {
                    m_ToGenerateDataTables.ForEach(item => item.Selected = false);
                }

                IsJumpScenePoint = GUILayout.Toggle(IsJumpScenePoint, "跳过ScenePoint表(该条件最优先)");

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Generate Data Tables", GUILayout.Width(160f)))
                {
                    // 生成数据表
                    GenerateDataTables();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            mScrollRectPosition = EditorGUILayout.BeginScrollView(mScrollRectPosition,GUILayout.MaxHeight(300f));
            {
                m_ToGenerateDataTables.ForEach(item =>
                {
                    EditorGUILayout.BeginHorizontal();
                    item.Selected = EditorGUILayout.Toggle(item.Selected, GUILayout.Width(15f));
                    EditorGUILayout.LabelField(item.DataTableName);
                    EditorGUILayout.EndHorizontal();
                });
            }
            EditorGUILayout.EndScrollView();
            
        }
        
        #endregion

        /// <summary>
        /// 生成数据表
        /// </summary>
        private void GenerateDataTables()
        {
            m_GeneratedDataTableCount = 0;

            const string titleName = "DataTables Generate";

            {
                var toGenerateDataTables = m_ToGenerateDataTables.Where(p => p.Selected)
                    .Select(p => p.DataTableName).ToList();
                for (var i = 0; i < toGenerateDataTables.Count; i++)
                {
                    UpdateProgress(i, toGenerateDataTables.Count, toGenerateDataTables[i], titleName);

                    try
                    {
                        var dataTableName = toGenerateDataTables[i];
                        if (IsJumpScenePoint)
                        {
                            if (dataTableName.ToLower().Equals("scenepoint".ToLower()))
                            {
                                continue;
                            }
                        }
                        GenerateDataTable(dataTableName);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();

            Debug.Log($"<color=green>Generate [{m_GeneratedDataTableCount}] Data Tables DONE!</color>");
        }

        /// <summary>
        /// 生成数据表
        /// </summary>
        private void GenerateDataTable(string dataTableName)
        {
            if (string.IsNullOrEmpty(m_InputXmlDirectoryPath) ||
                !Directory.Exists(m_InputXmlDirectoryPath))
            {
                Debug.LogError("XML File Directory is Invalid!");
                return;
            }

            // 设置工作目录
            DataTableGenerator.InitWorkingPath(m_OutputBinaryDirectoryPath, m_OutputCodeDirectoryPath,
                m_InputTemplateFilePath);

            DataTableProcessor dataTableProcessor = null;
            try
            {
                dataTableProcessor =
                    DataTableGenerator.CreateDataTableProcessor(m_InputXmlDirectoryPath, dataTableName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to Process table {dataTableName} msg:\n{ex.ToString()}");
                return;
            }

            if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
            {
                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                return;
            }

            // 生成数据表二进制文件
            DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
            // 生成数据表 C# 类
            DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);

            m_GeneratedDataTableCount++;

            Debug.Log($"Generate Data Table <color=green>[{dataTableName}]</color> OK.");
        }
    }
    
    public void InitDataTableTool()
    {
        dataTableTool.Init();
    }
    #endregion
    
    
}

   



