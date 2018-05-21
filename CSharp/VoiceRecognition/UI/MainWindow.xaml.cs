using System;
using System.Windows;
using System.Windows.Controls;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContextChanged += (sender, args) =>
            {
                WithDataContextOf<ScrollingTextViewModel>(DataContext, scrollModel =>
                {
                    AzureScroll.ScrollToBottom(); //Start at bottom
                    GoogleScroll.ScrollToBottom(); //Start at bottom

                    _HandleScroll(scrollModel.AzureModel, AzureScroll);
                    _HandleScroll(scrollModel.GoogleModel, GoogleScroll);
                });
            };
        }

        private void _HandleScroll(ItemsViewModelBase model, ScrollViewer scroll)
        {
            var isAtBottom = true;
            model.RegisterPreMessageUpdateAction(() => //This fires just before messages updated
            {
                isAtBottom = _IsScrollAtBottom(scroll);
            });
            model.RegisterPostMessageUpdateAction(() => //This fires just after messges updated
            {
                //Keep at bottom if either scroll already at bottom -OR- this user sent the last changed message to trigger the re-draw
                if (isAtBottom)
                {
                    AzureScroll.ScrollToBottom();
                }
            });
        }

        private static bool _IsScrollAtTop(ScrollViewer scroll) => scroll.VerticalOffset.Equals(0.0);
        private static bool _IsScrollAtBottom(ScrollViewer scroll) => scroll.VerticalOffset.Equals(scroll.ScrollableHeight);

        public static void WithDataContextOf<T>(object dataContext, Action<T> action) where T : class
        {
            if (dataContext == null)
            {
                return;
            }

            var model = dataContext as T;
            if (model == null)
            {
                return;
            }
            action(model);
        }
    }
}
