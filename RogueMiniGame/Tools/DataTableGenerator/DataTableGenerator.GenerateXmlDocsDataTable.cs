using System.IO;

public partial class DataTableGenerator
{
    private static string OutputDataTableDirectoryPath = string.Empty;
    private static string OutputCSharpCodeDirectoryPath = string.Empty;
    private static string InputCSharpCodeTemplateFilePath = string.Empty;

    public static void InitWorkingPath(string outputDataTableDirectoryPath, string outputCSharpCodeDirectoryPath,
        string inputCSharpCodeTemplateFilePath)
    {
        OutputDataTableDirectoryPath = outputDataTableDirectoryPath;
        OutputCSharpCodeDirectoryPath = outputCSharpCodeDirectoryPath;
        InputCSharpCodeTemplateFilePath = inputCSharpCodeTemplateFilePath;
    }

    public static DataTableProcessor CreateDataTableProcessor(string dataTablePath, string dataTableName)
    {
        return new DataTableProcessor(
            dataTableName,
            Path.Combine(dataTablePath, dataTableName + ".xml"));
    }
}
