using UnityEngine;
namespace EpubParser.Models
{
    public class Enums
    {
        public enum eReadingStatus
        {
            Unknown,
            Unread,
            Reading,
            Finished,
            DidNotFinish
        }
        public enum ePageCountSource
        {
            None,
            Epub, // TODO: need to make sure that we can actually GET a pagecount from some epubs; if not, we need to change this enum
            GoogleBooksAPI,
            HardcoverAPI
        }
        public enum eShelfType
        {
            None,
            Manual,
            Genre,
            PageCount
        }
        public enum AnnotationType
        {
            None,
            Note,
            Bookmark
        }
    }
}
