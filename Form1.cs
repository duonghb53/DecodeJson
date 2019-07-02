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
            try
            {
                List<string> dataContent = new List<string>();
                var data = new List<infoChat>();

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
                            infoChat info = new infoChat();
                            if (chat.content == null) continue;
                            info.sender_name = chat.sender_name.ToString();
                            info.content = chat.content.ToString();
                            data.Add(info);
                        }
                        r.Close();
                    }

                    //Decode
                    string senderPrev = string.Empty;
                    string content = string.Empty;
                    bool flag = false;
                    foreach (infoChat line in data)
                    {
                        string sender_name = DecodeString(line.sender_name);
                        if (sender_name.Equals(senderPrev))
                        {
                            content += " " + DecodeString(line.content);
                            senderPrev = sender_name;
                            flag = true;
                            continue;
                        }
                        else
                        {
                            if (flag)
                            {
                                dataContent.Add(content);
                                flag = false;
                            }
                            content = DecodeString(line.content);
                            senderPrev = sender_name;
                        }
                    }
                    string fileOut = folderOutput + "\\" + dirName + ".txt";
                    //FileStream fs = new FileStream(fileOut, FileMode.Create);//Tạo file mới tên là test.txt 
                    using (StreamWriter output = new StreamWriter(fileOut))
                    {
                        for (int i = dataContent.Count - 1; i >= 0; i--)
                        {
                            output.WriteLine(dataContent[i]);
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
            Encoding targetEncoding = Encoding.GetEncoding("ISO-8859-1");
            var unescapeText = System.Text.RegularExpressions.Regex.Unescape(text);
            return Encoding.UTF8.GetString(targetEncoding.GetBytes(unescapeText));
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
