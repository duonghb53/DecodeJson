using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CrawlerData
{
    public partial class Form1 : Form
    {
        //private const string WEB_URL = @"https://vnexpress.net/suc-khoe/dau-lung-nen-nam-nem-cung-hay-mem-3944694.html";
        private const string WEB_URL = @"https://dantri.com.vn/suc-khoe/nguoi-nha-benh-nhan-say-xin-dam-vao-mat-nu-bac-si-20190627112323245.htm";
        //private const string WEB_URL = @"https://vnexpress.net/suc-khoe/be-trai-2-tuoi-bi-suy-dinh-duong-vi-tac-ta-trang-3945669.html";
        private const string PATTERN_DANTRI = @"<p>(?<title>[^<]*)<\/[pP]>";


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                string result = String.Empty;
                var data = new List<string>();

                List<string> listFile = GetAllFileJson(textBox2.Text);
                string folderRoot = System.IO.Directory.GetCurrentDirectory();

                string folderOutput = System.IO.Path.Combine(folderRoot, "Output");

                if (!System.IO.Directory.Exists(folderOutput))
                {
                    Directory.CreateDirectory(folderOutput);
                }


                foreach (string fileJson in listFile)
                {
                    string folder = Path.GetDirectoryName(fileJson);
                    var dirName = folder.Split('\\').Last();

                    //Tach Json
                    using (StreamReader r = new StreamReader(fileJson))
                    {
                        string json = r.ReadToEnd();
                        dynamic oJson = JsonConvert.DeserializeObject(json);
                        foreach (var chat in oJson.messages)
                        {
                            if (chat.content == null) continue;
                            data.Add(chat.content.ToString());
                        }
                        r.Close();
                    }

                    //Decode
                    foreach (string line in data)
                    {
                        string output = line.Replace("\'\"", String.Empty);
                        output = output.Replace("\"\'", String.Empty);
                        result += DecodeString(output) + Environment.NewLine;
                    }
                    string fileOut = folderOutput + "\\" + dirName + ".txt";
                    using (StreamWriter output = new StreamWriter(fileOut))
                    {
                        output.WriteLine(result);
                        output.Close();
                    }
                    textBox1.AppendText(fileJson + Environment.NewLine);
                }

                MessageBox.Show("Finish: " + listFile.Count.ToString() + " files");
            }
            catch (Exception ex)
            {
                MessageBox.Show("button1_Click: " + ex.ToString());
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox2.Text = folderDlg.SelectedPath;
                //Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private string DecodeString(string text)
        {
            Encoding targetEncoding = Encoding.GetEncoding("ISO-8859-1");
            var unescapeText = System.Text.RegularExpressions.Regex.Unescape(text);
            return Encoding.UTF8.GetString(targetEncoding.GetBytes(unescapeText));
        }

        private List<string> GetAllFileJson(string folderRoot)
        {
            List<string> listFolder = new List<string>();
            List<string> listFile = new List<string>();
            listFolder = GetSubDirectories(folderRoot);
            if (listFolder.Count == 0)
            {
                MessageBox.Show("Folder Input khong chua Folder con can xu li", "Warning!");
                return null;
            }

            foreach (string childFolder in listFolder)
            {
                string[] subFile = Directory.GetFiles(childFolder, "*.json");
                foreach (string file in subFile)
                {
                    listFile.Add(file);
                }
            }
            return listFile;
        }

        public List<string> GetSubDirectories(string folderName)
        {
            List<string> listFolder = new List<string>();
            // Get all subdirectories
            string[] subdirectoryEntries = Directory.GetDirectories(folderName);

            // Loop through them to see if they have any other subdirectories
            foreach (string subdirectory in subdirectoryEntries)
            {
                LoadSubDirs(subdirectory, ref listFolder);
                listFolder.Add(subdirectory);
            }
            return listFolder;
        }

        private void LoadSubDirs(string dir, ref List<string> listFolder)
        {

            //Console.WriteLine(dir);
            string[] subdirectoryEntries = Directory.GetDirectories(dir);

            foreach (string subdirectory in subdirectoryEntries)
            {

                LoadSubDirs(subdirectory, ref listFolder);
                listFolder.Add(subdirectory);
            }
        }

        string ReadTextFromUrl(string url)
        {
            try
            {
                // WebClient is still convenient
                // Assume UTF8, but detect BOM - could also honor response charset I suppose
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return textReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReadTextFromUrl: " + ex.ToString());
                return null;
            }       
        }

        public string GetContentNews(string data)
        {
            string result = String.Empty;
            //result = data;
            try
            {
                Regex regex = new Regex(textBox2.Text);
                MatchCollection matches = regex.Matches(data);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            result += match.Value.ToString() + Environment.NewLine;
                            //result = result.Replace(match.Value.ToString(), String.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetContentNews: " + ex.ToString());
                return null;
            }
            return result;
        }

        private string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);

            return htmlString;
        }
    }
}
