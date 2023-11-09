namespace Theoistic.PDF;

internal class HtmlToPdfDocument : IDocument
{
    public List<ObjectSettings> Objects { get;  private set;}
    
    private GlobalSettings globalSettings = new GlobalSettings();

    public GlobalSettings GlobalSettings {
        get { return globalSettings; }
        set {
            globalSettings = value;
        }
    }

    public HtmlToPdfDocument()
    {
        Objects = new List<ObjectSettings>();
    }

    public IEnumerable<IObject> GetObjects()
    {
        return Objects.ToArray();
    }   
}
