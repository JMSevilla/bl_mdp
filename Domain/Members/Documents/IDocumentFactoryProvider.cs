namespace WTW.MdpService.Domain.Members;

public interface IDocumentFactoryProvider
{
    IDocumentFactory GetFactory(DocumentType type);
}