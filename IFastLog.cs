using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using System.Collections.Generic;

namespace FastLog.Core
{
    public interface IFastLog
    {
        void Save(LogModel model);

        void Delete(string type, string title, string id);

        void Delete(string type, string title, List<string> id);

        List<string> Type(int size = 10);

        List<Dictionary<string, object>> GetList(string type, int size = 10);

        int Count(string type);

        PageResult Page(string type, string title, string content, string person, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true);
    }
}