using System;
using System.Windows;

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
                WithDataContextOf<ScrollingTextViewModel>(DataContext, model =>
                {
                    Scroll.ScrollToBottom(); //Start at bottom

                    var isAtBottom = true;
                    model.RegisterPreMessageUpdateAction(() => //This fires just before messages updated
                    {
                        isAtBottom = _IsScrollAtBottom();
                    });
                    model.RegisterPostMessageUpdateAction(() => //This fires just after messges updated
                    {
                        //Keep at bottom if either scroll already at bottom -OR- this user sent the last changed message to trigger the re-draw
                        if (isAtBottom)
                        {
                            Scroll.ScrollToBottom();
                        }
                    });
                });
            };
        }

        private bool _IsScrollAtTop() => Scroll.VerticalOffset.Equals(0.0);
        private bool _IsScrollAtBottom() => Scroll.VerticalOffset.Equals(Scroll.ScrollableHeight);

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
