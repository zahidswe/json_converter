using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using Dapper;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace JsonConvertor
{
    public partial class Form1 : Form
    {
        public string DestinationFolder { get; set; }

        public IDbConnection _db { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Text File";
            openFileDialog.Filter = "TXT files|*.txt";

            List<string> errors = new List<string>();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    OpenDatabase();
                    DestinationFolder = _db.Database;

                    if (!Directory.Exists(DestinationFolder))
                        Directory.CreateDirectory(DestinationFolder);

                    string filename = openFileDialog.FileName;

                    string[] filelines = File.ReadAllLines(filename);

                    for (int a = 0; a < filelines.Length; a++)
                    {
                        string[] tables = filelines[a].Split(',');

                        foreach (var table in tables)
                        {
                            try
                            {
                                ConvertTableDataToJson(table);
                            }
                            catch (Exception ex)
                            {
                                errors.Add("Error for " + table + ": " + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
                finally
                {
                    if (_db.State == ConnectionState.Open)
                        _db.Close();
                }
                MessageBox.Show("Successfully Converted" + (errors.Any() ? " with following errors: " + string.Join("\n", errors) : "."));
            }
        }
        public void ConvertTableDataToJson(string table)
        {
            var items = _db.Query("select * from " + table).ToArray();
            string json = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });

            File.WriteAllText(DestinationFolder + "/" + table + ".json", json);
        }


        private void OpenDatabase()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            if (_db.State == ConnectionState.Closed)
                _db.Open();
        }
    }
}
