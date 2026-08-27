using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Services
{
    public interface IParseProgressReporter
    {
        void ReportBookParsed(EpubMetadataModel book);
        void ReportCoverCached(string bookId, string cachedCoverPath);
        void ReportError(ParseError error);
    }
}
