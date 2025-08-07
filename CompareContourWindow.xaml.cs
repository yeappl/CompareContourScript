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
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;

namespace CompareContourScript
{
    /// <summary>
    /// Interaction logic for CompareContourWindow.xaml
    /// </summary>
    /// 
    public class ContourMatchItem
    {
        public string StructureName { get; set; }
        public double DiceCoefficient { get; set; }
    }

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
                    StructureSetComboBox1.Items.Add(structureSet);
                    StructureSetComboBox2.Items.Add(structureSet);
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

            var resultItems = new List<ContourMatchItem>();
            foreach (var name in commonNames)
            {
                var s1 = ss1.Structures.First(s => s.Id == name);
                var s2 = ss2.Structures.First(s => s.Id == name);

                double dsc = ComputeDiceCoefficient(s1, s2);

                resultItems.Add(new ContourMatchItem
                {
                    StructureName = name,
                    DiceCoefficient = dsc
                });
            }

            ResultsGrid.ItemsSource = resultItems;
        }

        private static double ComputeDiceCoefficient(Structure structure1, Structure structure2)
        {
            VVector p = new VVector();
            double volumeIntersection = 0;
            double volumeStructure1 = 0;
            double volumeStructure2 = 0;
            int intersectionCount = 0;
            int structure1Count = 0;
            int structure2Count = 0;
            double diceCoefficient = 0;

            Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
            Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
            Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds);

            // to allow the resolution to be on the same scale in each direction
            double startZ = Math.Floor(combinedRectBounds.Z - 1);
            double endZ = (startZ + Math.Round(combinedRectBounds.SizeZ + 2));
            double startX = Math.Floor(combinedRectBounds.X - 1);
            double endX = (startX + Math.Round(combinedRectBounds.SizeX + 2));
            double startY = Math.Floor(combinedRectBounds.Y - 1);
            double endY = (startY + Math.Round(combinedRectBounds.SizeY + 2));

            if (structure1 != structure2)
            {
                if (structure1Bounds.Contains(structure2Bounds))
                {
                    volumeIntersection = structure2.Volume;
                    volumeStructure1 = structure1.Volume;
                    volumeStructure2 = structure2.Volume;
                }
                else if (structure2Bounds.Contains(structure1Bounds))
                {
                    volumeIntersection = structure1.Volume;
                    volumeStructure1 = structure1.Volume;
                    volumeStructure2 = structure2.Volume;
                }
                else
                {
                    // using the bounds of each rectangle as the ROI for calculating overlap
                    for (double z = startZ; z < endZ; z += .5)
                    {
                        for (double y = startY; y < endY; y += 1)
                        {
                            for (double x = startX; x < endX; x += 1)
                            {
                                p.x = x;
                                p.y = y;
                                p.z = z;

                                // if ((structure2Bounds.Contains(p.x, p.y, p.z)) && (structure1.IsPointInsideSegment(p)) && (structure2.IsPointInsideSegment(p)))
                                if ((structure1.IsPointInsideSegment(p)) && (structure2.IsPointInsideSegment(p)))
                                {
                                    intersectionCount++;
                                }
                                if (structure1.IsPointInsideSegment(p))
                                    structure1Count++;
                                if (structure2.IsPointInsideSegment(p))
                                    structure2Count++;
                                volumeIntersection = (intersectionCount * 0.001 * .5);
                                volumeStructure1 = (structure1Count * 0.001 * .5);
                                volumeStructure2 = (structure2Count * 0.001 * .5);
                            }
                        }
                    }
                }
                diceCoefficient = Math.Round(((2 * volumeIntersection) / (volumeStructure1 + volumeStructure2)), 3);
                return diceCoefficient;
            }
            else
            {
                diceCoefficient = 1;
                return diceCoefficient;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
