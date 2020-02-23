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
    private readonly string inputString;
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
    public void RemoveSignatureCheck(string pattern, int startIndex)
    {
      var bytePattern = new BytePattern
      {
        Pattern = pattern.Replace("\n", " ").Replace("\r", " ").Split(' '),
      };
      var index = bytePattern.Match(inputString);
      Output[index + startIndex] = Convert.ToByte("E9", 16);
      Output[index + startIndex + 1] = (byte)(Output[index + startIndex + 2] + 1);
      Output[index + startIndex + 2] = Output[index + startIndex + 3];
      Output[index + startIndex + 3] = 0;
      Output[index + startIndex + 5] = Convert.ToByte("90", 16); ;
    }
    public void RemoveVideoMemoryCheck(string pattern, int startIndex)
    {
      var bytePattern = new BytePattern
      {
        Pattern = pattern.Replace("\n", " ").Replace("\r", " ").Split(' '),
      };
      var index = bytePattern.Match(inputString) + startIndex;
      Output[index] = Convert.ToByte("EB", 16);
    }
    public void RemoveVideoCardCheck(string pattern, int startIndex)
    {
      var bytePattern = new BytePattern
      {
        Pattern = pattern.Replace("\n", " ").Replace("\r", " ").Split(' '),
      };
      var index = bytePattern.Match(inputString) + startIndex;
      Output[index] = Convert.ToByte("EB", 16);
    }
  }
}
