﻿using System;
using System.IO;
using System.Reflection;

namespace Adrian_db_app
{
    public class LogWriter
    {
        private string m_exePath = string.Empty;
        public LogWriter()
        {
            //LogWrite(logMessage);
        }
        public void LogWrite(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log_"+DateTime.Now.ToString()+".txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("----------------------------------------------------");
            }
            catch (Exception ex)
            {
            }
        }
    }
}