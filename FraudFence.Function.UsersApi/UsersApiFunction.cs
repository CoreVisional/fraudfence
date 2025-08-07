using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using FraudFence.EntityModels.Dto;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.UsersApi;

public class UsersApiFunction
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly string _userPoolId;
    private readonly string _appClientId;

    public UsersApiFunction()
    {
        _cognitoClient = new AmazonCognitoIdentityProviderClient();
        // In a real application, use a more robust configuration management system.
        var config = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));
        _userPoolId = config.GetProperty("AWS").GetProperty("Cognito").GetProperty("UserPoolId").GetString()!;
        _appClientId = config.GetProperty("AWS").GetProperty("Cognito").GetProperty("AppClientId").GetString()!;
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Request received: {request.HttpMethod} {request.Path}");
        context.Logger.LogInformation($"Request body: {request.Body}");

        try
        {
            return request.HttpMethod.ToUpper() switch
            {
                "POST" when request.Path.EndsWith("/register") => await RegisterUser(request, context),
                "POST" when request.Path.EndsWith("/login") => await LoginUser(request, context),
                "GET" when request.Path.EndsWith("/users") => await ListUsers(context),
                "GET" when request.Path.Contains("/users/") => await GetUser(request, context),
                "PUT" when request.Path.Contains("/users/") => await UpdateUser(request, context),
                "DELETE" when request.Path.Contains("/users/") => await DeleteUser(request, context),
                "POST" when request.Path.Contains("/users/") && request.Path.Contains("/groups") => await AddUserToGroup(request, context),
                _ => new APIGatewayProxyResponse { StatusCode = 404, Body = "Not Found" }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"An error occurred: {ex.Message}\n{ex.StackTrace}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { message = "Internal Server Error" })
            };
        }
    }

    private async Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var registrationDto = JsonSerializer.Deserialize<RegistrationDTO>(request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (registrationDto == null || string.IsNullOrWhiteSpace(registrationDto.Email) || string.IsNullOrWhiteSpace(registrationDto.Password))
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid registration data." };
        }

        var createUserRequest = new AdminCreateUserRequest
        {
            UserPoolId = _userPoolId,
            Username = registrationDto.Email,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = registrationDto.Email },
                new AttributeType { Name = "name", Value = registrationDto.Name ?? registrationDto.Email },
                new AttributeType { Name = "email_verified", Value = "true" }
            },
            MessageAction = MessageActionType.SUPPRESS // Do not send welcome email
        };

        try
        {
            var createUserResponse = await _cognitoClient.AdminCreateUserAsync(createUserRequest);
            context.Logger.LogInformation($"User {registrationDto.Email} created successfully.");

            var setPasswordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = registrationDto.Email,
                Password = registrationDto.Password,
                Permanent = true
            };

            await _cognitoClient.AdminSetUserPasswordAsync(setPasswordRequest);
            context.Logger.LogInformation($"Password for user {registrationDto.Email} set successfully.");

            return new APIGatewayProxyResponse
            {
                StatusCode = 201,
                Body = JsonSerializer.Serialize(new { message = "User registered successfully.", userId = createUserResponse.User.Username })
            };
        }
        catch (UsernameExistsException)
        {
            return new APIGatewayProxyResponse { StatusCode = 409, Body = "User with this email already exists." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error registering user: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred during registration." };
        }
    }

    private async Task<APIGatewayProxyResponse> LoginUser(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var loginDto = JsonSerializer.Deserialize<LoginDTO>(request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid login data." };
        }

        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _appClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", loginDto.Email },
                { "PASSWORD", loginDto.Password }
            }
        };

        try
        {
            var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest);
            context.Logger.LogInformation($"User {loginDto.Email} logged in successfully.");
            context.Logger.LogInformation($"Cognito Response: {JsonSerializer.Serialize(authResponse)}");

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new
                {
                    IdToken = authResponse.AuthenticationResult.IdToken,
                    AccessToken = authResponse.AuthenticationResult.AccessToken,
                    RefreshToken = authResponse.AuthenticationResult.RefreshToken,
                    ExpiresIn = authResponse.AuthenticationResult.ExpiresIn
                })
            };
        }
        catch (NotAuthorizedException)
        {
            return new APIGatewayProxyResponse { StatusCode = 401, Body = "Invalid credentials." };
        }
        catch (UserNotFoundException)
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "User not found." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error logging in user: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred during login." };
        }
    
    }

    private async Task<APIGatewayProxyResponse> ListUsers(ILambdaContext context)
    {
        try
        {
            var request = new ListUsersRequest { UserPoolId = _userPoolId };
            var response = await _cognitoClient.ListUsersAsync(request);

            var userList = new List<UserViewModel>();
            foreach (var user in response.Users)
            {
                var groupsRequest = new AdminListGroupsForUserRequest
                {
                    UserPoolId = _userPoolId,
                    Username = user.Username
                };
                var groupsResponse = await _cognitoClient.AdminListGroupsForUserAsync(groupsRequest);

                userList.Add(new UserViewModel
                {
                    Id = user.Username, // Using Cognito's Username (sub) as the ID
                    Email = user.Attributes.FirstOrDefault(a => a.Name == "email")?.Value,
                    Name = user.Attributes.FirstOrDefault(a => a.Name == "name")?.Value,
                    IsActive = user.Enabled,
                    UserName = user.Username,
                    Roles = groupsResponse.Groups.Select(g => g.GroupName).ToList()
                });
            }

            return new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(userList) };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error listing users: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred while listing users." };
        }
    }

    private async Task<APIGatewayProxyResponse> GetUser(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userId = request.PathParameters["id"];
        try
        {
            var userRequest = new AdminGetUserRequest { UserPoolId = _userPoolId, Username = userId };
            var response = await _cognitoClient.AdminGetUserAsync(userRequest);

            var userViewModel = new UserViewModel
            {
                Id = response.Username,
                Email = response.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value,
                Name = response.UserAttributes.FirstOrDefault(a => a.Name == "name")?.Value,
                IsActive = response.Enabled,
                UserName = response.Username
            };

            return new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(userViewModel) };
        }
        catch (UserNotFoundException)
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "User not found." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error getting user {userId}: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred." };
        }
    }

    private async Task<APIGatewayProxyResponse> UpdateUser(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userId = request.PathParameters["id"];
        var userDto = JsonSerializer.Deserialize<EditUserViewModel>(request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userDto == null)
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid user data." };
        }

        try
        {
            var updateUserRequest = new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _userPoolId,
                Username = userId,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "name", Value = userDto.Name },
                    new AttributeType { Name = "email", Value = userDto.Email },
                    new AttributeType { Name = "phone_number", Value = userDto.PhoneNumber }
                }
            };
            await _cognitoClient.AdminUpdateUserAttributesAsync(updateUserRequest);

            // Role management would go here if needed, by adding/removing users from Cognito Groups.

            return new APIGatewayProxyResponse { StatusCode = 200, Body = "User updated successfully." };
        }
        catch (UserNotFoundException)
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "User not found." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error updating user {userId}: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred during update." };
        }
    }

    private async Task<APIGatewayProxyResponse> DeleteUser(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userId = request.PathParameters["id"];
        try
        {
            var deleteUserRequest = new AdminDeleteUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            };
            await _cognitoClient.AdminDeleteUserAsync(deleteUserRequest);

            return new APIGatewayProxyResponse { StatusCode = 204 }; // No Content
        }
        catch (UserNotFoundException)
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "User not found." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error deleting user {userId}: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred during deletion." };
        }
    }
    private async Task<APIGatewayProxyResponse> AddUserToGroup(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userId = request.PathParameters["id"];
        var groupData = JsonSerializer.Deserialize<JsonElement>(request.Body);
        var groupName = groupData.GetProperty("groupName").GetString();

        if (string.IsNullOrEmpty(groupName))
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Group name is required." };
        }

        try
        {
            var addUserToGroupRequest = new AdminAddUserToGroupRequest
            {
                UserPoolId = _userPoolId,
                Username = userId,
                GroupName = groupName
            };
            await _cognitoClient.AdminAddUserToGroupAsync(addUserToGroupRequest);

            return new APIGatewayProxyResponse { StatusCode = 200, Body = "User added to group successfully." };
        }
        catch (UserNotFoundException)
        {
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "User not found." };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error adding user {userId} to group {groupName}: {ex.Message}");
            return new APIGatewayProxyResponse { StatusCode = 500, Body = "An error occurred." };
        }
    }
}