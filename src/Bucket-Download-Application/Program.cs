using Bucket_Download_Application.Common.Constants;
using Bucket_Download_Application.Common.Model;
using Bucket_Download_Application.Helper;

try
{
    Console.WriteLine("AWS S3 Application");
    var s3Client = AWSS3Helper.CreateAmazonS3Client(new AWSCredentials
    {
        AccessKey = AWSAppSettingConstant.AWS_ACCESS_KEY,
        SecretKey = AWSAppSettingConstant.AWS_SECRET_KEY,
    }, Amazon.RegionEndpoint.USWest2);

    Console.WriteLine($"Starting download of bucket: {AWSAppSettingConstant.BUCKET_NAME}");

    await AWSS3Helper.DownloadEntireBucket(
        s3Client,
        AWSAppSettingConstant.BUCKET_NAME,
        AWSAppSettingConstant.LOCAL_FOLDER_PATH);

    Console.WriteLine("Download complete.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}