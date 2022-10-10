using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ftpuploader.secret;

namespace ftpuploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private readonly Secret secret;

        public MainWindow()
        {
            this.secret = new Secret();
            InitializeComponent();
            this.URIText.Text = this.secret.URI;
            this.PathText.Text = this.secret.Path;
            this.FileNameText.Text = this.secret.FileName;
            this.UserNameText.Text = this.secret.UserName;
            this.PasswordText.Text = this.secret.Password;

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.ProgressChanged += this.backgroundworker_ProgressChanged;
            this.backgroundWorker.RunWorkerCompleted += this.backgroundworker_RunWorkerCompleted;
            this.backgroundWorker.DoWork += this.backgroundworker_DoWork;
            this.backgroundWorker.WorkerReportsProgress = true;
        }

        private void backgroundworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.FTPProgressBar.Value = e.ProgressPercentage;
            this.FTPProgressLabel.Content = $"{e.ProgressPercentage}% uploaded";
        }

        private void backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.FTPProgressLabel.Content = "Upload complete.";
        }

        private void backgroundworker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            this.SendFiles(
                (string)parameters[0],
                (string)parameters[1],
                (string)parameters[2],
                (string)parameters[3],
                (string)parameters[4],
                sender);
        }
        private void SendFiles(string requestURI, string localFilePath, string fileName, string userName, string password, object sender)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(requestURI + "/" + fileName);
                request.EnableSsl = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(userName, password);
                Stream requestStream = request.GetRequestStream();
                FileStream fileStream = File.OpenRead(localFilePath);

                byte[] buffer = new byte[1024];
                double fileLength = (double)fileStream.Length;
                int bytesRead = 0;
                double readCount = 0;

                do
                {
                    bytesRead = fileStream.Read(buffer, 0, 1024);
                    requestStream.Write(buffer, 0, bytesRead);
                    readCount += (double)bytesRead;
                    double percentageCompleted = readCount / fileLength * 100;
                    this.backgroundWorker.ReportProgress((int)percentageCompleted);
                } while (bytesRead > 0);

                fileStream.Close();
                requestStream.Close();
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string uri = this.URIText.Text;
            string path = this.PathText.Text;
            string filename = this.FileNameText.Text;
            string username = this.UserNameText.Text;
            string password = this.PasswordText.Text;
            object[] send = new object[] { uri, path, filename, username, password };
            this.backgroundWorker.RunWorkerAsync(send);
        }
    }
}
