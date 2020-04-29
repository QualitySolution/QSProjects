using QS.Print;

namespace QS.DocTemplates
{
    public interface IPrintableOdtDocument : IPrintableDocument
    {
        IDocTemplate GetTemplate();
    }
}