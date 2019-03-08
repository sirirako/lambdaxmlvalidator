using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace xmlchecker
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        string stxsd { get; set; }

        const string BUCKET_NAME = "";
        const string SCHEMA_FILENAME = "";
        const string SCHEMA = "";

        public string Schema_Target { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            Amazon.XRay.Recorder.Handlers.AwsSdk.AWSSDKHandler.RegisterXRayForAllServices();
            S3Client = new AmazonS3Client();
            var bucketName = System.Environment.GetEnvironmentVariable(BUCKET_NAME);
            var schemaName = System.Environment.GetEnvironmentVariable(SCHEMA_FILENAME);
            Schema_Target = System.Environment.GetEnvironmentVariable(SCHEMA);
            stxsd = GetObject(bucketName, schemaName).Result;
            //stxsd = GetObject("siri-lambda-test","books.xsd").Result;
            System.IO.File.WriteAllText("/tmp/books.xsd", stxsd);
            

        }



        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if (s3Event == null)
            {
                return null;
            }

            try
            {
                //var response = await this.S3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);

                var response = await S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
                using (var filereader = new StreamReader(response.ResponseStream))
                {
                    String s3object = await filereader.ReadToEndAsync();
                    XmlReaderSettings settings = new XmlReaderSettings();
                    byte[] byteArray = Encoding.ASCII.GetBytes(s3object);
                    MemoryStream stream = new MemoryStream(byteArray);
                    XmlReader xmlReaderS3object = XmlReader.Create(stream);
                    string curFile = "/tmp/books.xsd";
                    context.Logger.LogLine(File.Exists(curFile) ? "File exists." : "File does not exist.");
                    context.Logger.LogLine("version 10");

                    //context.Logger.LogLine(s3object);
                    

                    //settings.Schemas.Add("urn:books", "/tmp/books.xsd");
                    
                    settings.Schemas.Add(Schema_Target, curFile);
                    settings.CheckCharacters = true;
                    settings.ValidationType = ValidationType.Schema;
                    //context.Logger.LogLine(s3object);

                    XmlReader reader = XmlReader.Create(xmlReaderS3object, settings);
                    XmlDocument document = new XmlDocument();
                    try
                    {
                        document.Load(reader);
                        ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationCallBack);

                        document.Validate(eventHandler);
                        return "good";
                    } catch (Exception e)
                    {
                        context.Logger.LogLine(e.Message);
                        return "bad";
                    };
                }

            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }

        public async Task<string> GetObject(string bucket, string key)
        {
            var response = await S3Client.GetObjectAsync("siri-lambda-test", "books.xsd");
            using (var reader = new StreamReader(response.ResponseStream))
            {
                String s3object = await reader.ReadToEndAsync();
                return s3object;
            }

        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            Console.WriteLine("Validation Error: {0}", e.Message);
        }

    }
}
