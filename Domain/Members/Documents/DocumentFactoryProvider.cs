using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.Domain.Members;

public class DocumentFactoryProvider : IDocumentFactoryProvider
{
    private readonly IEnumerable<IDocumentFactory> _factories;
    private readonly ILogger<DocumentFactoryProvider> _logger;

    public DocumentFactoryProvider(IEnumerable<IDocumentFactory> factories, ILogger<DocumentFactoryProvider> logger)
    {
        _factories = factories;
        _logger = logger;
    }

    public IDocumentFactory GetFactory(DocumentType type)
    {
        var factory = _factories.FirstOrDefault(f => f.DocumentType == type);

        if (factory == null)
        {
            var message = $"No document factory defined for type: {type}";
            _logger.LogError(message);
            throw new ArgumentException(message);
        }

        return factory;
    }
}