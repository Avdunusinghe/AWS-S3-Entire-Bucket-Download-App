using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;


namespace Bucket_Download_Application.Helper
{
    public static class AWSS3Helper
    {
        public static AmazonS3Client CreateAmazonS3Client(
            Bucket_Download_Application.Common.Model.AWSCredentials credentialsValues,
            Amazon.RegionEndpoint regionEndPoint)
        {
            var awsCredential = new BasicAWSCredentials(credentialsValues.AccessKey, credentialsValues.SecretKey);

            var config = new AmazonS3Config()
            {
                RegionEndpoint = regionEndPoint
            };

            var client = new AmazonS3Client(awsCredential, config);

            return client;

        }

        public static async Task DownloadEntireBucket(
            IAmazonS3 s3Client,
            string bucketName,
            string localFolderPath)
        {
            string continuationToken = null;

            do
            {
                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    ContinuationToken = continuationToken
                };

                var listObjectsResponse = await s3Client.ListObjectsV2Async(listObjectsRequest);

                foreach (var s3Object in listObjectsResponse.S3Objects)
                {
                    // Skip "folder-like" entries (objects with keys ending in "/")
                    if (s3Object.Key.EndsWith("/"))
                    {
                        Console.WriteLine($"Skipping folder entry: {s3Object.Key}");
                        continue;
                    }

                    // Determine the local file path based on the S3 object key
                    string localFilePath = Path.Combine(localFolderPath, s3Object.Key);

                    // Debug: Log the local file path
                    Console.WriteLine($"Processing file: {localFilePath}");

                    // Ensure the directory exists
                    string? directoryPath = Path.GetDirectoryName(localFilePath);

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        if (!Directory.Exists(directoryPath))
                        {
                            // Debug: Log the directory creation attempt
                            Console.WriteLine($"Creating directory: {directoryPath}");
                            Directory.CreateDirectory(directoryPath); // Create the folder if it doesn't exist
                        }
                    }

                    // Skip downloading if the file already exists
                    if (File.Exists(localFilePath))
                    {
                        Console.WriteLine($"File already exists: {s3Object.Key}. Skipping download.");
                        continue;
                    }


                    // Download the file
                    var getObjectRequest = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = s3Object.Key
                    };

                    using (var response = await s3Client.GetObjectAsync(getObjectRequest))
                    using (var responseStream = response.ResponseStream)
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Downloaded: {s3Object.Key}");
                }

                continuationToken = listObjectsResponse.NextContinuationToken;

            } while (continuationToken != null);
        }
    }
}


