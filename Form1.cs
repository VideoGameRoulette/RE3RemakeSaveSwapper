using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace RE3RemakeSaveSwapper
{
    public partial class Form1 : Form
    {
        readonly string SteamAppID = "952060";
        readonly string BackupFolder = $"{Application.StartupPath}\\Backup\\";
        readonly string NewGameData = $"{Application.StartupPath}\\Resources\\";
        readonly string NewGamePlusData = $"{Application.StartupPath}\\NewGamePlus\\";
        public string SaveFolder { get; private set; }
        public int SaveCount { get; private set; }
        public int BackupSaveCount { get; private set; }

        FolderBrowserDialog fbd = new FolderBrowserDialog();

        //FORM INIT
        public Form1()
        {
            InitializeComponent();
            if (!Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(BackupFolder);
                Directory.CreateDirectory($"{BackupFolder}ASSIST");
                Directory.CreateDirectory($"{BackupFolder}STANDARD");
                Directory.CreateDirectory($"{BackupFolder}HARDCORE");
                Directory.CreateDirectory($"{BackupFolder}NIGHTMARE");
                Directory.CreateDirectory($"{BackupFolder}INFERNO");
            }
            if (!Directory.Exists(NewGamePlusData))
            {
                var result = MessageBox.Show("No New Game + Data Found Backup Data?");
                if (result == DialogResult.OK)
                {
                    UpdateSteamList();
                    BackupNGP(SaveFolder, NewGamePlusData);
                }
            }
            else
            {
                ImportSaves.Enabled = true;
                NG.Enabled = true;
                NGP.Enabled = true;
                UpdateSteamList();
                UpdateBackupList();
            }
        }

        //FORM LOAD
        private void Form1_Load(object sender, System.EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string srtVersion = string.Format("v{0}", fvi.FileVersion.ToString());
            this.Text += string.Format(" - {0}", srtVersion);
        }

        //TIMER EVENTS
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            UpdateUI();
        }

        //FORM CLICK EVENTS
        private void OpenFolder_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = SaveFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void ExportSaves_Click(object sender, System.EventArgs e)
        {
            if (Directory.Exists(BackupFolder))
            {
                CopyAllFiles(SaveFolder, $"{BackupFolder}{Difficulty.Text}");
            }
            else
            {
                Directory.CreateDirectory(BackupFolder);
                CopyAllFiles(SaveFolder, $"{BackupFolder}{Difficulty.Text}");
            }
        }

        private void ImportSaves_Click(object sender, System.EventArgs e)
        {
            CopyAllFiles($"{BackupFolder}{Difficulty.Text}", SaveFolder);
        }

        private void NG_Click(object sender, System.EventArgs e)
        {

            foreach (var file in Directory.GetFiles(SaveFolder))
            {
                File.Delete(file);
            }
            var source = $"{NewGameData}data00-1.bin";
            var destination = $"{SaveFolder}\\data00-1.bin";
            File.Copy(source, destination);
        }

        private void NGP_Click(object sender, System.EventArgs e)
        {
            var file = $"{NewGamePlusData}\\data00-1.bin";
            File.Copy(file, file.Replace(NewGamePlusData, SaveFolder), true);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.twitch.tv/videogameroulette");
        }

        //FORM FUNCTIONS
        public bool GetSaveFile()
        {

            if (SaveFolder == null)
            {

                if (Directory.Exists("C:\\Steam\\userdata"))
                {
                    return OpenSaveDirectory("C:\\Steam\\userdata");
                }

                else if (Directory.Exists("D:\\Steam\\userdata"))
                {
                    return OpenSaveDirectory("D:\\Steam\\userdata");
                }

                else if (Directory.Exists("E:\\Steam\\userdata"))
                {
                    return OpenSaveDirectory("E:\\Steam\\userdata");
                }

                else if (Directory.Exists("C:\\Program Files (x86)\\Steam\\userdata"))
                {
                    return OpenSaveDirectory("C:\\Program Files (x86)\\Steam\\userdata");
                }

                else
                {
                    return ChooseFolder();
                }

            }

            else
            {
                return true;
            }

        }

        public bool OpenSaveDirectory(string currentDirectory)
        {
            string[] UserDirectories = Directory.GetDirectories(currentDirectory);
            foreach (string dir in UserDirectories)
            {
                if (Directory.Exists($"{dir}\\{SteamAppID}"))
                {
                    SaveFolder = $"{dir}\\{SteamAppID}\\remote\\win64_save";
                    UserSaveDir.Text = $"...{SteamAppID}\\remote\\win64_save";
                    return true;
                }
            }
            return false;
        }

        public bool ChooseFolder()
        {
            var result = MessageBox.Show("Error Finding Steam userdata! Would you like to set manually? ex. C:\\Program Files (x86)\\Steam\\userdata", "Steam Userdata Error");

            if (result == DialogResult.OK)
            {
                result = fbd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    return OpenSaveDirectory(fbd.SelectedPath);
                }

                else
                {
                    return ChooseFolder();
                }

            }

            else
            {
                return false;
            }
        }

        void BackupNGP(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (file == $"{sourceDir}\\data00-1.bin")
                {
                    File.Copy(file, file.Replace(sourceDir, targetDir), true);
                }
            }
        }

        void CopyAllFiles(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, file.Replace(sourceDir, targetDir), true);
            }
            UpdateSteamList();
            UpdateBackupList();
        }

        //FORM UPDATE FUNCTIONS
        void UpdateUI()
        {
            if (SaveFolder != null)
            {
                int check1 = Directory.GetFiles(SaveFolder).Length;
                int check2 = Directory.GetFiles($"{BackupFolder}{Difficulty.Text}").Length;
                if (check1 != SaveCount)
                {
                    SaveCount = check1;
                    UpdateSteamList();
                }

                else if (check2 != BackupSaveCount)
                {
                    BackupSaveCount = check2;
                    UpdateBackupList();
                }
            }
        }

        void UpdateSteamList()
        {
            bool result = GetSaveFile();

            if (result)
            {
                SteamSavesList.Items.Clear();
                DirectoryInfo di = new DirectoryInfo(SaveFolder);
                FileInfo[] files = di.GetFiles("*.bin");

                foreach (FileInfo file in files)
                {
                    SteamSavesList.Items.Add(file.Name);
                }
            }

            else
            {
                MessageBox.Show("Error Locating Save Folder.");
            }

        }

        void UpdateBackupList()
        {
            if (Directory.Exists($"{BackupFolder}{Difficulty.Text}"))
            {
                BackupSavesList.Items.Clear();
                DirectoryInfo di = new DirectoryInfo($"{BackupFolder}{Difficulty.Text}");
                FileInfo[] files = di.GetFiles("*.bin");

                foreach (FileInfo file in files)
                {
                    BackupSavesList.Items.Add(file.Name);
                }

                ImportSaves.Enabled = true;
            }

            else
            {
                BackupSavesList.Items.Clear();
                ImportSaves.Enabled = false;
            }

        }

        
    }
}
