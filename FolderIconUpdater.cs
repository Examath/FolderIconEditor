using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderIconEditor
{
	/// <summary>
	/// Wrapper for shell32.dll api to manipulate folder icons
	/// </summary>
	internal static class FolderIconUpdater
	{
		// Method adapted from https://stackoverflow.com/a/73381494/10701111

		/// <summary>
		/// Sets or retrieves custom folder settings. This function reads from and writes to Desktop.ini.
		/// See <see href="https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shgetsetfoldercustomsettings"/> for documentation.
		/// </summary>
		/// <remarks>
		/// SHGetSetFolderCustomSettings is available for use in select operating systems.
		/// It may be altered or unavailable in subsequent versions.
		/// </remarks>
		/// <param name="pfcs">A pointer to a SHFOLDERCUSTOMSETTINGS structure that provides or receives the custom folder settings.</param>
		/// <param name="pszPath">A pointer to a null-terminated Unicode string that contains the path to the folder. 
		/// The length of pszPath must be MAX_PATH or less, including the terminating null character.</param>
		/// <param name="dwReadWrite">A flag that controls the action of the function. It may be one of the following values: FCS_READ (0x00000001), FCS_FORCEWRITE (0x00000002)</param>
		/// <returns>If this function succeeds, it returns S_OK (0x0000). Otherwise, it returns an HRESULT error code.</returns>
		[DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SHGetSetFolderCustomSettings(IntPtr pfcs, string pszPath, uint dwReadWrite);

		/// <summary>
		/// Use pfcs to set the custom folder's settings regardless of whether the values are already present.
		/// </summary>
		private const uint FCS_FORCEWRITE = 0x00000002;

		/// <summary>
		/// Folder attribute mask flag: pszIconFile contains the path to the file containing the folder's icon.
		/// </summary>
		private const uint FCSM_ICONFILE = 0x00000010;

		// Incomplete docs. See for more:
		//  https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-shfoldercustomsettings
		/// <summary>
		/// Holds custom folder settings. This structure is used with the <see cref="SHGetSetFolderCustomSettings(nint, string, uint)"/> function.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFOLDERCUSTOMSETTINGS
		{
			/// <summary>
			/// The size of the structure, in bytes.
			/// </summary>
			public uint dwSize;

			/// <summary>
			/// A DWORD value specifying which folder attributes to read or write from this structure.
			/// Go to <see href="https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-shfoldercustomsettings"/>
			/// to see the values to indicate which structure members are valid
			/// </summary>
			public uint dwMask;

			// Following fields not required
			public IntPtr pvid;
			public IntPtr pszWebViewTemplate;
			public uint cchWebViewTemplate;
			public IntPtr pszWebViewTemplateVersion;
			public IntPtr pszInfoTip;
			public uint cchInfoTip;
			public IntPtr pclsid;
			public uint dwFlags;

			/// <summary>
			/// A pointer to a null-terminated buffer containing the path to file containing the folder's icon.
			/// </summary>
			public IntPtr pszIconFile;

			/// <summary>
			/// If the SHGetSetFolderCustomSettings parameter dwReadWrite is FCS_READ, this is the size of the pszIconFile buffer, in characters. 
			/// If not, this is the number of characters to write from that buffer. Set this parameter to 0 to write the entire string.
			/// </summary>
			public uint cchIconFile;

			/// <summary>
			/// The index of the icon within the file named in pszIconFile.
			/// </summary>
			public int iIconIndex;

			// Following fields not required
			public IntPtr pszLogo;
			public uint cchLogo;
		}

		/// <summary>
		/// Sets the icon of a folder
		/// </summary>
		/// <param name="folderPath">The directory to set</param>
		/// <param name="iconPath">The path to the icon</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="COMException"></exception>
		internal static void SetFolderIcon(string folderPath, string? iconPath)
		{
			if (string.IsNullOrWhiteSpace(folderPath))
			{
				throw new ArgumentException("Invalid folder path.");
			}

			IntPtr iconPathPtr = IntPtr.Zero;
			IntPtr fcsPtr = IntPtr.Zero;

			try
			{
				// Allocate memory for the icon path
				if (iconPath != null) iconPathPtr = Marshal.StringToHGlobalAuto(iconPath);

				// Initialize the SHFOLDERCUSTOMSETTINGS structure
				SHFOLDERCUSTOMSETTINGS fcs = new()
				{
					dwSize = (uint)Marshal.SizeOf(typeof(SHFOLDERCUSTOMSETTINGS)),
					dwMask = FCSM_ICONFILE,
					pszIconFile = iconPathPtr,
					cchIconFile = 0,
					iIconIndex = 0
				};

				// Allocate structure
				fcsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SHFOLDERCUSTOMSETTINGS)));
				Marshal.StructureToPtr(fcs, fcsPtr, false);

				// Call SHGetSetFolderCustomSettings
				int result = SHGetSetFolderCustomSettings(fcsPtr, folderPath, FCS_FORCEWRITE);
				if (result != 0) throw new COMException("Error occurred calling SHGetSetFolderCustomSettings()", result);
			}
			finally
			{
				// Free the allocated memory
				if (iconPathPtr != IntPtr.Zero) Marshal.FreeHGlobal(iconPathPtr);
				if (fcsPtr != IntPtr.Zero) Marshal.FreeHGlobal(fcsPtr);
			}
		}
	}
}