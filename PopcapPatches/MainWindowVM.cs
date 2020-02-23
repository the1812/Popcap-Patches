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
    public void SelectFile(string filePath = null)
    {
      if (filePath is null)
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
      else
      {
        fileFullPath = filePath;
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

    public string SignatureCheckLeft { get; set; } = "C.{1} FF D2 84 C0";
    public string SignatureCheckCenter { get; set; } = "0F";
    public string SignatureCheckRight { get; set; } = "* * * 00 00 68";
    public string VideoMemoryCheckLeft { get; set; } = "FF C1 E8 14 * * * * * * * * * * * *";
    public string VideoMemoryCheckCenter { get; set; } = "73";
    public string VideoMemoryCheckRight { get; set; } = "";
    public string VideoCardCheckLeft { get; set; } = "8B * 50 EB 03 8D * * * * * * FF FF 84 C0";
    public string VideoCardCheckCenter { get; set; } = "75";
    public string VideoCardCheckRight { get; set; } = "";

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
          var signatureCheckPattern = $"{SignatureCheckLeft} {SignatureCheckCenter} {SignatureCheckRight}";
          patch.RemoveSignatureCheck(signatureCheckPattern, SignatureCheckLeft.Split(' ').Length);
          AddLog("Remove signature check: OK");
          if (RemoveVideoMemoryCheck)
          {
            suffix += "+vmem";
            var videoMemoryPattern = $"{VideoMemoryCheckLeft} {VideoMemoryCheckCenter} {VideoMemoryCheckRight}";
            patch.RemoveVideoMemoryCheck(videoMemoryPattern, VideoMemoryCheckLeft.Split(' ').Length);
            AddLog("Remove video memory check: OK");
          }
          if (RemoveVideoCardCheck)
          {
            suffix += "+vard";
            var videoCardPattern = $"{VideoCardCheckLeft} {VideoCardCheckCenter} {VideoCardCheckRight}";
            patch.RemoveVideoCardCheck(videoCardPattern, VideoCardCheckLeft.Split(' ').Length);
            AddLog("Remove video card check: OK");
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
