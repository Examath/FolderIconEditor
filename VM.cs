using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderIconEditor
{
    public partial class VM : ObservableValidator
    {
        #region Folders

        private string _FolderDirectory = "";
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
            DirectoryInfo directoryInfo = new DirectoryInfo(FolderDirectory);

            if (directoryInfo.Exists)
            {
                directoryInfo.
            }
        }


        #endregion

        #region Icons

        #endregion

        #region Merger

        #endregion
    }
}
