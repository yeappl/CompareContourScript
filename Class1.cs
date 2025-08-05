using CompareContourScript;
using System;
using System.Windows;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext context)
        {
            if (context.Patient == null)
            {
                MessageBox.Show("No patient is loaded.", "Error");
                return;
            }

            Window window = new CompareContourWindow(context);
            window.ShowDialog();

        }
    }
}
