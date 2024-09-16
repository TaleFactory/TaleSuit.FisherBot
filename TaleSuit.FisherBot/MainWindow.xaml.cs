using AdonisUI.Controls;
using TaleSuit.FisherBot.Context;

namespace TaleSuit.FisherBot;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AdonisWindow
{
    public MainWindow()
    {
        DataContext = new MainWindowContext();
        InitializeComponent();
    }
}