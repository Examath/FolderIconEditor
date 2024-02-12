using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using FolderIcons;

namespace FolderIconEditor
{
    public partial class Folder : ObservableObject
    {
        private const string DESKTOP_INI = "\\desktop.ini";
        private const string ICON_PROPERTY_HEADER = "[.ShellClassInfo]";
        private const string ICON_PROPERTY_START = "IconFile=";
        private const string NOT_FOUND_PLACEHOLDER = "/Subject404.ico";

        /// <summary>
        /// Info about this directory
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; private set; }

        /// <summary>
        /// Path to Ini file
        /// </summary>
        private string DesktopIniPath => DirectoryInfo.FullName + DESKTOP_INI;

        #region Folder Properties

        private string _Name;
        /// <summary>
        /// Gets the name of this folder
        /// </summary>
        public string Name
        {
            get => _Name;
            private set => SetProperty(ref _Name, value);
        }

        /// <summary>
        /// Gets the subfolders
        /// </summary>
        public ObservableCollection<Folder> Children { get; private set; } = new();

        #endregion

        #region Icon

        private string? _IconPath;
        /// <summary>
        /// Gets or sets the path to the folder's icon
        /// </summary>
        public string? IconPath
        {
            get
            {
                if (_IconPath == null) return null;
                else if (Path.Exists(_IconPath)) return _IconPath;
                else return NOT_FOUND_PLACEHOLDER;
            }
            set
            {
                if (SetProperty(ref _IconPath, value))
                {
                    SetIconPath(value);
                }
            }
        }

        private string? GetIconPath()
        {
            if (Path.Exists(DesktopIniPath))
            {
                string[] data = File.ReadAllLines(DesktopIniPath);
                foreach (string line in data)
                {
                    if (line.StartsWith(ICON_PROPERTY_START))
                    {
                        return line[ICON_PROPERTY_START.Length..].Split(',')[0];
                    }
                }
            }
            return null;
        }

        private void SetIconPath(string? iconPath)
        {
            if (iconPath != null)
            {
                IniWriter.WriteValue(".ShellClassInfo", "IconFile",
                     iconPath, DesktopIniPath);
                IniWriter.WriteValue(".ShellClassInfo", "IconIndex",
                     "0", DesktopIniPath);
                SetIniFileAttributes(DesktopIniPath);
                SetFolderAttributes(DirectoryInfo.FullName);                
            }

            string? actualIcon = GetIconPath();
            if (actualIcon != iconPath)
            {
                _IconPath = actualIcon;
                OnPropertyChanged(nameof(IconPath));
            }
        }

        #endregion

        public Folder(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
            _Name = DirectoryInfo.Name;
            _IconPath = GetIconPath();

            foreach (DirectoryInfo subDirectoryInfo in DirectoryInfo.GetDirectories())
            {
                Children.Add(new Folder(subDirectoryInfo));
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region helper Methods

        // https://www.codeproject.com/Articles/9331/Create-Icons-for-Folders-in-Windows-Explorer-Using
        private bool SetIniFileAttributes(string path)
        {    
            // Set ini file attribute to "Hidden"
            if ((File.GetAttributes(path) & FileAttributes.Hidden)
                != FileAttributes.Hidden)
            {
                File.SetAttributes(path, File.GetAttributes(path)
                                   | FileAttributes.Hidden);
            }

            // Set ini file attribute to "System"
            if ((File.GetAttributes(path) & FileAttributes.System)
                   != FileAttributes.System)
            {
                File.SetAttributes(path, File.GetAttributes(path)
                                    | FileAttributes.System);
            }

            return true;
        }

        // https://www.codeproject.com/Articles/9331/Create-Icons-for-Folders-in-Windows-Explorer-Using
        private bool SetFolderAttributes(string path)
        {    
            // Set folder attribute to "System"
            if ((File.GetAttributes(path) & FileAttributes.System)
                != FileAttributes.System)
            {
                File.SetAttributes(path, File.GetAttributes
                                  (path) | FileAttributes.System);
            }

            return true;
        }

        #endregion
    }
}
