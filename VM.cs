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
            _FolderDirectory = Settings.Default.DefaultFolder;
            UpdateFolders();
            LoadIcons(Settings.Default.IconSource);
            Debug.WriteLine("Load Complete");
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

        public static ValidationResult? DirectoryExists(string directory, ValidationContext context)
        {
            if (Directory.Exists(directory)) return ValidationResult.Success;
            else return new("Directory does not exist");
        }

        private void UpdateFolders()
        {
            Settings.Default.DefaultFolder = _FolderDirectory;

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

        public ObservableCollection<Folder> Folders { get; private set; } = new();

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
                    ResetIconCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Icons

        public ObservableCollection<IconReference> IconReferences { get; private set; } = new();

        private void LoadIcons(string directory)
        {
            IconReferences.Clear();

            foreach (string path in Directory.GetFiles(directory, "*.ico", SearchOption.AllDirectories))
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

        public bool CanResetIcon => SelectedFolder != null;

        [RelayCommand(CanExecute = nameof(CanResetIcon))]
        public void ResetIcon()
        {
            if (SelectedFolder != null)
            {
                SelectedFolder.IconPath = null;
            }
        }

        #endregion
    }
}
