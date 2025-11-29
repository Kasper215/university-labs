using System.Windows.Forms;
using System.IO;

namespace trpolab
{
    public partial class Form1 : Form
    {
        public static User? CurrentUser;
        public Form1()
        {
            InitializeComponent();
            if (CurrentUser != null)
                this.Text = $"Главная ({CurrentUser.FullName}, {CurrentUser.Role})";
            this.клиентыToolStripMenuItem.Click += (s,e) => new ClientsForm().ShowDialog();
            this.заказыToolStripMenuItem.Click += (s,e) => new OrdersForm().ShowDialog();
            this.складToolStripMenuItem.Click += (s,e) => new InventoryForm().ShowDialog();
            this.производствоToolStripMenuItem.Click += (s,e) => new ProductionForm().ShowDialog();
            this.отчетыToolStripMenuItem.Click += (s,e) => new ReportsForm().ShowDialog();
            this.настройкиToolStripMenuItem.Click += (s,e) => new SettingsForm().ShowDialog();

            // Логотип: пробуем рядом с exe (logo.png), затем в assets/logo.png
            var baseDir = AppContext.BaseDirectory;
            var directLogo = Path.Combine(baseDir, "logo.png");
            var assetsLogo = Path.Combine(baseDir, "assets", "logo.png");
            var path = File.Exists(directLogo) ? directLogo : (File.Exists(assetsLogo) ? assetsLogo : null);
            try
            {
                if (path != null)
                {
                    using var bmpTemp = new System.Drawing.Bitmap(path);
                    this.pictureBoxLogo.Image = new System.Drawing.Bitmap(bmpTemp);
                }
            }
            catch { /* игнорируем ошибки чтения логотипа */ }
        }
    }
}
