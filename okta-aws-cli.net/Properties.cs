using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace okta_aws_cli.net
{
    public class Properties
    {
        private Dictionary<string, string> list;
        private string filename;

        public Dictionary<string, string> List { get { return this.list; } }

        public Properties(string file)
        {
            Reload(file);
        }

        public string GetProperty(string field, string defValue = null)
        {
            return (list.ContainsKey(field)) ? (list[field]) : defValue;
        }

        public void SetProperty(string field, Object value)
        {
            if (!list.ContainsKey(field))
                list.Add(field, value.ToString());
            else
                list[field] = value.ToString();
        }

        public void Save()
        {
            Save(this.filename);
        }

        public void Save(string filename)
        {
            this.filename = filename;

            if (!File.Exists(filename))
                File.Create(filename);

            StreamWriter file = new StreamWriter(filename);

            foreach (string prop in list.Keys.ToArray())
                if (!string.IsNullOrWhiteSpace(list[prop]))
                    file.WriteLine(prop + "=" + list[prop]);

            file.Close();
        }

        public void Reload()
        {
            Reload(this.filename);
        }

        public void Reload(string fileName)
        {
            this.filename = fileName;
            list = new Dictionary<string, string>();

            if (File.Exists(fileName))
                LoadFromFile(fileName);
            else
                File.Create(fileName);
        }

        private void LoadFromFile(string file)
        {
            foreach (string line in File.ReadAllLines(file))
            {
                if ((!string.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    try
                    {
                        //ignore dublicates
                        list.Add(key, value);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
