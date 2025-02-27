using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderIconEditor.Properties;
using System.Diagnostics;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Input;

namespace FolderIconEditor
{
    public partial class VM : ObservableValidator
    {
        #region Init

        public VM()
		{
			if (!Directory.Exists(Settings.Default.DefaultFolder))
            {
                Settings.Default.DefaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Settings.Default.Save();
            }
			_FolderDirectory = Settings.Default.DefaultFolder;
            UpdateFolders();


			if (!Directory.Exists(Settings.Default.IconSource))
			{
				Settings.Default.IconSource = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				Settings.Default.Save();
			}
			_IconDirectory = Settings.Default.IconSource;
            UpdateIcons();

            Debug.WriteLine("Load Complete");

        }

		#endregion

		#region Validation

		public static ValidationResult? DirectoryExists(string directory, ValidationContext context)
		{
			if (Directory.Exists(directory)) return ValidationResult.Success;
			else return new("Directory does not exist");
		}

		#endregion

		#region Folders

		private string _FolderDirectory;
        /// <summary>
        /// Gets or sets the directory of the folders to edit
        /// </summary>
        [CustomValidation(typeof(VM), nameof(DirectoryExists))]
        public string FolderDirectory
        {
            get => _FolderDirectory;
            set
            {
                if(SetProperty(ref _FolderDirectory, value, true)) UpdateFolders();                    
            }
        }

        public ObservableCollection<Folder> Folders { get; private set; } = new();

        private void UpdateFolders()
        {
            Settings.Default.DefaultFolder = _FolderDirectory;
            Settings.Default.Save();

            SelectedFolder = null;
            Folders.Clear();

            DirectoryInfo directoryInfo = new(FolderDirectory);

            if (directoryInfo.Exists)
            {
                foreach (DirectoryInfo subDirectory in directoryInfo.EnumerateDirectories())
                {
                    Folders.Add(new(subDirectory));
                }
            }
        }

        private Folder? _SelectedFolder;
        /// <summary>
        /// Gets or sets the selected folder
        /// </summary>
        public Folder? SelectedFolder
        {
            get => _SelectedFolder;
            set
            {
                if (SetProperty(ref _SelectedFolder, value)) 
                {
                    SelectedIcon = null;
                    SetIconCommand.NotifyCanExecuteChanged();
                    UnsetIconCommand.NotifyCanExecuteChanged();
                }
            }
        }

		#endregion

		#region Icons

		private string _IconDirectory;
		/// <summary>
		/// Gets or sets the directory of the icon source
		/// </summary>
		[CustomValidation(typeof(VM), nameof(DirectoryExists))]
		public string IconDirectory
		{
			get => _IconDirectory;
			set
			{
				if (SetProperty(ref _IconDirectory, value, true)) UpdateFolders();
			}
		}

		public ObservableCollection<IconReference> IconReferences { get; private set; } = new();

        private void UpdateIcons()
		{
			Settings.Default.IconSource = _IconDirectory;
			Settings.Default.Save();

            SelectedIcon = null;
			IconReferences.Clear();

            foreach (string path in Directory.GetFiles(_IconDirectory, "*.ico", SearchOption.AllDirectories))
            {
                IconReferences.Add(new IconReference(path));
            }
        }

        private IconReference? _SelectedIcon;
        /// <summary>
        /// Gets or sets the selected icon
        /// </summary>
        public IconReference? SelectedIcon
        {
            get => _SelectedIcon;
            set
            {
                if (SetProperty(ref _SelectedIcon, value))
                {
                    SetIconCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public bool CanSetIcon => SelectedFolder != null && SelectedIcon != null;

        [RelayCommand(CanExecute = nameof(CanSetIcon))]
        public void SetIcon()
        {
            if (SelectedIcon != null && SelectedFolder != null)
            {
                SelectedFolder.IconPath = SelectedIcon.Path;
            }
        }

        public bool CanUnsetIcon => SelectedFolder != null;

        [RelayCommand(CanExecute = nameof(CanUnsetIcon))]
        public void UnsetIcon()
        {
            if (SelectedFolder != null)
            {
                SelectedFolder.IconPath = null;
            }
        }

        #endregion
    }
}
