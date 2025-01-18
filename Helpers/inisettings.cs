using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Runtime.InteropServices;
namespace ByteKnightConsole.Helpers
{
    public class inisettings
    {



        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringA", CharSet = CharSet.Ansi)]
        private static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, string lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi)]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi)]
        private static extern int DeletePrivateProfileSection(string Section, int NoKey, int NoSetting, string FileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileIntA", CharSet = CharSet.Ansi)]
        private static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);

        private string strFilename;

        public string Path;
        /// <summary>
        /// Appends a message with a timestamp to the specified log file.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="logFilePath">The path of the log file.</param>
        private void LogMessage(string message, string logFilePath)
        {
            try
            {
                // Append the message to the log file
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                ExceptionHandler.LogError(ex);
                // Handle any exceptions that may occur while writing to the log file
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
        /// <summary>
        /// Reads a value from an INI file for the specified section and key.
        /// </summary>
        /// <param name="Section">The INI section.</param>
        /// <param name="Key">The key within the section.</param>
        /// <param name="DefaultValue">The default value if the key is not found.</param>
        /// <param name="BufferSize">The buffer size for reading the value.</param>
        /// <returns>The value read from the INI file, or "Error" if reading fails.</returns>
        public string ReadValue(string Section, string Key, string DefaultValue = "", int BufferSize = 1024)
        {
            string ReadValueRet = default;

            if (string.IsNullOrEmpty(Path))
            {
                Interaction.MsgBox("No path given" + Constants.vbNewLine + "Could not read Value", MsgBoxStyle.Critical, "No path given");
                ReadValueRet = "Error";
                return ReadValueRet;
            }

            if (File.Exists(Path) == false)
            {
                Interaction.MsgBox("File does not exist" + Constants.vbNewLine + "Could not read Value", MsgBoxStyle.Critical, "File does not exist");
                ReadValueRet = "Error";
                return ReadValueRet;
            }

            string sTemp = Strings.Space(BufferSize);
            int Length = GetPrivateProfileString(Section, Key, DefaultValue, sTemp, BufferSize, Path);
            return Strings.Left(sTemp, Length);

        }
        /// <summary>
        /// Retrieves a boolean value from the INI file for the specified section and key.
        /// </summary>
        /// <param name="Section">The INI section.</param>
        /// <param name="Key">The key within the section.</param>
        /// <param name="Default">The default boolean value if the key is not found.</param>
        /// <returns>The boolean value from the INI file.</returns>
        public bool GetBoolean(string Section, string Key, bool Default)
        {
            // Returns a boolean from your INI file
            return GetPrivateProfileInt(Section, Key, Conversions.ToInteger(Default), strFilename) == 1;
        }
        /// <summary>
        /// Gets the currently set path for the INI file.
        /// </summary>
        /// <returns>The file path.</returns>
        public string GetPath()
        {
            return Path;
        }
        /// <summary>
        /// Writes a value to the INI file for the specified section and key.
        /// </summary>
        /// <param name="Section">The INI section.</param>
        /// <param name="Key">The key within the section.</param>
        /// <param name="Value">The value to write.</param>
        /// <param name="path">The file path to the INI file.</param>
        public void WriteValue(string Section, string Key, string Value, string path)
        {

            if (string.IsNullOrEmpty(Path))
            {
                //do nothing
                //Interaction.MsgBox("No path given" + Constants.vbNewLine + "Could not write Value", MsgBoxStyle.Critical, "No path given");
                return;
            }

            string Ordner;
            Ordner = System.IO.Path.GetDirectoryName(path);
            if (Directory.Exists(Ordner) == false)
            {
                //do nothing
                //Interaction.MsgBox("File does not exist" + Constants.vbNewLine + "Could not write Value", MsgBoxStyle.Critical, "Files does not exist");
                return;
            }

            WritePrivateProfileString(Section, Key, Value, Path);
        }
        /// <summary>
        /// Deletes a key from the specified section of the INI file.
        /// </summary>
        /// <param name="Section">The INI section.</param>
        /// <param name="Key">The key to delete.</param>
        public void DeleteKey(string Section, string Key)
        {

            if (string.IsNullOrEmpty(Path))
            {
                Interaction.MsgBox("No path given" + Constants.vbNewLine + "Could not delete Key", MsgBoxStyle.Critical, "No path given");
                return;
            }

            string Ordner;
            Ordner = System.IO.Path.GetDirectoryName(Path);
            if (Directory.Exists(Ordner) == false)
            {
                Interaction.MsgBox("File does not exist" + Constants.vbNewLine + "Could not delete Key", MsgBoxStyle.Critical, "File does not exist");
                return;
            }

            string arglpString = null;
            WritePrivateProfileString(Section, Key, arglpString, Path);
        }
        /// <summary>
        /// Deletes an entire section from the INI file.
        /// </summary>
        /// <param name="Section">The section to delete.</param>
        public void DeleteSection(string Section)
        {

            if (string.IsNullOrEmpty(Path))
            {
                Interaction.MsgBox("No path given" + Constants.vbNewLine + "Could not delete Section", MsgBoxStyle.Critical, "No path given");
                return;
            }

            if (File.Exists(Path) == false)
            {
                Interaction.MsgBox("File does not exist (anymore)" + Constants.vbNewLine + "Could not delete Section", MsgBoxStyle.Critical, "File does not exist");
                return;
            }

            DeletePrivateProfileSection(Section, 0, 0, Path);
        }
        //destructor
        ~inisettings()
        {

        }




    }
}