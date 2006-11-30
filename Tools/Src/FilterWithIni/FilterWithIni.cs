/* Copyright (c) Microsoft Corporation. All rights reserved. */

// FilterWithIni.cs
//
// Copies Console.In to Console.Out, filtering as specified by an Assemble.ini
// file containing an [Errors] section (errors to look for) and [NotErrors]
// (errors to exclude).
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

/// <summary>
/// Implements the application.
/// </summary>
///
class App
{
	/// <summary>
	/// Error strings from the [Errors] section of Assemble.ini.
	/// </summary>
	List<string> m_errorStrings = new List<string>(100);

	/// <summary>
	/// Error exception strings from the [NotErrors] section of Assemble.ini.
	/// </summary>
	List<string> m_notErrorStrings = new List<string>(100);

	/// <summary>
	/// Errors from target executable, after filtering using
	/// <c>m_errorStrings</c> and <c>m_notErrorStrings</c>.
	/// </summary>
	List<string> m_errors = new List<string>(200);

	/// <summary>
	/// Used to serialize console output.
	/// </summary>
	Object m_lock = new Object();


	/// <summary>
	/// Application entry point.
	/// </summary>
	///
	/// <param name="args">Command-line arguments.</param>
	///
	public static int Main(string[] args)
	{
		// execute the application
		try
		{
			  App app = new App(args);
		}
		catch (UsageException)
		{
			Console.Error.WriteLine(
				"Usage: FilterWithIni <path-to-Assemble.ini> <command> <arg>...\n" +
				"\n" +
				"Executes a command line, filtering as specified by an Assemble.ini file\n" +
				"containing an [Errors] section (errors to look for) and a [NotErrors] section\n" +
				"(errors to exclude).\n");
			return 1;
		}
#if !DEBUG
		catch (Exception ex)
		{
			Console.Error.WriteLine("Error: {0}", ex.Message);
			return 1;
		}
#endif

		return 0;
	}

	/// <summary>
	/// Executes the application.
	/// </summary>
	///
	/// <param name="args">Command-line arguments.</param>
	///
	App(string[] args)
	{
		// parse command-line arguments; set <iniFilePath> to the path to the
		// Assemble.ini file; set <targetExe> to the name of the target
		// executable; set <targetArgs> to the command-line arguments of the
		// target
		string iniFilePath;
		if ((args.Length < 2) || (args[0] == "/?"))
			throw new UsageException();
		iniFilePath = args[0];
		string targetExe = args[1];
		string[] targetArgs = new string[args.Length - 2];
		Array.Copy(args, 2, targetArgs, 0, args.Length - 2);

		// parse the Assemble.ini file; set <m_errorStrings> and
		// <m_notErrorStrings> to the collections of strings in the [Errors]
		// and [NotErrors] sections, respectively, converted to lowercase
		string line;
		using (StreamReader iniFile = new StreamReader(iniFilePath))
		{
			// loop once per line in Assemble.ini
			string currentSection = null;
			while ((line = iniFile.ReadLine()) != null)
			{
                // remove comment, if any
                int ichComment = line.IndexOf(';');
                if (ichComment >= 0)
                    line = line.Substring(0, ichComment);

				// delete leading and trailing white space, if any
				line = line.Trim();

				// do nothing if the line is blank
				if (line.Length == 0)
					continue;

				// process the line
				if (line.StartsWith("["))
					currentSection = line;
				else
				if (currentSection == "[Errors]")
					m_errorStrings.Add(line.ToLower());
				else
				if (currentSection == "[NotErrors]")
					m_notErrorStrings.Add(line.ToLower());
			}
		}

		// set <startInfo> to information about the target process
		StringBuilder targetArgsString = new StringBuilder(300);
		foreach (string targetArg in targetArgs)
		{
			if (targetArgsString.Length > 0)
				targetArgsString.Append(' ');
			if ((targetArg.IndexOf(' ') >= 0) ||
			    (targetArg.IndexOf('"') >= 0))
			{
				targetArgsString.AppendFormat("\"{0}\"",
					targetArg.Replace("\"", "\"\""));
			}
			else
				targetArgsString.Append(targetArg);
		}
		ProcessStartInfo startInfo = new ProcessStartInfo(targetExe,
			targetArgsString.ToString());

		// execute the target process; populate <m_errors> with a list of
		// errors according to Assemble.ini [Errors] and [NotErrors] sections
        DateTime buildStartTime = DateTime.Now;
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		Process process = new Process();
		process.StartInfo = startInfo;
		process.OutputDataReceived +=
			new DataReceivedEventHandler(process_OutputDataReceived);
		process.ErrorDataReceived +=
			new DataReceivedEventHandler(process_ErrorDataReceived);
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();
        TimeSpan buildDuration = DateTime.Now - buildStartTime;

        // output the build time
        int totalSeconds = (int)(buildDuration.TotalSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        Console.WriteLine();
        Console.WriteLine("Build completed in: {0}:{1:n2}", minutes,
            String.Format(((seconds < 10) ? "0{0}" : "{0}"), seconds));

		// output <m_errors>
		Console.WriteLine();
		ConsoleColor previousForegroundColor = Console.ForegroundColor;
		if (m_errors.Count > 0)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("__________ Error Summary __________");
			Console.WriteLine();
			foreach (string error in m_errors)
				Console.WriteLine(error);
			Console.WriteLine("___________________________________");
		}
		else
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("__________ Error Summary __________");
			Console.WriteLine();
			Console.WriteLine("             No Errors");
			Console.WriteLine("___________________________________");
		}
		Console.ForegroundColor = previousForegroundColor;
	}

	/// <summary>
	/// Called when the target executable emits a line to standard output.
	/// </summary>
	///
	void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		lock (m_lock)
			ProcessOutputLineFromTarget(e.Data, false);
	}

	/// <summary>
	/// Called when the target executable emits a line to standard error.
	/// </summary>
	///
	void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		lock (m_lock)
			ProcessOutputLineFromTarget(e.Data, true);
	}

	/// <summary>
	/// Processes a line of output from the target executable.
	/// </summary>
	///
	/// <param name="line">The line of output.</param>
	///
	/// <param name="errorStream"><c>true</c> if the line was sent to standard
	/// 	error, <c>false</c> if the line was sent to standard output.
	///
	void ProcessOutputLineFromTarget(string line, bool errorStream)
	{
		// sometimes <line> is null -- not sure why
		if (line == null)
			return;

		// set <isError> to true iff <line> is an error according to the
		// [Errors] and [NotErrors] sections of Assemble.ini; set
		// <isSuppressedError> to true iff <line> is in [Errors] but is
		// suppressed because it's also in [NotErrors]
		string lowercaseLine = line.ToLower();
		bool isError = FindSubstrings(lowercaseLine, m_errorStrings);
		bool isSuppressedError;
		if (isError)
		{
			isSuppressedError = FindSubstrings(lowercaseLine,
				m_notErrorStrings);
			if (isSuppressedError)
				isError = false;
		}
		else
			isSuppressedError = false;

		// if <line> is an error according to Assemble.ini, change the
		// background color and record it in <m_errors>
		ConsoleColor previousForegroundColor = Console.ForegroundColor;
        if (isError)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            m_errors.Add(line);
        }
        else
            if (isSuppressedError)
                Console.ForegroundColor = ConsoleColor.DarkGray;

		// write <line>
		Console.WriteLine(line);

		// if the background color was changed, restore it
		if (isError || isSuppressedError)
			Console.ForegroundColor = previousForegroundColor;
	}

	/// <summary>
	/// Returns <c>true</c> if a given line contains any substring from a
	/// given list, <c>false</c> otherwise.
	/// </summary>
	///
	bool FindSubstrings(string line, List<string> list)
	{
		foreach (string item in list)
		{
			if (line.IndexOf(item) >= 0)
				return true;
		}
		return false;
	}
}

/// <summary>
/// Indicates incorrect command-line usage.
/// </summary>
///
class UsageException : Exception
{
}

