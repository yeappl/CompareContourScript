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
                    StructureSetComboBox2.Items.Add(structureSet.Id);
                }
            }
            else
            {
                MessageBox.Show("No structure set is loaded for this patient.", "Error");
            }
            
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            var ss1 = StructureSetComboBox1.SelectedItem as StructureSet;
            var ss2 = StructureSetComboBox2.SelectedItem as StructureSet;

            if (ss1 == null || ss2 == null)
            {
                MessageBox.Show("Please select two structure sets.");
                return;
            }

            // Find contour names common to both sets (case-sensitive exact match)
            var namesInSs1 = ss1.Structures.Select(s => s.Id).ToHashSet();
            var namesInSs2 = ss2.Structures.Select(s => s.Id).ToHashSet();

            var commonNames = namesInSs1.Intersect(namesInSs2).OrderBy(n => n).ToList();

            if (commonNames.Count == 0)
            {
                MessageBox.Show("No matching contour names found in both structure sets.");
                return;
            }

            // Build message string
            var sb = new StringBuilder();
            sb.AppendLine("Comparing contours:");

            foreach (var name in commonNames)
            {
                sb.AppendLine(name);
            }

            MessageBox.Show(sb.ToString());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
