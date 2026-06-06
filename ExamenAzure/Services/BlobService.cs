using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ExamenAzure.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "movies";

        public BlobService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureBlobStorage")
                ?? throw new ArgumentNullException("AzureBlobStorage connection string is missing.");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadVideoAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return uniqueFileName;
        }

        public string GenerateSasToken(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Check connection string credentials.");
            }

            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b", // 'b' означає Blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(120)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }
    }
}
