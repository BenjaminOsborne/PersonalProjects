using System.Windows.Controls;
using System.Windows.Input;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for ConversationView.xaml
    /// </summary>
    public partial class ConversationView : UserControl
    {
        public ConversationView()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                CurrentChat.PreviewKeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Return && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) == false)
                    {
                        SendChat.Command.Execute(null);
                        args.Handled = true;
                    }
                };
            };
        }
    }
}
