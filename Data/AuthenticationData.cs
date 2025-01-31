using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Authentication.Models;
using System.Collections.Generic;

namespace Authentication.Data
{
    public interface IAuthenticationData
    {
        Task<Boolean> AddProfileData(Profile profile);
        Task<Profile> GetProfileData(LoginType loginType);
        Task<Boolean> UpdateProfileData(Profile profile);
    }
    public class AuthenticationData: IAuthenticationData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "Authentication";
        public AuthenticationData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<Boolean> AddProfileData(Profile profile)
        {

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
            {
                { "uuid", new AttributeValue { S = Guid.NewGuid().ToString() } },
                { "username", new AttributeValue { S = profile.username } },
                { "password", new AttributeValue { S = profile.password } },
                { "securitytoken", new AttributeValue { S = profile.securitytoken } },
                { "lastTokenUpdateTime", new AttributeValue { S = profile.lastTokenUpdateTime } }
            }
            };

            try
            {
                // Perform the DynamoDB PutItemAsync operation
                await _dynamoDbClient.PutItemAsync(putRequest);
                return true; // Return the choice object after adding it to DynamoDB
            }
            catch (Exception ex)
            {
                // Log the exception and handle any errors
                Console.WriteLine($"Error adding item to DynamoDB: {ex.Message}");
                throw;
            }
        }

        public async Task<Profile> GetProfileData(LoginType loginType)
        {

            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = loginType.type+"-index",
                KeyConditionExpression = loginType.type+"= :"+ loginType.type,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                    { ":"+loginType.type, new AttributeValue { S = loginType.value } }
                    }
            };

            try
            {
                // Perform the DynamoDB PutItemAsync operation
                var response = await _dynamoDbClient.QueryAsync(request);
                Profile profile = new Profile();
                if (response.Items != null && response.Items.Count > 0)
                {
                    var item = response.Items[0];
                    profile.username = item.ContainsKey("username") ? item["username"].S : null;
                    profile.password = item.ContainsKey("password") ? item["password"].S : null;
                    profile.securitytoken = item.ContainsKey("securitytoken") ? item["securitytoken"].S : null;
                    profile.lastTokenUpdateTime = item.ContainsKey("lastTokenUpdateTime") ? item["lastTokenUpdateTime"].S : null;
                    profile.uuid = item.ContainsKey("uuid") ? item["uuid"].S : null;
                }
                return profile;
            }
            catch (Exception ex)
            {
                // Log the exception and handle any errors
                Console.WriteLine($"Error adding item to DynamoDB: {ex.Message}");
                throw; // Rethrow the exception to handle it at a higher level if needed
            }
        }

        public async Task<Boolean> UpdateProfileData(Profile profile)
        {

            var request = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "uuid", new AttributeValue { S = profile.uuid } },
            },
                UpdateExpression = "SET password = :password, securitytoken = :securitytoken, lastTokenUpdateTime = :lastTokenUpdateTime",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":password", new AttributeValue { S = profile.password } },
                    { ":securitytoken", new AttributeValue { S = profile.securitytoken } },
                    { ":lastTokenUpdateTime", new AttributeValue { S = profile.lastTokenUpdateTime } },
                }
            };

            try
            {
                // Perform the DynamoDB PutItemAsync operation
                await _dynamoDbClient.UpdateItemAsync(request);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception and handle any errors
                Console.WriteLine($"Error adding item to DynamoDB: {ex.Message}");
                throw;
            }
        }
    }
}
