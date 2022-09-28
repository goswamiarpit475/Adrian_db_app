using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Adrian_db_app
{
    public partial class Form1 : Form
    {
        string currDir = string.Empty;
        Dictionary<int, string> connectionStrings = new Dictionary<int, string>();
        Dictionary<string, string> queries = new Dictionary<string, string>();
        string lastRunDate = string.Empty;
        bool isFileSaveEnabled = false;
        string fileType = string.Empty;
        string fileSaveLocation = string.Empty;

        bool isPostEnabled = false;
        string postFileType = string.Empty;
        string postUrl = string.Empty;
        LogWriter logWriterObj = null;

        public Form1()
        {
            InitializeComponent();
            logWriterObj = new LogWriter();
            
        }
        private void loadFile()
        {
            try
            {
                connectionStrings = new Dictionary<int, string>();
                queries = new Dictionary<string, string>();
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                currDir = Path.GetDirectoryName(path);

                XmlDocument doc = new XmlDocument();
                doc.Load(currDir + @"\config.xml");
                XmlNodeList connectionStringList = doc.SelectNodes("/config/connectionStrings/connectionString");
                foreach (XmlNode connectionString in connectionStringList)
                {
                    int dbkey = Convert.ToInt32(connectionString.Attributes["key"].Value);
                    string cn = connectionString.InnerText;
                    connectionStrings.Add(dbkey, cn);
                }
                XmlNodeList queryList = doc.SelectNodes("/config/queries/query");
                foreach (XmlElement queryElement in queryList)
                {
                    string queryKey = queryElement.Attributes["key"].Value;
                    string queryValue = queryElement.InnerText;
                    queries.Add(queryKey, queryValue);
                }
                XmlNodeList fileNode = doc.SelectNodes("/config/output/file");
                isFileSaveEnabled = Convert.ToBoolean(fileNode[0]["enabled"].InnerText);
                if (isFileSaveEnabled)
                {
                    fileType = fileNode[0]["type"].InnerText;
                    fileSaveLocation = fileNode[0]["location"].InnerText;
                }

                XmlNodeList postNode = doc.SelectNodes("/config/output/post");
                isPostEnabled = Convert.ToBoolean(postNode[0]["enabled"].InnerText);
                if (isPostEnabled)
                {
                    postFileType = postNode[0]["type"].InnerText;
                    postUrl = postNode[0]["url"].InnerText;
                }//last_run_date
                XmlNodeList lastRunDateNodeList = doc.SelectNodes("/config/last_run_date");
                lastRunDate = lastRunDateNodeList[0].InnerText;

                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(new TreeNode(doc.DocumentElement.Name));
                TreeNode tNode = new TreeNode();
                tNode = treeView1.Nodes[0];

                // SECTION 3. Populate the TreeView with the DOM nodes.
                AddNode(doc.DocumentElement, tNode);
                treeView1.ExpandAll();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    inTreeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = inTreeNode.Nodes[i];
                    AddNode(xNode, tNode);
                }
            }
            else
            {
                // Here you need to pull the data from the XmlNode based on the
                // type of node, whether attribute values are required, and so forth.
                inTreeNode.Text = (inXmlNode.OuterXml).Trim();
            }
        }
        private void WriteFile(string saveLocation,string dbKey,string queryKey,string fileType)
        {

        }
        private void PostFileToURL(string url,string dbKey,string queryKey)
        {

        }
        /*private void RunQuery(int dbKey, string queryKey)
        {
            string connectionString = connectionStrings[dbKey];
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand command = new SqlCommand(queries[queryKey], conn);
            
            // int result = command.ExecuteNonQuery();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine(String.Format("{0}", reader["id"]));
                }
            }

            conn.Close();
        }*/
        private DataTable GetDataFromDB(string ConnectionString, string SqlQuery)
        {
            DataTable Dt = new DataTable();
            try
            {
                SqlConnection SqlCon = new SqlConnection(ConnectionString);
                SqlDataAdapter Ada = new SqlDataAdapter(SqlQuery, SqlCon);
                Ada.Fill(Dt);
                return Dt;
            }
            catch(Exception ex) 
            {
                string logString = "query-->" + SqlQuery + " ||| connection string" + ConnectionString + "\n";
                logString += ex.Message;
                logWriterObj.LogWrite(logString);
                return Dt;
            }
        }
        private void exportToFile(string s,string location,int dbkey,string queryKey,string type) 
        {
            StreamWriter sw = null;
            try
            {
                string path = fileSaveLocation;//@"C:\Documents\{{DATABASE_KEY}}\{{QUERY_KEY}}\{{QUERY_KEY}}_{{EPOCH_TIME}}.{{TYPE}}";
            path = path.Replace("{{DATABASE_KEY}}", dbkey.ToString());
            path = path.Replace("{{QUERY_KEY}}", queryKey.ToString());
            path = path.Replace("{{EPOCH_TIME}}", GetEpochTime());
            path = path.Replace("{{TYPE}}", type);
           
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            string Filename = path;

                    sw = new StreamWriter(Filename, false);
                    
                    sw.WriteLine(s);
                    
                    sw.Close();
                logWriterObj.LogWrite("file created successflly||||\n" + dbkey.ToString() + "|||" + queries[queryKey]);
            }
            catch (Exception Ex)
            {
                // Logger.Log("", "", Ex.Message);

                if (sw != null)
                {
                    sw.Close();
                }
                logWriterObj.LogWrite("Error in exportToFile method||||\n"+dbkey.ToString()+"|||"+queryKey+"\n"+Ex.Message);
            }
            finally
            {
            }
        }

        private string GetXMLFromDataTable(DataTable dt1)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt1); // Table 1
            string dsXml = ds.GetXml();
            MemoryStream s = new MemoryStream();
            string response=string.Empty;
            using (StreamWriter fs = new StreamWriter(s))// (xmlFile)) // XML File Path
            {
                ds.WriteXml(fs);
                var res = Encoding.UTF8.GetString(s.GetBuffer(), 0, (int)s.Length);
                response = res.ToString();
            }
            
            return response;
        }
        private string GetCSVfromDataTable(DataTable Dt)
        {
            string response = string.Empty;
            if (Dt != null)
            {
                //StreamWriter sw = null;
                StringBuilder sb = new StringBuilder();
                //sw = new StreamWriter(s);
                
                    string Head = "";
                    for (int j = 0; j < Dt.Columns.Count; j++)
                    {
                        if (Head.Trim() != "")
                        {
                            Head += ",\"" + Dt.Columns[j].ColumnName + "\"";
                        }
                        else
                        {
                            Head += "\"" + Dt.Columns[j].ColumnName + "\"";
                        }
                    }
                sb.AppendLine(Head);
                //sb.AppendLine();
                    //sw.WriteLine();
                    for (int j = 0; j < Dt.Rows.Count; j++)
                    {
                        string[] dataArr = new String[Dt.Rows[j].ItemArray.Length];
                        for (int i = 0; i < Dt.Rows[j].ItemArray.Length; i++)
                        {
                            object o = Dt.Rows[j].ItemArray[i].ToString();
                            dataArr[i] = "\"" + o.ToString() + "\"";
                        }
                        sb.AppendLine(string.Join(",", dataArr));
                    }
                    //var res = Encoding.UTF8.GetString(s.GetBuffer(), 0, (int)s.Length);
                   response= sb.ToString();
            }
            else
            {
                // ClientScript.RegisterStartupScript(typeof(Page), "script", "&lt;script language=JavaScript>alert('No data to export')</script>");
                return string.Empty;
            }
            return response;
        }
        public string postXMLData(string destinationUrl, string requestXml,string type)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/"+type+"; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            return null;
        }

        private string GetEpochTime()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

            return unixTimeMilliseconds.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(KeyValuePair<int,string> connectionString in connectionStrings)
            {
                foreach(KeyValuePair<string,string> query in queries)
                {
                    string formattedQuery = GetFormattedQuery(query.Value);
                    DataTable dt = GetDataFromDB(connectionString.Value, formattedQuery);
                    if (isFileSaveEnabled)
                    {
                        //exportToCSV(dt, fileSaveLocation, connectionString.Key, query.Key);
                        
                        if (fileType.ToUpper() == "CSV")
                        {
                            string csvString = GetCSVfromDataTable(dt);
                            exportToFile(csvString, fileSaveLocation, connectionString.Key, query.Key,"csv");
                        }
                        else if (fileType.ToUpper() == "XML")
                        {
                            string xmlString = GetXMLFromDataTable(dt);
                            exportToFile(xmlString, fileSaveLocation, connectionString.Key, query.Key, "xml");
                        }
                    }
                    if (isPostEnabled)
                    {

                        if (postFileType.ToUpper() == "CSV")
                        {
                            string csvString = GetCSVfromDataTable(dt);
                            //exportToFile(csvString, fileSaveLocation, connectionString.Key, query.Key, "csv");
                            string url = postUrl;
                            url = url.Replace("{{DATABASE_KEY}}", connectionString.Key.ToString());
                            url = url.Replace("{{QUERY_KEY}}", query.Key);
                            postXMLData(url, csvString, "csv");
                        }
                        else if (postFileType.ToUpper() == "XML")
                        {
                            string xmlString = GetXMLFromDataTable(dt);
                            string url = postUrl;
                            url = url.Replace("{{DATABASE_KEY}}", connectionString.Key.ToString());
                            url = url.Replace("{{QUERY_KEY}}", query.Key);
                            postXMLData(url, xmlString, "xml");
                        }      
                    }
                }
            }
            saveXMLLastRunDate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadFile();
        }
        private void saveXMLLastRunDate()
        {
            string toDate = DateTime.Now.ToString("MM/dd/yyyy");
            XmlDocument doc = new XmlDocument();
            doc.Load(currDir + @"\config.xml");
            XmlNodeList lastRunDateNodeList = doc.SelectNodes("/config/last_run_date");
            lastRunDateNodeList[0].InnerText= toDate;
            doc.Save(currDir + @"\config.xml");
        }
        private string GetFormattedQuery(string query)
        {
            string formattedQuery = query;
            string lastRun = lastRunDate;
            string[] dateSplit = lastRun.Split('/');
            DateTime fromDate = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[0]),Convert.ToInt32(dateSplit[1]));
            DateTime lastRunDateUpdated = fromDate.AddDays(-1);
            lastRun = lastRunDateUpdated.ToString("MM/dd/yyyy");
            string toDate = DateTime.Now.ToString("MM/dd/yyyy");
            if (query.Contains("{{FROM_DATE}}"))
            {
                formattedQuery = formattedQuery.Replace("{{FROM_DATE}}", lastRun);
            }
            if (query.Contains("{{TO_DATE}}"))
            {
                formattedQuery = formattedQuery.Replace("{{TO_DATE}}", toDate);
            }
            return formattedQuery;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadFile();
        }
    }
}
