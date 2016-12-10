using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SudokuSolver;

namespace SodukuWPFA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("In MainWindow");
        }

        public TextBox[,] textBoxArray;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Define textBox array
            textBoxArray = new TextBox[,]{  {textBox1,textBox2,textBox3,textBox4,textBox5,textBox6,textBox7,textBox8,textBox9},
                                                {textBox10,textBox11,textBox12,textBox13,textBox14,textBox15,textBox16,textBox17,textBox18},
                                                {textBox19,textBox20,textBox21,textBox22,textBox23,textBox24,textBox25,textBox26,textBox27},
                                                {textBox28,textBox29,textBox30,textBox31,textBox32,textBox33,textBox34,textBox35,textBox36},
                                                {textBox37,textBox38,textBox39,textBox40,textBox41,textBox42,textBox43,textBox44,textBox45},
                                                {textBox46,textBox47,textBox48,textBox49,textBox50,textBox51,textBox52,textBox53,textBox54},
                                                {textBox55,textBox56,textBox57,textBox58,textBox59,textBox60,textBox61,textBox62,textBox63},
                                                {textBox64,textBox65,textBox66,textBox67,textBox68,textBox69,textBox70,textBox71,textBox72},
                                                {textBox73,textBox74,textBox75,textBox76,textBox77,textBox78,textBox79,textBox80,textBox81}};
        }

        private void useDefault_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)useDefault.IsChecked)
                LoadDefault(3);
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        textBoxArray[i, j].Text = "";
                        textBoxArray[i, j].FontSize = 22;
                        textBoxArray[i, j].FontWeight = FontWeights.Normal;
                    }
                }
            }
        }

        private void LoadDefault(int mode)
        {
            InitialSetup setup = new InitialSetup(mode);
            List<IndividualCell> cellList = setup.SetCellList();

            foreach (IndividualCell singleCell in cellList)
            {
                if (singleCell.valueKnown)
                {
                    textBoxArray[singleCell.row - 1, singleCell.column - 1].Text = singleCell.cellValue.ToString();
                    textBoxArray[singleCell.row - 1, singleCell.column - 1].FontSize = 28;
                    textBoxArray[singleCell.row - 1, singleCell.column - 1].FontWeight = FontWeights.ExtraBold;
                }
                else
                    textBoxArray[singleCell.row - 1, singleCell.column - 1].Text = "";
            }
        }
        
        private void findSolution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[,] initialValues = LoadTextGridIntoCellList();
                InitialSetup initialSetup = new InitialSetup(initialValues);
                List<IndividualCell> cellList = initialSetup.SetCellList();

                bool solutionFound = SodukuConsoleProgram.MainLogic(ref cellList);
                
                if (!solutionFound)
                    throw new Exception("Solution not found!");

                if(!UtilityLogic.FinalSolutionFound(cellList))
                    throw new Exception("Solution not found with original cellList!");

                //print values out
                foreach (IndividualCell singleCell in cellList)
                {
                    if(singleCell.valueKnown)
                        textBoxArray[singleCell.row - 1, singleCell.column - 1].Text = singleCell.cellValue.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBar.Text = ex.Message;
            }

        }

        private int[,] LoadTextGridIntoCellList()
        {
            int[,] initialValues = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    TextBox localTextBox = textBoxArray[i, j];
                    int localInt;
                    if (localTextBox.Text == "")
                        localInt = 0;
                    else
                    {
                        try
                        {
                            localInt = Int32.Parse(localTextBox.Text);
                            if (localInt < 0 || localInt > 9)
                                throw new Exception("Integer not between 1 and 9");
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Invalid string for textBox: " +
                                                localTextBox.Name + ".\n" + ex.Message);
                        }
                    }

                    initialValues[i, j] = localInt;
                    if (localInt > 0 && localInt < 10)
                    {
                        localTextBox.FontSize = 28;
                        localTextBox.FontWeight = FontWeights.ExtraBold;
                    }
                }
            }
            return initialValues;
        }

        #region Old Text_Changed Methods
        //private string AssignData(TextBox textBox)
        //{
        //    justChanged = true;
        //    //Determine position
        //    string name = textBox.Name.ToString();
        //    int number = Int32.Parse(name.Substring(7));
        //    int x = -1;
        //    int y = Math.DivRem(number, 9, out x);
        //    y++;
        //    return x.ToString() + "_" + y.ToString() + "_" + textBox.Text; //i_j_val
        //}

        private bool justChanged = false;
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!justChanged)
            //{
            //    string result = AssignData((TextBox)e.Source);
            //    ((TextBox)sender).Text = result;
            //}
            //else
            //    justChanged = false;
        }


        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void textBox4_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox12_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox14_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox15_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox16_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox17_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox18_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox19_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox20_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox21_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox22_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox23_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox24_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox25_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox26_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox27_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox28_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox29_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox30_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox31_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox32_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox33_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox34_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox35_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox36_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox37_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox38_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox39_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox40_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox41_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox42_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox43_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox44_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox45_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox46_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox47_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox48_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox49_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox50_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox51_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox52_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox53_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox54_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox55_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox56_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox57_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox58_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox59_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox60_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox61_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox62_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox63_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox64_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox65_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox66_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox67_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox68_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox69_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox70_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox71_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox72_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox73_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox74_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox75_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox76_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox77_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox78_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox79_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox80_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox81_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

#endregion

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var binding = new int[]{1,2,3,4,5};
        }

    }
}
