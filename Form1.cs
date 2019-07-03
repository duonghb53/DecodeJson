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
        public Form1()
        {
            InitializeComponent();
        }

        public class infoChat
        {
            public string sender_name;
            public string content;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
            //MessageBox.Show(textBox2.Text);

            try
            {
                List<string> listFile = GetAllFileJson(textBox2.Text);
                string folderRoot = System.IO.Directory.GetCurrentDirectory();
                string folderOutput = System.IO.Path.Combine(folderRoot, "Output");

                if (!System.IO.Directory.Exists(folderOutput))
                {
                    Directory.CreateDirectory(folderOutput);
                }


                foreach (string fileJson in listFile)
                {
                    List<string> dataContent = new List<string>();
                    var dataRaw = new List<infoChat>();
                    string folder = Path.GetDirectoryName(fileJson);
                    var dirName = folder.Split('\\').Last();

                    //Tach Json
                    using (StreamReader r = new StreamReader(fileJson))
                    {
                        string json = r.ReadToEnd();
                        dynamic oJson = JsonConvert.DeserializeObject(json);
                        foreach (var chat in oJson.messages)
                        {
                            infoChat info = new infoChat();
                            if (chat.content == null) continue;
                            info.sender_name = chat.sender_name.ToString();
                            info.content = chat.content.ToString();
                            dataRaw.Add(info);
                        }
                        r.Close();
                    }

                    //Decode
                    string senderPrev = string.Empty;
                    string content = string.Empty;
                    bool iv = false;

                    for (int i = dataRaw.Count - 1; i > 0; i--)
                    { 
                        dataRaw[i].content = dataRaw[i].content.Replace("\\\\", "");
                        dataRaw[i].content = dataRaw[i].content.Replace("\\", "");
                        dataRaw[i - 1].content = dataRaw[i - 1].content.Replace("\\\\", "");
                        dataRaw[i - 1].content = dataRaw[i - 1].content.Replace("\\", "");                    
                        string cur_name = DecodeString(dataRaw[i].sender_name);
                        string next_name = DecodeString(dataRaw[i - 1].sender_name);
                        if (!iv)
                        {
                            content = DecodeString(dataRaw[i].content);
                        }
                        if (cur_name.Equals(next_name))
                        {
                            content += " " + DecodeString(dataRaw[i - 1].content);
                            iv = true;
                            continue;
                        }
                        else
                        {
                            dataContent.Add(content);
                            iv = false;
                        }
                    }

                    string fileOut = folderOutput + "\\" + dirName + ".txt";
                    using (StreamWriter output = new StreamWriter(fileOut))
                    {
                        foreach (string str in dataContent)
                        {
                            output.WriteLine(str);
                        }
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
            }
        }

        private string DecodeString(string text)
        {
            //try
            //{
                Encoding targetEncoding = Encoding.GetEncoding("ISO-8859-1");
                var unescapeText = System.Text.RegularExpressions.Regex.Unescape(text);
                return Encoding.UTF8.GetString(targetEncoding.GetBytes(unescapeText));
            //}
            //catch (Exception ex)
            //{
            //    return string.Empty;
            //}

        }

        private List<string> GetAllFileJson(string folderRoot)
        {
            List<string> listFolder = new List<string>();
            List<string> listFile = new List<string>();
            listFolder = GetSubDirectories(folderRoot);
            listFolder.Add(folderRoot);
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
    }
}
