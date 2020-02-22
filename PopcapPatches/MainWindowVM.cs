using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PopcapPatches
{
  sealed class MainWindowVM : INotifyPropertyChanged
  {
    private readonly MainWindow window;
    public MainWindowVM(MainWindow window)
    {
      this.window = window;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    private string log = "Ready";
    public string Log
    {
      get => log;
      set
      {
        log = value;
        OnPropertyChanged(nameof(Log));
      }
    }
    public void AddLog(string log)
    {
      Log += $"\n{log}";
    }


    private string filename = "<No file selected>";
    public string Filename
    {
      get => filename;
      set
      {
        filename = value;
        OnPropertyChanged(nameof(Filename));
        OnPropertyChanged(nameof(CanPatch));
      }
    }
    public bool CanPatch => !Filename.StartsWith("<") && NotBusy;
    public bool RemoveVideoMemoryCheck { get; set; } = true;
    public bool RemoveVideoCardCheck { get; set; } = true;


    private string fileFullPath = "";
    public void SelectFile()
    {
      var dialog = new OpenFileDialog
      {
        Title = "Select file to patch",
        Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
        DefaultExt = ".exe",
      };
      if (dialog.ShowDialog(window) ?? false)
      {
        fileFullPath = dialog.FileName;
        Filename = Path.GetFileName(fileFullPath);
      }
    }


    private bool notBusy = true;
    public bool NotBusy
    {
      get => notBusy;
      set
      {
        notBusy = value;
        OnPropertyChanged(nameof(NotBusy));
        OnPropertyChanged(nameof(CanPatch));
      }
    }

    public async void RunPatch()
    {
      await Task.Run(() =>
      {
        try
        {
          NotBusy = false;
          Log = "";
          AddLog("Start patching...");
          AddLog($"File: {Filename}");
          var suffix = "-nosig";
          var patch = new Patch(File.ReadAllBytes(fileFullPath));
          AddLog("Remove signature check");
          patch.RemoveSignatureCheck();
          if (RemoveVideoMemoryCheck)
          {
            suffix += "+vmem";
            AddLog("Remove video memory check");
            patch.RemoveVideoMemoryCheck();
          }
          if (RemoveVideoCardCheck)
          {
            suffix += "+vard";
            AddLog("Remove video card check");
            patch.RemoveVideoCardCheck();
          }
          var outputFilename = $"{Path.GetFileNameWithoutExtension(Filename)}{suffix}{Path.GetExtension(Filename)}";
          var outputPath = Path.Combine(Path.GetDirectoryName(fileFullPath), outputFilename);
          AddLog($"Output: {outputFilename}");
          File.WriteAllBytes(outputPath, patch.Output);
          AddLog($"Patch completed");
        }
        catch (PatchException ex)
        {
          AddLog($"Error: {ex.Message}");
        }
        finally
        {
          NotBusy = true;
        }
      });
    }

  }
}
