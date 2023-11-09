namespace Theoistic.PDF;

internal interface IDocument : ISettings
{
   IEnumerable<IObject> GetObjects();
}

internal interface IObject : ISettings
{
    byte[] GetContent();
}

internal interface ISettings { }
