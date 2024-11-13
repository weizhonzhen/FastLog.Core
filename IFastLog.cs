using FastLog.Core.ES.Model;
using System.Collections.Generic;

namespace FastLog.Core
{
    public interface IFastLog
    {
        void Save(string type, string title, string message);

        void Delete(string type, string title);

        List<string> Type();

        List<Dictionary<string, object>> GetList(string type);

        int Count(string type);

        PageResult Page(string type, string title, string content, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true);
    }
}
