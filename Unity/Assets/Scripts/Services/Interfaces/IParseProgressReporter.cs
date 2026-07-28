using EpubParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpubParser.Services
{
    public interface IParseProgressReporter
    {
        void ReportBookParsed(EpubMetadataModel book);
        void ReportCoverCached(string bookId, string cachedCoverPath);
        void ReportError(ParseError error);
    }
}
