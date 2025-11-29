namespace trpolab
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DataRepository.LoadAll();
            var loginForm = new LoginForm();
            if(loginForm.ShowDialog() == DialogResult.OK)
            {
                Form1.CurrentUser = loginForm.AuthUser;
                Application.ApplicationExit += (s,e) => DataRepository.SaveAll();
                Application.Run(new Form1());
            }
        }
    }
}