using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.Infrastructure.Aws;

public class AwsClient : IAwsClient
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<AwsClient> _logger;

    public AwsClient(IAmazonS3 client, ILogger<AwsClient> logger)
    {
        _s3Client = client;
        _logger = logger;
    }

    public async Task<Either<Error, MemoryStream>> File(string uri)
    {
        try
        {
            Uri s3Uri = new Uri(uri);
            var awsRequest = new GetObjectRequest { BucketName = s3Uri.Host, Key = s3Uri.AbsolutePath.TrimStart('/') };

            _logger.LogInformation("Trying to retrieve file from S3. Uri: {s3Uri}. Bucket name: {s3BucketName}. Key: {s3Key}.",
                uri, awsRequest.BucketName, awsRequest.Key);
            using var response = await _s3Client.GetObjectAsync(awsRequest);

            _logger.LogInformation("S3 response http status code: {s3HttpStatusCode}.", response.HttpStatusCode);

            var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            ms.Position = 0;

            return ms;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve file from S3 bucket. Uri: {s3Uri}.", uri);
            return Error.New(e.Message);
        }
    }
}