using System;

namespace ShamefulOldGit.Actors
{
	internal class EmailDetailsFileIncorrect : Exception
	{
		private readonly string _line;
		private readonly int _lineNumber;

		public EmailDetailsFileIncorrect(string line, int lineNumber)
		{
			_line = line;
			_lineNumber = lineNumber;
		}

		public override string Message => $"Line {_lineNumber} needs to start with {_line}";
	}
}