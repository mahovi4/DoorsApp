using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Program
{
    internal static readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    internal static readonly IniFile ini = new IniFile(appPath + "\\Settings.ini");

    internal static readonly Main mainForm = new Main();
    internal static readonly Konstruktor konstruktorForm = new Konstruktor();
    internal static readonly WorkFiles workFilesForm = new WorkFiles();
    internal static readonly WorkTable workTableForm = new WorkTable();
    internal static readonly InWork inWorkForm = new InWork();

    /// <summary>
    /// Главная точка входа для приложения.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        //Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(inWorkForm);
    }
}
