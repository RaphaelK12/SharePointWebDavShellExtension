﻿using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SharePointWebDavShellExtension.Helpers
{
	public static class Path
	{
		[DllImport("shlwapi.dll")]
		private static extern bool PathIsNetworkPath(string pszPath);

		[DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WNetGetConnection(
			 [MarshalAs(UnmanagedType.LPTStr)] string localName,
			 [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
			 ref int length);

		public static bool IsNetworkPath(string path)
		{
			return PathIsNetworkPath(path);
		}

		/// <summary>
		/// Given a path, returns the UNC path or the original. (No exceptions
		/// are raised by this function directly). For example, "P:\2008-02-29"
		/// might return: "\\networkserver\Shares\Photos\2008-02-09"
		/// </summary>
		/// <param name="originalPath">The path to convert to a UNC Path</param>
		/// <returns>A UNC path. If a network drive letter is specified, the
		/// drive letter is converted to a UNC or network path. If the 
		/// originalPath cannot be converted, it is returned unchanged.</returns>
		public static string GetUNCPath(string originalPath)
		{
			StringBuilder sb = new StringBuilder(512);
			int size = sb.Capacity;

			// look for the {LETTER}: combination ...
			if (originalPath.Length > 2 && originalPath[1] == ':')
			{
				// don't use char.IsLetter here - as that can be misleading
				// the only valid drive letters are a-z && A-Z.
				char c = originalPath[0];
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
				{
					int error = WNetGetConnection(originalPath.Substring(0, 2),
						 sb, ref size);
					if (error == 0)
					{
						DirectoryInfo dir = new DirectoryInfo(originalPath);

						string path = System.IO.Path.GetFullPath(originalPath)
							 .Substring(System.IO.Path.GetPathRoot(originalPath).Length);
						return System.IO.Path.Combine(sb.ToString().TrimEnd(), path);
					}
				}
			}

			return originalPath;
		}

		// Converts an file path from sharepointed unc to an url
		public static string ConvertToUrl(string uncPath, bool makeServerRelative)
		{
			//\\sp2013@8080\DavWWWRoot\Style Library\test.css
			var url = uncPath.Replace('\\', '/').Replace('@', ':').Replace(SharePointShellExtensionContextMenu.WebDavRootFolder, string.Empty).Replace("//", "/").Trim('/');
			url = "http://" + url;

			if (makeServerRelative)
			{
				var httpTrimmed = url.Replace("http://", string.Empty);
				url = httpTrimmed.Substring(httpTrimmed.IndexOf('/'));
			}

			return url;
		}
	}
}
