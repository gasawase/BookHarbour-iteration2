using Assets.Scripts.Models;
using System;

namespace Assets.Scripts.Services
{
    public class ParseProgressReporter : IParseProgressReporter
    {
        public event Action<EpubMetadataModel> BookParsed;
        public event Action<string, string> CoverCached;
        public event Action<ParseError> ErrorOccurred;

        public void ReportBookParsed(EpubMetadataModel book)
        {
            BookParsed?.Invoke(book);
        }

        public void ReportCoverCached(string bookUID, string cachedPath)
        {
            CoverCached?.Invoke(bookUID, cachedPath);
        }

        public void ReportError(ParseError error)
        {
            ErrorOccurred?.Invoke(error);
        }
    }
}
