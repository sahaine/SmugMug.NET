using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmugMugTest.v2
{
   public class IniFile
   {
      public string Path;

      public bool Exists { get { return System.IO.File.Exists(Path); } }

      [DllImport("kernel32")]
      private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
      [DllImport("kernel32")]
      private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

      /// <summary>
      /// INIFile Constructor.
      /// </summary>
      /// <param name="iniPath"></param>
      public IniFile(string iniPath)
      {
         Path = iniPath;
      }

      /// <summary>
      /// Write Data to the INI File
      /// </summary>
      /// <param name="section"></param>
      /// Section name
      /// <param name="key"></param>
      /// Key Name
      /// <param name="value"></param>
      /// Value Name
      public void IniWriteValue(string section, string key, string value)
      {
         WritePrivateProfileString(section, key, value.Replace("\r\n", "|"), Path);
      }

      public void IniWriteValue(string key, string value)
      {
         WritePrivateProfileString("Data", key, value.Replace("\r\n", "|"), Path);
      }

      /// <summary>
      /// Read Data Value From the Ini File
      /// </summary>
      /// <param name="section"></param>
      /// <param name="key"></param>
      /// <param name="Path"></param>
      /// <returns></returns>
      public string IniReadValue(string section, string key)
      {
         var temp = new StringBuilder(255);
         var i = GetPrivateProfileString(section, key, "", temp, 255, Path);
         return temp.ToString().Replace("|", "\r\n");

      }

      public string IniReadValue(string key)
      {

         return IniReadValue("Data", key);

      }
   }
}
