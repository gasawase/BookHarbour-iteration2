using Assets.Scripts.Models;
using System;

namespace Assets.Scripts.Services
{
    public class ProgressReporter : IParseProgressReporter
    {
        public event Action<EpubMetadataModel> BookParsed;
        public event Action<string, string, UIBookData> CoverCached;
        public event Action<ParseError> ErrorOccurred;

        public void ReportBookParsed(EpubMetadataModel book)
        {
            BookParsed?.Invoke(book);
        }

        public void ReportCoverCached(string bookUID, string cachedPath, UIBookData uiBookData)
        {
            CoverCached?.Invoke(bookUID, cachedPath, uiBookData);
        }

        public void ReportError(ParseError error)
        {
            ErrorOccurred?.Invoke(error);
        }

    }
}
