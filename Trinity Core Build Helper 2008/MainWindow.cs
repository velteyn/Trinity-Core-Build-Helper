using System;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;


namespace Trinity_Core_Build_Helper_2008
{
    public partial class MainWindow : Form
    {
        private string downloadPath;
        public MainWindow()
        {
            InitializeComponent();
            //-----------------------------------------
            this.downloadPath = recuperaDownloadPath(); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //***  leggo i settings che mi servono  ***
            String trinirtyURL = Properties.Settings.Default.TrinityURL;
            String proxySett = Properties.Settings.Default.ProxySettings;
            String fullDBURL = Properties.Settings.Default.FullDBLink;
            int proxyPort = Properties.Settings.Default.ProxyPort;
            try
            {


                /// <summary>
                /// Qui inizio a scaricare il file nella cartellawork
                /// </summary>

                WebClient wClient = new WebClient();

                if (!proxySett.Equals(""))
                {
                    WebProxy prx = new WebProxy();
                    Uri prxUrl = new Uri(proxySett + ":" + proxyPort);
                    prx.Address = prxUrl;
                    prx.UseDefaultCredentials = true;
                    wClient.Proxy = prx;
                }
                textBoxOutput.AppendText("Initiating download.\n"); 
                new FileIOPermission(FileIOPermissionAccess.Write, downloadPath).Demand();
                wClient.DownloadFile(trinirtyURL, downloadPath + "\\TrinityCore.zip");
                wClient.DownloadFile(fullDBURL, downloadPath + "\\FullDB.rar");
                textBoxOutput.AppendText("Download finished.\n"); 
                

            }
            catch (System.IO.IOException excp)
            {
                MessageBox.Show(excp.Message);
            }

        }
        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
                                 
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
            {
                File.Delete(zipPathAndFile);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //qua lo devo scompattare !!
            textBoxOutput.AppendText("Uncompressing...\n");
            try
            {
                UnZipFiles(downloadPath + "\\TrinityCore.zip", downloadPath, null, true);
            }
            catch ( FileNotFoundException ) {
                textBoxOutput.AppendText("Trinity ZIP not found n!\n");
            
            }
            Chilkat.Rar rar = new Chilkat.Rar();
            if (rar.Open(downloadPath + "\\FullDB.rar"))
            {
                rar.Unrar(downloadPath);
                textBoxOutput.AppendText("Unrar the main DB!\n");
                File.Delete(downloadPath + "\\FullDB.rar");
            }
            else
             textBoxOutput.AppendText("Unrar main DB FAILED !!\n"+rar.LastErrorText);

            rar.Close();
            

            textBoxOutput.AppendText("Done!\n");

           
        }
        private String recuperaDownloadPath()
        {

            //application exec path
            string path;
            path = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            String[] pezziPath = path.Split('\\');
            int i = 0;
            for (i = pezziPath.Length - 1; i > 0; i--)
            {

                if (pezziPath[i].Equals("Trinity Core Build Helper 2008"))
                    break;

            }
            path = "";
            for (int k = 1; k <= i; k++)
            {
                if (k > 1)
                    path = path + '\\' + pezziPath[k];
                else
                    path = pezziPath[k];
            }

            if (i == 0)
            {
                MessageBox.Show("Program direcrtory must be 'Trinity Core Helper'");
                Application.Exit();
            }
            else
                path = path + "\\work";

            return path;

        }
     
        private void button3_Click(object sender, EventArgs e)
        {
            //bene ho tutto !
            //adesso ci colleghiamo al db e uno alla volta 
            //parsiamo tutti i files e li ne facciamo le query fino ad arrivare alla cazzo di fine
            //qui  mi buildo la lista dei files da smandruppare
            

            string[] dirPaths = Directory.GetDirectories(this.downloadPath);
            string[] filePaths = Directory.GetFiles(this.downloadPath); 

            String TrinityCoreDir = dirPaths[0];

            /// <summary>
            //The following steps are done using your database management program (ex. HeidiSQL or SQLYog): 

            //1.Create the three databases by importing C:\Trinity\sql\create\create_mysql.sql. You now have three databases - auth, characters, and world. 
            //2.Import auth database structure by importing C:\Trinity\sql\base\auth_database.sql to the auth DB. 
            //3.Import characters database structure by importing C:\Trinity\sql\base\character_database.sql to the characters DB. 
            //4.Import the world database structure by extracting and importing the "TDB_full" .sql file you downloaded from the Downloading the Database section. 
            /// </summary>

            //e poi uno alla volta eseguo 
            // il comando che ne crea un entry nel db !!!
            //zio can !!
            String sqlFile = TrinityCoreDir + "\\sql\\create\\create_mysql.sql";

            executeSQL(sqlFile);

             sqlFile = TrinityCoreDir + "\\sql\\base\\auth_database.sql";

             executeSQL(sqlFile, "auth");

             sqlFile = TrinityCoreDir + "\\sql\\base\\characters_database.sql";

            executeSQL(sqlFile, "characters");

            textBoxOutput.AppendText("Base DB structure CREATED !!\n");

            //full DB !!
            textBoxOutput.AppendText("Building world DB\n");
            foreach (String item in filePaths)
            {
                if (item.Contains("full"))
                    executeSQL(item, "world");
                
            }
            
            //ora i parziali dentro alla cartella UPDATE ! AND YOU ARE READY TO GO !!


            string[] updateWorldPathsFiles = Directory.GetFiles(TrinityCoreDir+"\\sql\\updates\\world");
            textBoxOutput.AppendText("Patching world DB\n");
            foreach (String item in updateWorldPathsFiles)
            {
                
                    executeSQL(item, "world");

            }

            string[] updateWorldPathsCharacters = Directory.GetFiles(TrinityCoreDir + "\\sql\\updates\\characters");
            textBoxOutput.AppendText("Patching characters DB\n");
            foreach (String item in updateWorldPathsCharacters)
            {

                executeSQL(item, "characters");

            }

            string[] updateWorldPathsAuth = Directory.GetFiles(TrinityCoreDir + "\\sql\\updates\\auth");
            textBoxOutput.AppendText("Patching auth DB\n");
            foreach (String item in updateWorldPathsAuth)
            {

                executeSQL(item, "auth");

            }


        }

        private void executeSQL(String fileUndPath) {

            String args = "--batch ";
            args += "--host=" + Properties.Settings.Default.MysqlIP + " ";
            args += "--port=" + Properties.Settings.Default.MySqlPort + " ";
            args += "--user=\"" + Properties.Settings.Default.MySqlRoot + "\" ";
            args += "--password=\"" + Properties.Settings.Default.MySqlPassword + "\" ";
            args += "--execute=\" source ";
            args += fileUndPath + "\"";

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.StartInfo.FileName = Properties.Settings.Default.MySqlBinDir + "\\mysql.exe";
            proc.StartInfo.Arguments = args;
            proc.Start();
            textBoxOutput.AppendText(args);
            //textBoxOutput.AppendText(proc.StandardOutput.ReadToEnd()); //only for debug
            String errOut = proc.StandardError.ReadToEnd();
            if (errOut.Contains("ERROR"))
                textBoxOutput.AppendText(errOut + "\n");
            proc.WaitForExit();
        
        }
        private void executeSQL(String fileUndPath, String database)
        {

            String args = "--batch ";
            args += "--host=" + Properties.Settings.Default.MysqlIP + " ";
            args += "--port=" + Properties.Settings.Default.MySqlPort + " ";
            args += "--user=\"" + Properties.Settings.Default.MySqlRoot + "\" ";
            args += "--database=\""+database+"\" ";
            args += "--password=\"" + Properties.Settings.Default.MySqlPassword + "\" ";
            args += "--execute=\" source ";
            args += fileUndPath + "\"";

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.StartInfo.FileName = Properties.Settings.Default.MySqlBinDir + "\\mysql.exe";
            proc.StartInfo.Arguments = args;
            textBoxOutput.AppendText("Parsing " + fileUndPath+"\n");
            proc.Start();
            textBoxOutput.AppendText(args);
            //textBoxOutput.AppendText(proc.StandardOutput.ReadToEnd()); //only for debug
            String errOut = proc.StandardError.ReadToEnd();
            if(errOut.Contains("ERROR"))
               textBoxOutput.AppendText(errOut+"\n");
            proc.WaitForExit();

        }
    }
}
