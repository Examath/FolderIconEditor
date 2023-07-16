using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FolderIconEditor
{
    public partial class DesktopIni : ObservableObject
    {
        public const string DESKTOP_INI = "\\desktop.ini";

        public DirectoryInfo DirectoryInfo { get; private set; }

        private string? _IconResource;
        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string? IconResource
        {
            get => _IconResource;
            set => SetProperty(ref _IconResource, value);
        }

        public async Task Load(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
            FileInfo[] fileInfos = DirectoryInfo.GetFiles();

            if (Path.Exists(directoryInfo.FullName + DESKTOP_INI))
            {
                string[] inif = await File.ReadAllLinesAsync(DESKTOP_INI);
            }
        }
    }
}
