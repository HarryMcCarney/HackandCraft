using System.Xml;

namespace DBVC
{
    public interface IDataSource
    {
        XmlDocument execStoredProc(string strProcName, XmlDocument strParameters);
    }
}
