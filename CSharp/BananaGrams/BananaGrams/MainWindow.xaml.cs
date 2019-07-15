using System.Windows;
using Processor;

namespace BananaGrams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WordModel _model;

        public MainWindow()
        {
            InitializeComponent();

            _model = new WordModel();
        }

        private void PerformSuggest(object sender, RoutedEventArgs e)
        {
            var suggested = _model.Suggest(Words.Text, Letters.Text);
            Answers.Text = suggested;
        }
    }
}
