using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// Рефакторинг примеров из Приложения А (govnokod.ru)
namespace RefactoredExamples
{
    // 1) Рефакторинг: Decompose Conditional, Extract Method
    // Было: if(mod==true) с дублированием кода
    public class Example1_SetMode
    {
        private Label label1;
        private Button button1, button2, button3, button4;

        // БЫЛО (плохо):
        private void set_mode_BAD(bool mod)
        {
            if (mod == true)
            {
                label1.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = false;
                button4.Enabled = false;
            }
            else
            {
                label1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = true;
                button4.Enabled = true;
            }
        }

        // СТАЛО (хорошо):
        private void SetMode(bool isEditMode)
        {
            SetControlsEnabled(isEditMode, label1, button1, button2);
            SetControlsEnabled(!isEditMode, button3, button4);
        }

        private void SetControlsEnabled(bool enabled, params Control[] controls)
        {
            foreach (var control in controls)
                control.Enabled = enabled;
        }
    }

    // 2) Рефакторинг: Replace Conditional with Polymorphism / Simplify
    // Было: бессмысленный switch, возвращающий одно и то же
    public class Example2_DriverStatus
    {
        private Dictionary<ClientStatus, string> m_driverStatusNames;

        public enum ClientStatus { Unknown, Free, Busy, InWay, Work, Break, Alarm }
        public class Driver { public ClientStatus Status { get; set; } }

        // БЫЛО (плохо):
        private string GetStatusName_BAD(Driver driver)
        {
            switch (driver.Status)
            {
                case ClientStatus.Unknown:
                    return m_driverStatusNames[ClientStatus.Unknown];
                case ClientStatus.Free:
                    return m_driverStatusNames[ClientStatus.Free];
                case ClientStatus.Busy:
                    return m_driverStatusNames[ClientStatus.Busy];
                case ClientStatus.InWay:
                    return m_driverStatusNames[ClientStatus.InWay];
                case ClientStatus.Work:
                    return m_driverStatusNames[ClientStatus.Work];
                case ClientStatus.Break:
                    return m_driverStatusNames[ClientStatus.Break];
                case ClientStatus.Alarm:
                    return m_driverStatusNames[ClientStatus.Alarm];
                default:
                    return "Unknown";
            }
        }

        // СТАЛО (хорошо):
        private string GetStatusName(Driver driver)
        {
            return m_driverStatusNames.TryGetValue(driver.Status, out var name) 
                ? name 
                : "Unknown";
        }
    }

    // 3) Рефакторинг: Replace Temp with Query, Simplify Expression
    // Было: i.ToString().Length == 1
    public class Example3_NumberLength
    {
        // БЫЛО (плохо):
        private void CheckNumber_BAD(uint i)
        {
            if (i.ToString().Length == 1)
            {
                // ...
            }
        }

        // СТАЛО (хорошо):
        private void CheckNumber(uint i)
        {
            if (IsSingleDigit(i))
            {
                // ...
            }
        }

        private bool IsSingleDigit(uint number) => number < 10;
    }

    // 4) Рефакторинг: Replace Loop with LINQ, Remove Control Flag
    // Было: цикл с конкатенацией строк
    public class Example4_StringConcatenation
    {
        // БЫЛО (плохо):
        private string GetSubstring_BAD(string source)
        {
            string destination = null;
            for (int i = 0; i < 13; i++)
                destination += source[i];
            return destination;
        }

        // СТАЛО (хорошо):
        private string GetSubstring(string source)
        {
            return source.Length >= 13 
                ? source.Substring(0, 13) 
                : source;
        }
    }

    // 5) Рефакторинг: Replace with Built-in Method
    // Было: множественные Replace для проверки числа
    public class Example5_IsNumber
    {
        // БЫЛО (плохо):
        private bool IsNumber_BAD(string str)
        {
            return (str.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "")
                .Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "")
                .Replace("8", "").Replace("9", "").Length == 0);
        }

        // СТАЛО (хорошо):
        private bool IsNumber(string str)
        {
            return !string.IsNullOrEmpty(str) && str.All(char.IsDigit);
        }
    }

    // 6) Рефакторинг: Extract Method, Inline Temp Variable
    // Было: дублирование выражений Replace
    public class Example6_DirectoryProcessing
    {
        // БЫЛО (плохо):
        private void ProcessDirectory_BAD(DirectoryInfo dir, string SubFolder, Stream stream)
        {
            stream.Write(new byte[] { (byte)1 }, 0, 1);
            stream.Read(new byte[1], 0, 1);
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(
                SubFolder.Replace('\\', '/') + dir.Name.Replace('\\', '/')).Length), 0, 4);
            stream.Write(Encoding.UTF8.GetBytes(SubFolder.Replace('\\', '/') +
                dir.Name.Replace('\\', '/')), 0,
                Encoding.UTF8.GetBytes(SubFolder.Replace('\\', '/') +
                dir.Name.Replace('\\', '/')).Length);
            stream.Read(new byte[1], 0, 1);
        }

        // СТАЛО (хорошо):
        private void ProcessDirectory(DirectoryInfo dir, string subFolder, Stream stream)
        {
            string normalizedPath = GetNormalizedPath(subFolder, dir.Name);
            byte[] pathBytes = Encoding.UTF8.GetBytes(normalizedPath);

            stream.Write(new byte[] { (byte)1 }, 0, 1);
            stream.Read(new byte[1], 0, 1);
            stream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            stream.Write(pathBytes, 0, pathBytes.Length);
            stream.Read(new byte[1], 0, 1);
        }

        private string GetNormalizedPath(string folder, string name)
        {
            return (folder + name).Replace('\\', '/');
        }
    }

    // 7) Рефакторинг: Replace Loop with Collection Initializer
    // Было: цикл со switch для заполнения массива
    public class Example7_DaysOfWeek
    {
        // БЫЛО (плохо):
        private string[] GetDays_BAD()
        {
            string[] days = new string[7];
            for (int i = 0; i < 7; i++)
            {
                switch (i)
                {
                    default:
                    case 0: days[i] = "Monday"; break;
                    case 1: days[i] = "Tuesday"; break;
                    case 2: days[i] = "Wednesday"; break;
                    case 3: days[i] = "Thursday"; break;
                    case 4: days[i] = "Friday"; break;
                    case 5: days[i] = "Saturday"; break;
                    case 6: days[i] = "Sunday"; break;
                }
            }
            return days;
        }

        // СТАЛО (хорошо):
        private string[] GetDays()
        {
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", 
                          "Friday", "Saturday", "Sunday" };
        }
    }

    // 8) Рефакторинг: Replace with String Interpolation / Format
    // Было: конкатенация строк для форматирования времени
    public class Example8_TimeFormatting
    {
        // БЫЛО (плохо):
        private void PrintTime_BAD()
        {
            DateTime dt = DateTime.Now;
            string h = dt.Hour.ToString().PadLeft(2, '0');
            string m = dt.Minute.ToString().PadLeft(2, '0');
            string s = dt.Second.ToString().PadLeft(2, '0');
            Console.WriteLine("--" + h + ":" + m + ":" + s + "--");
        }

        // СТАЛО (хорошо):
        private void PrintTime()
        {
            Console.WriteLine($"--{DateTime.Now:HH:mm:ss}--");
        }
    }

    // 9) Рефакторинг: Simplify Boolean Expression, Extract Method
    // Было: странная проверка на нечетность
    public class Example9_IsOdd
    {
        // БЫЛО (плохо):
        private bool IsOdd_BAD(int Counter)
        {
            return ((int)(Counter / 2) != Counter / 2.00 && Counter != 0);
        }

        // СТАЛО (хорошо):
        private bool IsOdd(int counter)
        {
            return counter != 0 && counter % 2 != 0;
        }
    }

    // 10) Рефакторинг: Consolidate Duplicate Conditional Fragments
    // Было: fl_end = true в обеих ветках
    public class Example10_ConsolidateConditional
    {
        private int Connected;
        private bool fl_end;

        // БЫЛО (плохо):
        private void CheckConnection_BAD()
        {
            if (Connected == 0)
            {
                Setup();
                fl_end = true;
            }
            else
                fl_end = true;
        }

        // СТАЛО (хорошо):
        private void CheckConnection()
        {
            if (Connected == 0)
                Setup();
            fl_end = true;
        }

        private void Setup() { }
    }

    // 11) Рефакторинг: Replace Loop with LINQ
    // Было: цикл с условием для фильтрации
    public class Example11_FilterList
    {
        // БЫЛО (плохо):
        private List<int> FilterList_BAD(List<int> arr)
        {
            List<int> tmpArr = new List<int>();
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] > 100)
                {
                }
                else
                    tmpArr.Add(arr[i]);
            }
            return tmpArr;
        }

        // СТАЛО (хорошо):
        private List<int> FilterList(List<int> arr)
        {
            return arr.Where(x => x <= 100).ToList();
        }
    }

    // 12) Рефакторинг: Simplify Conditional, Extract Method
    // Было: проверка длины массива
    public class Example12_ValidateIds
    {
        public class FormData { public string[] Keys { get; set; } }

        // БЫЛО (плохо):
        private void ValidateIds_BAD(FormData form)
        {
            var ids = form.Keys;
            if (ids.Length == 0 || ids.Length > 1)
            {
                throw new Exception();
            }
        }

        // СТАЛО (хорошо):
        private void ValidateIds(FormData form)
        {
            if (!HasExactlyOneKey(form))
                throw new InvalidOperationException("Form must have exactly one key");
        }

        private bool HasExactlyOneKey(FormData form) => form.Keys.Length == 1;
    }

    // 13) Рефакторинг: Extract Method, Simplify String Operations
    // Было: Split и Replace для разбора имени
    public class Example13_ParseName
    {
        public class Customer { public string Name { get; set; } }

        // БЫЛО (плохо):
        private void ParseName_BAD(Customer customer)
        {
            string[] nameParts = customer.Name.Split(' ');
            string firstName = nameParts[0];
            string lastName = customer.Name.Replace(nameParts[0], "").TrimStart(' ');
        }

        // СТАЛО (хорошо):
        private (string firstName, string lastName) ParseName(Customer customer)
        {
            var parts = customer.Name.Split(new[] { ' ' }, 2);
            return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
        }
    }

    // 14) Рефакторинг: Remove Control Flag (break), Simplify
    // Было: foreach с break на первой итерации
    public class Example14_GetFirstId
    {
        // БЫЛО (плохо):
        private int GetFirstId_BAD(string sourceIDs)
        {
            int sourceId = 0;
            foreach (string id in sourceIDs.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                sourceId = Convert.ToInt32(id);
                break;
            }
            return sourceId;
        }

        // СТАЛО (хорошо):
        private int GetFirstId(string sourceIDs)
        {
            var firstId = sourceIDs.Split(',')
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            return firstId != null ? Convert.ToInt32(firstId) : 0;
        }
    }

    // 15) Рефакторинг: Simplify String Operations, Extract Method
    // Было: сложная работа с массивами char
    public class Example15_GenerateEmail
    {
        private string PersonName;

        // БЫЛО (плохо):
        private string GenerateEmail_BAD()
        {
            string res;
            int i = PersonName.IndexOf(" ");
            char[] str1 = new char[i];
            PersonName.CopyTo(0, str1, 0, i);
            string str11 = new string(str1);
            char[] str2 = new char[PersonName.Length - i - 1];
            PersonName.CopyTo(i + 1, str2, 0, PersonName.Length - i - 1);
            string str22 = new string(str2);
            res = str11.ToString() + "." + str22.ToString();
            if (res.Length > 20)
            {
                str1 = new char[20];
                res.CopyTo(0, str1, 0, 20);
                res = new string(str1);
            }
            res += "@domain.ua";
            return res;
        }

        // СТАЛО (хорошо):
        private string GenerateEmail()
        {
            var parts = PersonName.Split(new[] { ' ' }, 2);
            if (parts.Length < 2) return "@domain.ua";

            string email = $"{parts[0]}.{parts[1]}";
            if (email.Length > 20)
                email = email.Substring(0, 20);

            return $"{email}@domain.ua";
        }
    }

    // 17) Рефакторинг: Extract Method, Simplify Logic
    // Было: сложная логика поиска середины текста
    public class Example17_SplitText
    {
        // БЫЛО (плохо):
        private string GetTextDiv2_BAD(string text)
        {
            int mid = text.Length / 2;
            int r = text.IndexOf(" ", mid); if (r < 0) r = 5000;
            int l = text.IndexOf(" ", 0, mid); if (l < 0) l = 5000;
            if (r - mid > mid - l)
                mid = l;
            else mid = r;
            if (mid == 5000) return "&nbsp" + text;
            return "&nbsp" + text.Substring(0, mid) + " <br/>&nbsp" + text.Substring(mid, text.Length - mid);
        }

        // СТАЛО (хорошо):
        private string GetTextDiv2(string text)
        {
            int splitIndex = FindBestSplitIndex(text);
            if (splitIndex < 0)
                return $"&nbsp{text}";

            return $"&nbsp{text.Substring(0, splitIndex)} <br/>&nbsp{text.Substring(splitIndex)}";
        }

        private int FindBestSplitIndex(string text)
        {
            int mid = text.Length / 2;
            int rightSpace = text.IndexOf(" ", mid);
            int leftSpace = text.LastIndexOf(" ", mid);

            if (rightSpace < 0 && leftSpace < 0) return -1;
            if (rightSpace < 0) return leftSpace;
            if (leftSpace < 0) return rightSpace;

            return (rightSpace - mid < mid - leftSpace) ? rightSpace : leftSpace;
        }
    }

    // 18) Рефакторинг: Simplify Initialization
    // Было: "$"[0] вместо '$'
    public class Example18_Constants
    {
        // БЫЛО (плохо):
        private static readonly char SPECIFIER_BAD = "$"[0];
        private static readonly char DELIMITER_BAD = ":"[0];
        private static readonly char[] DELIMITER_ARRAY_BAD = new char[1] { DELIMITER_BAD };

        // СТАЛО (хорошо):
        private const char SPECIFIER = '$';
        private const char DELIMITER = ':';
        private static readonly char[] DELIMITER_ARRAY = new[] { DELIMITER };
    }

    // 19) Рефакторинг: Extract Method, Decompose Conditional
    // Было: огромное тернарное выражение
    public class Example19_GetEmailAddress
    {
        public class Config
        {
            public static string GetSetting(string key) => null;
        }
        public class Globals
        {
            public static dynamic GetHostPortalSettings() => null;
        }

        // БЫЛО (плохо):
        private string GetMailTo_BAD()
        {
            string mailTo = ((Config.GetSetting("AdminNotifications_EmailAddress") == null) ||
                (Config.GetSetting("AdminNotifications_EmailAddress").Length <= 0)) ?
                Globals.GetHostPortalSettings().HostSettings["SMTPPassword"].ToString() :
                Config.GetSetting("AdminNotifications_EmailAddress");
            return mailTo;
        }

        // СТАЛО (хорошо):
        private string GetMailTo()
        {
            string configEmail = Config.GetSetting("AdminNotifications_EmailAddress");
            return IsValidEmail(configEmail) 
                ? configEmail 
                : GetDefaultEmail();
        }

        private bool IsValidEmail(string email) => !string.IsNullOrEmpty(email);
        private string GetDefaultEmail() => Globals.GetHostPortalSettings()?.HostSettings["SMTPPassword"]?.ToString();
    }

    // 20) Рефакторинг: Extract Method, Replace Magic Numbers, Use Collection
    // Было: 20 отдельных if для проверки файлов
    public class Example20_CheckPath
    {
        // БЫЛО (плохо) - слишком длинный метод с дублированием
        private bool CheckPath_BAD(string path)
        {
            int n = 0;
            if (Directory.Exists(path + "SCLAD")) n += 1;
            if (Directory.Exists(path + "REAL")) n += 1;
            if (Directory.Exists(path + "DOSTAVKA")) n += 1;
            if (File.Exists(path + "analit.dbf")) n += 1;
            // ... еще 16 проверок
            return n == 20;
        }

        // СТАЛО (хорошо):
        private bool CheckPath(string path)
        {
            return RequiredDirectories.All(dir => Directory.Exists(Path.Combine(path, dir))) &&
                   RequiredFiles.All(file => File.Exists(Path.Combine(path, file)));
        }

        private static readonly string[] RequiredDirectories = { "SCLAD", "REAL", "DOSTAVKA" };
        private static readonly string[] RequiredFiles = {
            "analit.dbf", "partner.dbf",
            @"SCLAD\mdoc.dbf", @"SCLAD\mdoc.fpt", @"SCLAD\mdocm.dbf",
            @"SCLAD\mgrup.dbf", @"SCLAD\mlabel.dbf", @"SCLAD\mlabel.fpt",
            @"REAL\rbookm.dbf", @"REAL\rbook.dbf", @"REAL\rbook.fpt",
            @"DOSTAVKA\avt.dbf", @"DOSTAVKA\avtm.dbf", @"DOSTAVKA\avtm.fpt",
            @"DOSTAVKA\cargo.dbf", @"DOSTAVKA\cargom.dbf", @"DOSTAVKA\zamena.dbf"
        };
    }

    // 21) Рефакторинг: Replace with String.Join, Remove Control Flag
    // Было: цикл с флагом first для конкатенации
    public class Example21_JoinContacts
    {
        private TextBox txtContacts;

        // БЫЛО (плохо):
        private void JoinContacts_BAD(string[] contacts)
        {
            txtContacts.Text = "";
            bool first = true;
            foreach (string contact in contacts)
            {
                if (first != true)
                    txtContacts.Text += ";";
                first = false;
                txtContacts.Text += contact;
            }
        }

        // СТАЛО (хорошо):
        private void JoinContacts(string[] contacts)
        {
            txtContacts.Text = string.Join(";", contacts);
        }
    }

    // 22) Рефакторинг: Simplify Boolean Assignment
    // Было: if-else для переключения bool
    public class Example22_ToggleBoolean
    {
        public class Game1 { public static bool clou; }

        // БЫЛО (плохо):
        private void ToggleCloud_BAD()
        {
            if (Game1.clou == true)
            { Game1.clou = false; }
            else
            { Game1.clou = true; }
        }

        // СТАЛО (хорошо):
        private void ToggleCloud()
        {
            Game1.clou = !Game1.clou;
        }
    }
}
