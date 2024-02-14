using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

internal class IniFile
{
    //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
    private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

    private readonly string IniPath;

    public IniFile(string IniPath)
    {
        this.IniPath = new FileInfo(IniPath).FullName.ToString();
        if (this.IniPath.Equals("")) MessageBox.Show("Не нашел файл " + IniPath + "!", "Ошибка INI-файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public bool KeyExists(string section, string key) {
        return ReadKey(section, key, true).Length > 0;
    }

    public string ReadKey(string section, string key, bool ex = false) {
        StringBuilder retVal = new StringBuilder(1024);
        GetPrivateProfileString(section, key, "", retVal, 1024, IniPath);
        if (!ex) {
            if (retVal.ToString().Equals("")) 
                MessageBox.Show("Не нашел ключь '" + key + "'" + System.Environment.NewLine + "В секции '" + section + "'!", "Ошибка INI-файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (retVal.ToString().IndexOf(".") > 0 & retVal.ToString().IndexOf(".") < 5)
            {
                if (!float.TryParse(retVal.ToString(), out float f))
                {
                    if (float.TryParse(retVal.ToString().Replace(".", ","), out float _f))
                        return _f.ToString();
                    else
                        MessageBox.Show("Не могу преобразовать значение '" + retVal.ToString() + "!", "Ошибка INI-файла!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    return f.ToString();
            }
            else if (retVal.ToString().IndexOf(",") > 0 & retVal.ToString().IndexOf(",") < 5)
            {
                if (!float.TryParse(retVal.ToString(), out float f))
                {
                    if (float.TryParse(retVal.ToString().Replace(",", "."), out float _f))
                        return _f.ToString();
                    else
                        MessageBox.Show("Не могу преобразовать значение '" + retVal.ToString() + "!", "Ошибка INI-файла!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    return f.ToString();
            }
        }
        return retVal.ToString();
    }

    public void WriteKey(string section, string key, string value) {
        WritePrivateProfileString(section, key, value, IniPath);
    }

    public void DeleteKey(string section, string key) {
        WriteKey(section, key, null);
    }

    public void DeleteSection(string section) {
        WriteKey(section, null, null);
    }
}
