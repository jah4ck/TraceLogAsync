using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TraceLogAsync
{
    
    public class LogWriter
    {
        
        private static LogWriter instance;
        private static Queue<Log> logQueue;
        private static string logDir = ConfigurationManager.AppSettings["logDir"];//ajouter dans conf
        //private static string logFile = ConfigurationManager.AppSettings["logFile"];//ajouter dans conf
        private static int maxLogAge = int.Parse(ConfigurationManager.AppSettings["maxLogAge"]);//age en seconde (toutes les x seconde on met à jour les log)
        private static int queueSize = int.Parse(ConfigurationManager.AppSettings["queueSize"]);//nombre dans queue (toutes les x ligne on met a jour les log)
        private static string CodeAppli = ConfigurationManager.AppSettings["CodeAppli"];
        private static DateTime LastFlushed = DateTime.Now;

        
        ~LogWriter()
        {
            FlushLog();
        }
        private LogWriter() 
        {
            

        }

        
       
        public static LogWriter Instance
        {
            get
            {
                
                if (instance == null)
                {
                    instance = new LogWriter();
                    logQueue = new Queue<Log>();
                }
                return instance;
            }
        }

        
        public void WriteToLog(string message, int codeErreur, string nameFile )
        {
           
            
            lock (logQueue)
            {
                
                Log logEntry = new Log(message, codeErreur, nameFile);
                logQueue.Enqueue(logEntry);

                
                if (logQueue.Count >= queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }            
        }

        private bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - LastFlushed;
            if (logAge.TotalSeconds >= maxLogAge)
            {
                LastFlushed = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }

       
        public void FlushLog()
        {

            while (logQueue.Count > 0)
            {
                Log entry = logQueue.Dequeue();

                string logPath = logDir + entry.NameFile + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter log = new StreamWriter(fs))
                    {

                        if (entry.Erreur == 2)
                        {
                            log.WriteLine(string.Format("{0}\t{1}\t{2}{3}", entry.LogTime, CodeAppli, "INFO : ", entry.Message));
                        }
                        else
                        {
                            log.WriteLine(string.Format("{0}\t{1}\t{2}{3}", entry.LogTime, CodeAppli, "ERREUR : ", entry.Message));
                        }

                    }
                }
            }
                        
        }
        
    }

    
    public class Log
    {
        public string Message { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }
        public int Erreur { get; set; }
        public string NameFile { get; set; }

        public Log(string message, int codeErreur, string nameFile)
        {
            Message = message;
            Erreur = codeErreur;
            NameFile = nameFile;
            LogDate = DateTime.Now.ToString("yyyy-MM-dd");
            LogTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
        }
    }

    

}
