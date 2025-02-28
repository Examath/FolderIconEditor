using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Examath.Core.Environment;

namespace FolderIconEditor
{
	public partial class Folder : ObservableObject
	{
		private const string DESKTOP_INI = "\\desktop.ini";
		private const string ICON_PROPERTY_KEY = "IconResource";
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

		public FileAttributes? FolderFileAttributes
		{
			get
			{
				try
				{
					return File.GetAttributes(DirectoryInfo.FullName);
				}
				catch (Exception)
				{
					return null;
				}
			}
		}

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
					if (line.StartsWith(ICON_PROPERTY_KEY + '='))
					{
						return line[(ICON_PROPERTY_KEY.Length + 1)..].Split(',')[0];
					}
				}
			}
			return null;
		}

		private void SetIconPath(string? iconPath)
		{
			try
			{
				FolderIconUpdater.SetFolderIcon(DirectoryInfo.FullName, iconPath);

				string? actualIcon = GetIconPath();
				if (actualIcon != iconPath)
				{
					_IconPath = actualIcon;
					OnPropertyChanged(nameof(IconPath));
				}
			}
			catch (Exception e)
			{
				Messager.OutException(e, "Setting Icon");
				throw;
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
	}
}
