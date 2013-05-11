

namespace DBVC
{
    public interface IResult
    {
        string errorMessage { get; set; }
        int status { get; set; }
        string dbMessage { get; set; }
        string procName { get; set; }
    }
}
