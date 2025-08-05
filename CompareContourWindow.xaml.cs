using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;


namespace CompareContourScript
{
    /// <summary>
    /// Interaction logic for CompareContourWindow.xaml
    /// </summary>
    public partial class CompareContourWindow : Window
    {
        private ScriptContext _context; 

        public CompareContourWindow(ScriptContext context)
        {
            InitializeComponent();
            _context = context; 

            var patient = context.Patient;

            if (patient != null)
            {
                PatientDetailsText.Text = "Patient Name: " + patient.FirstName + " " + patient.LastName + "\n" + "Patient ID: " + patient.Id;
            }

            if (_context.StructureSet != null)
            {
                foreach (var structureSet in _context.Patient.StructureSets)
                {
                    StructureSetComboBox1.Items.Add(structureSet.Id);
                }
            }
            else
            {
                MessageBox.Show("No structure set is loaded for this patient.", "Error");
            }
            
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
