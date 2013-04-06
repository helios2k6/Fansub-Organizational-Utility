using FileNameParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FansubOrganizationalUtility
{
	public static class Program
	{
		private static void PrintProgramHeader()
		{
			Console.WriteLine("Fansub Organizational Tool");
		}

		private static void PrintHelp()
		{
			var builder = new StringBuilder();
			builder.Append("<this program> (Directory with anime. Current directory by default)");

			Console.WriteLine(builder.ToString());
		}

		private static string GetWorkingDirectory(IEnumerable<string> args)
		{
			if (!args.Any())
			{
				return Directory.GetCurrentDirectory();
			}

			if (Directory.Exists(args.First()))
			{
				return args.First();
			}

			throw new InvalidOperationException("Could not get the working directory");
		}

		private static void MoveFansubFile(string workingDirectory, string filePath, FansubFile file)
		{
			var directoryToMoveTo = Path.Combine(workingDirectory, file.SeriesName);

			if (!Directory.Exists(directoryToMoveTo))
			{
				Directory.CreateDirectory(directoryToMoveTo);
			}
			try
			{
				File.Move(filePath, Path.Combine(directoryToMoveTo, Path.GetFileName(filePath)));
			}
			catch (Exception e)
			{
				Console.WriteLine(string.Format("Could not move file {0}. Reason: {1}", filePath, e.Message));
			}
		}

		private static void OrganizeMediaFiles(string workingDirectory)
		{
			var allMediaFiles = Directory.EnumerateFiles(workingDirectory, "*.mkv")
				.Union(Directory.EnumerateFiles(workingDirectory, "*.mp4"))
				.Union(Directory.EnumerateFiles(workingDirectory, "*.avi"))
				.Union(Directory.EnumerateFiles(workingDirectory, "*.wmv"));

			foreach (var f in allMediaFiles)
			{
				var fansubFile = FansubFileParsers.ParseFansubFile(Path.GetFileName(f));
				if (fansubFile == null)
				{
					Console.WriteLine(string.Format("Could not process file: {0}", f));
				}
				else
				{
					MoveFansubFile(workingDirectory, f, fansubFile);
				}
			}
		}

		public static void Main(string[] args)
		{
			string workingDirectory;
			try
			{
				workingDirectory = GetWorkingDirectory(args);
			}
			catch (InvalidOperationException)
			{
				PrintProgramHeader();
				PrintHelp();
				return;
			}

			OrganizeMediaFiles(workingDirectory);
		}
	}
}
