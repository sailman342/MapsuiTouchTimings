using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RoadBookXF.RBDebugTools
{
    public class RBLogger
    {
        public static int LoggerID { get; private set; } = 0;

        private DateTime startTime;
        private string logText = "";
        public RBLogger()
        {

        }

        public void Clear()
        {
            LoggerID++;
            startTime = DateTime.Now;
            logText = "";
        }

        public void SetTimeToZero()
        {
            startTime = DateTime.Now;
        }

        public void Log(string text)
        {
            string addTxt = $" ({LoggerID:D2}) {(DateTime.Now-startTime):fff} {text} \r\n";
            logText += addTxt;
        }

        public void WriteToDiagnosticDebug()
        {
            System.Diagnostics.Debug.Write(logText +"\r\n");
            logText = "\r\n";
        }

        public void WriteToDebugLabel(Label label)
        {
            label.Text += logText + "\r\n";
            logText = "\r\n";
        }
    }
}
