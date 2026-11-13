using System.Diagnostics;
using System.IO;

namespace SteelSeriesSonarCompanion.CoreApp.Logging
{
	public class FileTraceListener : TraceListener
	{
		private StreamWriter Writer { get; set; }
		private bool IsAtLineStart { get; set; } = true;

		private const string FOLDER_NAME = "SteelSeriesSonarCompanion";
		private const string FILE_NAME = "companion.log";
		private const string PREVIOUS_FILE_NAME = "companion.old.log";
		private const long MAX_FILE_SIZE_BYTES = 1024 * 1024;
		private const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

		private FileTraceListener (StreamWriter writer)
		{
			Writer = writer;
		}

		public static FileTraceListener? TryCreate ()
		{
			try
			{
				string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FOLDER_NAME);
				Directory.CreateDirectory(folderPath);
				string filePath = Path.Combine(folderPath, FILE_NAME);

				FileInfo fileInfo = new(filePath);

				if (fileInfo.Exists == true && fileInfo.Length > MAX_FILE_SIZE_BYTES)
				{
					File.Move(filePath, Path.Combine(folderPath, PREVIOUS_FILE_NAME), true);
				}

				FileStream stream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				return new FileTraceListener(new StreamWriter(stream) { AutoFlush = true });
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override void Write (string? message)
		{
			if (IsAtLineStart == true)
			{
				Writer.Write($"{DateTime.Now.ToString(TIMESTAMP_FORMAT)} ");
			}

			Writer.Write(message);
			IsAtLineStart = false;
		}

		public override void WriteLine (string? message)
		{
			Write(message);
			Writer.WriteLine();
			IsAtLineStart = true;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing == true)
			{
				Writer.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
