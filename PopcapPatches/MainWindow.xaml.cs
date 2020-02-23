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

namespace PopcapPatches
{
  public partial class MainWindow : Window
  {
    private readonly MainWindowVM vm;
    public MainWindow()
    {
      InitializeComponent();
      vm = new MainWindowVM(this);
      DataContext = vm;
    }

    private void buttonOpenFile_Click(object sender, RoutedEventArgs e)
    {
      vm.SelectFile();
    }

    private void buttonPatch_Click(object sender, RoutedEventArgs e)
    {
      vm.RunPatch();
    }
    private void Window_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files.Length > 0)
        {
          vm.SelectFile(files[0]);
        }
      }
    }
  }
}
