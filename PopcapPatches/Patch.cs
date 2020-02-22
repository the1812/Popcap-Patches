using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PopcapPatches
{
  sealed class PatchException: Exception
  {
    public PatchException()
    {
    }
    public PatchException(string message): base(message) 
    {
    }
    public PatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
  sealed class Patch
  {
    private string inputString;
    public byte[] Output { get; private set; }
    public Patch(byte[] input)
    {
      var hex = new StringBuilder(input.Length * 2);
      foreach (var b in input)
      {
        hex.AppendFormat("{0:X2}", b);
      }
      inputString = hex.ToString();
      Debug.WriteLine("inputString: " + inputString.Substring(0, 36));
      Output = new byte[input.Length];
      input.CopyTo(Output, 0);
    }
    sealed class BytePattern
    {
      public string[] Pattern { get; set; }
      public int Match(string data)
      {
        var regexText = Pattern.Select(p =>
        {
          if (p == "*")
          {
            return ".{2}";
          }
          return p.ToUpperInvariant();
        }).Aggregate((acc, it) => acc + it);
        var regex = new Regex(regexText);
        var matches = regex.Matches(data);
        if (matches.Count == 0)
        {
          throw new PatchException("No match in data");
        }
        if (matches.Count > 1)
        {
          throw new PatchException("Too many matches in data");
        }
        return matches[0].Index / 2;
      }
    }
    public void RemoveSignatureCheck()
    {
      var pattern = new BytePattern
      {
        Pattern = new string[]
        {
          "C.{1}", "FF", "D2", "84", "C0",
          "0F", "*", "*", "*", "00", "00", "68",
        }
      };
      var index = pattern.Match(inputString);
      Output[index + 5] = Convert.ToByte("E9", 16);
      Output[index + 6] = (byte)(Output[index + 7] + 1);
      Output[index + 7] = Output[index + 8];
      Output[index + 8] = 0;
      Output[index + 10] = Convert.ToByte("90", 16); ;
    }
    public void RemoveVideoMemoryCheck()
    {
      var pattern = new BytePattern
      {
        Pattern = new string[]
        {
          "FF", "C1", "E8", "14", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*",
          "73",
        }
      };
      var index = pattern.Match(inputString) + 16;
      Output[index] = Convert.ToByte("EB", 16);
    }
    public void RemoveVideoCardCheck()
    {
      var pattern = new BytePattern
      {
        Pattern = new string[]
        {
          "8B", "*", "50", "EB", "03", "8D", "*", "*", "*", "*", "*", "*", "FF", "FF", "84", "C0",
          "75",
        }
      };
      var index = pattern.Match(inputString) + 16;
      Output[index] = Convert.ToByte("EB", 16);
    }
  }
}
